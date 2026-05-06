# Tài liệu Sơ đồ Cơ sở Dữ liệu (Database Schema)

Tài liệu này cung cấp chi tiết về sơ đồ cơ sở dữ liệu của hệ thống SmartHome, bao gồm các bảng, cột, mối quan hệ và kiểu dữ liệu.

## 1. Tổng quan
Cơ sở dữ liệu sử dụng **Entity Framework Core** với chiến lược kế thừa **Table-Per-Type (TPT)** để quản lý thiết bị. Hệ thống được thiết kế để chạy trên **PostgreSQL**.

---

## 2. Định nghĩa các bảng

### 2.1. `users` (Người dùng)
Lưu trữ thông tin tài khoản người dùng.
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `user_id` | Guid | PK | Định danh duy nhất cho người dùng. |
| `email` | string | Unique, Not Null | Địa chỉ email (dùng để đăng nhập). |
| `password` | string | Not Null | Mật khẩu đã mã hóa. |
| `full_name` | string | Not Null | Họ tên hiển thị của người dùng. |

### 2.2. `rooms` (Phòng)
Nhóm các thiết bị vào các không gian vật lý hoặc logic.
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `room_id` | Guid | PK | Định danh duy nhất cho phòng. |
| `name` | string | Not Null | Tên phòng (ví dụ: "Phòng khách"). |
| `ruser_id` | Guid | FK (`users`) | Người dùng sở hữu phòng này. |

### 2.3. `devices` (Thiết bị - Bảng gốc)
Bảng cơ sở cho tất cả các thiết bị thông minh (Sử dụng kế thừa TPT).
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | Guid | PK | Định danh duy nhất cho thiết bị. |
| `name` | string | Not Null | Tên hiển thị của thiết bị. |
| `feed_key` | string | Unique, Not Null | Khóa Feed MQTT cho Adafruit IO. |
| `state` | string | - | Trạng thái kết nối (ví dụ: "CONNECTED"). |
| `type` | string | Enum | Loại thiết bị: `OUTPUT` hoặc `SENSOR`. |
| `install_date`| DateTime | Not Null | Ngày thêm thiết bị. |
| `update_date` | DateTime | Not Null | Thời điểm cập nhật cuối cùng. |
| `droom_id` | Guid | FK (`rooms`) | Phòng nơi thiết bị được lắp đặt. |

### 2.4. `output_devices` (Thiết bị đầu ra)
Mở rộng từ `devices` cho các thiết bị có thể điều khiển (Đèn, Quạt, v.v.).
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | Guid | PK, FK (`devices`) | Liên kết tới bản ghi thiết bị gốc. |
| `auto` | boolean | Not Null | Chế độ tự động của thiết bị. |
| `onoff_state`| string | Enum | Trạng thái hiện tại: `ON`, `OFF`, hoặc `AUTO`. |
| `current_value`| double? | - | Giá trị hiện tại (ví dụ: tốc độ quạt, độ sáng). |

### 2.5. `sensors` (Cảm biến)
Mở rộng từ `devices` cho các thiết bị thu thập dữ liệu (Nhiệt độ, Độ ẩm, v.v.).
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `device_id` | Guid | PK, FK (`devices`) | Liên kết tới bản ghi thiết bị gốc. |
| `threshold_min`| double? | - | Ngưỡng cảnh báo tối thiểu. |
| `threshold_max`| double? | - | Ngưỡng cảnh báo tối đa. |

### 2.6. `sensor_data` (Dữ liệu cảm biến)
Lịch sử dữ liệu đo đạc từ các cảm biến.
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `id` | int | PK, Identity | ID duy nhất cho bản ghi. |
| `sensor_device_id` | Guid | FK (`sensors`) | Cảm biến tạo ra dữ liệu này. |
| `time` | DateTime | Not Null | Thời điểm ghi nhận dữ liệu. |
| `value` | double | Not Null | Giá trị số được ghi lại. |

### 2.7. `action_logs` (Nhật ký hoạt động)
Lưu vết các tương tác với thiết bị.
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
| :--- | :--- | :--- | :--- |
| `logs_id` | Guid | PK | ID duy nhất cho nhật ký. |
| `timestamp` | DateTime | Not Null | Thời điểm xảy ra hoạt động. |
| `log_type` | string | Enum | Loại nhật ký: `MANUAL` (Thủ công) hoặc `AUTO` (Tự động). |
| `device_name` | string | Not Null | Tên thiết bị tại thời điểm ghi nhật ký. |
| `action` | string | Not Null | Hành động thực hiện (ví dụ: "Bật Đèn"). |
| `detail` | string | - | Mô tả chi tiết sự kiện. |
| `logdevice_id`| Guid | FK (`devices`), Null | Tham chiếu tới thiết bị (có thể null). |

---

## 3. Tóm tắt các mối quan hệ
- **Người dùng -> Phòng**: Một-Nhiều (Một người dùng có nhiều phòng).
- **Phòng -> Thiết bị**: Một-Nhiều (Một phòng có nhiều thiết bị).
- **Thiết bị -> Thiết bị đầu ra/Cảm biến**: Một-Một (Quan hệ kế thừa).
- **Cảm biến -> Dữ liệu cảm biến**: Một-Nhiều (Một cảm biến có nhiều dữ liệu đo đạc).
- **Cảm biến -> Thiết bị đầu ra**: Một-Nhiều (Một cảm biến có thể điều khiển nhiều thiết bị qua `connected_sensor_id`).
- **Thiết bị -> Nhật ký hoạt động**: Một-Nhiều (Một thiết bị có nhiều nhật ký).
