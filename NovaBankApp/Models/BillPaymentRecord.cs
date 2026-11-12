namespace NovaBankApp.Models;

public record BillPaymentRecord(
    string TransactionId,
    BillPaymentType PaymentType,
    string PhoneNumber,
    decimal Amount,
    DateTimeOffset ProcessedAt,
    bool IsSuccessful,
    string Message,
    string? AccountNumber = null);
