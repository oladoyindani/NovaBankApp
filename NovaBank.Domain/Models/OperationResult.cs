namespace NovaBankApp.Models;

public record OperationResult(bool IsSuccessful, string Message)
{
    public static OperationResult Success(string message) => new(true, message);
    public static OperationResult Failure(string message) => new(false, message);
}
