using System.Security.Cryptography;
using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class AccountService
{
    private readonly IAccountRepository accountRepository;
    private readonly AuthenticationService authenticationService;

    public AccountService(IAccountRepository accountRepository, AuthenticationService authenticationService)
    {
        this.accountRepository = accountRepository;
        this.authenticationService = authenticationService;
    }

    public OperationResult CreateAccount(string customerName, string state, string phoneNumber, string password, out BankAccount? account)
    {
        account = null;
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(phoneNumber)) return OperationResult.Failure("Name, state, and phone number are required.");
        if (!phoneNumber.All(char.IsDigit)) return OperationResult.Failure("Phone number must contain digits only.");
        if (!authenticationService.IsValidPassword(password)) return OperationResult.Failure("Password must be at least 4 characters.");
        if (accountRepository.GetByCustomerName(customerName) is not null) return OperationResult.Failure("An account already exists for that user name.");

        var accountNumber = GenerateAccountNumber();
        var newAccount = new BankAccount(accountNumber, customerName.Trim(), state.Trim(), phoneNumber.Trim(), authenticationService.HashPassword(password));
        if (!accountRepository.Add(newAccount)) return OperationResult.Failure("Unable to create the account. Please try again.");

        account = newAccount;
        return OperationResult.Success("Account created successfully.");
    }

    public void SeedDemoAccounts()
    {
        CreateDemoAccount("Brian Naheemat", "Ojo", "08010000001", "test123", "1000000001", 2500m);
        CreateDemoAccount("Francis Chocboy", "Apapa", "08010000002", "test123", "1000000002", 1800m);
    }

    private void CreateDemoAccount(string name, string state, string phone, string password, string accountNumber, decimal openingBalance)
    {
        if (accountRepository.GetByAccountNumber(accountNumber) is not null) return;
        var account = new BankAccount(accountNumber, name, state, phone, authenticationService.HashPassword(password));
        account.Credit(openingBalance, "Demo opening balance");
        accountRepository.Add(account);
    }

    private string GenerateAccountNumber()
    {
        string accountNumber;
        do accountNumber = RandomNumberGenerator.GetInt32(1_000_000_000, int.MaxValue).ToString();
        while (accountRepository.AccountNumberExists(accountNumber));
        return accountNumber;
    }
}
