using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBank.Infrastructure;

internal sealed class SqlAccountRepository(SqlConnectionFactory connectionFactory) : IAccountRepository
{
    private readonly Dictionary<BankAccount, int> trackedAccounts = new(ReferenceEqualityComparer.Instance);

    public bool Add(BankAccount account)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        try
        {
            var token = connection.QuerySingle<byte[]>(
                """
                INSERT INTO dbo.Accounts (AccountNumber, CustomerName, City, PhoneNumber, PasswordHash, Balance, IsFrozen)
                OUTPUT inserted.Version
                VALUES (@AccountNumber, @CustomerName, @City, @PhoneNumber, @PasswordHash, @Balance, @IsFrozen);
                """,
                new
                {
                    account.AccountNumber,
                    account.CustomerName,
                    account.City,
                    account.PhoneNumber,
                    account.PasswordHash,
                    account.Balance,
                    account.IsFrozen
                });
            account.SetConcurrencyToken(token);
            trackedAccounts[account] = account.Transactions.Count;
            return true;
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            return false;
        }
    }

    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        var row = connection.QuerySingleOrDefault<AccountRow>(
            """
            SELECT AccountNumber, CustomerName, City, PhoneNumber, PasswordHash, Balance, IsFrozen, Version
            FROM dbo.Accounts
            WHERE AccountNumber = @AccountNumber;
            """,
            new { AccountNumber = accountNumber });
        return row is null ? null : LoadAggregate(connection, row);
    }

    public BankAccount? GetByCustomerName(string customerName)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        var row = connection.QuerySingleOrDefault<AccountRow>(
            """
            SELECT AccountNumber, CustomerName, City, PhoneNumber, PasswordHash, Balance, IsFrozen, Version
            FROM dbo.Accounts
            WHERE CustomerName = @CustomerName;
            """,
            new { CustomerName = customerName });
        return row is null ? null : LoadAggregate(connection, row);
    }

    public IReadOnlyList<AccountSummary> GetSummaries()
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        return connection.Query<AccountSummary>(
            """
            SELECT AccountNumber, CustomerName, City, PhoneNumber, Balance, IsFrozen
            FROM dbo.Accounts
            ORDER BY CustomerName;
            """).AsList();
    }

    public bool AccountNumberExists(string accountNumber)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        return connection.ExecuteScalar<bool>(
            "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Accounts WHERE AccountNumber = @AccountNumber) THEN 1 ELSE 0 END;",
            new { AccountNumber = accountNumber });
    }

    public void SaveChanges(params BankAccount[] accounts)
    {
        if (accounts.Length == 0)
        {
            return;
        }

        using var connection = connectionFactory.Create();
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
        var updatedTokens = new List<(BankAccount Account, byte[] Token, int TransactionCount)>();

        var uniqueAccounts = new HashSet<BankAccount>(ReferenceEqualityComparer.Instance);
        foreach (var account in accounts)
        {
            if (!uniqueAccounts.Add(account))
            {
                continue;
            }

            if (!trackedAccounts.TryGetValue(account, out var savedTransactionCount))
            {
                throw new InvalidOperationException("The account must be loaded from this repository before it can be saved.");
            }

            var token = connection.QuerySingleOrDefault<byte[]>(
                """
                UPDATE dbo.Accounts
                SET Balance = @Balance, IsFrozen = @IsFrozen
                OUTPUT inserted.Version
                WHERE AccountNumber = @AccountNumber AND Version = @Version;
                """,
                new
                {
                    account.AccountNumber,
                    account.Balance,
                    account.IsFrozen,
                    Version = account.ConcurrencyToken
                },
                transaction);

            if (token is null)
            {
                throw new DBConcurrencyException(
                    "The account was updated by another request. Reload it and retry the operation.");
            }

            var accountTransactions = account.Transactions;
            foreach (var entry in accountTransactions.Skip(savedTransactionCount))
            {
                connection.Execute(
                    """
                    INSERT INTO dbo.AccountTransactions (AccountNumber, OccurredAt, Description)
                    VALUES (@AccountNumber, @OccurredAt, @Description);
                    """,
                    new
                    {
                        account.AccountNumber,
                        entry.OccurredAt,
                        entry.Description
                    },
                    transaction);
            }

            updatedTokens.Add((account, token, accountTransactions.Count));
        }

        transaction.Commit();

        foreach (var update in updatedTokens)
        {
            update.Account.SetConcurrencyToken(update.Token);
            trackedAccounts[update.Account] = update.TransactionCount;
        }

    }

    public OperationResult RecordIncomingTransfer(
        Guid transferId,
        string recipientAccountNumber,
        string senderName,
        decimal amount)
    {
        using var connection = connectionFactory.Create();
        connection.Open();
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

        var recipientExists = connection.ExecuteScalar<bool>(
            """
            SELECT CASE WHEN EXISTS
            (
                SELECT 1 FROM dbo.Accounts WITH (UPDLOCK, HOLDLOCK)
                WHERE AccountNumber = @RecipientAccountNumber
            ) THEN 1 ELSE 0 END;
            """,
            new { RecipientAccountNumber = recipientAccountNumber },
            transaction);

        if (!recipientExists)
        {
            transaction.Rollback();
            return OperationResult.Failure("Recipient account was not found at this bank.");
        }

        var existingTransfer = connection.QuerySingleOrDefault<IncomingTransferRow>(
            """
            SELECT TransferId, RecipientAccountNumber, SenderName, Amount
            FROM dbo.IncomingTransfers WITH (UPDLOCK, HOLDLOCK)
            WHERE TransferId = @TransferId;
            """,
            new { TransferId = transferId },
            transaction);
        if (existingTransfer is not null)
        {
            transaction.Rollback();
            var sameRequest = string.Equals(
                    existingTransfer.RecipientAccountNumber,
                    recipientAccountNumber,
                    StringComparison.Ordinal)
                && string.Equals(existingTransfer.SenderName, senderName, StringComparison.Ordinal)
                && existingTransfer.Amount == amount;
            return sameRequest
                ? OperationResult.Success("This transfer was already processed.")
                : OperationResult.Failure("Transfer ID was previously used for a different transfer.");
        }

        connection.Execute(
            """
            INSERT INTO dbo.IncomingTransfers (TransferId, RecipientAccountNumber, SenderName, Amount, ReceivedAt)
            VALUES (@TransferId, @RecipientAccountNumber, @SenderName, @Amount, @ReceivedAt);
            """,
            new
            {
                TransferId = transferId,
                RecipientAccountNumber = recipientAccountNumber,
                SenderName = senderName,
                Amount = amount,
                ReceivedAt = DateTimeOffset.UtcNow
            },
            transaction);

        connection.Execute(
            """
            UPDATE dbo.Accounts
            SET Balance = Balance + @Amount
            WHERE AccountNumber = @RecipientAccountNumber;

            INSERT INTO dbo.AccountTransactions (AccountNumber, OccurredAt, Description)
            VALUES (@RecipientAccountNumber, @OccurredAt, @Description);
            """,
            new
            {
                RecipientAccountNumber = recipientAccountNumber,
                Amount = amount,
                OccurredAt = DateTimeOffset.UtcNow,
                Description = $"Received inter-bank transfer from {senderName}: +${amount:N2}"
            },
            transaction);

        transaction.Commit();
        return OperationResult.Success($"${amount:N2} was credited to the recipient account.");
    }

    private BankAccount LoadAggregate(SqlConnection connection, AccountRow row)
    {
        var transactions = connection.Query<Transaction>(
            """
            SELECT OccurredAt, Description
            FROM dbo.AccountTransactions
            WHERE AccountNumber = @AccountNumber
            ORDER BY OccurredAt, TransactionId;
            """,
            new { row.AccountNumber }).AsList();

        var account = BankAccount.Rehydrate(
            row.AccountNumber,
            row.CustomerName,
            row.City,
            row.PhoneNumber,
            row.PasswordHash,
            row.Balance,
            row.IsFrozen,
            transactions,
            row.Version);
        trackedAccounts[account] = transactions.Count;
        return account;
    }

    private sealed record AccountRow(
        string AccountNumber,
        string CustomerName,
        string City,
        string PhoneNumber,
        string PasswordHash,
        decimal Balance,
        bool IsFrozen,
        byte[] Version);

    private sealed record IncomingTransferRow(
        Guid TransferId,
        string RecipientAccountNumber,
        string SenderName,
        decimal Amount);
}
