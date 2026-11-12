using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaBankApp.Contracts;
using NovaBankApp.Models;
using NovaBankApp.Services;
using WebApiNovaBank.Contracts;
using ProviderRequest = WebApiNovaBank.Contracts.ProviderBillPaymentRequest;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Authorize]
[Route("api/provider-bill-payments")]
public sealed class ProviderBillPaymentsController(IBillPaymentProviderService paymentService) : ControllerBase
{
    /// <summary>Submit an airtime payment.</summary>
    [HttpPost("airtime")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Submit an airtime payment")]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<ProviderBillPaymentResponse> PayAirtime(ProviderRequest request) =>
        Process(BillPaymentType.Airtime, request);

    /// <summary>Submit a data payment.</summary>
    [HttpPost("data")]
    [Authorize(Roles = "Customer")]
    [EndpointSummary("Submit a data payment")]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<ProviderBillPaymentResponse> PayData(ProviderRequest request) =>
        Process(BillPaymentType.Data, request);

    /// <summary>Get an airtime or data payment status.</summary>
    [HttpGet("{transactionId}")]
    [EndpointSummary("Get a provider payment status")]
    [ProducesResponseType<ProviderBillPaymentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProviderBillPaymentResponse> GetStatus(string transactionId)
    {
        var response = User.IsInRole("Admin")
            ? paymentService.GetStatus(transactionId)
            : User.FindFirstValue(ClaimTypes.NameIdentifier) is { } accountNumber
                ? paymentService.GetStatusForAccount(transactionId, accountNumber)
                : null;
        return response is null ? NotFound() : Ok(ToResponse(response));
    }

    private ActionResult<ProviderBillPaymentResponse> Process(BillPaymentType paymentType, ProviderRequest request)
    {
        var response = paymentService.Process(
            paymentType,
            new BillPaymentRequest(request.TransactionId, request.PhoneNumber, request.Amount),
            User.FindFirstValue(ClaimTypes.NameIdentifier));
        var mapped = ToResponse(response);

        if (response.IsDuplicate)
        {
            return Conflict(mapped);
        }

        if (!response.IsSuccessful)
        {
            return BadRequest(mapped);
        }

        return CreatedAtAction(
            nameof(GetStatus),
            new { transactionId = response.TransactionId },
            mapped);
    }

    private static ProviderBillPaymentResponse ToResponse(BillPaymentResponse response) => new(
        response.TransactionId,
        response.PaymentType,
        response.IsSuccessful,
        response.IsDuplicate,
        response.Message,
        response.ProcessedAt);
}
