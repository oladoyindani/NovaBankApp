using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class BankingService
{
    private readonly IAccountRepository accountRepository;

    public BankingService(IAccountRepository accountRepository) => this.accountRepository = accountRepository;

    public OperationResult Deposit(BankAccount account, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        account.Credit(amount, $"Deposited: +${amount:N2}");
        return OperationResult.Success($"Added ${amount:N2}. New balance: ${account.Balance:N2}");
    }

    public OperationResult Withdraw(BankAccount account, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return account.TryDebit(amount, $"Withdrew: -${amount:N2}")
            ? OperationResult.Success($"Withdrew ${amount:N2}. Remaining balance: ${account.Balance:N2}")
            : OperationResult.Failure("Insufficient funds for this withdrawal.");
    }

    public BankAccount? FindRecipient(string accountNumber) => accountRepository.GetByAccountNumber(accountNumber);

    public OperationResult ValidateInterBankTransfer(BankAccount sender, decimal amount)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return sender.Balance >= amount ? OperationResult.Success(string.Empty) : OperationResult.Failure("Insufficient funds for this transfer.");
    }

    public OperationResult TransferWithinBank(BankAccount sender, BankAccount recipient, decimal amount)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        if (sender.AccountNumber == recipient.AccountNumber) return OperationResult.Failure("You cannot transfer money to the same account.");
        if (!sender.TryDebit(amount, $"Intra-bank transfer to {recipient.CustomerName} ({recipient.AccountNumber}): -${amount:N2}")) return OperationResult.Failure("Insufficient funds for this transfer.");
        recipient.Credit(amount, $"Received intra-bank transfer from {sender.CustomerName}: +${amount:N2}");
        return OperationResult.Success($"Transfer successful. ${amount:N2} sent to {recipient.CustomerName}.");
    }

    public OperationResult DebitForInterBankTransfer(BankAccount sender, decimal amount, string recipientAccountNumber)
    {
        if (!CanInitiateTransaction(sender, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return sender.TryDebit(amount, $"Inter-bank transfer to {recipientAccountNumber}: -${amount:N2}")
            ? OperationResult.Success($"${amount:N2} sent to external account {recipientAccountNumber}.")
            : OperationResult.Failure("Insufficient funds for this transfer.");
    }

    public OperationResult ReceiveInterBankTransfer(string recipientAccountNumber, string senderName, decimal amount)
    {
        var recipient = accountRepository.GetByAccountNumber(recipientAccountNumber);
        if (recipient is null) return OperationResult.Failure("Recipient account was not found at this bank.");
        if (!IsPositiveAmount(amount, out var result)) return result;
        recipient.Credit(amount, $"Received inter-bank transfer from {senderName}: +${amount:N2}");
        return OperationResult.Success($"${amount:N2} was credited to {recipient.CustomerName}.");
    }

    public OperationResult PayBill(BankAccount account, BillCategory category, decimal amount)
    {
        if (!CanInitiateTransaction(account, out var result)) return result;
        if (!IsPositiveAmount(amount, out result)) return result;
        return account.TryDebit(amount, $"Paid {category.Name}: -${amount:N2}")
            ? OperationResult.Success($"Paid ${amount:N2} for {category.Name}. Remaining balance: ${account.Balance:N2}")
            : OperationResult.Failure("Insufficient funds for this bill.");
    }

    private static bool CanInitiateTransaction(BankAccount account, out OperationResult result)
    {
        result = account.IsFrozen ? OperationResult.Failure("This account is frozen. It can only receive money.") : OperationResult.Success(string.Empty);
        return result.IsSuccessful;
    }

    private static bool IsPositiveAmount(decimal amount, out OperationResult result)
    {
        result = amount > 0 ? OperationResult.Success(string.Empty) : OperationResult.Failure("Enter a valid positive amount.");
        return result.IsSuccessful;
    }
}
