namespace NovaBankApp.Contracts;

/// <summary>Defines the JSON acknowledgement returned by the receiving bank instance.</summary>
public record InterBankTransferResponse(bool IsSuccessful, string Message);
