using System;
using System.Collections.Generic;


Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("==================================================");
Console.WriteLine("        WELCOME TO NOVABANK SECURE TERMINAL      ");
Console.WriteLine("==================================================");
Console.ResetColor();

Console.Write("Enter your name: ");
string name = Console.ReadLine()?.Trim() ?? "Valued Customer";

Console.Write("Enter your branch city: ");
string city = Console.ReadLine()?.Trim() ?? "Main Branch";

// Secure PIN Creation Loop (Ensures custom PIN is properly saved)
string accountPin = "";
while (string.IsNullOrEmpty(accountPin))
{
    Console.Write("Create your 4-digit numeric security PIN: ");
    string? userPinInput = Console.ReadLine()?.Trim();

    if (!string.IsNullOrEmpty(userPinInput) && userPinInput.Length == 4 && int.TryParse(userPinInput, out _))
    {
        accountPin = userPinInput;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[Success] PIN successfully configured and saved.");
        Console.ResetColor();
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Error] Invalid PIN. It must be exactly 4 digits. Please try again.");
        Console.ResetColor();
    }
}

// --- 2. DYNAMIC BANNER ---
string currentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");
string line1 = $"Welcome, {name}!";
string line2 = $"Branch: {city} | Status: Active";
string line3 = $"Session Date: {currentDate}";

int maxLength = Math.Max(line1.Length, Math.Max(line2.Length, line3.Length));
string border = new string('=', maxLength + 4);

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\n" + border);
Console.WriteLine($"= {line1.PadRight(maxLength)} =");
Console.WriteLine($"= {line2.PadRight(maxLength)} =");
Console.WriteLine($"= {line3.PadRight(maxLength)} =");
Console.WriteLine(border + "\n");
Console.ResetColor();

int attempts = 3;
bool authenticated = false;

while (attempts > 0 && !authenticated)
{
    Console.Write($"Enter your 4-digit PIN ({attempts} attempts remaining): ");
    string? enteredPin = Console.ReadLine()?.Trim();

    if (enteredPin == accountPin)
    {
        authenticated = true;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n[Access Granted] Welcome back to your portal.\n");
        Console.ResetColor();
    }
    else
    {
        attempts--;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Access Denied] Incorrect PIN.");
        Console.ResetColor();
    }
}

if (!authenticated)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\n[Security Alert] Maximum attempts reached. Terminal locked.");
    Console.ResetColor();
    return; // Exit application
}

decimal balance = 5420.50m;
List<string> transactionHistory = new List<string>();
bool running = true;

while (running)
{
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("--------------------------------------------------");
    Console.ResetColor();
    Console.WriteLine("MAIN MENU:");
    Console.WriteLine("  1. Check Balance");
    Console.WriteLine("  2. Deposit Funds");
    Console.WriteLine("  3. Transfer / Withdraw Funds");
    Console.WriteLine("  4. View Transaction History");
    Console.WriteLine("  5. Exit Session");
    Console.Write("\nSelect an option (1-5): ");

    string? choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    string timestamp = DateTime.Now.ToString("HH:mm:ss");

    switch (choice)
    {
        case "1":
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[Balance Check] Current Balance: ${balance:N2}");
            Console.ResetColor();
            transactionHistory.Add($"[{timestamp}] Checked balance: ${balance:N2}");
            break;

        case "2":
            Console.Write("Enter amount to deposit ($): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal depositAmount) && depositAmount > 0)
            {
                balance += depositAmount;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Deposit Success] Added ${depositAmount:N2}. New Balance: ${balance:N2}");
                Console.ResetColor();
                transactionHistory.Add($"[{timestamp}] Deposited: +${depositAmount:N2}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Please enter a valid positive amount.");
                Console.ResetColor();
            }
            break;

        case "3":
            Console.Write("Enter amount to transfer/withdraw ($): ");
            if (decimal.TryParse(Console.ReadLine(), out decimal transferAmount) && transferAmount > 0)
            {
                if (transferAmount <= balance)
                {
                    balance -= transferAmount;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[Transfer Success] Sent ${transferAmount:N2}. Remaining: ${balance:N2}");
                    Console.ResetColor();
                    transactionHistory.Add($"[{timestamp}] Transferred: -${transferAmount:N2}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[Transfer Failed] Insufficient funds in account.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Error] Please enter a valid positive number.");
                Console.ResetColor();
            }
            break;

        case "4":
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("=== TRANSACTION AUDIT TRAIL ===");
            if (transactionHistory.Count == 0)
            {
                Console.WriteLine("No transactions recorded in this session yet.");
            }
            else
            {
                foreach (var tx in transactionHistory)
                {
                    Console.WriteLine($" - {tx}");
                }
            }
            Console.ResetColor();
            break;

        case "5":
            running = false;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Thank you for banking with NovaBank, {name}. Have a wonderful day!");
            Console.ResetColor();
            break;

        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Error] Invalid selection. Please choose a number between 1 and 5.");
            Console.ResetColor();
            break;
    }

    Console.WriteLine();
}