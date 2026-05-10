# Truy Vết Luồng Dữ Liệu (Data Flow Tracing)

Hệ thống xử lý hai luồng dữ liệu chính: Luồng HTTP Request (từ người dùng) và Luồng MQTT Message (từ thiết bị/Adafruit).

## 1. Luồng HTTP Request (REST API)
Đây là luồng khi người dùng tương tác qua giao diện (ví dụ: Bật đèn).

1.  **Client (Mobile/Web):** Gửi một yêu cầu `POST /api/devices/{id}/control` với trạng thái "ON".
2.  **API Controller (`DevicesController`):** Nhận yêu cầu, kiểm tra quyền sở hữu (JWT Token), và gọi `_deviceService.ControlDeviceAsync`.
3.  **Application Service (`DeviceService`):**
    - Kiểm tra trạng thái thiết bị trong DB.
    - Gọi `_mqttService.PublishAsync` để gửi lệnh tới Adafruit IO.
    - Tạo một bản ghi log mới.
4.  **Infrastructure (`AdafruitMqttService`):** Thực thi lệnh gửi tin nhắn qua giao thức MQTT.
5.  **Database:** Lưu lại thay đổi trạng thái và log hành động.
6.  **Response:** Trả về kết quả thành công cho người dùng.

## 2. Luồng MQTT Message (Từ Adafruit IO)
Đây là luồng khi thiết bị gửi dữ liệu lên (Sensor) hoặc xác nhận trạng thái (Output).

1.  **Background Service (`MqttHostedService`):** Lắng nghe các tin nhắn từ các Feed đã Subscribe.
2.  **Message Received:** Khi có tin nhắn mới, service gọi `IMqttMessageProcessor.ProcessMessageAsync`.
3.  **Processor (`MqttMessageProcessor`):**
    - **Nếu là Sensor:**
        - Lưu giá trị mới vào bảng `SensorData`.
        - Kiểm tra ngưỡng (`ThresholdMin`, `ThresholdMax`).
        - Nếu vượt ngưỡng, tự động gửi lệnh bật/tắt tới các thiết bị `OutputDevice` liên quan (Logic tự động hóa).
    - **Nếu là OutputDevice:**
        - Cập nhật trạng thái thực tế của thiết bị vào DB (Đồng bộ hóa).
        - Ghi log nếu có sự thay đổi trạng thái (ví dụ: người dùng nhấn nút cứng tại thiết bị).
4.  **Database:** Cập nhật dữ liệu sensor, trạng thái thiết bị và log.

## 3. Luồng Tự Động Hóa (Automation Logic)
- **Kích hoạt:** Khi `MqttMessageProcessor` nhận dữ liệu Sensor.
- **Xử lý:**
    - Tìm các `OutputDevice` đang ở chế độ `AUTO` và được liên kết với Sensor đó.
    - So sánh giá trị Sensor với `Threshold`.
    - Nếu thỏa mãn điều kiện -> Gửi lệnh MQTT tới `OutputDevice` đó -> Ghi log loại `AUTO`.
