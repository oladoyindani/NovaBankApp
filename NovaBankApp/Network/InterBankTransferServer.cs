using System.Net;
using System.Text;
using System.Text.Json;
using NovaBankApp.Contracts;
using NovaBankApp.Services;

namespace NovaBankApp.Network;

/// <summary>Hosts the minimal HTTP endpoint that accepts funds from another NovaBank instance.</summary>
public class InterBankTransferServer : IDisposable
{
    // Web JSON clients commonly send camelCase property names, so accept them case-insensitively.
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpListener listener = new();
    private readonly BankingService bankingService;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>Builds the local transfer receiver around the shared banking rules.</summary>
    public InterBankTransferServer(BankingService bankingService) => this.bankingService = bankingService;

    /// <summary>Starts listening in the background for POST requests to <c>/api/transfers</c>.</summary>
    public bool Start(int port, out string message)
    {
        try
        {
            listener.Prefixes.Add($"http://localhost:{port}/");
            listener.Start();
            _ = Task.Run(ListenAsync);
            message = $"Inter-bank receiver listening at http://localhost:{port}/api/transfers";
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
        if (context.Request.HttpMethod != "POST" || context.Request.Url?.AbsolutePath != "/api/transfers")
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
            return;
        }

        var request = await JsonSerializer.DeserializeAsync<InterBankTransferRequest>(context.Request.InputStream, jsonOptions, cancellationTokenSource.Token);
        var result = request is null
            ? new InterBankTransferResponse(false, "Transfer request body is required.")
            : ToResponse(bankingService.ReceiveInterBankTransfer(request.RecipientAccountNumber, request.SenderName, request.Amount));

        context.Response.StatusCode = result.IsSuccessful ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result));
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
