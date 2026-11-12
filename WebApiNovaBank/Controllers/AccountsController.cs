using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaBankApp.Models;
using NovaBankApp.Services;
using WebApiNovaBank.Contracts;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    /// <summary>Create an account.</summary>
    [HttpPost]
    [EndpointSummary("Create an account")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<AccountResponse> Create(CreateAccountRequest request)
    {
        var result = accountService.CreateAccount(
            request.CustomerName,
            request.City,
            request.PhoneNumber,
            request.Password,
            out var account);

        if (!result.IsSuccessful || account is null)
        {
            return Conflict(new OperationResponse(false, result.Message));
        }

        return CreatedAtAction(
            nameof(GetByAccountNumber),
            new { accountNumber = account.AccountNumber },
            ToResponse(account));
    }

    /// <summary>Get the authenticated customer's account, or an account for an administrator.</summary>
    [HttpGet("{accountNumber}")]
    [Authorize]
    [EndpointSummary("Get an account")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountResponse> GetByAccountNumber(string accountNumber)
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && !string.Equals(subject, accountNumber, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var account = accountService.GetByAccountNumber(accountNumber);
        return account is null ? NotFound() : Ok(ToResponse(account));
    }

    private static AccountResponse ToResponse(BankAccount account) => new(
        account.AccountNumber,
        account.CustomerName,
        account.City,
        account.PhoneNumber,
        account.Balance,
        account.IsFrozen);
}
