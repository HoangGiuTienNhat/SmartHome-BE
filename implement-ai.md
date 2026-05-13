# NHẬT KÝ TRIỂN KHAI MODULE AI (IMPLEMENTATION LOG)

Tài liệu này ghi lại toàn bộ quá trình thực hiện, các thay đổi mã nguồn và cấu hình liên quan đến tính năng điều khiển bằng AI.

## Bước 1: Hạ tầng & Cơ sở dữ liệu (Hoàn thành)
**Ngày thực hiện:** 13/05/2026

### 1.1. Cấu trúc Database
- **Tạo Entity `AiLog`**: Lưu trữ lịch sử câu lệnh của người dùng, phản hồi từ AI và trạng thái thực thi.
  - File: `src/SmartHome.Domain/Entities/AiLog.cs`
- **Cấu hình DbContext**: 
  - Đăng ký `DbSet<AiLog>`.
  - Cấu hình Table-per-Type (nếu cần) và Fluent API trong `OnModelCreating`.
  - File: `src/SmartHome.Infrastructure/Data/SmartHomeDbContext.cs`
- **Migration**: 
  - Tạo migration: `AddAiLogTable`.
  - Cập nhật Database thành công (PostgreSQL).

### 1.2. Lớp Repository
- **IAiLogRepository**: Interface định nghĩa các thao tác lưu log.
- **AiLogRepository**: Triển khai lưu dữ liệu vào PostgreSQL.
- **Cập nhật IDeviceRepository**: Thêm `GetOutputDevicesByUserIdAsync` để lấy danh sách thiết bị đầu ra của một người dùng cụ thể làm ngữ cảnh (Context) cho AI.

### 1.3. Lớp Application (Interfaces & DTOs)
- **DTOs**: 
  - `AiControlRequest`, `AiControlResponse` (Giao tiếp với API).
  - `AiAnalysisResult` (Kết quả trả về từ AI Provider).
- **Interfaces**:
  - `IAiService`: Xử lý logic nghiệp vụ chính.
  - `IAiProvider`: Interface giao tiếp với AI bên ngoài (Gemini).

### 1.4. Đăng ký Dịch vụ (Dependency Injection)
- Đăng ký `AiLogRepository`, `AiService`, `GeminiAiProvider` trong `Program.cs`.
- Cấu hình `HttpClient` cho `IAiProvider`.

---

## Bước 2: Triển khai Lớp Application (Hoàn thành)
**Ngày thực hiện:** 13/05/2026

### 2.1. Logic xử lý trong AiService
- **Chuẩn bị Context**: Truy vấn danh sách Phòng và Thiết bị đầu ra của User hiện tại.
- **Phân tích câu lệnh**: Gửi Context + Câu lệnh cho `IAiProvider` để lấy kết quả phân tích cấu trúc JSON.
- **Thực thi hành động**:
  - `DEVICE`: Điều khiển 1 thiết bị cụ thể.
  - `ROOM`: Lặp qua toàn bộ thiết bị trong phòng để gửi lệnh.
  - `GLOBAL`: Điều khiển toàn bộ thiết bị đầu ra của User.
- **Lưu Nhật ký**: Ghi lại toàn bộ quá trình vào bảng `AiLog`.
- File: `src/SmartHome.Application/Services/AiService.cs`

---

## Bước 3: Triển khai Lớp Infrastructure (Hoàn thành)
**Ngày thực hiện:** 13/05/2026

### 3.1. Thiết lập Prompt System
- Xây dựng nội dung chỉ dẫn (Instruction) nghiêm ngặt, bao gồm việc nhúng danh sách Phòng và Thiết bị thực tế của User vào Prompt.
- Ép kiểu phản hồi của AI luôn là JSON theo cấu trúc đã định nghĩa.

### 3.2. Gọi API Gemini
- Triển khai `GeminiAiProvider` sử dụng `HttpClient`.
- Tự động hóa việc cấu hình `response_mime_type: "application/json"` để đảm bảo AI trả về JSON sạch, không lẫn markdown.
- Xử lý lỗi kết nối và parse kết quả an toàn.
- File: `src/SmartHome.Infrastructure/ExternalServices/GeminiAiProvider.cs`

---

## Bước 4: Hoàn thiện API & Kiểm thử (Hoàn thành)
**Ngày thực hiện:** 13/05/2026

### 4.1. AiController
- Tạo endpoint `POST /api/ai/control`.
- Lấy `UserId` từ JWT Token Claims.
- File: `src/SmartHome.API/Controllers/AiController.cs`

### 4.2. Kiểm thử thực tế
- Đã build thành công toàn bộ Solution.
- Endpoint đã sẵn sàng tiếp nhận câu lệnh ngôn ngữ tự nhiên.
- **Sửa lỗi 404 khi gọi Gemini**: Chuyển đổi API version từ `v1beta` sang `v1` để đảm bảo tính ổn định và khả năng tương thích với Model `gemini-1.5-flash`.

---

## 5. Đặc tả API mới (AI Control)

**Endpoint**: `POST /api/ai/control`  
**Authentication**: JWT Token (Bearer)

**Request Body**:
```json
{
  "command": "Bật quạt trong phòng khách 1"
}
```

**Response Body (SUCCESS)**:
```json
{
  "status": "SUCCESS",
  "responseMessage": "Đã bật quạt trong phòng khách 1 cho bạn rồi nhé!"
}
```

**Response Body (ERROR/NOT_FOUND)**:
```json
{
  "status": "NOT_FOUND",
  "responseMessage": "Không tìm thấy thiết bị hoặc phòng phù hợp..."
}
```

---

## Tổng kết các thành phần đã triển khai:
1. **Domain**: `AiLog` entity, `IAiLogRepository`, `IAiProvider`, `IAiService`.
2. **Infrastructure**: `SmartHomeDbContext` (AiLog table), `AiLogRepository`, `GeminiAiProvider` (Gemini API integration).
3. **Application**: `AiService` (Orchestration logic), DTOs cho AI.
4. **API**: `AiController` (Endpoint thực thi).
