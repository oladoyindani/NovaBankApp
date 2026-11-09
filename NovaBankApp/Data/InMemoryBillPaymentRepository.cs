using NovaBankApp.Models;

namespace NovaBankApp.Data;

public class InMemoryBillPaymentRepository : IBillPaymentRepository
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, BillPaymentRecord> paymentsByTransactionId = new(StringComparer.OrdinalIgnoreCase);

    public bool TryAdd(BillPaymentRecord payment, out BillPaymentRecord? existingPayment)
    {
        lock (syncRoot)
        {
            if (paymentsByTransactionId.TryGetValue(payment.TransactionId, out existingPayment))
            {
                return false;
            }

            paymentsByTransactionId.Add(payment.TransactionId, payment);
            existingPayment = null;
            return true;
        }
    }

    public BillPaymentRecord? GetByTransactionId(string transactionId)
    {
        lock (syncRoot)
        {
            return paymentsByTransactionId.GetValueOrDefault(transactionId);
        }
    }

    public IReadOnlyList<BillPaymentRecord> GetAll()
    {
        lock (syncRoot)
        {
            return paymentsByTransactionId.Values
                .OrderByDescending(payment => payment.ProcessedAt)
                .ToList();
        }
    }
}
