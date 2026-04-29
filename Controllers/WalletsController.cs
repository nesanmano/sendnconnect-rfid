using CardManagement.Api.Models.DTOs.Wallets;
using CardManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardManagement.Api.Controllers;

[ApiController]
[Route("api/cards/{cardId}/wallet")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>Get wallet for a card.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string cardId, CancellationToken ct)
    {
        var result = await _walletService.GetByCardIdAsync(cardId, ct);
        return Ok(result);
    }

    /// <summary>Load funds into the wallet.</summary>
    [HttpPost("load")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Load(string cardId, [FromBody] LoadWalletRequest request, CancellationToken ct)
    {
        var result = await _walletService.LoadAsync(cardId, request, ct);
        return Ok(result);
    }

    /// <summary>Spend from the wallet.</summary>
    [HttpPost("spend")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Spend(string cardId, [FromBody] SpendWalletRequest request, CancellationToken ct)
    {
        var result = await _walletService.SpendAsync(cardId, request, ct);
        return Ok(result);
    }

    /// <summary>Refund a previous spend transaction.</summary>
    [HttpPost("refund")]
    [ProducesResponseType(typeof(WalletResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund(string cardId, [FromBody] RefundWalletRequest request, CancellationToken ct)
    {
        var result = await _walletService.RefundAsync(cardId, request, ct);
        return Ok(result);
    }
}
