using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Details for paying a supported bill from the authenticated account.</summary>
public sealed record PayBillRequest
{
    /// <summary>The exact name of a supported bill category.</summary>
    [Required, StringLength(80)]
    public required string Category { get; init; }

    /// <summary>The amount to pay.</summary>
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }
}
