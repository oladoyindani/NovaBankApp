namespace WebApiNovaBank.Services;

public sealed class AdminCredentialsOptions
{
    public const string SectionName = "AdminCredentials";

    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
