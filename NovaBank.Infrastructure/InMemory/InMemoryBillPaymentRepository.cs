using NovaBankApp.Models;

namespace NovaBankApp.Data;

public class InMemoryBillPaymentRepository : IBillPaymentRepository
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, BillPaymentRecord> paymentsByTransactionId = new(StringComparer.OrdinalIgnoreCase);

    public bool TryAdd(BillPaymentRecord payment)
    {
        lock (syncRoot)
        {
            if (paymentsByTransactionId.ContainsKey(payment.TransactionId))
            {
                return false;
            }

            paymentsByTransactionId.Add(payment.TransactionId, payment);
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
