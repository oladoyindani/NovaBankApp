namespace WebApiNovaBank.Contracts;

/// <summary>A provider payment record visible to an authorized administrator.</summary>
public sealed record AdminBillPaymentResponse(
    string TransactionId,
    string PaymentType,
    string PhoneNumber,
    decimal Amount,
    DateTimeOffset ProcessedAt,
    bool IsSuccessful,
    string Message);
