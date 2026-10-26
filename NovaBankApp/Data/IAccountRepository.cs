using NovaBankApp.Models;

namespace NovaBankApp.Data;

/// <summary>Abstracts account storage from the rest of the application.</summary>
public interface IAccountRepository
{
    bool Add(BankAccount account);
    BankAccount? GetByAccountNumber(string accountNumber);
    BankAccount? GetByCustomerName(string customerName);
    IReadOnlyList<AccountSummary> GetSummaries();
    bool AccountNumberExists(string accountNumber);
}
