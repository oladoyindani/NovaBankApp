using System.Net;
using System.Text;
using System.Text.Json;
using NovaBankApp.Contracts;
using NovaBankApp.Services;

namespace NovaBankApp.Network;

public class InterBankTransferServer : IDisposable
{
    private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpListener listener = new();
    private readonly BankingService bankingService;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public InterBankTransferServer(BankingService bankingService) => this.bankingService = bankingService;

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

    private async Task ListenAsync()
    {
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            try { await ProcessRequestAsync(await listener.GetContextAsync()); }
            catch (HttpListenerException) when (cancellationTokenSource.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (cancellationTokenSource.IsCancellationRequested) { }
            catch (Exception exception) { Console.Error.WriteLine($"[Inter-bank receiver error] {exception.Message}"); }
        }
    }

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

    private static InterBankTransferResponse ToResponse(Models.OperationResult result) => new(result.IsSuccessful, result.Message);

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        listener.Close();
        cancellationTokenSource.Dispose();
    }
}
