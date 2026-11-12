using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>A positive amount of money to deposit or withdraw.</summary>
public sealed record AmountRequest
{
    /// <summary>The amount in the account's currency.</summary>
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }
}
