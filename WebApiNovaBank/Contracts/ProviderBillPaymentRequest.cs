using System.ComponentModel.DataAnnotations;

namespace WebApiNovaBank.Contracts;

/// <summary>Details for an airtime or data-provider payment.</summary>
public sealed record ProviderBillPaymentRequest
{
    /// <summary>A client-generated GUID used to identify and deduplicate the payment.</summary>
    [Required, RegularExpression(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$")]
    public required string TransactionId { get; init; }

    /// <summary>The recipient phone number.</summary>
    [Required, RegularExpression(@"^\d{10,15}$")]
    public required string PhoneNumber { get; init; }

    /// <summary>The payment amount.</summary>
    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; init; }
}
