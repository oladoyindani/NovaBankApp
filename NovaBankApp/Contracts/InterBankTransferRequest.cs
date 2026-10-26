namespace NovaBankApp.Contracts;

/// <summary>Defines the JSON payload sent from one NovaBank console instance to another.</summary>
public record InterBankTransferRequest(string RecipientAccountNumber, string SenderName, decimal Amount);
