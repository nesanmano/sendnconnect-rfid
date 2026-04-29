using CardManagement.Api.Models.DTOs.Cards;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly ICardService _cardService;

    public CardsController(ICardService cardService)
    {
        _cardService = cardService;
    }

    /// <summary>Register a new RFID card.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterCardRequest request, CancellationToken ct)
    {
        var result = await _cardService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Get a card by ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _cardService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>List cards with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? holderId = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await _cardService.ListAsync(page, pageSize, holderId, status, ct);
        return Ok(result);
    }

    /// <summary>Update card metadata.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCardRequest request, CancellationToken ct)
    {
        var result = await _cardService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Block a card.</summary>
    [HttpPut("{id}/block")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Block(string id, [FromBody] BlockCardRequest request, CancellationToken ct)
    {
        var result = await _cardService.BlockAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Unblock a card.</summary>
    [HttpPut("{id}/unblock")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unblock(string id, CancellationToken ct)
    {
        var result = await _cardService.UnblockAsync(id, ct);
        return Ok(result);
    }

    /// <summary>Wipe a card (zero balance and deactivate).</summary>
    [HttpPut("{id}/wipe")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Wipe(string id, CancellationToken ct)
    {
        var result = await _cardService.WipeAsync(id, ct);
        return Ok(result);
    }
}
