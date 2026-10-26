using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class AdminService
{
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Admin@123";
    private readonly IAccountRepository accountRepository;

    public AdminService(IAccountRepository accountRepository) => this.accountRepository = accountRepository;

    public bool IsAdmin(string userName, string password) => userName == AdminUserName && password == AdminPassword;

    public IReadOnlyList<AccountSummary> GetAccounts() => accountRepository.GetSummaries();

    public OperationResult Freeze(string accountNumber) => SetFrozenStatus(accountNumber, true);

    public OperationResult Unfreeze(string accountNumber) => SetFrozenStatus(accountNumber, false);

    private OperationResult SetFrozenStatus(string accountNumber, bool isFrozen)
    {
        var account = accountRepository.GetByAccountNumber(accountNumber);
        if (account is null) return OperationResult.Failure("Account number was not found.");
        account.SetFrozen(isFrozen);
        return OperationResult.Success($"Account {account.AccountNumber} is now {(isFrozen ? "frozen" : "active")}.");
    }
}
