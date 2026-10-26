namespace NovaBankApp.Models;

/// <summary>Provides a success flag and a display-ready message for a banking operation.</summary>
public record OperationResult(bool IsSuccessful, string Message)
{
    public static OperationResult Success(string message) => new(true, message);
    public static OperationResult Failure(string message) => new(false, message);
}
