using CardManagement.Api.Models.DTOs.CustomFields;
using CardManagement.Api.Models.Enums;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomFieldsController : ControllerBase
{
    private readonly ICustomFieldService _customFieldService;

    public CustomFieldsController(ICustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    /// <summary>Define a new custom field for an entity type.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomFieldResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCustomFieldRequest request, CancellationToken ct)
    {
        var result = await _customFieldService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>List custom field definitions for an entity type.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] CustomFieldEntityType entityType, CancellationToken ct)
    {
        var result = await _customFieldService.ListAsync(entityType, ct);
        return Ok(result);
    }

    /// <summary>Update a custom field definition.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomFieldResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] CreateCustomFieldRequest request, CancellationToken ct)
    {
        var result = await _customFieldService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Soft-delete a custom field definition.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _customFieldService.DeleteAsync(id, ct);
        return NoContent();
    }
}
