namespace NovaBankApp.Contracts;

public record BillPaymentResponse(
    string TransactionId,
    string PaymentType,
    bool IsSuccessful,
    bool IsDuplicate,
    string Message,
    DateTimeOffset ProcessedAt);
