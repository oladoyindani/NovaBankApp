using NovaBankApp.Models;

namespace NovaBankApp.Data;

public interface IAccountRepository
{
    bool Add(BankAccount account);
    BankAccount? GetByAccountNumber(string accountNumber);
    BankAccount? GetByCustomerName(string customerName);
    IReadOnlyList<AccountSummary> GetSummaries();
    bool AccountNumberExists(string accountNumber);
    void SaveChanges(params BankAccount[] accounts);
    OperationResult RecordIncomingTransfer(
        Guid transferId,
        string recipientAccountNumber,
        string senderName,
        decimal amount);
}
