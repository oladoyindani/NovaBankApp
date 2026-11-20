using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class AdminService : IAdminService
{
    private readonly IAccountRepository accountRepository;

    public AdminService(IAccountRepository accountRepository) => this.accountRepository = accountRepository;

    public IReadOnlyList<AccountSummary> GetAccounts() => accountRepository.GetSummaries();

    public OperationResult Freeze(string accountNumber) => SetFrozenStatus(accountNumber, true);

    public OperationResult Unfreeze(string accountNumber) => SetFrozenStatus(accountNumber, false);

    private OperationResult SetFrozenStatus(string accountNumber, bool isFrozen)
    {
        var account = accountRepository.GetByAccountNumber(accountNumber);
        if (account is null) return OperationResult.Failure("Account number was not found.");
        account.SetFrozen(isFrozen);
        accountRepository.SaveChanges(account);
        return OperationResult.Success($"Account {account.AccountNumber} is now {(isFrozen ? "frozen" : "active")}.");
    }
}
