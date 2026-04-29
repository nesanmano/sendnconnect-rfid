using CardManagement.Api.Models.DTOs.CardHolders;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardHoldersController : ControllerBase
{
    private readonly ICardHolderService _cardHolderService;

    public CardHoldersController(ICardHolderService cardHolderService)
    {
        _cardHolderService = cardHolderService;
    }

    /// <summary>Create a new card holder.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CardHolderResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCardHolderRequest request, CancellationToken ct)
    {
        var result = await _cardHolderService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get card holder by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CardHolderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _cardHolderService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>List card holders with optional search.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _cardHolderService.ListAsync(page, pageSize, search, ct);
        return Ok(result);
    }

    /// <summary>Update a card holder.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CardHolderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCardHolderRequest request, CancellationToken ct)
    {
        var result = await _cardHolderService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Soft-delete a card holder.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _cardHolderService.DeleteAsync(id, ct);
        return NoContent();
    }
}
