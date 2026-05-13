# SmartHome API Documentation

*Lưu ý: Ngoại trừ 2 API của phần Authentication, tất cả các API còn lại đều bắt buộc phải có Header xác thực: `Authorization: Bearer <your_jwt_token>`.*

**Base URL:** `https://localhost:7096` (hoặc `http://localhost:5189`)

---

### 1. Quản lý Người dùng (Authentication)

#### 1.1. Đăng ký tài khoản
* **Endpoint:** `POST /api/auth/register`
* **Headers:** `Content-Type: application/json`
* **Body:**
```json
{
  "email": "nhat.nguyen@example.com",
  "password": "StrongPassword123!",
  "fullName": "Nguyen Tien Nhat"
}
```
* **Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "nhat.nguyen@example.com",
  "fullName": "Nguyen Tien Nhat"
}
```

#### 1.2. Đăng nhập
* **Endpoint:** `POST /api/auth/login`
* **Headers:** `Content-Type: application/json`
* **Body:**
```json
{
  "email": "nhat.nguyen@example.com",
  "password": "StrongPassword123!"
}
```
* **Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "nhat.nguyen@example.com",
  "fullName": "Nguyen Tien Nhat"
}
```

---

### 2. Quản lý Phòng (Rooms)

#### 2.1. Lấy danh sách phòng
* **Endpoint:** `GET /api/rooms`
* **Headers:** `Authorization: Bearer <token>`
* **Response (200 OK):**
```json
[
  {
    "roomId": "d3b07384-d9a7-4b68-9128-4eb7d0046b0a",
    "roomName": "Phòng Khách"
  },
  {
    "roomId": "a1b2c3d4-e5f6-7890-1234-56789abcdef0",
    "roomName": "Phòng Ngủ"
  }
]
```

#### 2.2. Tạo phòng mới
* **Endpoint:** `POST /api/rooms`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Body:**
```json
{
  "roomName": "Nhà Bếp"
}
```
* **Response (201 Created):**
```json
{
  "roomId": "e2f3g4h5-i6j7-k8l9-m0n1-o2p3q4r5s6t7",
  "roomName": "Nhà Bếp"
}
```

#### 2.3. Cập nhật thông tin phòng
* **Endpoint:** `PUT /api/rooms/{roomId}`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `roomId` (Guid)
* **Body:**
```json
{
  "roomName": "Phòng Khách Tầng 1"
}
```
* **Response (200 OK):**
```json
{
  "roomId": "d3b07384-d9a7-4b68-9128-4eb7d0046b0a",
  "roomName": "Phòng Khách Tầng 1"
}
```

#### 2.4. Xóa phòng
* **Endpoint:** `DELETE /api/rooms/{roomId}`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `roomId` (Guid)
* **Response (204 No Content):** (Không có body)

---

### 3. Quản lý Thiết bị (Devices)

#### 3.1. Lấy danh sách thiết bị trong phòng
* **Endpoint:** `GET /api/rooms/{roomId}/devices`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `roomId` (Guid)
* **Response (200 OK):**
```json
[
  {
    "deviceId": "5f3c1b82-8d7b-4b1a-a2c3-1d4e5f6g7h8i",
    "deviceName": "Đèn trần",
    "feedKey": "phong-khach-den-tran",
    "type": "OUTPUT",
    "state": "CONNECTED",
    "auto": false,
    "onOffState": "OFF",
    "currentValue": 0,
    "connectedSensorId": "7e8f9g0h-1i2j-3k4l-5m6n-7o8p9q0r1s2t",
    "thresholdMin": null,
    "thresholdMax": null
  },
  {
    "deviceId": "7e8f9g0h-1i2j-3k4l-5m6n-7o8p9q0r1s2t",
    "deviceName": "Cảm biến nhiệt độ",
    "feedKey": "phong-khach-cam-bien-nhiet-do",
    "type": "SENSOR",
    "state": "CONNECTED",
    "auto": null,
    "onOffState": null,
    "currentValue": null,
    "connectedSensorId": null,
    "thresholdMin": 20.0,
    "thresholdMax": 35.0
  }
]
```

#### 3.2. Thêm thiết bị mới
* **Endpoint:** `POST /api/rooms/{roomId}/devices`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `roomId` (Guid)
* **Body (Thiết bị đầu ra - Output):**
```json
{
  "deviceName": "Quạt thông gió",
  "type": "Output",
  "connectedSensorId": "7e8f9g0h-1i2j-3k4l-5m6n-7o8p9q0r1s2t"
}
```
* **Body (Cảm biến - Sensor):**
```json
{
  "deviceName": "Cảm biến độ ẩm",
  "type": "Sensor",
  "thresholdMin": 40.0,
  "thresholdMax": 80.0
}
```
* **Response (200 OK):** (Trả về thông tin thiết bị vừa tạo tương tự 3.1)

#### 3.3. Cập nhật thiết bị
* **Endpoint:** `PUT /api/devices/{deviceId}`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `deviceId` (Guid)
* **Body:**
```json
{
  "deviceName": "Cảm biến nhiệt độ phòng ngủ",
  "thresholdMin": 22.5,
  "thresholdMax": 28.0,
  "connectedSensorId": "guid-moi"
}
```
* **Response (200 OK):** (Trả về thông tin thiết bị sau khi cập nhật)

#### 3.4. Xóa thiết bị
* **Endpoint:** `DELETE /api/devices/{deviceId}`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `deviceId` (Guid)
* **Response (204 No Content):** (Không có body)

---

### 4. Điều khiển & Giám sát (Control & Data)

#### 4.1. Điều khiển thiết bị đầu ra
* **Endpoint:** `POST /api/devices/{deviceId}/control`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `deviceId` (Guid)
* **Body:**
```json
{
  "status": "ON", // "ON", "OFF", "AUTO"
  "value": 50     // (Tùy chọn) Độ sáng, tốc độ... (0-100)
}
```
* **Response (200 OK):**
```json
{
  "message": "Control command sent successfully."
}
```

#### 4.2. Lấy dữ liệu lịch sử cảm biến
* **Endpoint:** `GET /api/devices/{deviceId}/data`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `deviceId` (Guid)
* **Query Parameters (Tùy chọn):**
    * `startDate`: `2026-05-01T00:00:00Z`
    * `endDate`: `2026-05-10T23:59:59Z`
* **Response (200 OK):**
```json
[
  {
    "id": "b1c2d3e4-f5g6-7h8i-9j0k-1l2m3n4o5p6q",
    "sensorDeviceId": "7e8f9g0h-1i2j-3k4l-5m6n-7o8p9q0r1s2t",
    "time": "2026-05-10T08:00:00Z",
    "value": 25.5
  },
  {
    "id": "c2d3e4f5-g6h7-8i9j-0k1l-2m3n4o5p6q7r",
    "sensorDeviceId": "7e8f9g0h-1i2j-3k4l-5m6n-7o8p9q0r1s2t",
    "time": "2026-05-10T08:05:00Z",
    "value": 26.0
  }
]
```

#### 4.3. Lấy lịch sử hoạt động chung (Action Logs)
* **Endpoint:** `GET /api/logs`
* **Headers:** `Authorization: Bearer <token>`
* **Query Parameters (Tùy chọn):**
    * `page`: `1` (mặc định)
    * `limit`: `20` (mặc định)
* **Response (200 OK):**
```json
[
  {
    "logsId": "a1b2c3d4-e5f6-7890-1234-56789abcdef0",
    "timestamp": "2026-05-10T09:15:00Z",
    "logType": 0, // 0: MANUAL, 1: AUTO
    "deviceName": "Đèn trần",
    "action": "Turn ON",
    "detail": "User turned ON device 'Đèn trần' via Interface.",
    "logdeviceId": "5f3c1b82-8d7b-4b1a-a2c3-1d4e5f6g7h8i"
  }
]
```

#### 4.4. Lấy lịch sử hoạt động của một thiết bị cụ thể
* * *

### 5. Điều khiển bằng trí tuệ nhân tạo (AI Control)

#### 5.1. Điều khiển thiết bị qua câu lệnh tự nhiên
* **Endpoint:** `POST /api/ai/control`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Body:**
```json
{
  "command": "Bật quạt trong phòng khách 1"
}
```
* **Phản hồi thành công (200 OK):**
```json
{
  "status": "SUCCESS",
  "responseMessage": "Đã bật quạt trong phòng khách 1 cho bạn rồi nhé!"
}
```
* **Phản hồi khi câu lệnh không rõ ràng (200 OK):**
```json
{
  "status": "AMBIGUOUS",
  "responseMessage": "Tôi tìm thấy nhiều thiết bị 'quạt', vui lòng chỉ rõ ở phòng nào?"
}
```
* **Phản hồi khi không tìm thấy (200 OK):**
```json
{
  "status": "NOT_FOUND",
  "responseMessage": "Không tìm thấy thiết bị hoặc phòng phù hợp trong danh sách của bạn."
}
```
* **Phản hồi lỗi hệ thống/AI (200 OK):**
```json
{
  "status": "ERROR",
  "responseMessage": "Lỗi kết nối AI: [Chi tiết lỗi]"
}
```

---
