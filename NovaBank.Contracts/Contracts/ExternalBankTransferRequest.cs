using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Details for transferring money to a configured partner bank.</summary>
public sealed record ExternalBankTransferRequest
{
    /// <summary>A client-generated identifier used to make retries safe.</summary>
    [Required]
    public Guid TransferId { get; init; }

    /// <summary>The destination partner's API base URL.</summary>
    [Required, Url, StringLength(300)]
    public required string BankUrl { get; init; }

    /// <summary>The destination account number.</summary>
    [Required, StringLength(32, MinimumLength = 1)]
    public required string RecipientAccountNumber { get; init; }

    /// <summary>The amount to transfer.</summary>
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }
}
