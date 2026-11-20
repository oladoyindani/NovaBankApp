using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBank.Infrastructure;

internal sealed class SqlBillPaymentRepository(SqlConnectionFactory connectionFactory) : IBillPaymentRepository
{
    public bool TryAdd(BillPaymentRecord payment)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        try
        {
            if (!Guid.TryParse(payment.TransactionId, out var paymentId))
            {
                throw new ArgumentException("Bill-payment transaction ID must be a GUID.", nameof(payment));
            }

            connection.Execute(
                """
                INSERT INTO dbo.BillPayments
                    (TransactionId, PaymentType, PhoneNumber, Amount, ProcessedAt, IsSuccessful, Message, AccountNumber)
                VALUES
                    (@TransactionId, @PaymentType, @PhoneNumber, @Amount, @ProcessedAt, @IsSuccessful, @Message, @AccountNumber);
                """,
                new
                {
                    TransactionId = paymentId,
                    payment.PaymentType,
                    payment.PhoneNumber,
                    payment.Amount,
                    payment.ProcessedAt,
                    payment.IsSuccessful,
                    payment.Message,
                    payment.AccountNumber
                });
            return true;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public BillPaymentRecord? GetByTransactionId(string transactionId)
    {
        if (!Guid.TryParse(transactionId, out var paymentId))
        {
            return null;
        }

        using var connection = connectionFactory.Create();
        connection.Open();
        return connection.QuerySingleOrDefault<BillPaymentRecord>(
            """
            SELECT CONVERT(varchar(36), TransactionId) AS TransactionId,
                   PaymentType, PhoneNumber, Amount, ProcessedAt, IsSuccessful, Message, AccountNumber
            FROM dbo.BillPayments
            WHERE TransactionId = @TransactionId;
            """,
            new { TransactionId = paymentId });
    }

    public IReadOnlyList<BillPaymentRecord> GetAll()
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        return connection.Query<BillPaymentRecord>(
            """
            SELECT CONVERT(varchar(36), TransactionId) AS TransactionId,
                   PaymentType, PhoneNumber, Amount, ProcessedAt, IsSuccessful, Message, AccountNumber
            FROM dbo.BillPayments
            ORDER BY ProcessedAt DESC;
            """).AsList();
    }
}
