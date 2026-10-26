namespace NovaBankApp.Models;

/// <summary>Represents one customer account and protects its mutable balance state.</summary>
public class BankAccount
{
    private readonly object syncRoot = new();
    private readonly List<Transaction> transactions = [];

    /// <summary>Creates a new account with no available funds.</summary>
    public BankAccount(string accountNumber, string customerName, string city, string phoneNumber, string passwordHash)
    {
        AccountNumber = accountNumber;
        CustomerName = customerName;
        City = city;
        PhoneNumber = phoneNumber;
        PasswordHash = passwordHash;
    }

    public string AccountNumber { get; }
    public string CustomerName { get; }
    public string City { get; }
    public string PhoneNumber { get; }
    public string PasswordHash { get; }
    public decimal Balance { get; private set; }
    public bool IsFrozen { get; private set; }

    /// <summary>Returns a safe read-only copy of this account's audit trail.</summary>
    public IReadOnlyList<Transaction> Transactions
    {
        get { lock (syncRoot) return transactions.ToList(); }
    }

    /// <summary>Credits funds, including money received while the account is frozen.</summary>
    public void Credit(decimal amount, string description)
    {
        lock (syncRoot)
        {
            Balance += amount;
            transactions.Add(new Transaction(DateTime.Now, description));
        }
    }

    /// <summary>Debits funds only when enough balance is available.</summary>
    public bool TryDebit(decimal amount, string description)
    {
        lock (syncRoot)
        {
            if (amount > Balance) return false;
            Balance -= amount;
            transactions.Add(new Transaction(DateTime.Now, description));
            return true;
        }
    }

    /// <summary>Stores a non-financial event in the account audit trail.</summary>
    public void AddAuditEntry(string description)
    {
        lock (syncRoot) transactions.Add(new Transaction(DateTime.Now, description));
    }

    /// <summary>Changes whether the customer can initiate transactions.</summary>
    public void SetFrozen(bool isFrozen)
    {
        lock (syncRoot)
        {
            IsFrozen = isFrozen;
            transactions.Add(new Transaction(DateTime.Now, isFrozen ? "Account frozen by administrator" : "Account unfrozen by administrator"));
        }
    }
}
