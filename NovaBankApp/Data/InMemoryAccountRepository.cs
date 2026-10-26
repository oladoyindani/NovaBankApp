using NovaBankApp.Models;

namespace NovaBankApp.Data;

/// <summary>Keeps accounts in memory for the lifetime of this console application.</summary>
public class InMemoryAccountRepository : IAccountRepository
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, BankAccount> accountsByNumber = new();

    /// <summary>Adds an account when both its account number and customer name are unused.</summary>
    public bool Add(BankAccount account)
    {
        lock (syncRoot)
        {
            if (accountsByNumber.ContainsKey(account.AccountNumber) || accountsByNumber.Values.Any(item => string.Equals(item.CustomerName, account.CustomerName, StringComparison.OrdinalIgnoreCase))) return false;
            accountsByNumber.Add(account.AccountNumber, account);
            return true;
        }
    }

    /// <summary>Finds an account from the unique number shown to the customer.</summary>
    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        lock (syncRoot) return accountsByNumber.GetValueOrDefault(accountNumber);
    }

    /// <summary>Finds an account from its case-insensitive login name.</summary>
    public BankAccount? GetByCustomerName(string customerName)
    {
        lock (syncRoot) return accountsByNumber.Values.SingleOrDefault(item => string.Equals(item.CustomerName, customerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Returns safe admin-facing views without exposing customer passwords.</summary>
    public IReadOnlyList<AccountSummary> GetSummaries()
    {
        lock (syncRoot) return accountsByNumber.Values.Select(item => new AccountSummary(item.AccountNumber, item.CustomerName, item.City, item.PhoneNumber, item.Balance, item.IsFrozen)).OrderBy(item => item.CustomerName).ToList();
    }

    /// <summary>Checks a generated number before it is assigned to a new customer.</summary>
    public bool AccountNumberExists(string accountNumber)
    {
        lock (syncRoot) return accountsByNumber.ContainsKey(accountNumber);
    }
}
