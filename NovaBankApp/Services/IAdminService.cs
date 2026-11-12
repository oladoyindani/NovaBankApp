using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IAdminService
{
    bool IsAdmin(string userName, string password);
    IReadOnlyList<AccountSummary> GetAccounts();
    OperationResult Freeze(string accountNumber);
    OperationResult Unfreeze(string accountNumber);
}
