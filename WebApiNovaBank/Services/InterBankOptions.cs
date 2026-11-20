namespace WebApiNovaBank.Services;

public sealed class InterBankOptions
{
    public const string SectionName = "InterBank";

    public string PartnerKey { get; init; } = string.Empty;
    public string[] AllowedBankUrls { get; init; } = [];
}
