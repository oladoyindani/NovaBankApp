using NovaBankApp.Models;

namespace NovaBankApp.Data;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, BankAccount> accountsByNumber = new();
    private readonly HashSet<Guid> processedIncomingTransfers = [];

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

    public void SaveChanges(params BankAccount[] accounts)
    {
    }

    public OperationResult RecordIncomingTransfer(
        Guid transferId,
        string recipientAccountNumber,
        string senderName,
        decimal amount)
    {
        lock (syncRoot)
        {
            if (!accountsByNumber.TryGetValue(recipientAccountNumber, out var recipient))
            {
                return OperationResult.Failure("Recipient account was not found at this bank.");
            }

            if (!processedIncomingTransfers.Add(transferId))
            {
                return OperationResult.Success("This transfer was already processed.");
            }

            recipient.Credit(amount, $"Received inter-bank transfer from {senderName}: +${amount:N2}");
            return OperationResult.Success($"${amount:N2} was credited to the recipient account.");
        }
    }
}
