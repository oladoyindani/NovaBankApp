namespace WebApiNovaBank.Contracts;

/// <summary>The result of an airtime or data-provider payment.</summary>
public sealed record ProviderBillPaymentResponse(
    string TransactionId,
    string PaymentType,
    bool IsSuccessful,
    bool IsDuplicate,
    string Message,
    DateTimeOffset ProcessedAt);
