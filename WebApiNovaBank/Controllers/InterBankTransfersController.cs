using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NovaBankApp.Contracts;
using NovaBankApp.Services;
using WebApiNovaBank.Services;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Route("api/transfers")]
public sealed class InterBankTransfersController(
    IBankingService bankingService,
    IOptions<InterBankOptions> interBankOptions) : ControllerBase
{
    /// <summary>Receive an authenticated, idempotent transfer from a partner bank.</summary>
    [HttpPost]
    [EndpointSummary("Receive an inter-bank transfer")]
    [ProducesResponseType<InterBankTransferResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<InterBankTransferResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<InterBankTransferResponse> Receive(
        InterBankTransferRequest request,
        [FromHeader(Name = "X-NovaBank-Partner-Key")] string? partnerKey)
    {
        if (!IsValidPartnerKey(partnerKey, interBankOptions.Value.PartnerKey))
        {
            return Unauthorized();
        }

        var result = bankingService.ReceiveInterBankTransfer(
            request.TransferId,
            request.RecipientAccountNumber,
            request.SenderName,
            request.Amount);

        return result.IsSuccessful
            ? Ok(new InterBankTransferResponse(true, result.Message))
            : BadRequest(new InterBankTransferResponse(false, result.Message));
    }

    private static bool IsValidPartnerKey(string? supplied, string expected)
    {
        if (string.IsNullOrEmpty(supplied))
        {
            return false;
        }

        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}
