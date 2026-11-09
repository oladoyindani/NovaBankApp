using NovaBankApp.Contracts;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class InterBankTransferService
{
    private readonly BankingService bankingService;
    private readonly IInterBankTransferClient transferClient;

    public InterBankTransferService(BankingService bankingService, IInterBankTransferClient transferClient)
    {
        this.bankingService = bankingService;
        this.transferClient = transferClient;
    }

    public async Task<OperationResult> TransferAsync(BankAccount sender, string bankUrl, string recipientAccountNumber, decimal amount)
    {
        var validation = bankingService.ValidateInterBankTransfer(sender, amount);
        if (!validation.IsSuccessful) return validation;

        var response = await transferClient.SendAsync(bankUrl, new InterBankTransferRequest(recipientAccountNumber, sender.CustomerName, amount));
        if (!response.IsSuccessful) return OperationResult.Failure($"Transfer was not completed: {response.Message}");

        return bankingService.DebitForInterBankTransfer(sender, amount, recipientAccountNumber);
    }
}
