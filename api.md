
*Lưu ý: Ngoại trừ 2 API của phần Authentication, tất cả các API còn lại đều bắt buộc phải có Header xác thực: `Authorization: Bearer <your_jwt_token>`.*

---

### 1. Quản lý Người dùng (Authentication)

#### 1.1. Đăng ký tài khoản
* **Endpoint:** `POST /api/auth/register`
* **Headers:** `Content-Type: application/json`
* **Body:**
```json
{
  "email": "nhat.nguyen@example.com", // string (email format)
  "password": "StrongPassword123!",   // string
  "full_name": "Nguyen Tien Nhat"     // string
}
```

#### 1.2. Đăng nhập
* **Endpoint:** `POST /api/auth/login`
* **Headers:** `Content-Type: application/json`
* **Body:**
```json
{
  "email": "nhat.nguyen@example.com", // string
  "password": "StrongPassword123!"    // string
}
```

---

### 2. Quản lý Phòng (Rooms)

#### 2.1. Lấy danh sách phòng
* **Endpoint:** `GET /api/rooms`
* **Headers:** `Authorization: Bearer <token>`
* **Params/Body:** Không có.

#### 2.2. Tạo phòng mới
* **Endpoint:** `POST /api/rooms`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Body:**
```json
{
  "room_name": "Phòng Khách" // string (bắt buộc)
}
```

#### 2.3. Cập nhật thông tin phòng
* **Endpoint:** `PUT /api/rooms/{roomId}`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `roomId` = `d3b07384-d9a7-4b68-9128-4eb7d0046b0a` (Guid)
* **Body:**
```json
{
  "room_name": "Phòng Khách Tầng 1" // string
}
```

#### 2.4. Xóa phòng
* **Endpoint:** `DELETE /api/rooms/{roomId}`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `roomId` = `d3b07384-d9a7-4b68-9128-4eb7d0046b0a` (Guid)

---

### 3. Quản lý Thiết bị (Devices)

#### 3.1. Lấy danh sách thiết bị trong phòng
* **Endpoint:** `GET /api/rooms/{roomId}/devices`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `roomId` = `d3b07384-d9a7-4b68-9128-4eb7d0046b0a` (Guid)

#### 3.2. Thêm thiết bị mới
* **Endpoint:** `POST /api/rooms/{roomId}/devices`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `roomId` = `d3b07384-d9a7-4b68-9128-4eb7d0046b0a` (Guid)
* **Body (Trường hợp là Thiết bị đầu ra - Output):**
```json
{
  "device_name": "Đèn trần", // string
  "type": "Output",          // string (Enum: "Output", "Sensor")
  "connected_sensor_id": "guid" // Guid (tùy chọn, ID cảm biến liên kết để chạy AUTO)
}
```
* **Body (Trường hợp là Cảm biến - Sensor):**
```json
{
  "device_name": "Cảm biến nhiệt độ", // string
  "type": "Sensor",                   // string
  "threshold_min": 18.5,              // float (tùy chọn, chỉ dành cho Sensor)
  "threshold_max": 30.0               // float (tùy chọn, chỉ dành cho Sensor)
}
```

#### 3.3. Cập nhật thiết bị / Cấu hình ngưỡng cảm biến
* **Endpoint:** `PUT /api/devices/{deviceId}`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `deviceId` = `5f3c1b82-8d7b-4b1a-a2c3-1d4e5f6g7h8i` (Guid)
* **Body:**
```json
{
  "device_name": "Cảm biến nhiệt độ góc phòng", // string (tùy chọn cập nhật)
  "threshold_min": 20.0,                        // float (tùy chọn)
  "threshold_max": 28.5,                        // float (tùy chọn)
  "connected_sensor_id": "guid"                 // Guid (tùy chọn cập nhật cho Output Device)
}
```

#### 3.4. Xóa thiết bị
* **Endpoint:** `DELETE /api/devices/{deviceId}`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `deviceId` = `5f3c1b82-8d7b-4b1a-a2c3-1d4e5f6g7h8i` (Guid)

---

### 4. Điều khiển & Giám sát (Control & Data)

#### 4.1. Điều khiển thiết bị đầu ra (Gọi lệnh gửi qua Adafruit)
* **Endpoint:** `POST /api/devices/{deviceId}/control`
* **Headers:** `Authorization: Bearer <token>`, `Content-Type: application/json`
* **Route Parameter:** `deviceId` = `a1b2c3d4-e5f6-7890-1234-56789abcdef0` (Guid - ID của thiết bị Output)
* **Body:**
```json
{
  "status": "ON", // string (Enum: "ON", "OFF", "AUTO")
  "value": 50     // decimal (tùy chọn, ví dụ: độ sáng, tốc độ quạt)
}
```

#### 4.2. Lấy dữ liệu lịch sử của Cảm biến (Để vẽ biểu đồ)
* **Endpoint:** `GET /api/devices/{deviceId}/data`
* **Headers:** `Authorization: Bearer <token>`
* **Route Parameter:** `deviceId` = `5f3c1b82-8d7b-4b1a-a2c3-1d4e5f6g7h8i` (Guid - ID của Sensor)
* **Query Parameters (Tùy chọn, dùng để lọc theo thời gian):**
    * `startDate` = `2025-11-01T00:00:00Z` (DateTime ISO 8601)
    * `endDate` = `2025-11-30T23:59:59Z` (DateTime ISO 8601)
* **URL Minh họa:** `/api/devices/{deviceId}/data?startDate=2025-11-01T00:00:00Z&endDate=2025-11-30T23:59:59Z`

#### 4.3. Lấy lịch sử hoạt động (Action Logs)
* **Endpoint:** `GET /api/logs`
* **Headers:** `Authorization: Bearer <token>`
* **Query Parameters (Phân trang):**
    * `page` = `1` (int)
    * `limit` = `20` (int)
* **URL Minh họa:** `/api/logs?page=1&limit=20`

---
