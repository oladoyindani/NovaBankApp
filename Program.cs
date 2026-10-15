
using System;

Console.Write("Enter your name: ");
string name = Console.ReadLine()?.Trim() ?? "Valued Customer";

Console.Write("Enter your branch city: ");
string city = Console.ReadLine()?.Trim() ?? "Main Branch";

string currentDate = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

string line1 = $"Welcome, {name}!";
string line2 = $"Branch: {city}";
string line3 = $"Date: {currentDate}";

int maxLength = Math.Max(line1.Length, Math.Max(line2.Length, line3.Length));
string border = new string('=', maxLength + 4);

Console.WriteLine("\n" + border);
Console.WriteLine($"= {line1.PadRight(maxLength)} =");
Console.WriteLine($"= {line2.PadRight(maxLength)} =");
Console.WriteLine($"= {line3.PadRight(maxLength)} =");
Console.WriteLine(border + "\n");

decimal balance = 5420.50m;
bool running = true;

while (running)
{
    Console.WriteLine("--------------------------------");
    Console.WriteLine("Select an option:");
    Console.WriteLine("1. Check balance");
    Console.WriteLine("2. Transfer");
    Console.WriteLine("3. Exit");
    Console.Write("Enter your choice (1-3): ");

    string? choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            Console.WriteLine(
                $"[Account Balance] Your current balance is: ${balance:N2}");
            break;

        case "2":
            Console.Write("Enter amount to transfer ($): ");

            if (decimal.TryParse(Console.ReadLine(), out decimal transferAmount)
                && transferAmount > 0)
            {
                if (transferAmount <= balance)
                {
                    balance -= transferAmount;

                    Console.WriteLine(
                        $"[Transfer Successful] Sent ${transferAmount:N2}. " +
                        $"Remaining balance: ${balance:N2}");
                }
                else
                {
                    Console.WriteLine(
                        "[Transfer Failed] Insufficient funds available.");
                }
            }
            else
            {
                Console.WriteLine(
                    "[Invalid Input] Please enter a valid positive number.");
            }

            break;

        case "3":
            running = false;

            Console.WriteLine(
                $"Thank you for banking with NovaBank, {name}. " +
                "Have a wonderful day!");

            break;

        default:
            Console.WriteLine(
                "[Error] Invalid selection. Please choose 1, 2, or 3.");
            break;
    }

    Console.WriteLine();
}
