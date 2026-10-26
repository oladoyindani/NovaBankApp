using NovaBankApp.Contracts;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

/// <summary>Coordinates external HTTP delivery with the local debit that completes an inter-bank transfer.</summary>
public class InterBankTransferService
{
    private readonly BankingService bankingService;
    private readonly IInterBankTransferClient transferClient;

    /// <summary>Builds the service from banking rules and an interface-based HTTP client.</summary>
    public InterBankTransferService(BankingService bankingService, IInterBankTransferClient transferClient)
    {
        this.bankingService = bankingService;
        this.transferClient = transferClient;
    }

    /// <summary>Sends funds to another app instance and debits the sender only after receipt is confirmed.</summary>
    public async Task<OperationResult> TransferAsync(BankAccount sender, string bankUrl, string recipientAccountNumber, decimal amount)
    {
        var validation = bankingService.ValidateInterBankTransfer(sender, amount);
        if (!validation.IsSuccessful) return validation;

        var response = await transferClient.SendAsync(bankUrl, new InterBankTransferRequest(recipientAccountNumber, sender.CustomerName, amount));
        if (!response.IsSuccessful) return OperationResult.Failure($"Transfer was not completed: {response.Message}");

        return bankingService.DebitForInterBankTransfer(sender, amount, recipientAccountNumber);
    }
}
