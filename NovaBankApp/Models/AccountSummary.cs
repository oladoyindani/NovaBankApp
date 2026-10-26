namespace NovaBankApp.Models;

public record AccountSummary(string AccountNumber, string CustomerName, string City, string PhoneNumber, decimal Balance, bool IsFrozen);
