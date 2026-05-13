# ĐẶC TẢ KỸ THUẬT: MODULE ĐIỀU KHIỂN THIẾT BỊ BẰNG AI (NATURAL LANGUAGE CONTROL)

## 1. Tổng quan
Module này cho phép người dùng điều khiển hệ thống SmartHome thông qua câu lệnh văn bản tiếng Việt tự nhiên. Hệ thống sẽ phân tích ý định (Intent) và thực thi hành động tương ứng dựa trên danh sách thiết bị thực tế của người dùng.

## 2. Phạm vi tính năng
Chỉ hỗ trợ 03 kịch bản điều khiển tường minh:
1.  **Điều khiển 01 thiết bị cụ thể:** "Bật đèn trần phòng khách".
2.  **Điều khiển toàn bộ thiết bị trong 01 phòng:** "Tắt hết thiết bị trong phòng ngủ".
3.  **Điều khiển toàn bộ thiết bị của người dùng:** "Bật tất cả thiết bị".

**Hành động hỗ trợ:** `ON` (Bật), `OFF` (Tắt), `AUTO` (Chế độ tự động).
**Các yêu cầu khác:** Hẹn giờ, kiểm tra trạng thái, tăng giảm thông số... sẽ bị từ chối và yêu cầu người dùng thử lại.

---

## 3. Cấu trúc Hệ thống (Architecture)

### 3.1. Thành phần chính
*   **AiController (API):** Tiếp nhận Request, xác thực người dùng.
*   **AiService (Application):** Điều phối luồng xử lý (Lấy context từ DB -> Gọi AI Provider -> Thực thi lệnh).
*   **GeminiAiProvider (Infrastructure):** Giao tiếp với Google Gemini API bằng Prompt chuyên biệt.
*   **DeviceService/MqttService:** Thực thi lệnh điều khiển thực tế qua giao thức MQTT.

### 3.2. Luồng dữ liệu (Data Flow)
1.  **Client** gửi Text + JWT Token tới `/api/ai/control`.
2.  **Backend** giải mã Token lấy `UserId`.
3.  **Backend** truy vấn DB lấy danh sách `Rooms` và `OutputDevices` của User đó.
4.  **Backend** gửi Text + Danh sách Context cho **Gemini API**.
5.  **Gemini** trả về kết quả định dạng JSON chuẩn.
6.  **Backend** phân tích kết quả:
    *   Nếu `SUCCESS`: Gọi `DeviceService` để gửi lệnh MQTT.
    *   Nếu lỗi/không rõ ràng: Trả về thông báo lỗi cho Client.
7.  **Lưu log** vào bảng `ai_logs`.

---

## 4. Đặc tả AI Prompt (System Instruction)

**System Role:** "Bạn là trình phân tích câu lệnh SmartHome chuyên dụng."

**Quy tắc phản hồi (Strict Rules):**
1.  **Tính tường minh:** Chỉ thực hiện khi xác định rõ đối tượng và hành động.
2.  **Xử lý nhập nhằng (Ambiguity):** Nếu có nhiều thiết bị trùng tên hoặc câu lệnh không rõ phòng, trả về `status: "AMBIGUOUS"`.
3.  **Phạm vi:** Trả về `targetType` thuộc [`DEVICE`, `ROOM`, `GLOBAL`].
4.  **Ngôn ngữ:** Phản hồi bằng tiếng Việt lịch sự, ngắn gọn.

**Định dạng đầu ra (JSON):**
```json
{
  "status": "SUCCESS | AMBIGUOUS | NOT_FOUND | NOT_SUPPORTED",
  "targetType": "DEVICE | ROOM | GLOBAL | null",
  "targetId": "string-guid | null",
  "action": "ON | OFF | AUTO | null",
  "responseMessage": "Thông báo phản hồi cho người dùng"
}
```

---

## 5. Thiết kế Dữ liệu & API

### 5.1. Database Update
**Bảng `ai_logs`:**
- `id` (Guid, PK)
- `user_id` (Guid, FK)
- `raw_command` (string): Câu lệnh gốc.
- `ai_response` (text): JSON kết quả từ AI.
- `status` (string): Trạng thái xử lý.
- `created_at` (DateTime).

### 5.2. API Endpoint
**POST `/api/ai/control`**
- **Auth:** Bearer Token.
- **Request Body:** `{ "command": "string" }`.
- **Response:** Trả về kết quả phân tích và trạng thái thực thi.

---

## 6. Kế hoạch triển khai (Roadmap)

### Bước 1: Chuẩn bị (Infrastructure)
- Đăng ký API Key cho Google Gemini.
- Cài đặt thư viện `Google.Ai.GenerativeLanguage` hoặc sử dụng `HttpClient` để gọi REST API.
- Tạo Migration cho bảng `ai_logs`.

### Bước 2: Phát triển lớp Application
- Tạo `AiCommandDto` và `AiAnalysisResultDto`.
- Triển khai `AiService` với logic lấy Context từ Repository.
- Xây dựng logic mapping từ kết quả AI sang hành động thực tế (vòng lặp điều khiển thiết bị).

### Bước 3: Phát triển lớp Infrastructure
- Triển khai `GeminiAiProvider`.
- Xây dựng System Prompt hoàn chỉnh (bao gồm việc nhúng danh sách thực tế của User).

### Bước 4: Hoàn thiện API & Kiểm thử
- Tạo `AiController`.
- Viết Unit Test cho các trường hợp: Chính xác, Sai tên, Trùng tên, Câu lệnh không hỗ trợ.

---

## 7. Các kịch bản kiểm thử mẫu (Test Cases)

| Câu lệnh | Kết quả mong đợi | Hành động hệ thống |
| :--- | :--- | :--- |
| "Bật đèn phòng khách" | SUCCESS | Gửi lệnh ON tới DeviceId của đèn phòng khách. |
| "Tắt hết thiết bị phòng ngủ" | SUCCESS | Lấy list thiết bị trong phòng ngủ, gửi lệnh OFF cho từng cái. |
| "Mở tất cả đèn trong nhà" | SUCCESS | Gửi lệnh ON cho toàn bộ Output Devices của User. |
| "Bật đèn" (Khi có nhiều đèn) | AMBIGUOUS | Phản hồi: "Vui lòng chỉ rõ bạn muốn bật đèn ở phòng nào?" |
| "Hẹn giờ 10 phút sau tắt quạt" | NOT_SUPPORTED | Phản hồi: "Tính năng hẹn giờ hiện chưa được hỗ trợ." |
| "Nấu cơm đi" | NOT_FOUND | Phản hồi: "Tôi không tìm thấy thiết bị phù hợp. Vui lòng thử lại." |
