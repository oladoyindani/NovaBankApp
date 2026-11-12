namespace WebApiNovaBank.Contracts;

/// <summary>An account summary returned to an authorized administrator.</summary>
public sealed record AdminAccountResponse(
    string AccountNumber,
    string CustomerName,
    string City,
    string PhoneNumber,
    decimal Balance,
    bool IsFrozen);
