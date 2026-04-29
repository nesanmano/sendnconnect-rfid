using CardManagement.Api.Models.DTOs.Merchants;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MerchantsController : ControllerBase
{
    private readonly IMerchantService _merchantService;

    public MerchantsController(IMerchantService merchantService)
    {
        _merchantService = merchantService;
    }

    /// <summary>Create a new merchant/POS terminal.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateMerchantRequest request, CancellationToken ct)
    {
        var result = await _merchantService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get merchant by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _merchantService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>List merchants with optional search.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _merchantService.ListAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    /// <summary>Update a merchant.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MerchantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateMerchantRequest request, CancellationToken ct)
    {
        var result = await _merchantService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Soft-delete a merchant.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _merchantService.DeleteAsync(id, ct);
        return NoContent();
    }
}
