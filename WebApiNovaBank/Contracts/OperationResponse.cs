namespace WebApiNovaBank.Contracts;

/// <summary>The outcome of a banking operation.</summary>
public sealed record OperationResponse(bool IsSuccessful, string Message);
