namespace NovaBankApp.Contracts;

public sealed record InterBankTransferRequest(
    Guid TransferId,
    string RecipientAccountNumber,
    string SenderName,
    decimal Amount);
