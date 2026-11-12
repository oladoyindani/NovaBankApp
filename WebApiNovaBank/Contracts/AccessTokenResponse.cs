namespace WebApiNovaBank.Contracts;

/// <summary>An API bearer token and its expiration time.</summary>
public sealed record AccessTokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt);
