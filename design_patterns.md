# Tài liệu Chi tiết Các Mẫu Thiết Kế (Design Patterns)

Tài liệu này phân tích sâu về kiến trúc và các mẫu thiết kế (Design Patterns) được áp dụng trong hệ thống SmartHome Backend.

---

## 1. Mẫu Kiến trúc (Architectural Patterns)

### 1.1. Repository Pattern (Mẫu Kho lưu trữ)
- **Bản chất**: Cung cấp một lớp trừu tượng giữa tầng logic nghiệp vụ (Service) và tầng truy cập dữ liệu (EF Core).
- **Cách triển khai chi tiết**:
    - **Interfaces**: Định nghĩa trong `SmartHome.Domain/Interfaces/Repositories`. Ví dụ: `IDeviceRepository` chứa các phương thức như `GetByFeedKeyAsync`, `AddAsync`.
    - **Implementations**: Nằm trong `SmartHome.Infrastructure/Repositories`. Sử dụng `SmartHomeDbContext` để thực hiện các truy vấn LINQ.
    - **Lợi ích**: Giúp dễ dàng thay đổi hệ quản trị cơ sở dữ liệu (ví dụ từ PostgreSQL sang SQL Server) mà không ảnh hưởng đến code xử lý nghiệp vụ.
- **Các bảng liên quan**: Toàn bộ database.

### 1.2. Service Pattern (Mẫu Dịch vụ)
- **Bản chất**: Đóng gói logic nghiệp vụ phức tạp, điều phối dữ liệu giữa các Repository và các dịch vụ bên ngoài.
- **Cách triển khai chi tiết**:
    - Ví dụ `DeviceService`: Chịu trách nhiệm kiểm tra quyền sở hữu phòng của người dùng trước khi cho phép tạo thiết bị, tự động tạo "Slug" cho `feed_key`, và gọi `IMqttService` để đăng ký lắng nghe dữ liệu từ thiết bị mới.
    - Chuyển đổi giữa Entity và DTO (Data Transfer Object).
- **Các bảng liên quan**: Toàn bộ database.

### 1.3. Dependency Injection (DI - Tiêm phụ thuộc)
- **Bản chất**: Quản lý việc khởi tạo và vòng đời của các đối tượng, giảm sự phụ thuộc cứng (tight coupling) giữa các thành phần.
- **Cách triển khai chi tiết**:
    - Cấu hình tại `SmartHome.API/Program.cs`.
    - **Scoped**: Repositories và Services (mỗi yêu cầu HTTP tạo một instance mới).
    - **Singleton**: `AdafruitMqttService` (chỉ một instance duy nhất chạy xuyên suốt ứng dụng để duy trì một kết nối MQTT ổn định).
    - **Transient**: Thường dùng cho các helper nhỏ (không trạng thái).

---

## 2. Mẫu Cấu trúc & Hành vi (Structural & Behavioral Patterns)

### 2.1. Observer Pattern (Mẫu Người quan sát - Qua giao thức MQTT)
- **Cơ chế hoạt động thực tế trong dự án**:
    - Đây là cơ chế **Event-Driven (Đẩy dữ liệu - PUSH)**, hoàn toàn **KHÔNG PHẢI Polling (Gửi yêu cầu liên tục/Quét định kỳ)**.
    - **Luồng hoạt động**:
        1. **Đăng ký (Subscribe)**: Khi Backend khởi động (`MqttHostedService`), nó gửi một danh sách các "Feed Key" muốn theo dõi tới Adafruit IO Broker.
        2. **Chờ đợi (Idle)**: Backend không gửi thêm bất kỳ yêu cầu nào sau đó. Nó ở trạng thái chờ.
        3. **Sự kiện (Event)**: Khi thiết bị (ví dụ cảm biến nhiệt độ) gửi dữ liệu lên Adafruit IO, hoặc khi người dùng nhấn nút vật lý.
        4. **Thông báo (Push)**: Broker Adafruit IO ngay lập tức "đẩy" tin nhắn đó về Backend thông qua kết nối TCP đã duy trì sẵn.
        5. **Xử lý (Update)**: Hàm callback `_mqttClient.ApplicationMessageReceivedAsync` trong `AdafruitMqttService.cs` được kích hoạt ngay lập tức để xử lý dữ liệu.
- **Lợi ích**: Tiết kiệm băng thông, giảm tải cho server và đảm bảo tính thời gian thực (real-time) cực cao.
- **Các bảng liên quan**: `devices`, `sensor_data`, `action_logs`.

### 2.2. Table-Per-Type (TPT) Inheritance (Kế thừa theo từng bảng)
- **Bản chất**: Ánh xạ cấu trúc kế thừa trong lập trình hướng đối tượng (OOP) vào cơ sở dữ liệu quan hệ.
- **Cách triển khai chi tiết**:
    - **Domain**: `Device` (cha), `OutputDevice` (con), `Sensor` (con).
    - **EF Core Configuration**: Trong `SmartHomeDbContext.cs`, sử dụng `.ToTable("devices")`, `.ToTable("output_devices")`, và `.ToTable("sensors")`.
    - **Cơ sở dữ liệu**: Khi tạo một `Sensor`, dữ liệu chung (tên, ngày tạo) nằm ở bảng `devices`, dữ liệu riêng (ngưỡng min/max) nằm ở bảng `sensors`. Hai bảng liên kết qua `device_id`.
- **Các bảng liên quan**: `devices`, `output_devices`, `sensors`.

### 2.3. Data Transfer Object (DTO) Pattern
- **Bản chất**: Tách biệt mô hình dữ liệu lưu trữ (Entity) và mô hình dữ liệu trao đổi với người dùng (API).
- **Cách triển khai chi tiết**:
    - Thư mục `SmartHome.Application/DTOs`.
    - Tránh lộ các thông tin nhạy cảm (như mật khẩu trong `User`) hoặc các thông tin kỹ thuật không cần thiết ra ngoài API.
    - Giúp API linh hoạt hơn, có thể gộp hoặc tách dữ liệu từ nhiều bảng trước khi trả về.

### 2.4. Middleware Pattern (Mẫu lớp trung gian)
- **Bản chất**: Một chuỗi các bộ xử lý nằm trong luồng xử lý yêu cầu HTTP của ASP.NET Core.
- **Cách triển khai chi tiết**:
    - `ExceptionMiddleware`: Bao bọc toàn bộ ứng dụng trong một khối `try-catch`. Khi có bất kỳ lỗi nào xảy ra ở bất kỳ tầng nào (Service, Repository), Middleware này sẽ bắt lấy, ghi log lỗi và trả về một phản hồi JSON chuẩn hóa cho client (ví dụ: lỗi 500 kèm thông điệp thân thiện).

### 2.5. Background Hosted Service Pattern
- **Bản chất**: Chạy các tác vụ nền độc lập với luồng xử lý yêu cầu HTTP của người dùng.
- **Cách triển khai chi tiết**:
    - `MqttHostedService`: Chạy ngay khi ứng dụng web khởi động. Nhiệm vụ chính là thiết lập kết nối MQTT một lần duy nhất và duy trì nó. Nếu kết nối bị ngắt, nó có logic tự động kết nối lại (Reconnection logic).

### 2.6. Strategy Pattern (kết hợp với Processor)
- **Bản chất**: Tách biệt logic xử lý tin nhắn tùy theo loại thiết bị.
- **Cách triển khai chi tiết**:
    - `MqttMessageProcessor.ProcessMessageAsync`: Kiểm tra xem thiết bị thuộc loại nào (`is Sensor` hay `is OutputDevice`).
    - Nếu là Sensor: Áp dụng logic lưu dữ liệu và kiểm tra ngưỡng tự động hóa.
    - Nếu là Output: Áp dụng logic đồng bộ trạng thái và ghi log thủ công.
- **Các bảng liên quan**: `sensor_data`, `action_logs`.

### 2.7. Singleton Pattern
- **Bản chất**: Đảm bảo một lớp chỉ có duy nhất một thực thể trong suốt vòng đời ứng dụng.
- **Cách triển khai chi tiết**:
    - `AdafruitMqttService` được đăng ký là Singleton vì việc duy trì nhiều kết nối MQTT tới cùng một tài khoản Adafruit IO có thể gây ra xung đột và lãng phí tài nguyên socket.

### 2.8. Factory Pattern
- **Bản chất**: Cung cấp cách thức tạo đối tượng mà không cần chỉ định chính xác lớp sẽ được tạo.
- **Cách triển khai chi tiết**:
    - `SmartHomeDbContextFactory`: Giúp các công cụ dòng lệnh (CLI) của Entity Framework tạo ra `DbContext` với các tham số cấu hình đúng đắn khi thực hiện lệnh `dotnet ef migrations add`.
