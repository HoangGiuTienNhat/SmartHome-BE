
```markdown
# TÀI LIỆU THIẾT KẾ BACKEND - SMART HOME IOT

## 1. Kiến Trúc Hệ Thống (Clean Architecture)
Hệ thống sử dụng **Clean Architecture** kết hợp **Repository Pattern** trên nền tảng .NET. Việc này giúp tách biệt logic nghiệp vụ khỏi các yếu tố công nghệ (Database, Adafruit MQTT), đảm bảo tính mở rộng và dễ bảo trì.

Dự án được chia thành 4 lớp (projects) chính:
* **Domain (Core):** Chứa các thực thể (Entities) và quy tắc nghiệp vụ cốt lõi, không phụ thuộc vào bất kỳ framework nào.
* **Application (Use Cases):** Chứa logic xử lý (Services), DTOs và định nghĩa các Interfaces (Contracts).
* **Infrastructure (Data & External Services):** Triển khai giao tiếp với cơ sở dữ liệu (Entity Framework Core) và các dịch vụ bên thứ ba (Adafruit MQTT).
* **API (Presentation):** Cung cấp các RESTful API endpoints, xử lý HTTP request/response và Dependency Injection.

## 2. Cấu Trúc Thư Mục (Folder/File Structure)
```text
SmartHome.Solution/
├── SmartHome.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Room.cs
│   │   ├── Device.cs
│   │   ├── SensorData.cs
│   │   └── ActionLog.cs
│   └── Enums/
│       ├── DeviceType.cs (OutputDevice = 1, Sensor = 2)
│       ├── DeviceState.cs (OFF = 0, ON = 1, AUTO = 2)
│       └── LogType.cs (Manual = 1, Auto = 2)
├── SmartHome.Application/
│   ├── DTOs/
│   │   ├── Auth/ (RegisterDto.cs, LoginDto.cs)
│   │   ├── Room/ (RoomDto.cs, RoomCreateDto.cs)
│   │   └── Device/ (DeviceDto.cs, DeviceControlDto.cs)
│   ├── Interfaces/
│   │   ├── Repositories/ (IUserRepository.cs, IDeviceRepository.cs)
│   │   └── Services/ (IDeviceService.cs, IAdafruitClient.cs)
│   └── Services/
│       └── DeviceService.cs
├── SmartHome.Infrastructure/
│   ├── Data/
│   │   └── ApplicationDbContext.cs
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   └── DeviceRepository.cs
│   └── ExternalServices/
│       ├── AdafruitMqttClient.cs
│       └── MqttBackgroundWorker.cs (Background Service lắng nghe sensor)
└── SmartHome.API/
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── RoomsController.cs
    │   └── DevicesController.cs
    ├── Middlewares/
    │   └── ExceptionMiddleware.cs
    ├── appsettings.json
    └── Program.cs
```

## 3. Thiết Kế Thực Thể (Entities) & Kiểu Dữ Liệu
* **User:** `Id` (Guid, PK), `Email` (string, unique), `PasswordHash` (string), `FullName` (string), `Rooms` (ICollection<Room>).
* **Room:** `Id` (Guid, PK), `UserId` (Guid, FK), `Name` (string), `Devices` (ICollection<Device>).
* **Device:** `Id` (Guid, PK), `RoomId` (Guid, FK), `Name` (string), `Type` (Enum), `State` (Enum), `InstallDate` (DateTime), `AdafruitFeedKey` (string), `ThresholdMin` (float?, Nullable), `ThresholdMax` (float?, Nullable).
* **SensorData:** `Id` (Guid, PK), `DeviceId` (Guid, FK), `Value` (float), `Time` (DateTime).
* **ActionLog:** `Id` (Guid, PK), `Timestamp` (DateTime), `LogType` (Enum), `DeviceName` (string), `Action` (string), `Detail` (string).

## 4. API Endpoints
Tất cả API (trừ Auth) yêu cầu Header `Authorization: Bearer {token}`.

**Authentication (`/api/auth`)**
* `POST /register`: Payload `{ email, password, full_name }`
* `POST /login`: Payload `{ email, password }` -> Trả về JWT Token.

**Rooms (`/api/rooms`)**
* `GET /`: Lấy danh sách phòng của user.
* `POST /`: Payload `{ room_name }` -> Tạo phòng mới.
* `PUT /{id}`: Payload `{ room_name }` -> Đổi tên phòng.
* `DELETE /{id}`: Xóa phòng (Cascade Delete các thiết bị).

**Devices (`/api/rooms/{roomId}/devices`)**
* `GET /`: Lấy danh sách thiết bị trong phòng.
* `POST /`: Payload `{ device_name, type, adafruit_feed_key, threshold_min?, threshold_max? }`
* `PUT /{id}`: Cập nhật thông tin thiết bị.
* `DELETE /{id}`: Xóa thiết bị.

**Device Control & Automation (`/api/devices`)**
* `POST /{id}/control`: Payload `{ state }` -> Gửi lệnh MQTT điều khiển và ghi log.

**Data & Logs (`/api/data`)**
* `GET /devices/{deviceId}/logs`: Lấy lịch sử ActionLog.
* `GET /devices/{deviceId}/sensors`: Lấy dữ liệu SensorData.

## 5. Luồng Xử Lý Dữ Liệu & Tự Động Hóa (IoT Integration)
1. **Điều khiển thủ công (Manual):** User gọi API `/control`. `DeviceService` sử dụng `AdafruitMqttClient` publish lệnh lên feed MQTT của Adafruit. Sau đó insert 1 bản ghi vào `ActionLog`.
2. **Thu thập & Tự động hóa (Auto):** Lớp Infrastructure chạy một `MqttBackgroundWorker` (Hosted Service) liên tục subscribe các feed cảm biến:
   * Khi có dữ liệu: Lưu giá trị vào bảng `SensorData`.
   * Kiểm tra Threshold: Nếu giá trị vượt `ThresholdMax` hoặc dưới `ThresholdMin`, tự động publish lệnh bật/tắt thiết bị Output tương ứng.
   * Ghi nhận: Lưu hành động tự động hóa vào `ActionLog` với `LogType = Auto`.
