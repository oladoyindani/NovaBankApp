namespace NovaBankApp.Models;

/// <summary>Contains non-sensitive account details for the admin account list.</summary>
public record AccountSummary(string AccountNumber, string CustomerName, string City, string PhoneNumber, decimal Balance, bool IsFrozen);
