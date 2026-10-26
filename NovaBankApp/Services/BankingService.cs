using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

/// <summary>Owns the business rules for customer-initiated banking operations.</summary>
public class BankingService
{
    private readonly IAccountRepository accountRepository;

    /// <summary>Builds the banking service around the shared account repository.</summary>
    public BankingService(IAccountRepository accountRepository) => this.accountRepository = accountRepository;

    /// <summary>Credits a customer deposit unless their account is frozen.</summary>
    public OperationResult Deposit(BankAccount account, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        account.Credit(amount, $"Deposited: +${amount:N2}");
        return OperationResult.Success($"Added ${amount:N2}. New balance: ${account.Balance:N2}");
    }

    /// <summary>Debits a withdrawal only when the account is active and funded.</summary>
    public OperationResult Withdraw(BankAccount account, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return account.TryDebit(amount, $"Withdrew: -${amount:N2}")
            ? OperationResult.Success($"Withdrew ${amount:N2}. Remaining balance: ${account.Balance:N2}")
            : OperationResult.Failure("Insufficient funds for this withdrawal.");
    }

    /// <summary>Finds an intra-bank recipient before the sender confirms their transfer.</summary>
    public BankAccount? FindRecipient(string accountNumber) => accountRepository.GetByAccountNumber(accountNumber);

    /// <summary>Checks an inter-bank transfer before the receiving bank is contacted.</summary>
    public OperationResult ValidateInterBankTransfer(BankAccount sender, decimal amount)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return sender.Balance >= amount ? OperationResult.Success(string.Empty) : OperationResult.Failure("Insufficient funds for this transfer.");
    }

    /// <summary>Moves money between two local accounts after all preconditions have been checked.</summary>
    public OperationResult TransferWithinBank(BankAccount sender, BankAccount recipient, decimal amount)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        if (sender.AccountNumber == recipient.AccountNumber) return OperationResult.Failure("You cannot transfer money to the same account.");
        if (!sender.TryDebit(amount, $"Intra-bank transfer to {recipient.CustomerName} ({recipient.AccountNumber}): -${amount:N2}")) return OperationResult.Failure("Insufficient funds for this transfer.");
        recipient.Credit(amount, $"Received intra-bank transfer from {sender.CustomerName}: +${amount:N2}");
        return OperationResult.Success($"Transfer successful. ${amount:N2} sent to {recipient.CustomerName}.");
    }

    /// <summary>Reserves money for an outgoing inter-bank transfer after the remote bank accepts it.</summary>
    public OperationResult DebitForInterBankTransfer(BankAccount sender, decimal amount, string recipientAccountNumber)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return sender.TryDebit(amount, $"Inter-bank transfer to {recipientAccountNumber}: -${amount:N2}")
            ? OperationResult.Success($"${amount:N2} sent to external account {recipientAccountNumber}.")
            : OperationResult.Failure("Insufficient funds for this transfer.");
    }

    /// <summary>Receives an external transfer; frozen accounts are intentionally allowed to receive money.</summary>
    public OperationResult ReceiveInterBankTransfer(string recipientAccountNumber, string senderName, decimal amount)
    {
        var recipient = accountRepository.GetByAccountNumber(recipientAccountNumber);
        if (recipient is null) return OperationResult.Failure("Recipient account was not found at this bank.");
        if (!IsPositiveAmount(amount, out var result)) return result;
        recipient.Credit(amount, $"Received inter-bank transfer from {senderName}: +${amount:N2}");
        return OperationResult.Success($"${amount:N2} was credited to {recipient.CustomerName}.");
    }

    /// <summary>Pays an existing bill category from an active customer account.</summary>
    public OperationResult PayBill(BankAccount account, BillCategory category, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return account.TryDebit(amount, $"Paid {category.Name}: -${amount:N2}")
            ? OperationResult.Success($"Paid ${amount:N2} for {category.Name}. Remaining balance: ${account.Balance:N2}")
            : OperationResult.Failure("Insufficient funds for this bill.");
    }

    /// <summary>Prevents frozen accounts from sending, withdrawing, depositing, or paying bills.</summary>
    private static bool CanInitiateTransaction(BankAccount account, out OperationResult result)
    {
        result = account.IsFrozen ? OperationResult.Failure("This account is frozen. It can only receive money.") : OperationResult.Success(string.Empty);
        return result.IsSuccessful;
    }

    /// <summary>Rejects zero and negative currency amounts in every money workflow.</summary>
    private static bool IsPositiveAmount(decimal amount, out OperationResult result)
    {
        result = amount > 0 ? OperationResult.Success(string.Empty) : OperationResult.Failure("Enter a valid positive amount.");
        return result.IsSuccessful;
    }
}
