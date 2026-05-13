using System;
using System.Threading.Tasks;
using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.DTOs.Responses;

namespace SmartHome.Application.Interfaces.Services;

public interface IAiService
{
    Task<AiControlResponse> ProcessCommandAsync(Guid userId, AiControlRequest request);
}
