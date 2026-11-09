namespace NovaBankApp.Models;

public record BillPaymentRecord(
    string TransactionId,
    BillPaymentType PaymentType,
    string PhoneNumber,
    decimal Amount,
    DateTime ProcessedAt,
    bool IsSuccessful,
    string Message);
