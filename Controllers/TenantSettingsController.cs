using CardManagement.Api.Models.DTOs.TenantSettings;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TenantSettingsController : ControllerBase
{
    private readonly ITenantSettingsService _settingsService;

    public TenantSettingsController(ITenantSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>Get current tenant settings (auto-creates defaults if none exist).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TenantSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await _settingsService.GetAsync(ct);
        return Ok(result);
    }

    /// <summary>Update tenant settings.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(TenantSettingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateTenantSettingsRequest request, CancellationToken ct)
    {
        var result = await _settingsService.UpdateAsync(request, ct);
        return Ok(result);
    }
}
