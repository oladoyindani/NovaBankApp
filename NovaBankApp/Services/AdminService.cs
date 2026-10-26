using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

/// <summary>Provides administrator authentication and account-freeze management.</summary>
public class AdminService
{
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Admin@123";
    private readonly IAccountRepository accountRepository;

    /// <summary>Builds the admin service from the shared account repository.</summary>
    public AdminService(IAccountRepository accountRepository) => this.accountRepository = accountRepository;

    /// <summary>Checks the preset administrator credentials.</summary>
    public bool IsAdmin(string userName, string password) => userName == AdminUserName && password == AdminPassword;

    /// <summary>Returns non-sensitive details for every locally stored customer account.</summary>
    public IReadOnlyList<AccountSummary> GetAccounts() => accountRepository.GetSummaries();

    /// <summary>Freezes a customer account and prevents customer-initiated activity.</summary>
    public OperationResult Freeze(string accountNumber) => SetFrozenStatus(accountNumber, true);

    /// <summary>Restores a frozen account's ability to initiate transactions.</summary>
    public OperationResult Unfreeze(string accountNumber) => SetFrozenStatus(accountNumber, false);

    /// <summary>Locates an account before changing its frozen status.</summary>
    private OperationResult SetFrozenStatus(string accountNumber, bool isFrozen)
    {
        var account = accountRepository.GetByAccountNumber(accountNumber);
        if (account is null) return OperationResult.Failure("Account number was not found.");
        account.SetFrozen(isFrozen);
        return OperationResult.Success($"Account {account.AccountNumber} is now {(isFrozen ? "frozen" : "active")}.");
    }
}
