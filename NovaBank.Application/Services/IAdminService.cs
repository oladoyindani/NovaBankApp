using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IAdminService
{
    IReadOnlyList<AccountSummary> GetAccounts();
    OperationResult Freeze(string accountNumber);
    OperationResult Unfreeze(string accountNumber);
}
