using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IAccountService
{
    OperationResult CreateAccount(
        string customerName,
        string state,
        string phoneNumber,
        string password,
        out BankAccount? account);

    void SeedDemoAccounts();
    BankAccount? GetByAccountNumber(string accountNumber);
    BankAccount? GetByCustomerName(string customerName);
}
