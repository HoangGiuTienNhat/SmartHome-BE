# Phân Tích Kiến Trúc Dự Án SmartHome

Dự án được xây dựng theo kiến trúc **Clean Architecture** (Kiến trúc sạch), chia thành 4 lớp chính nhằm đảm bảo tính độc lập, dễ kiểm thử và bảo trì.

## 1. Cấu trúc các lớp (Layers)

### 1.1. SmartHome.Domain (Lớp Lõi)
- **Nhiệm vụ:** Chứa các thực thể (Entities), Enum, và định nghĩa các Interface cho Repository. Đây là lớp trung tâm, không phụ thuộc vào bất kỳ lớp nào khác.
- **Thành phần chính:**
    - `Entities/`: `User`, `Room`, `Device`, `Sensor`, `OutputDevice`, `SensorData`, `ActionLog`.
    - `Enums/`: `DeviceType`, `DeviceStatus`, `LogType`.
    - `Interfaces/Repositories/`: Định nghĩa các phương thức giao tiếp với Database (như `IDeviceRepository`, `IUserRepository`).

### 1.2. SmartHome.Application (Lớp Ứng Dụng)
- **Nhiệm vụ:** Chứa logic nghiệp vụ (Business Logic) của ứng dụng. Nó điều phối luồng dữ liệu giữa Domain và các lớp bên ngoài.
- **Thành phần chính:**
    - `Services/`: Triển khai các dịch vụ như `AuthService` (đã bổ sung logic đổi mật khẩu), `DeviceService`, `MqttMessageProcessor` (đã bổ sung logic ghi log cho Sensor).
    - `DTOs/`: (Data Transfer Objects) Các lớp chứa dữ liệu để truyền tải giữa API và Application (Requests/Responses).
    - `Interfaces/Services/`: Định nghĩa các Service để lớp API sử dụng.

### 1.3. SmartHome.Infrastructure (Lớp Hạ Tầng)
- **Nhiệm vụ:** Triển khai các chi tiết kỹ thuật như truy cập cơ sở dữ liệu (EF Core), gọi API bên ngoài, hoặc dịch vụ MQTT.
- **Thành phần chính:**
    - `Data/`: `SmartHomeDbContext` cấu hình thực thể và Database.
    - `Repositories/`: Triển khai các Interface từ lớp Domain bằng EF Core.
    - `ExternalServices/`: `AdafruitMqttService`, `AdafruitApiService`.

### 1.4. SmartHome.API (Lớp Hiển Thị)
- **Nhiệm vụ:** Điểm vào của ứng dụng, cung cấp các RESTful API và các dịch vụ chạy ngầm (Background Services).
- **Thành phần chính:**
    - `Controllers/`: `AuthController`, `RoomsController`, `DevicesController`.
    - `BackgroundServices/`: `MqttHostedService` (Duy trì kết nối MQTT 24/7).
    - `Middlewares/`: `ExceptionMiddleware` xử lý lỗi tập trung.

## 2. Các mẫu thiết kế (Design Patterns)
- **Repository Pattern:** Tách biệt logic truy cập dữ liệu khỏi logic nghiệp vụ.
- **Dependency Injection (DI):** Giảm sự phụ thuộc giữa các thành phần, dễ dàng thay thế và unit test.
- **Table-per-Type (TPT):** Sử dụng trong database để quản lý kế thừa (Device -> Sensor/OutputDevice).
- **Background Service:** Sử dụng để lắng nghe các tin nhắn MQTT từ Adafruit một cách liên tục.
