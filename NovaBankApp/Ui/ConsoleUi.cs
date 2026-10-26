using NovaBankApp.Models;
using NovaBankApp.Services;

namespace NovaBankApp.Ui;

/// <summary>Contains every console prompt, menu, colour, and display operation.</summary>
public class ConsoleUi
{
    /// <summary>Displays the application title when the terminal launches.</summary>
    public void ShowStartupBanner()
    {
        WriteColored(ConsoleColor.Cyan, "==================================================");
        WriteColored(ConsoleColor.Cyan, "        WELCOME TO NOVABANK SECURE TERMINAL      ");
        WriteColored(ConsoleColor.Cyan, "==================================================");
    }

    /// <summary>Reads trimmed console input and supplies a fallback for an absent input stream.</summary>
    public string Prompt(string message, string fallback = "")
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim() ?? fallback;
    }

    /// <summary>Displays a message with an optional foreground colour.</summary>
    public void ShowMessage(string message, ConsoleColor? color = null)
    {
        if (color.HasValue) Console.ForegroundColor = color.Value;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>Shows the launch menu that separates registration from login.</summary>
    public string PromptHomeMenu()
    {
        Console.WriteLine("\nHOME MENU:\n  1. Create Account\n  2. Login\n  3. Exit");
        return Prompt("Select an option (1-3): ");
    }

    /// <summary>Shows the actions available to an authenticated customer.</summary>
    public string PromptCustomerMenu(BankAccount account)
    {
        Console.WriteLine($"\nCUSTOMER PORTAL - {account.CustomerName} ({account.AccountNumber})");
        Console.WriteLine("  1. Check Balance\n  2. Deposit Funds\n  3. Withdraw Funds\n  4. Transfer Funds\n  5. Pay Bills\n  6. Multi-Currency Wallet\n  7. View Transaction History\n  8. Logout");
        return Prompt("Select an option (1-8): ");
    }

    /// <summary>Shows the actions reserved for the preset administrator account.</summary>
    public string PromptAdminMenu()
    {
        Console.WriteLine("\nADMIN PORTAL:\n  1. View Accounts\n  2. Freeze Account\n  3. Unfreeze Account\n  4. Logout");
        return Prompt("Select an option (1-4): ");
    }

    /// <summary>Shows the two possible routes for sending money.</summary>
    public string PromptTransferMenu()
    {
        Console.WriteLine("\nTRANSFER MENU:\n  1. Intra-Bank Transfer\n  2. Inter-Bank Transfer\n  3. Cancel");
        return Prompt("Select transfer type (1-3): ");
    }

    /// <summary>Shows bill categories and returns the chosen menu value.</summary>
    public string PromptBillCategory(IReadOnlyList<BillCategory> categories)
    {
        Console.WriteLine("=== BILL PAYMENTS PORTAL ===");
        foreach (var category in categories) Console.WriteLine($"  {category.MenuOption}. {category.Name}");
        return Prompt("Select bill category: ");
    }

    /// <summary>Shows an account number prominently after successful onboarding.</summary>
    public void ShowAccountCreated(BankAccount account)
    {
        ShowMessage("\n[Account Created] Your account has been opened with a $0.00 balance.", ConsoleColor.Green);
        ShowMessage($"YOUR NOVABANK ACCOUNT NUMBER: {account.AccountNumber}", ConsoleColor.Yellow);
        ShowMessage("Copy and save this number now. You will need it to receive transfers.", ConsoleColor.Yellow);
    }

    /// <summary>Displays the customer identity returned from a local account-number lookup.</summary>
    public void ShowRecipient(BankAccount recipient) => ShowMessage($"Recipient found: {recipient.CustomerName} | {recipient.City} | Account: {recipient.AccountNumber}", ConsoleColor.Cyan);

    /// <summary>Displays the current account balance in USD.</summary>
    public void ShowBalance(BankAccount account) => ShowMessage($"Current Balance: ${account.Balance:N2}", ConsoleColor.Cyan);

    /// <summary>Displays mock currency conversions in the existing wallet format.</summary>
    public void ShowPortfolio(decimal usdBalance, IReadOnlyList<CurrencyBalance> balances)
    {
        ShowMessage("\n=== NOVABANK GLOBAL MULTI-CURRENCY WALLET ===", ConsoleColor.Magenta);
        Console.WriteLine($"Base Wallet Balance (USD): ${usdBalance:N2}");
        foreach (var balance in balances) Console.WriteLine($" * {balance.Name} ({balance.Code} @ {balance.ExchangeRate:N2}): {balance.Symbol}{balance.Amount:N2}");
    }

    /// <summary>Displays a customer's personal transaction history.</summary>
    public void ShowTransactions(BankAccount account)
    {
        ShowMessage("=== TRANSACTION AUDIT TRAIL ===", ConsoleColor.Magenta);
        if (account.Transactions.Count == 0) Console.WriteLine("No transactions recorded yet.");
        else foreach (var transaction in account.Transactions) Console.WriteLine($" - {transaction}");
    }

    /// <summary>Displays all local customer accounts to an authenticated administrator.</summary>
    public void ShowAccounts(IReadOnlyList<AccountSummary> accounts)
    {
        ShowMessage("=== CUSTOMER ACCOUNTS ===", ConsoleColor.Magenta);
        foreach (var account in accounts) Console.WriteLine($"{account.AccountNumber} | {account.CustomerName} | {account.City} | ${account.Balance:N2} | {(account.IsFrozen ? "FROZEN" : "ACTIVE")}");
    }

    /// <summary>Parses currency values and accepts only positive amounts.</summary>
    public static bool TryReadPositiveAmount(string input, out decimal amount) => decimal.TryParse(input, out amount) && amount > 0;

    /// <summary>Writes one coloured line and always resets the terminal colour afterward.</summary>
    private static void WriteColored(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
