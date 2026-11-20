namespace NovaBankApp.Contracts;

public interface IInterBankTransferClient
{
    Task<InterBankTransferResponse> SendAsync(string bankUrl, InterBankTransferRequest request, CancellationToken cancellationToken = default);
}
