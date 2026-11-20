using NovaBankApp.Contracts;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IBillPaymentProviderService
{
    BillPaymentResponse Process(
        BillPaymentType paymentType,
        BillPaymentRequest request,
        string? accountNumber = null);

    BillPaymentResponse? GetStatus(string transactionId);
    BillPaymentResponse? GetStatusForAccount(string transactionId, string accountNumber);
    IReadOnlyList<BillPaymentRecord> GetPaymentHistory();
}
