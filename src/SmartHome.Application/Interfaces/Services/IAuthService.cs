using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;

namespace SmartHome.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}
