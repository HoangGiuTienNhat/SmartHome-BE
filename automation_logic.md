# Nghiệp vụ Tự động hóa và Đồng bộ hóa (Automation & Sync Logic)

Tài liệu này mô tả chi tiết các quy tắc nghiệp vụ mới được thiết lập cho hệ thống SmartHome, tập trung vào mối quan hệ giữa Cảm biến (Sensor) và Thiết bị đầu ra (Output Device).

## 1. Mối quan hệ Sensor - Output Device
Mỗi thiết bị đầu ra (`OutputDevice`) có thể được liên kết với một cảm biến (`Sensor`) thông qua trường `ConnectedSensorId`.
- **Mục đích:** Để cảm biến có thể tự động điều khiển thiết bị đầu ra dựa trên các ngưỡng giá trị (Thresholds).
- **Phạm vi:** Một cảm biến có thể điều khiển nhiều thiết bị đầu ra (1-N).

## 2. Chế độ Hoạt động (AUTO vs MANUAL)
Hệ thống phân tách rõ ràng giữa hai chế độ để tránh xung đột quyền điều khiển:

### 2.1. Chế độ MANUAL (Thủ công)
- **Kích hoạt:** Khi người dùng điều khiển thiết bị (Bật/Tắt/Chỉnh giá trị) qua App hoặc trực tiếp trên Dashboard của Adafruit IO.
- **Hành vi:** 
    - Trường `Auto` được set thành `false`.
    - Trạng thái `OnOffState` cập nhật thành `ON` hoặc `OFF`.
    - **Cảm biến sẽ không còn quyền điều khiển thiết bị này** ngay cả khi giá trị vượt ngưỡng.
- **Log:** Ghi nhật ký với `LogType = MANUAL`.

### 2.2. Chế độ AUTO (Tự động)
- **Kích hoạt:** Người dùng chủ động chuyển trạng thái thiết bị sang `AUTO` qua App hoặc Adafruit.
- **Hành vi:**
    - Trường `Auto` được set thành `true`.
    - Trạng thái `OnOffState` ban đầu là `AUTO`.
    - **Cảm biến bắt đầu tiếp quản điều khiển** dựa trên logic ngưỡng.
- **Log:** Các hành động do cảm biến kích hoạt sẽ được ghi với `LogType = AUTO`.

## 3. Logic Ngưỡng (Threshold Logic) - Kịch bản 1
Khi một Cảm biến gửi dữ liệu mới, hệ thống sẽ kiểm tra tất cả các thiết bị Output được liên kết có `Auto == true`:

1. **Điều kiện BẬT:**
   - Nếu `Giá trị Sensor > ThresholdMax`
   - VÀ (Thiết bị đang `OFF` hoặc đang ở trạng thái `AUTO` chờ xử lý)
   - **Hành động:** Tự động bật thiết bị (`ON`). Gửi lệnh `1` lên Adafruit.

2. **Điều kiện TẮT:**
   - Nếu `Giá trị Sensor < ThresholdMin`
   - VÀ (Thiết bị đang `ON` hoặc đang ở trạng thái `AUTO` chờ xử lý)
   - **Hành động:** Tự động tắt thiết bị (`OFF`). Gửi lệnh `0` lên Adafruit.

3. **Vùng Đệm (Hysteresis):**
   - Nếu giá trị nằm giữa `Min` và `Max`, thiết bị giữ nguyên trạng thái hiện tại để tránh việc bật/tắt liên tục khi dữ liệu dao động nhỏ.

## 4. Đồng bộ hóa với Adafruit IO
Hệ thống sử dụng cơ chế **Smart Sync** để phân biệt giữa tin nhắn xác nhận tự động và sự can thiệp của con người:

- **Chiều Xuống (App -> Adafruit):** Khi bạn điều khiển qua API, Backend sẽ cập nhật DB và Publish MQTT ngay lập tức.
- **Chiều Ngược (Adafruit -> App):** 
    - **Trường hợp Xác nhận (Confirmation):** Nếu giá trị nhận về từ MQTT **trùng khớp** với trạng thái hiện tại trong Database (ví dụ: Sensor vừa bật đèn và Adafruit gửi xác nhận đã bật), hệ thống sẽ **giữ nguyên chế độ AUTO**.
    - **Trường hợp Can thiệp (Intervention):** Nếu giá trị nhận về **khác** với Database khi đang ở chế độ AUTO (ví dụ: người dùng nhấn nút trên Dashboard Adafruit để tắt đèn khi Sensor đang bật), hệ thống sẽ lập tức chuyển sang chế độ **MANUAL** (`Auto = false`) để nhường quyền điều khiển cho con người.
    - **Lệnh Chế độ:** Nếu nhận được chuỗi `"AUTO"`, hệ thống luôn cập nhật `Auto = true`.

## 5. Phân tích Case Study: Duy trì trạng thái
- **Yêu cầu:** Nếu thiết bị đang được Sensor bật (AUTO), người dùng muốn can thiệp nhấn "BẬT" trên App để duy trì trạng thái đó mãi mãi (không cho Sensor tự tắt khi giá trị giảm).
- **Giải pháp:** Khi người dùng thao tác qua App, `DeviceService` sẽ chủ động set `Auto = false` ngay lập tức trước khi gửi lệnh đi. Do đó, thiết bị sẽ thoát khỏi sự điều khiển của Sensor và duy trì trạng thái ON theo ý muốn người dùng.

---

## 6. Demo API

### 5.1. Liên kết Output Device với Sensor
Sử dụng API cập nhật thiết bị để tạo mối liên kết.

* **Endpoint:** `PUT /api/devices/{outputDeviceId}`
* **Body:**
```json
{
  "connectedSensorId": "37e6e893-e9f1-4293-8037-117917021996" // ID cua sensor
}
```

### 5.2. Chuyển thiết bị sang chế độ AUTO
* **Endpoint:** `POST /api/devices/{outputDeviceId}/control`
* **Body:**
```json
{
  "status": "AUTO"
}
```
* **Kết quả:** `Auto` thành `true`, `OnOffState` thành `AUTO`. Hệ thống sẵn sàng tự động hóa.

### 5.3. Điều khiển thủ công (Hủy chế độ AUTO)
* **Endpoint:** `POST /api/devices/{outputDeviceId}/control`
* **Body:**
```json
{
  "status": "ON",
  "value": 1
}
```
* **Kết quả:** `Auto` thành `false`, `OnOffState` thành `ON`. Sensor sẽ không còn tác động đến thiết bị này.

---

## 6. Các bước cần làm sau khi cập nhật Code

1. **Chạy Migration:** Bạn **CẦN** chạy cập nhật Database vì cấu trúc bảng `output_devices` đã thay đổi (thêm cột `connected_sensor_id`).
   ```bash
   # Tại thư mục gốc project
   dotnet ef database update --project src/SmartHome.Infrastructure --startup-project src/SmartHome.API
   ```

2. **Kiểm tra Build:** Đảm bảo mọi thứ vẫn ổn định.
   ```bash
   dotnet build
   ```

3. **Thiết lập Liên kết:** Truy cập Database hoặc dùng API `PUT /api/devices/{id}` để gán `ConnectedSensorId` cho các thiết bị Output của bạn để trải nghiệm tính năng mới.
