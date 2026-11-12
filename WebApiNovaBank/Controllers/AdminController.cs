using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaBankApp.Services;
using WebApiNovaBank.Contracts;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(
    IAdminService adminService,
    IBillPaymentProviderService billPaymentService) : ControllerBase
{
    /// <summary>List account summaries.</summary>
    [HttpGet("accounts")]
    [EndpointSummary("List accounts")]
    [ProducesResponseType<IReadOnlyList<AdminAccountResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminAccountResponse>> GetAccounts() =>
        Ok(adminService.GetAccounts()
            .Select(account => new AdminAccountResponse(
                account.AccountNumber,
                account.CustomerName,
                account.City,
                account.PhoneNumber,
                account.Balance,
                account.IsFrozen))
            .ToArray());

    /// <summary>Freeze an account.</summary>
    [HttpPut("accounts/{accountNumber}/freeze")]
    [EndpointSummary("Freeze an account")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> Freeze(string accountNumber) =>
        MapAccountResult(adminService.Freeze(accountNumber));

    /// <summary>Unfreeze an account.</summary>
    [HttpPut("accounts/{accountNumber}/unfreeze")]
    [EndpointSummary("Unfreeze an account")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status404NotFound)]
    public ActionResult<OperationResponse> Unfreeze(string accountNumber) =>
        MapAccountResult(adminService.Unfreeze(accountNumber));

    /// <summary>List airtime and data provider payment history.</summary>
    [HttpGet("provider-bill-payments")]
    [EndpointSummary("List provider payment history")]
    [ProducesResponseType<IReadOnlyList<AdminBillPaymentResponse>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AdminBillPaymentResponse>> GetProviderPaymentHistory() =>
        Ok(billPaymentService.GetPaymentHistory()
            .Select(payment => new AdminBillPaymentResponse(
                payment.TransactionId,
                payment.PaymentType.ToString(),
                payment.PhoneNumber,
                payment.Amount,
                payment.ProcessedAt,
                payment.IsSuccessful,
                payment.Message))
            .ToArray());

    private ActionResult<OperationResponse> MapAccountResult(NovaBankApp.Models.OperationResult result) =>
        result.IsSuccessful
            ? Ok(new OperationResponse(true, result.Message))
            : NotFound(new OperationResponse(false, result.Message));
}
