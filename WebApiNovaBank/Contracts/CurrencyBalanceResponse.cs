namespace WebApiNovaBank.Contracts;

/// <summary>A converted view of an account balance in another currency.</summary>
public sealed record CurrencyBalanceResponse(
    string Name,
    string Code,
    string Symbol,
    decimal ExchangeRate,
    decimal Amount);
