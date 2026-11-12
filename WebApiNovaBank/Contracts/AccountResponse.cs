namespace WebApiNovaBank.Contracts;

/// <summary>Public account information; it never includes credential data.</summary>
public sealed record AccountResponse(
    string AccountNumber,
    string CustomerName,
    string City,
    string PhoneNumber,
    decimal Balance,
    bool IsFrozen);
