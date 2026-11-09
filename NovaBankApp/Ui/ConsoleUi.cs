using NovaBankApp.Models;
using NovaBankApp.Services;

namespace NovaBankApp.Ui;

public class ConsoleUi
{
    public void ShowStartupBanner()
    {
        WriteColored(ConsoleColor.Cyan, "==================================================");
        WriteColored(ConsoleColor.Cyan, "        WELCOME TO NOVABANK SECURE TERMINAL      ");
        WriteColored(ConsoleColor.Cyan, "==================================================");
    }

    public string Prompt(string message, string fallback = "")
    {
        Console.Write(message);
        return Console.ReadLine()?.Trim() ?? fallback;
    }

    public void ShowMessage(string message, ConsoleColor? color = null)
    {
        if (color.HasValue) Console.ForegroundColor = color.Value;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public string PromptHomeMenu()
    {
        Console.WriteLine("\nHOME MENU:\n  1. Create Account\n  2. Login\n  3. Exit");
        return Prompt("Select an option (1-3): ");
    }

    public string PromptCustomerMenu(BankAccount account)
    {
        Console.WriteLine($"\nCUSTOMER PORTAL - {account.CustomerName} ({account.AccountNumber})");
        Console.WriteLine("  1. Check Balance\n  2. Deposit Funds\n  3. Withdraw Funds\n  4. Transfer Funds\n  5. Pay Bills\n  6. Multi-Currency Wallet\n  7. View Transaction History\n  8. Logout");
        return Prompt("Select an option (1-8): ");
    }

    public string PromptAdminMenu()
    {
        Console.WriteLine("\nADMIN PORTAL:\n  1. View Accounts\n  2. Freeze Account\n  3. Unfreeze Account\n  4. View Bill Payment API History\n  5. Logout");
        return Prompt("Select an option (1-5): ");
    }

    public string PromptTransferMenu()
    {
        Console.WriteLine("\nTRANSFER MENU:\n  1. Intra-Bank Transfer\n  2. Inter-Bank Transfer\n  3. Cancel");
        return Prompt("Select transfer type (1-3): ");
    }

    public string PromptBillCategory(IReadOnlyList<BillCategory> categories)
    {
        Console.WriteLine("=== BILL PAYMENTS PORTAL ===");
        foreach (var category in categories) Console.WriteLine($"  {category.MenuOption}. {category.Name}");
        return Prompt("Select bill category: ");
    }

    public void ShowAccountCreated(BankAccount account)
    {
        ShowMessage("\n[Account Created] Your account has been opened with a $0.00 balance.", ConsoleColor.Green);
        ShowMessage($"YOUR NOVABANK ACCOUNT NUMBER: {account.AccountNumber}", ConsoleColor.Yellow);
        ShowMessage("Copy and save this number now. You will need it to receive transfers.", ConsoleColor.Yellow);
    }

    public void ShowRecipient(BankAccount recipient) => ShowMessage($"Recipient found: {recipient.CustomerName} | {recipient.City} | Account: {recipient.AccountNumber}", ConsoleColor.Cyan);

    public void ShowBalance(BankAccount account) => ShowMessage($"Current Balance: ${account.Balance:N2}", ConsoleColor.Cyan);

    public void ShowPortfolio(decimal usdBalance, IReadOnlyList<CurrencyBalance> balances)
    {
        ShowMessage("\n=== NOVABANK GLOBAL MULTI-CURRENCY WALLET ===", ConsoleColor.Magenta);
        Console.WriteLine($"Base Wallet Balance (USD): ${usdBalance:N2}");
        foreach (var balance in balances) Console.WriteLine($" * {balance.Name} ({balance.Code} @ {balance.ExchangeRate:N2}): {balance.Symbol}{balance.Amount:N2}");
    }

    public void ShowTransactions(BankAccount account)
    {
        ShowMessage("=== TRANSACTION AUDIT TRAIL ===", ConsoleColor.Magenta);
        if (account.Transactions.Count == 0) Console.WriteLine("No transactions recorded yet.");
        else foreach (var transaction in account.Transactions) Console.WriteLine($" - {transaction}");
    }

    public void ShowAccounts(IReadOnlyList<AccountSummary> accounts)
    {
        ShowMessage("=== CUSTOMER ACCOUNTS ===", ConsoleColor.Magenta);
        foreach (var account in accounts) Console.WriteLine($"{account.AccountNumber} | {account.CustomerName} | {account.City} | ${account.Balance:N2} | {(account.IsFrozen ? "FROZEN" : "ACTIVE")}");
    }

    public void ShowBillPaymentHistory(IReadOnlyList<BillPaymentRecord> payments)
    {
        ShowMessage("=== BILL PAYMENT API HISTORY ===", ConsoleColor.Magenta);
        if (payments.Count == 0)
        {
            Console.WriteLine("No bill-payment API requests have been processed.");
            return;
        }

        foreach (var payment in payments)
        {
            Console.WriteLine($"{payment.ProcessedAt:yyyy-MM-dd HH:mm:ss} | {payment.TransactionId} | {payment.PaymentType} | {payment.PhoneNumber} | ${payment.Amount:N2} | {(payment.IsSuccessful ? "SUCCESS" : "FAILED")} | {payment.Message}");
        }
    }

    public static bool TryReadPositiveAmount(string input, out decimal amount) => decimal.TryParse(input, out amount) && amount > 0;

    private static void WriteColored(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
