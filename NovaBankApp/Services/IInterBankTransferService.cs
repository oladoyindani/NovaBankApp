using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IInterBankTransferService
{
    Task<OperationResult> TransferAsync(
        BankAccount sender,
        string bankUrl,
        string recipientAccountNumber,
        decimal amount,
        CancellationToken cancellationToken = default);
}
