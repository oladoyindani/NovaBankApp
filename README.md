# NovaBank Secure Console Terminal

A feature-rich, production-grade C# console banking application built with modern **Top-Level Statements**. NovaBank provides secure terminal-based banking workflows, complete with PIN authentication, multi-currency wallet conversions, bill payments, and real-time session audit ledgers.

---

## Key Features

* **Secure Authentication Layer**: Requires a custom 4-digit numeric PIN configuration on startup, featuring a 3-attempt lock mechanism for security.
* **Dynamic Welcome Banner**: Automatically calculates text string lengths to render a responsive ASCII border box (`=`) tailored to the user's name and branch.
* **Core Account Operations**:
  * **Balance Checking**: Displays current balances formatted securely with currency and comma separators (`:N2`).
  * **Deposits**: Increment funds instantly with real-time feedback.
  * **Transfers & Withdrawals**: Secure balance validation to prevent overdrafts.
* **Comprehensive Bill Payments Portal**: Supports 5 major categories:
  * Airtime Recharge
  * Internet Data Bundles
  * Electricity Tokens (Prepaid/Postpaid)
  * Cable TV (DStv/GOtv)
  * Broadband / Fiber ISP
* **Multi-Currency Global Forex Wallet**: Instantly converts standard USD balances into global currencies (**EUR, GBP, and NGN**) using realistic mock exchange rates.
* **Session Audit Ledger**: Itemized, timestamped transaction history logging every action taken during the session.

---

## Tech Stack & Requirements

* **Language**: C# 9+ (Top-Level Statements)
* **Runtime**: .NET 6.0 SDK or higher (`.NET 8.0` / `.NET 9.0` compatible)
* **Platform**: Cross-platform (Windows, macOS, Linux)

---

## Getting Started & Installation

1. **Clone or Create the Project**
   Open your terminal and initialize a new C# console application:
   ```bash
   dotnet new console -n NovaBank.Welcome
   cd NovaBank.Welcome
   ```

2. **Add the Source Code**
   Replace the contents of your `Program.cs` file with the application script.

3. **Run the Application**
   Execute the program using the .NET CLI:
   ```bash
   dotnet run
   ```

---

## Usage Walkthrough

1. **User Setup**: Enter your preferred customer name and branch city when prompted.
2. **PIN Creation**: Set up your secure 4-digit numeric PIN.
3. **Authentication**: Enter your PIN to unlock the main terminal.
4. **Navigation**: Use the interactive numeric menu (`1` through `7`) to execute transactions, pay bills, or view your multi-currency portfolio and audit log.

---

## License
This project is developed for educational and portfolio demonstration purposes.