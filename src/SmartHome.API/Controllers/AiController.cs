using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Application.DTOs.Requests;
using SmartHome.Application.Interfaces.Services;

namespace SmartHome.API.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
        _aiService = aiService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpPost("control")]
    public async Task<IActionResult> Control([FromBody] AiControlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Command))
        {
            return BadRequest(new { message = "Command cannot be empty." });
        }

        var result = await _aiService.ProcessCommandAsync(GetUserId(), request);
        
        return Ok(result);
    }
}
