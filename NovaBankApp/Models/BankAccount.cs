namespace NovaBankApp.Models;

public class BankAccount
{
    private readonly object syncRoot = new();
    private readonly List<Transaction> transactions = [];

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

    public IReadOnlyList<Transaction> Transactions
    {
        get { lock (syncRoot) return transactions.ToList(); }
    }

    public void Credit(decimal amount, string description)
    {
        lock (syncRoot)
        {
            Balance += amount;
            transactions.Add(new Transaction(DateTime.Now, description));
        }
    }

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

    public void AddAuditEntry(string description)
    {
        lock (syncRoot) transactions.Add(new Transaction(DateTime.Now, description));
    }

    public void SetFrozen(bool isFrozen)
    {
        lock (syncRoot)
        {
            IsFrozen = isFrozen;
            transactions.Add(new Transaction(DateTime.Now, isFrozen ? "Account frozen by administrator" : "Account unfrozen by administrator"));
        }
    }
}
