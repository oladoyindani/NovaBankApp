using NovaBankApp.Models;

namespace NovaBankApp.Data;

public interface IBillPaymentRepository
{
    bool TryAdd(BillPaymentRecord payment);
    BillPaymentRecord? GetByTransactionId(string transactionId);
    IReadOnlyList<BillPaymentRecord> GetAll();
}
