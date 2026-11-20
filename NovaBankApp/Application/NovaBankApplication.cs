using System.Net.Http;
using NovaBank.ApiClient;
using NovaBankApp.Ui;
using WebApiNovaBank.Contracts;

namespace NovaBankApp.Application;

public sealed class NovaBankApplication(BankApiClient apiClient)
{
    private static readonly IReadOnlyList<string> BillCategories =
    [
        "Airtime Recharge",
        "Data Bundle",
        "Electricity Bill",
        "Cable TV Subscription",
        "Broadband ISP"
    ];

    private readonly ConsoleUi ui = new();

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        ui.ShowStartupBanner();
        ui.ShowMessage("API: " + apiClient.BaseAddress, ConsoleColor.DarkGray);

        var running = true;
        while (running)
        {
            switch (ui.PromptHomeMenu())
            {
                case "1":
                    await CreateAccountAsync(cancellationToken);
                    break;
                case "2":
                    await LoginAsync(cancellationToken);
                    break;
                case "3":
                    running = false;
                    ui.ShowMessage("Thank you for banking with NovaBank. Goodbye!", ConsoleColor.Yellow);
                    break;
                default:
                    ui.ShowMessage("Select a number from 1 to 3.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private async Task CreateAccountAsync(CancellationToken cancellationToken)
    {
        ui.ShowMessage("\n=== CREATE NOVABANK ACCOUNT ===", ConsoleColor.Cyan);
        var request = new CreateAccountRequest
        {
            CustomerName = ui.Prompt("Full name / login user name: "),
            City = ui.Prompt("City: "),
            PhoneNumber = ui.Prompt("Phone number: "),
            Password = ui.Prompt("Create password (at least 12 characters): ")
        };

        await RunApiActionAsync(async () =>
        {
            var account = await apiClient.CreateAccountAsync(request, cancellationToken);
            ui.ShowAccountCreated(account);
        });
    }

    private async Task LoginAsync(CancellationToken cancellationToken)
    {
        ui.ShowMessage("\n=== LOGIN ===", ConsoleColor.Cyan);
        var request = new LoginRequest
        {
            UserName = ui.Prompt("User name: "),
            Password = ui.Prompt("Password: ")
        };

        await RunApiActionAsync(async () =>
        {
            var token = await apiClient.LoginAsync(request, cancellationToken);
            apiClient.SetBearerToken(token.AccessToken);

            if (string.Equals(token.Role, "Admin", StringComparison.Ordinal))
            {
                ui.ShowMessage("Administrator login successful.", ConsoleColor.Green);
                await ShowAdminMenuAsync(cancellationToken);
                return;
            }

            ui.ShowMessage($"Welcome back, {request.UserName}.", ConsoleColor.Green);
            await ShowCustomerMenuAsync(cancellationToken);
        });
    }

    private async Task ShowCustomerMenuAsync(CancellationToken cancellationToken)
    {
        var running = true;
        while (running)
        {
            var account = await apiClient.GetMyAccountAsync(cancellationToken);
            switch (ui.PromptCustomerMenu(account))
            {
                case "1":
                    ui.ShowBalance(account);
                    break;
                case "2":
                    await DepositAsync(cancellationToken);
                    break;
                case "3":
                    await WithdrawAsync(cancellationToken);
                    break;
                case "4":
                    await TransferAsync(cancellationToken);
                    break;
                case "5":
                    await PayBillAsync(cancellationToken);
                    break;
                case "6":
                    await ShowPortfolioAsync(cancellationToken);
                    break;
                case "7":
                    await ShowTransactionsAsync(cancellationToken);
                    break;
                case "8":
                    running = false;
                    ui.ShowMessage("You have been logged out.", ConsoleColor.Yellow);
                    apiClient.ClearBearerToken();
                    break;
                default:
                    ui.ShowMessage("Select a number from 1 to 8.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private async Task ShowAdminMenuAsync(CancellationToken cancellationToken)
    {
        var running = true;
        while (running)
        {
            switch (ui.PromptAdminMenu())
            {
                case "1":
                    await RunApiActionAsync(async () =>
                        ui.ShowAccounts(await apiClient.GetAccountsAsync(cancellationToken)));
                    break;
                case "2":
                    await SetFrozenStatusAsync(freeze: true, cancellationToken);
                    break;
                case "3":
                    await SetFrozenStatusAsync(freeze: false, cancellationToken);
                    break;
                case "4":
                    await RunApiActionAsync(async () =>
                        ui.ShowBillPaymentHistory(await apiClient.GetProviderPaymentHistoryAsync(cancellationToken)));
                    break;
                case "5":
                    running = false;
                    ui.ShowMessage("Administrator logged out.", ConsoleColor.Yellow);
                    apiClient.ClearBearerToken();
                    break;
                default:
                    ui.ShowMessage("Select a number from 1 to 5.", ConsoleColor.Red);
                    break;
            }
        }
    }

    private Task DepositAsync(CancellationToken cancellationToken) =>
        RunAmountOperationAsync("Amount to deposit ($): ", apiClient.DepositAsync, cancellationToken);

    private Task WithdrawAsync(CancellationToken cancellationToken) =>
        RunAmountOperationAsync("Amount to withdraw ($): ", apiClient.WithdrawAsync, cancellationToken);

    private async Task RunAmountOperationAsync(
        string prompt,
        Func<AmountRequest, CancellationToken, Task<OperationResponse>> operation,
        CancellationToken cancellationToken)
    {
        if (!TryReadAmount(prompt, out var amount))
        {
            return;
        }

        await RunApiActionAsync(async () =>
        {
            var result = await operation(new AmountRequest { Amount = amount }, cancellationToken);
            ui.ShowOperationResult(result);
        });
    }

    private async Task TransferAsync(CancellationToken cancellationToken)
    {
        switch (ui.PromptTransferMenu())
        {
            case "1":
                if (!TryReadAmount("Amount to transfer ($): ", out var amount))
                {
                    return;
                }

                await RunApiActionAsync(async () =>
                {
                    var result = await apiClient.TransferWithinBankAsync(
                        new InternalTransferRequest
                        {
                            RecipientAccountNumber = ui.Prompt("Recipient account number: "),
                            Amount = amount
                        },
                        cancellationToken);
                    ui.ShowOperationResult(result);
                });
                break;
            case "2":
                if (!TryReadAmount("Amount to transfer ($): ", out var externalAmount))
                {
                    return;
                }

                await RunApiActionAsync(async () =>
                {
                    var result = await apiClient.TransferToBankAsync(
                        new ExternalBankTransferRequest
                        {
                            TransferId = Guid.NewGuid(),
                            BankUrl = ui.Prompt("Receiving bank API base URL: "),
                            RecipientAccountNumber = ui.Prompt("Receiving account number: "),
                            Amount = externalAmount
                        },
                        cancellationToken);
                    ui.ShowOperationResult(result);
                });
                break;
            case "3":
                break;
            default:
                ui.ShowMessage("Select a number from 1 to 3.", ConsoleColor.Red);
                break;
        }

    }

    private async Task PayBillAsync(CancellationToken cancellationToken)
    {
        var selectedCategory = ui.PromptBillCategory(BillCategories);
        if (!BillCategories.Contains(selectedCategory, StringComparer.OrdinalIgnoreCase))
        {
            ui.ShowMessage("Invalid bill category selection.", ConsoleColor.Red);
            return;
        }

        if (!TryReadAmount($"Amount to pay for {selectedCategory} ($): ", out var amount))
        {
            return;
        }

        await RunApiActionAsync(async () =>
        {
            var result = await apiClient.PayBillAsync(
                new PayBillRequest { Category = selectedCategory, Amount = amount },
                cancellationToken);
            ui.ShowOperationResult(result);
        });
    }

    private async Task ShowPortfolioAsync(CancellationToken cancellationToken) =>
        await RunApiActionAsync(async () =>
        {
            var account = await apiClient.GetMyAccountAsync(cancellationToken);
            var balances = await apiClient.GetPortfolioAsync(cancellationToken);
            ui.ShowPortfolio(account.Balance, balances);
        });

    private async Task ShowTransactionsAsync(CancellationToken cancellationToken) =>
        await RunApiActionAsync(async () =>
            ui.ShowTransactions(await apiClient.GetTransactionsAsync(cancellationToken)));

    private async Task SetFrozenStatusAsync(bool freeze, CancellationToken cancellationToken)
    {
        var accountNumber = ui.Prompt("Account number: ");
        await RunApiActionAsync(async () =>
        {
            var result = freeze
                ? await apiClient.FreezeAccountAsync(accountNumber, cancellationToken)
                : await apiClient.UnfreezeAccountAsync(accountNumber, cancellationToken);
            ui.ShowOperationResult(result);
        });
    }

    private bool TryReadAmount(string prompt, out decimal amount)
    {
        if (ConsoleUi.TryReadPositiveAmount(ui.Prompt(prompt), out amount))
        {
            return true;
        }

        ui.ShowMessage("Enter a valid positive amount.", ConsoleColor.Red);
        return false;
    }

    private async Task RunApiActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (HttpRequestException exception)
        {
            ui.ShowMessage(exception.Message, ConsoleColor.Red);
        }
    }
}
