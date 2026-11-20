using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Information required to open a NovaBank account.</summary>
public sealed record CreateAccountRequest
{
    /// <summary>The customer's full name, used as the login name.</summary>
    [Required, StringLength(100, MinimumLength = 2)]
    public required string CustomerName { get; init; }

    /// <summary>The customer's city.</summary>
    [Required, StringLength(100)]
    public required string City { get; init; }

    /// <summary>A phone number containing 10 to 15 digits.</summary>
    [Required, RegularExpression(@"^\d{10,15}$")]
    public required string PhoneNumber { get; init; }

    /// <summary>A password containing at least 12 characters.</summary>
    [Required, StringLength(128, MinimumLength = 12)]
    public required string Password { get; init; }
}
