using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApiNovaBank.Contracts;

namespace NovaBank.ApiClient;

public sealed class BankApiClient : IDisposable
{
    private readonly HttpClient httpClient;

    public BankApiClient(string baseAddress)
    {
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var baseUri)
            || baseUri.Scheme is not ("http" or "https"))
        {
            throw new ArgumentException("The API base address must be an absolute HTTP or HTTPS URL.", nameof(baseAddress));
        }

        httpClient = new HttpClient
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public Uri BaseAddress => httpClient.BaseAddress!;

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken = default) =>
        await SendAsync<AccountResponse>(
            () => httpClient.PostAsJsonAsync("api/accounts", request, cancellationToken),
            cancellationToken);

    public async Task<AccessTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        await SendAsync<AccessTokenResponse>(
            () => httpClient.PostAsJsonAsync("api/auth/token", request, cancellationToken),
            cancellationToken);

    public void SetBearerToken(string token) =>
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public void ClearBearerToken() => httpClient.DefaultRequestHeaders.Authorization = null;

    public Task<AccountResponse> GetMyAccountAsync(CancellationToken cancellationToken = default) =>
        GetAsync<AccountResponse>("api/accounts/me", cancellationToken);

    public Task<OperationResponse> DepositAsync(AmountRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<AmountRequest, OperationResponse>("api/accounts/me/deposits", request, cancellationToken);

    public Task<OperationResponse> WithdrawAsync(AmountRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<AmountRequest, OperationResponse>("api/accounts/me/withdrawals", request, cancellationToken);

    public Task<OperationResponse> TransferWithinBankAsync(InternalTransferRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<InternalTransferRequest, OperationResponse>("api/accounts/me/transfers", request, cancellationToken);

    public Task<OperationResponse> TransferToBankAsync(ExternalBankTransferRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<ExternalBankTransferRequest, OperationResponse>("api/accounts/me/inter-bank-transfers", request, cancellationToken);

    public Task<IReadOnlyList<string>> GetBillCategoriesAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<string>>("api/accounts/me/bill-categories", cancellationToken);

    public Task<OperationResponse> PayBillAsync(PayBillRequest request, CancellationToken cancellationToken = default) =>
        PostAsync<PayBillRequest, OperationResponse>("api/accounts/me/bill-payments", request, cancellationToken);

    public Task<IReadOnlyList<TransactionResponse>> GetTransactionsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<TransactionResponse>>("api/accounts/me/transactions", cancellationToken);

    public Task<IReadOnlyList<CurrencyBalanceResponse>> GetPortfolioAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<CurrencyBalanceResponse>>("api/accounts/me/portfolio", cancellationToken);

    public Task<IReadOnlyList<AdminAccountResponse>> GetAccountsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<AdminAccountResponse>>("api/admin/accounts", cancellationToken);

    public Task<OperationResponse> FreezeAccountAsync(string accountNumber, CancellationToken cancellationToken = default) =>
        PutAsync($"api/admin/accounts/{Uri.EscapeDataString(accountNumber)}/freeze", cancellationToken);

    public Task<OperationResponse> UnfreezeAccountAsync(string accountNumber, CancellationToken cancellationToken = default) =>
        PutAsync($"api/admin/accounts/{Uri.EscapeDataString(accountNumber)}/unfreeze", cancellationToken);

    public Task<IReadOnlyList<AdminBillPaymentResponse>> GetProviderPaymentHistoryAsync(CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<AdminBillPaymentResponse>>("api/admin/provider-bill-payments", cancellationToken);

    public void Dispose() => httpClient.Dispose();

    private async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken) =>
        await SendAsync<TResponse>(() => httpClient.GetAsync(path, cancellationToken), cancellationToken);

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken) =>
        await SendAsync<TResponse>(
            () => httpClient.PostAsJsonAsync(path, request, cancellationToken),
            cancellationToken);

    private async Task<OperationResponse> PutAsync(string path, CancellationToken cancellationToken) =>
        await SendAsync<OperationResponse>(
            () => httpClient.PutAsync(path, content: null, cancellationToken),
            cancellationToken);

    private static async Task<TResponse> SendAsync<TResponse>(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        using var response = await send();
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"API request failed with {(int)response.StatusCode} ({response.StatusCode}): {body}",
                inner: null,
                response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("The API returned an empty or invalid response body.");
    }
}
