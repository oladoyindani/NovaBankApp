using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class CurrencyWalletService
{
    private readonly IReadOnlyList<CurrencyBalance> currencies =
    [
        new("Euro", "EUR", "€", 0.92m), new("British Pound", "GBP", "£", 0.79m), new("Nigerian Naira", "NGN", "₦", 1520m)
    ];

    public IReadOnlyList<CurrencyBalance> GetBalances(BankAccount account) => currencies.Select(currency => currency with { Amount = account.Balance * currency.ExchangeRate }).ToList();
}

public record CurrencyBalance(string Name, string Code, string Symbol, decimal ExchangeRate, decimal Amount = 0);
