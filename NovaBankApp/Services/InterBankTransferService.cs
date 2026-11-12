using NovaBankApp.Contracts;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class InterBankTransferService : IInterBankTransferService
{
    private readonly IBankingService bankingService;
    private readonly IInterBankTransferClient transferClient;

    public InterBankTransferService(IBankingService bankingService, IInterBankTransferClient transferClient)
    {
        this.bankingService = bankingService;
        this.transferClient = transferClient;
    }

    public async Task<OperationResult> TransferAsync(
        BankAccount sender,
        string bankUrl,
        string recipientAccountNumber,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var validation = bankingService.ValidateInterBankTransfer(sender, amount);
        if (!validation.IsSuccessful) return validation;

        var response = await transferClient.SendAsync(
            bankUrl,
            new InterBankTransferRequest(recipientAccountNumber, sender.CustomerName, amount),
            cancellationToken);
        if (!response.IsSuccessful) return OperationResult.Failure($"Transfer was not completed: {response.Message}");

        return bankingService.DebitForInterBankTransfer(sender, amount, recipientAccountNumber);
    }
}
