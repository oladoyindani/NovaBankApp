using NovaBankApp.Configuration;
using NovaBankApp.Data;
using NovaBankApp.Models;
using NovaBankApp.Network;
using NovaBankApp.Services;
using NovaBankApp.Ui;

namespace NovaBankApp.Application;

public class NovaBankApplication
{
    private readonly ConsoleUi ui = new();
    private readonly AuthenticationService authenticationService = new();
    private readonly IAccountRepository accountRepository = new InMemoryAccountRepository();
    private readonly IReadOnlyList<BillCategory> billCategories =
    [
        new("1", "Airtime Recharge"), new("2", "Data Bundle"), new("3", "Electricity Bill"),
        new("4", "Cable TV Subscription"), new("5", "Broadband ISP")
    ];

    public void Run(string[] arguments)
    {
        var bankingService = new BankingService(accountRepository);
        var accountService = new AccountService(accountRepository, authenticationService);
        var adminService = new AdminService(accountRepository);
        var interBankTransferService = new InterBankTransferService(bankingService, new HttpInterBankTransferClient());
        var options = BankServerOptions.FromArguments(arguments);
        using var transferServer = new InterBankTransferServer(bankingService);

        accountService.SeedDemoAccounts();
        ui.ShowStartupBanner();
        var started = transferServer.Start(options.Port, out var serverMessage);
        ui.ShowMessage(serverMessage, started ? ConsoleColor.DarkGray : ConsoleColor.Yellow);
        ShowHomeMenu(accountService, bankingService, adminService, interBankTransferService);
    }

    private void ShowHomeMenu(AccountService accountService, BankingService bankingService, AdminService adminService, InterBankTransferService interBankTransferService)
    {
        var running = true;
        while (running)
        {
            switch (ui.PromptHomeMenu())
            {
                case "1": CreateAccount(accountService); break;
                case "2": Login(bankingService, adminService, interBankTransferService); break;
                case "3": running = false; ui.ShowMessage("Thank you for banking with NovaBank. Goodbye!", ConsoleColor.Yellow); break;
                default: ui.ShowMessage("Select a number from 1 to 3.", ConsoleColor.Red); break;
            }
        }
    }

    private void CreateAccount(AccountService accountService)
    {
        ui.ShowMessage("\n=== CREATE NOVABANK ACCOUNT ===", ConsoleColor.Cyan);
        var name = ui.Prompt("Full name / login user name: ");
        var city = ui.Prompt("City: ");
        var phoneNumber = ui.Prompt("Phone number: ");
        var password = ui.Prompt("Create password (at least 4 characters): ");
        var result = accountService.CreateAccount(name, city, phoneNumber, password, out var account);

        if (!result.IsSuccessful || account is null)
        {
            ShowResult(result);
            return;
        }

        ui.ShowAccountCreated(account);
    }

    private void Login(BankingService bankingService, AdminService adminService, InterBankTransferService interBankTransferService)
    {
        ui.ShowMessage("\n=== LOGIN ===", ConsoleColor.Cyan);
        var userName = ui.Prompt("User name: ");
        var password = ui.Prompt("Password: ");

        if (adminService.IsAdmin(userName, password))
        {
            ui.ShowMessage("Administrator login successful.", ConsoleColor.Green);
            ShowAdminMenu(adminService);
            return;
        }

        var account = accountRepository.GetByCustomerName(userName);
        if (account is null || !authenticationService.VerifyPassword(account, password))
        {
            ui.ShowMessage("Invalid user name or password.", ConsoleColor.Red);
            return;
        }

        ui.ShowMessage($"Welcome back, {account.CustomerName}.", ConsoleColor.Green);
        ShowCustomerMenu(account, bankingService, interBankTransferService);
    }

    private void ShowCustomerMenu(BankAccount account, BankingService bankingService, InterBankTransferService interBankTransferService)
    {
        var running = true;
        while (running)
        {
            switch (ui.PromptCustomerMenu(account))
            {
                case "1": ui.ShowBalance(account); account.AddAuditEntry("Checked balance"); break;
                case "2": Deposit(account, bankingService); break;
                case "3": Withdraw(account, bankingService); break;
                case "4": Transfer(account, bankingService, interBankTransferService); break;
                case "5": PayBill(account, bankingService); break;
                case "6": ShowPortfolio(account); break;
                case "7": ui.ShowTransactions(account); break;
                case "8": running = false; ui.ShowMessage("You have been logged out.", ConsoleColor.Yellow); break;
                default: ui.ShowMessage("Select a number from 1 to 8.", ConsoleColor.Red); break;
            }
        }
    }

    private void ShowAdminMenu(AdminService adminService)
    {
        var running = true;
        while (running)
        {
            switch (ui.PromptAdminMenu())
            {
                case "1": ui.ShowAccounts(adminService.GetAccounts()); break;
                case "2": ShowResult(adminService.Freeze(ui.Prompt("Account number to freeze: "))); break;
                case "3": ShowResult(adminService.Unfreeze(ui.Prompt("Account number to unfreeze: "))); break;
                case "4": running = false; ui.ShowMessage("Administrator logged out.", ConsoleColor.Yellow); break;
                default: ui.ShowMessage("Select a number from 1 to 4.", ConsoleColor.Red); break;
            }
        }
    }

    private void Deposit(BankAccount account, BankingService bankingService)
    {
        if (!ConsoleUi.TryReadPositiveAmount(ui.Prompt("Amount to deposit ($): "), out var amount))
        {
            ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
            return;
        }
        ShowResult(bankingService.Deposit(account, amount));
    }

    private void Withdraw(BankAccount account, BankingService bankingService)
    {
        if (!ConsoleUi.TryReadPositiveAmount(ui.Prompt("Amount to withdraw ($): "), out var amount))
        {
            ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
            return;
        }
        ShowResult(bankingService.Withdraw(account, amount));
    }

    private void Transfer(BankAccount account, BankingService bankingService, InterBankTransferService interBankTransferService)
    {
        switch (ui.PromptTransferMenu())
        {
            case "1": TransferWithinBank(account, bankingService); break;
            case "2": TransferToAnotherBank(account, interBankTransferService); break;
            case "3": break;
            default: ui.ShowMessage("Select a number from 1 to 3.", ConsoleColor.Red); break;
        }
    }

    private void TransferWithinBank(BankAccount sender, BankingService bankingService)
    {
        var recipient = bankingService.FindRecipient(ui.Prompt("Recipient account number: "));
        if (recipient is null)
        {
            ui.ShowMessage("No local account matches that account number.", ConsoleColor.Red);
            return;
        }

        ui.ShowRecipient(recipient);
        if (!ConsoleUi.TryReadPositiveAmount(ui.Prompt("Amount to transfer ($): "), out var amount))
        {
            ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
            return;
        }
        if (!authenticationService.VerifyPassword(sender, ui.Prompt("Confirm your password to send money: ")))
        {
            ui.ShowMessage("Password confirmation failed. Transfer cancelled.", ConsoleColor.Red);
            return;
        }

        ShowResult(bankingService.TransferWithinBank(sender, recipient, amount));
    }

    private void TransferToAnotherBank(BankAccount sender, InterBankTransferService interBankTransferService)
    {
        var bankUrl = ui.Prompt("Receiving bank URL (example: http://localhost:5001): ");
        var recipientAccountNumber = ui.Prompt("Receiving account number: ");
        if (!ConsoleUi.TryReadPositiveAmount(ui.Prompt("Amount to transfer ($): "), out var amount))
        {
            ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
            return;
        }
        if (!authenticationService.VerifyPassword(sender, ui.Prompt("Confirm your password to send money: ")))
        {
            ui.ShowMessage("Password confirmation failed. Transfer cancelled.", ConsoleColor.Red);
            return;
        }

        ShowResult(interBankTransferService.TransferAsync(sender, bankUrl, recipientAccountNumber, amount).GetAwaiter().GetResult());
    }

    private void PayBill(BankAccount account, BankingService bankingService)
    {
        var selectedOption = ui.PromptBillCategory(billCategories);
        var category = billCategories.SingleOrDefault(item => item.MenuOption == selectedOption);
        if (category is null)
        {
            ui.ShowMessage("Invalid bill category selection.", ConsoleColor.Red);
            return;
        }

        ui.Prompt($"Account, meter, or phone number for {category.Name}: ");
        if (!ConsoleUi.TryReadPositiveAmount(ui.Prompt("Bill amount ($): "), out var amount))
        {
            ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
            return;
        }
        ShowResult(bankingService.PayBill(account, category, amount));
    }

    private void ShowPortfolio(BankAccount account)
    {
        var currencyWalletService = new CurrencyWalletService();
        ui.ShowPortfolio(account.Balance, currencyWalletService.GetBalances(account));
        account.AddAuditEntry("Viewed multi-currency portfolio");
    }

    private void ShowResult(OperationResult result) => ui.ShowMessage(result.Message, result.IsSuccessful ? ConsoleColor.Green : ConsoleColor.Red);
}
