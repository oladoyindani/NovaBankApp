namespace WebApiNovaBank.Services;

public sealed record AuthenticatedUser(string Subject, string UserName, string Role);
