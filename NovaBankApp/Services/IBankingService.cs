using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IBankingService
{
    OperationResult Deposit(BankAccount account, decimal amount);
    OperationResult Withdraw(BankAccount account, decimal amount);
    BankAccount? FindRecipient(string accountNumber);
    OperationResult ValidateInterBankTransfer(BankAccount sender, decimal amount);
    OperationResult TransferWithinBank(BankAccount sender, BankAccount recipient, decimal amount);
    OperationResult DebitForInterBankTransfer(BankAccount sender, decimal amount, string recipientAccountNumber);
    OperationResult ReceiveInterBankTransfer(string recipientAccountNumber, string senderName, decimal amount);
    OperationResult PayBill(BankAccount account, BillCategory category, decimal amount);
}
