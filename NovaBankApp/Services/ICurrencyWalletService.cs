using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface ICurrencyWalletService
{
    IReadOnlyList<CurrencyBalance> GetBalances(BankAccount account);
}
