using System.Net.Http.Json;
using NovaBankApp.Contracts;

namespace NovaBankApp.Network;

/// <summary>Delivers inter-bank transfers to another locally running NovaBank HTTP listener.</summary>
public class HttpInterBankTransferClient : IInterBankTransferClient
{
    private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    /// <summary>Posts a transfer request and turns HTTP failures into customer-friendly responses.</summary>
    public async Task<InterBankTransferResponse> SendAsync(string bankUrl, InterBankTransferRequest request, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(bankUrl, UriKind.Absolute, out var bankUri)) return new InterBankTransferResponse(false, "Enter a valid bank URL, for example http://localhost:5001.");
        var endpoint = new Uri(bankUri, "/api/transfers");

        try
        {
            using var response = await httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
            var transferResponse = await response.Content.ReadFromJsonAsync<InterBankTransferResponse>(cancellationToken: cancellationToken);
            return transferResponse ?? new InterBankTransferResponse(false, "The receiving bank returned an invalid response.");
        }
        catch (HttpRequestException)
        {
            return new InterBankTransferResponse(false, "Could not contact the receiving bank. Confirm its URL and that it is running.");
        }
        catch (TaskCanceledException)
        {
            return new InterBankTransferResponse(false, "The receiving bank did not respond in time.");
        }
    }
}
