using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaBankApp.Models;
using NovaBankApp.Services;
using WebApiNovaBank.Contracts;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/accounts/me")]
public sealed class BankingController(
    IAccountService accountService,
    IBankingService bankingService,
    ICurrencyWalletService currencyWalletService) : ControllerBase
{
    private static readonly IReadOnlyList<BillCategory> BillCategories =
    [
        new("1", "Airtime Recharge"),
        new("2", "Data Bundle"),
        new("3", "Electricity Bill"),
        new("4", "Cable TV Subscription"),
        new("5", "Broadband ISP")
    ];

    /// <summary>Get the authenticated customer's account.</summary>
    [HttpGet]
    [EndpointSummary("Get my account")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountResponse> GetMyAccount()
    {
        var account = GetCurrentAccount();
        return account is null ? NotFound() : Ok(ToAccountResponse(account));
    }

    /// <summary>List the bill categories supported by this application.</summary>
    [HttpGet("bill-categories")]
    [EndpointSummary("List supported bill categories")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> GetBillCategories() =>
        Ok(BillCategories.Select(category => category.Name).ToArray());

    /// <summary>Deposit money into the authenticated customer's account.</summary>
    [HttpPost("deposits")]
    [EndpointSummary("Deposit money")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<OperationResponse> Deposit(AmountRequest request)
    {
        var account = GetCurrentAccount();
        if (account is null)
        {
            return NotFound();
        }

        var result = bankingService.Deposit(account, request.Amount);
        return result.IsSuccessful
            ? Ok(new OperationResponse(true, result.Message))
            : Conflict(new OperationResponse(false, result.Message));
    }

    /// <summary>Withdraw money from the authenticated customer's account.</summary>
    [HttpPost("withdrawals")]
    [EndpointSummary("Withdraw money")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<OperationResponse> Withdraw(AmountRequest request)
    {
        var account = GetCurrentAccount();
        if (account is null)
        {
            return NotFound();
        }

        var result = bankingService.Withdraw(account, request.Amount);
        return result.IsSuccessful
            ? Ok(new OperationResponse(true, result.Message))
            : Conflict(new OperationResponse(false, result.Message));
    }

    /// <summary>Transfer money to another NovaBank account.</summary>
    [HttpPost("transfers")]
    [EndpointSummary("Transfer money to another NovaBank account")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<OperationResponse> Transfer(InternalTransferRequest request)
    {
        var sender = GetCurrentAccount();
        if (sender is null)
        {
            return NotFound();
        }

        var recipient = bankingService.FindRecipient(request.RecipientAccountNumber);
        if (recipient is null)
        {
            return NotFound(new OperationResponse(false, "Recipient account was not found."));
        }

        var result = bankingService.TransferWithinBank(sender, recipient, request.Amount);
        return result.IsSuccessful
            ? Ok(new OperationResponse(true, result.Message))
            : Conflict(new OperationResponse(false, result.Message));
    }

    /// <summary>Pay a supported bill from the authenticated customer's account.</summary>
    [HttpPost("bill-payments")]
    [EndpointSummary("Pay a bill")]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<OperationResponse>(StatusCodes.Status409Conflict)]
    public ActionResult<OperationResponse> PayBill(PayBillRequest request)
    {
        var account = GetCurrentAccount();
        if (account is null)
        {
            return NotFound();
        }

        var category = BillCategories.SingleOrDefault(item =>
            string.Equals(item.Name, request.Category, StringComparison.OrdinalIgnoreCase));
        if (category is null)
        {
            return BadRequest(new OperationResponse(false, "Unsupported bill category."));
        }

        var result = bankingService.PayBill(account, category, request.Amount);
        return result.IsSuccessful
            ? Ok(new OperationResponse(true, result.Message))
            : Conflict(new OperationResponse(false, result.Message));
    }

    /// <summary>Get the authenticated customer's transaction history.</summary>
    [HttpGet("transactions")]
    [EndpointSummary("Get my transaction history")]
    [ProducesResponseType<IReadOnlyList<TransactionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<TransactionResponse>> GetTransactions()
    {
        var account = GetCurrentAccount();
        return account is null
            ? NotFound()
            : Ok(account.Transactions
                .OrderByDescending(transaction => transaction.OccurredAt)
                .Select(transaction => new TransactionResponse(
                    transaction.OccurredAt,
                    transaction.Description))
                .ToArray());
    }

    /// <summary>Get the authenticated customer's balance converted to supported currencies.</summary>
    [HttpGet("portfolio")]
    [EndpointSummary("Get my currency portfolio")]
    [ProducesResponseType<IReadOnlyList<CurrencyBalanceResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<IReadOnlyList<CurrencyBalanceResponse>> GetPortfolio()
    {
        var account = GetCurrentAccount();
        return account is null
            ? NotFound()
            : Ok(currencyWalletService.GetBalances(account)
                .Select(balance => new CurrencyBalanceResponse(
                    balance.Name,
                    balance.Code,
                    balance.Symbol,
                    balance.ExchangeRate,
                    balance.Amount))
                .ToArray());
    }

    private BankAccount? GetCurrentAccount()
    {
        var accountNumber = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return accountNumber is null ? null : accountService.GetByAccountNumber(accountNumber);
    }

    private static AccountResponse ToAccountResponse(BankAccount account) => new(
        account.AccountNumber,
        account.CustomerName,
        account.City,
        account.PhoneNumber,
        account.Balance,
        account.IsFrozen);
}
