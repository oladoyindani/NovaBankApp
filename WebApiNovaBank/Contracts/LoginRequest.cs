using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Credentials used to obtain an API access token.</summary>
public sealed record LoginRequest
{
    /// <summary>The customer name or configured administrator username.</summary>
    [Required, StringLength(100)]
    public required string UserName { get; init; }

    /// <summary>The account password.</summary>
    [Required, StringLength(128)]
    public required string Password { get; init; }
}
