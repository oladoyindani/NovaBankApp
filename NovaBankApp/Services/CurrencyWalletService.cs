using NovaBankApp.Models;

namespace NovaBankApp.Services;

/// <summary>Converts a USD account balance into display-only mock foreign-currency values.</summary>
public class CurrencyWalletService
{
    private readonly IReadOnlyList<CurrencyBalance> currencies =
    [
        new("Euro", "EUR", "€", 0.92m), new("British Pound", "GBP", "£", 0.79m), new("Nigerian Naira", "NGN", "₦", 1520m)
    ];

    /// <summary>Calculates all configured currency views from the supplied customer balance.</summary>
    public IReadOnlyList<CurrencyBalance> GetBalances(BankAccount account) => currencies.Select(currency => currency with { Amount = account.Balance * currency.ExchangeRate }).ToList();
}

/// <summary>Contains one mock exchange rate and its calculated display amount.</summary>
public record CurrencyBalance(string Name, string Code, string Symbol, decimal ExchangeRate, decimal Amount = 0);
