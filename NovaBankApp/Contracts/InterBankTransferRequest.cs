namespace NovaBankApp.Contracts;

public record InterBankTransferRequest(string RecipientAccountNumber, string SenderName, decimal Amount);
