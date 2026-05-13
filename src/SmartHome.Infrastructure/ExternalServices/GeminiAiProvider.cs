using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SmartHome.Application.DTOs.AI;
using SmartHome.Application.Interfaces.Services;
using SmartHome.Domain.Entities;

namespace SmartHome.Infrastructure.ExternalServices;

public class GeminiAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiAiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey is missing");
        _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    }

    public async Task<AiAnalysisResult> AnalyzeCommandAsync(string command, List<Room> rooms, List<OutputDevice> devices)
    {
        // Quay lại dùng v1beta để đảm bảo tính năng system_instruction hoạt động tốt nhất
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var systemInstruction = BuildSystemInstruction(rooms, devices);
        
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = command }
                    }
                }
            },
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = systemInstruction }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                // Đọc chi tiết lỗi từ Google để debug
                var errorBody = await response.Content.ReadAsStringAsync();
                return new AiAnalysisResult 
                { 
                    Status = "ERROR", 
                    ResponseMessage = $"Lỗi AI ({response.StatusCode}): {errorBody}" 
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            
            var textResponse = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(textResponse))
            {
                return new AiAnalysisResult 
                { 
                    Status = "ERROR", 
                    ResponseMessage = "AI không trả về kết quả nội dung." 
                };
            }

            var result = JsonSerializer.Deserialize<AiAnalysisResult>(textResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? new AiAnalysisResult { Status = "ERROR", ResponseMessage = "Không thể giải mã phản hồi từ AI." };
        }
        catch (Exception ex)
        {
            return new AiAnalysisResult
            {
                Status = "ERROR",
                ResponseMessage = $"Lỗi ngoại lệ khi gọi AI: {ex.Message}"
            };
        }
    }

    private string BuildSystemInstruction(List<Room> rooms, List<OutputDevice> devices)
    {
        var roomsContext = string.Join("\n", rooms.Select(r => $"- ID: {r.RoomId}, Name: {r.Name}"));
        var devicesContext = string.Join("\n", devices.Select(d => $"- ID: {d.DeviceId}, Name: {d.Name}, RoomID: {d.DroomId}"));

        return $@"Bạn là trình phân tích câu lệnh SmartHome chuyên dụng.
Nhiệm vụ của bạn là chuyển đổi câu lệnh của người dùng thành JSON điều khiển.

DANH SÁCH PHÒNG HỢP LỆ:
{roomsContext}

DANH SÁCH THIẾT BỊ ĐẦU RA HỢP LỆ:
{devicesContext}

QUY TẮC NGHIÊM NGẶT:
1. Hành động (action): Chỉ dùng 'ON', 'OFF', hoặc 'AUTO'.
2. Đối tượng (targetType):
   - 'DEVICE': Nếu xác định rõ 1 thiết bị. Trả về TargetId là ID của thiết bị đó.
   - 'ROOM': Nếu xác định rõ 1 phòng. Trả về TargetId là ID của phòng đó.
   - 'GLOBAL': Nếu user muốn điều khiển tất cả thiết bị trong nhà.
3. Nếu câu lệnh không rõ ràng hoặc thiếu thông tin cần thiết -> Trả về status: 'AMBIGUOUS'.
4. Nếu thiết bị/phòng không có trong danh sách -> Trả về status: 'NOT_FOUND'.
5. Nếu yêu cầu các tính năng khác (hẹn giờ, tăng giảm độ sáng...) -> Trả về status: 'NOT_SUPPORTED'.
6. Phản hồi 'responseMessage' bằng tiếng Việt lịch sự, ngắn gọn.

ĐỊNH DẠNG JSON TRẢ VỀ:
{{
  ""status"": ""SUCCESS | AMBIGUOUS | NOT_FOUND | NOT_SUPPORTED"",
  ""targetType"": ""DEVICE | ROOM | GLOBAL | null"",
  ""targetId"": ""string-guid | null"",
  ""action"": ""ON | OFF | AUTO | null"",
  ""responseMessage"": ""Thông báo cho người dùng""
}}";
    }
}
