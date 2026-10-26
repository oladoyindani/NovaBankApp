namespace NovaBankApp.Contracts;

/// <summary>Abstracts HTTP delivery so the transfer workflow does not depend directly on HttpClient.</summary>
public interface IInterBankTransferClient
{
    /// <summary>Sends an inter-bank transfer request to another running bank instance.</summary>
    Task<InterBankTransferResponse> SendAsync(string bankUrl, InterBankTransferRequest request, CancellationToken cancellationToken = default);
}
