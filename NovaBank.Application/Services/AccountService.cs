using System.Security.Cryptography;
using NovaBankApp.Data;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository accountRepository;
    private readonly IAuthenticationService authenticationService;

    public AccountService(IAccountRepository accountRepository, IAuthenticationService authenticationService)
    {
        this.accountRepository = accountRepository;
        this.authenticationService = authenticationService;
    }

    public OperationResult CreateAccount(string customerName, string state, string phoneNumber, string password, out BankAccount? account)
    {
        account = null;
        if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(phoneNumber)) return OperationResult.Failure("Name, state, and phone number are required.");
        if (!phoneNumber.All(char.IsDigit)) return OperationResult.Failure("Phone number must contain digits only.");
        if (!authenticationService.IsValidPassword(password)) return OperationResult.Failure("Password must be at least 12 characters.");
        if (accountRepository.GetByCustomerName(customerName) is not null) return OperationResult.Failure("An account already exists for that user name.");

        var accountNumber = GenerateAccountNumber();
        var newAccount = new BankAccount(accountNumber, customerName.Trim(), state.Trim(), phoneNumber.Trim(), authenticationService.HashPassword(password));
        if (!accountRepository.Add(newAccount)) return OperationResult.Failure("Unable to create the account. Please try again.");

        account = newAccount;
        return OperationResult.Success("Account created successfully.");
    }

    public BankAccount? GetByAccountNumber(string accountNumber) =>
        accountRepository.GetByAccountNumber(accountNumber);

    public BankAccount? GetByCustomerName(string customerName) =>
        accountRepository.GetByCustomerName(customerName);

    private string GenerateAccountNumber()
    {
        string accountNumber;
        do accountNumber = RandomNumberGenerator.GetInt32(1_000_000_000, int.MaxValue).ToString();
        while (accountRepository.AccountNumberExists(accountNumber));
        return accountNumber;
    }
}
