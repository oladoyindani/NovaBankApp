namespace WebApiNovaBank.Contracts;

/// <summary>A transaction or audit event recorded on an account.</summary>
public sealed record TransactionResponse(DateTimeOffset OccurredAt, string Description);
