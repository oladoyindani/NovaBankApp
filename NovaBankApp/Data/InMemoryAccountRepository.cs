using NovaBankApp.Models;

namespace NovaBankApp.Data;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, BankAccount> accountsByNumber = new();

    public bool Add(BankAccount account)
    {
        lock (syncRoot)
        {
            if (accountsByNumber.ContainsKey(account.AccountNumber) || accountsByNumber.Values.Any(item => string.Equals(item.CustomerName, account.CustomerName, StringComparison.OrdinalIgnoreCase))) return false;
            accountsByNumber.Add(account.AccountNumber, account);
            return true;
        }
    }

    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        lock (syncRoot) return accountsByNumber.GetValueOrDefault(accountNumber);
    }

    public BankAccount? GetByCustomerName(string customerName)
    {
        lock (syncRoot) return accountsByNumber.Values.SingleOrDefault(item => string.Equals(item.CustomerName, customerName, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<AccountSummary> GetSummaries()
    {
        lock (syncRoot) return accountsByNumber.Values.Select(item => new AccountSummary(item.AccountNumber, item.CustomerName, item.City, item.PhoneNumber, item.Balance, item.IsFrozen)).OrderBy(item => item.CustomerName).ToList();
    }

    public bool AccountNumberExists(string accountNumber)
    {
        lock (syncRoot) return accountsByNumber.ContainsKey(accountNumber);
    }
}
