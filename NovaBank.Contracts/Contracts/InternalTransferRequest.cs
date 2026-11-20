using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Details for transferring money to another NovaBank account.</summary>
public sealed record InternalTransferRequest
{
    /// <summary>The destination NovaBank account number.</summary>
    [Required, RegularExpression(@"^\d{10}$")]
    public required string RecipientAccountNumber { get; init; }

    /// <summary>The amount to transfer.</summary>
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }
}
