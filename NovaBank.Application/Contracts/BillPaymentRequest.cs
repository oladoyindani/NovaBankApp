namespace NovaBankApp.Contracts;

public record BillPaymentRequest(string TransactionId, string PhoneNumber, decimal Amount);
