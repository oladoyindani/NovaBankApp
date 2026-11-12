using NovaBankApp.Contracts;
using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class BillPaymentProviderService : IBillPaymentProviderService
{
    private readonly IBillPaymentRepository billPaymentRepository;

    public BillPaymentProviderService(IBillPaymentRepository billPaymentRepository)
    {
        this.billPaymentRepository = billPaymentRepository;
    }

    public BillPaymentResponse Process(
        BillPaymentType paymentType,
        BillPaymentRequest request,
        string? accountNumber = null)
    {
        if (!Guid.TryParse(request.TransactionId, out var transactionGuid))
        {
            return Failure(request.TransactionId, paymentType, "Transaction ID must be a valid GUID.");
        }

        var transactionId = transactionGuid.ToString("D");
        var validationFailure = Validate(request);
        var response = validationFailure is null
            ? Success(transactionId, paymentType, request.PhoneNumber, request.Amount)
            : Failure(transactionId, paymentType, validationFailure);
        var payment = new BillPaymentRecord(
            transactionId,
            paymentType,
            request.PhoneNumber,
            request.Amount,
            response.ProcessedAt,
            response.IsSuccessful,
            response.Message,
            accountNumber);

        if (!billPaymentRepository.TryAdd(payment))
        {
            return new BillPaymentResponse(
                transactionId,
                paymentType.ToString(),
                false,
                true,
                "Duplicate transaction ID. Check the original payment status before retrying.",
                DateTimeOffset.UtcNow);
        }

        return response;
    }

    public BillPaymentResponse? GetStatus(string transactionId)
    {
        var payment = billPaymentRepository.GetByTransactionId(transactionId);
        return ToResponse(payment);
    }

    public BillPaymentResponse? GetStatusForAccount(string transactionId, string accountNumber)
    {
        var payment = billPaymentRepository.GetByTransactionId(transactionId);
        return payment is null || !string.Equals(payment.AccountNumber, accountNumber, StringComparison.Ordinal)
            ? null
            : ToResponse(payment);
    }

    public IReadOnlyList<BillPaymentRecord> GetPaymentHistory() => billPaymentRepository.GetAll();

    private static BillPaymentResponse? ToResponse(BillPaymentRecord? payment) => payment is null
        ? null
        : new BillPaymentResponse(
            payment.TransactionId,
            payment.PaymentType.ToString(),
            payment.IsSuccessful,
            false,
            payment.Message,
            payment.ProcessedAt);

    private static string? Validate(BillPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber)
            || !request.PhoneNumber.All(char.IsDigit)
            || request.PhoneNumber.Length is < 10 or > 15)
        {
            return "Phone number must contain 10 to 15 digits.";
        }

        if (request.Amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        return null;
    }

    private static BillPaymentResponse Success(string transactionId, BillPaymentType paymentType, string phoneNumber, decimal amount) => new(
        transactionId,
        paymentType.ToString(),
        true,
        false,
        $"{paymentType} purchase of ${amount:N2} for {phoneNumber} was completed.",
        DateTimeOffset.UtcNow);

    private static BillPaymentResponse Failure(string transactionId, BillPaymentType paymentType, string message) => new(
        transactionId,
        paymentType.ToString(),
        false,
        false,
        message,
        DateTimeOffset.UtcNow);
}
