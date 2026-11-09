using System.Net;
using System.Text;
using System.Text.Json;
using NovaBankApp.Contracts;
using NovaBankApp.Models;
using NovaBankApp.Services;

namespace NovaBankApp.Network;

/// <summary>Hosts the HTTP endpoints for inter-bank transfers and bill-payment requests.</summary>
public class InterBankTransferServer : IDisposable
{
    // Web JSON clients commonly send camelCase property names, so accept them case-insensitively.
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly JsonSerializerOptions billPaymentResponseJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    private readonly HttpListener listener = new();
    private readonly BankingService bankingService;
    private readonly BillPaymentProviderService billPaymentProviderService;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>Builds the local transfer receiver around the shared banking rules.</summary>
    public InterBankTransferServer(BankingService bankingService, BillPaymentProviderService billPaymentProviderService)
    {
        this.bankingService = bankingService;
        this.billPaymentProviderService = billPaymentProviderService;
    }

    /// <summary>Starts listening in the background for POST requests to <c>/api/transfers</c>.</summary>
    public bool Start(int port, out string message)
    {
        try
        {
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            _ = Task.Run(ListenAsync);
            message = $"API listening at http://localhost:{port}/api/transfers, /api/bills/airtime, and /api/bills/data";
            return true;
        }
        catch (HttpListenerException exception)
        {
            message = $"Inter-bank receiver could not start: {exception.Message}";
            return false;
        }
    }

    /// <summary>Continuously accepts requests until the console application exits.</summary>
    private async Task ListenAsync()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            try { await ProcessRequestAsync(await listener.GetContextAsync()); }
            catch (HttpListenerException) when (cancellationTokenSource.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (cancellationTokenSource.IsCancellationRequested) { }
            // Log unexpected handler failures so a malformed request does not silently leave callers waiting.
            catch (Exception exception) { Console.Error.WriteLine($"[Inter-bank receiver error] {exception.Message}"); }
        }
    }

    /// <summary>Routes one HTTP request and sends a JSON acknowledgement to the sender.</summary>
    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath ?? string.Empty;

        if (context.Request.HttpMethod == "POST" && path == "/api/transfers")
        {
            await ProcessTransferAsync(context);
            return;
        }

        if (context.Request.HttpMethod == "POST" && path is "/api/bills/airtime" or "/api/bills/data")
        {
            var paymentType = path.EndsWith("airtime", StringComparison.Ordinal) ? BillPaymentType.Airtime : BillPaymentType.Data;
            await ProcessBillPaymentAsync(context, paymentType);
            return;
        }

        if (context.Request.HttpMethod == "GET" && path.StartsWith("/api/bills/payments/", StringComparison.Ordinal))
        {
            var transactionId = path["/api/bills/payments/".Length..];
            await ProcessBillPaymentStatusAsync(context, transactionId);
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        context.Response.Close();
    }

    private async Task ProcessTransferAsync(HttpListenerContext context)
    {
        var request = await JsonSerializer.DeserializeAsync<InterBankTransferRequest>(context.Request.InputStream, jsonOptions, cancellationTokenSource.Token);
        var result = request is null
            ? new InterBankTransferResponse(false, "Transfer request body is required.")
            : ToResponse(bankingService.ReceiveInterBankTransfer(request.RecipientAccountNumber, request.SenderName, request.Amount));

        await WriteJsonAsync(context, result, result.IsSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest);
    }

    private async Task ProcessBillPaymentAsync(HttpListenerContext context, BillPaymentType paymentType)
    {
        BillPaymentRequest? request;
        try
        {
            request = await JsonSerializer.DeserializeAsync<BillPaymentRequest>(context.Request.InputStream, jsonOptions, cancellationTokenSource.Token);
        }
        catch (JsonException)
        {
            var invalidRequest = new BillPaymentResponse(
                string.Empty,
                paymentType.ToString(),
                false,
                false,
                "Request body must contain valid JSON.",
                DateTime.UtcNow);
            await WriteJsonAsync(context, invalidRequest, HttpStatusCode.BadRequest, billPaymentResponseJsonOptions);
            return;
        }

        var result = request is null
            ? new BillPaymentResponse(string.Empty, paymentType.ToString(), false, false, "Bill payment request body is required.", DateTime.UtcNow)
            : billPaymentProviderService.Process(paymentType, request);
        var statusCode = result.IsDuplicate
            ? HttpStatusCode.Conflict
            : result.IsSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest;

        await WriteJsonAsync(context, result, statusCode, billPaymentResponseJsonOptions);
    }

    private async Task ProcessBillPaymentStatusAsync(HttpListenerContext context, string transactionId)
    {
        var result = billPaymentProviderService.GetStatus(transactionId);
        if (result is null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
            return;
        }

        await WriteJsonAsync(context, result, HttpStatusCode.OK, billPaymentResponseJsonOptions);
    }

    private async Task WriteJsonAsync(
        HttpListenerContext context,
        object value,
        HttpStatusCode statusCode,
        JsonSerializerOptions? serializerOptions = null)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, serializerOptions));
        await context.Response.OutputStream.WriteAsync(payload, cancellationTokenSource.Token);
        context.Response.Close();
    }

    /// <summary>Maps an internal operation result to the public HTTP response contract.</summary>
    private static InterBankTransferResponse ToResponse(Models.OperationResult result) => new(result.IsSuccessful, result.Message);

    /// <summary>Stops the listener and releases its local TCP port.</summary>
    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        listener.Close();
        cancellationTokenSource.Dispose();
    }
}
