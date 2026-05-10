# Tổng Hợp API và Cấu Trúc Database

## 1. Danh sách API (Endpoints)

### 1.1. Xác thực (Auth)
- **POST `/api/auth/register`**: Đăng ký tài khoản.
    - *Body*: `{ "email": "...", "password": "...", "fullName": "..." }`
- **POST `/api/auth/login`**: Đăng nhập lấy JWT Token.
    - *Body*: `{ "email": "...", "password": "..." }`
- **POST `/api/auth/change-password`**: Đổi mật khẩu người dùng (Yêu cầu Token).
    - *Body*: `{ "oldPassword": "...", "newPassword": "...", "confirmPassword": "..." }`

### 1.2. Quản lý phòng (Rooms)
- **GET `/api/rooms`**: Lấy danh sách phòng của người dùng.
- **POST `/api/rooms`**: Tạo phòng mới.
    - *Body*: `{ "roomName": "..." }`
- **PUT `/api/rooms/{id}`**: Cập nhật tên phòng.
- **DELETE `/api/rooms/{id}`**: Xóa phòng.

### 1.3. Quản lý thiết bị (Devices)
- **GET `/api/rooms/{roomId}/devices`**: Lấy tất cả thiết bị trong một phòng.
- **POST `/api/rooms/{roomId}/devices`**: Thêm thiết bị mới (Sensor hoặc Output).
    - *Body (Output)*: `{ "deviceName": "Đèn", "type": "Output", "connectedSensorId": "..." }`
    - *Body (Sensor)*: `{ "deviceName": "Nhiệt độ", "type": "Sensor", "thresholdMin": 20, "thresholdMax": 35 }`
- **PUT `/api/devices/{id}`**: Cập nhật thông tin thiết bị/ngưỡng.
- **DELETE `/api/devices/{id}`**: Xóa thiết bị.
- **POST `/api/devices/{id}/control`**: Điều khiển thiết bị đầu ra.
    - *Body*: `{ "status": "ON/OFF/AUTO", "value": 50 }`
- **GET `/api/devices/{id}/data`**: Lấy lịch sử dữ liệu của cảm biến.
- **GET `/api/devices/{id}/logs`**: Lấy nhật ký hoạt động của một thiết bị.
- **GET `/api/logs`**: Lấy toàn bộ nhật ký hoạt động của người dùng (phân trang).

---

## 2. Cấu Trúc Database (Schema)

Hệ thống sử dụng SQL Server với các bảng chính sau:

### 2.1. Bảng `users` (Thông tin người dùng)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `user_id` | `uniqueidentifier` | PK | Mã định danh duy nhất (GUID) |
| `email` | `nvarchar(450)` | Unique, Not Null | Email đăng nhập |
| `password` | `nvarchar(max)` | Not Null | Mật khẩu (đã mã hóa) |
| `full_name` | `nvarchar(max)` | Not Null | Họ và tên |

### 2.2. Bảng `rooms` (Quản lý phòng)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `room_id` | `uniqueidentifier` | PK | Mã phòng |
| `name` | `nvarchar(max)` | Not Null | Tên phòng |
| `ruser_id` | `uniqueidentifier` | FK -> `users` | Thuộc về người dùng nào |

### 2.3. Bảng `devices` (Bảng cơ sở của thiết bị - TPT)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | `uniqueidentifier` | PK | Mã thiết bị |
| `name` | `nvarchar(max)` | Not Null | Tên thiết bị |
| `feed_key` | `nvarchar(450)` | Unique, Not Null | Key của feed trên Adafruit |
| `state` | `nvarchar(max)` | | Trạng thái hiển thị chung |
| `type` | `nvarchar(max)` | Not Null | `SENSOR` hoặc `OUTPUT` |
| `install_date` | `datetime2` | Not Null | Ngày lắp đặt |
| `update_date` | `datetime2` | Not Null | Ngày cập nhật cuối |
| `droom_id` | `uniqueidentifier` | FK -> `rooms` | Thuộc về phòng nào |

### 2.4. Bảng `output_devices` (Kế thừa từ `devices`)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | `uniqueidentifier` | PK, FK -> `devices` | Mã thiết bị |
| `auto` | `bit` | Not Null | Chế độ tự động (True/False) |
| `onoff_state` | `nvarchar(max)` | | Trạng thái ON/OFF/AUTO |
| `current_value` | `decimal(18,2)` | | Giá trị hiện tại (ví dụ: độ sáng) |
| `connected_sensor_id` | `uniqueidentifier` | FK -> `sensors` | Cảm biến điều khiển thiết bị này |

### 2.5. Bảng `sensors` (Kế thừa từ `devices`)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | `uniqueidentifier` | PK, FK -> `devices` | Mã thiết bị |
| `threshold_min` | `decimal(18,2)` | Nullable | Ngưỡng dưới |
| `threshold_max` | `decimal(18,2)` | Nullable | Ngưỡng trên |

### 2.6. Bảng `sensor_data` (Dữ liệu lịch sử cảm biến)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `id` | `uniqueidentifier` | PK | |
| `sensor_device_id` | `uniqueidentifier` | FK -> `sensors` | Dữ liệu của cảm biến nào |
| `time` | `datetime2` | Not Null | Thời điểm ghi nhận |
| `value` | `decimal(18,2)` | Not Null | Giá trị đo được |

### 2.7. Bảng `action_logs` (Nhật ký hoạt động)
| Trường | Kiểu dữ liệu | Tính chất | Mô tả |
| :--- | :--- | :--- | :--- |
| `logs_id` | `uniqueidentifier` | PK | |
| `timestamp` | `datetime2` | Not Null | Thời gian log |
| `log_type` | `nvarchar(max)` | Not Null | `MANUAL` hoặc `AUTO` |
| `device_name` | `nvarchar(max)` | Not Null | Tên thiết bị tại thời điểm đó |
| `action` | `nvarchar(max)` | Not Null | Hành động (Bật/Tắt/...) |
| `detail` | `nvarchar(max)` | | Chi tiết sự kiện |
| `logdevice_id` | `uniqueidentifier` | FK -> `devices` | Liên kết với thiết bị nào |
