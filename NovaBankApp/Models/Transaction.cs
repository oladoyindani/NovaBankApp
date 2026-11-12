namespace NovaBankApp.Models;

public record Transaction(DateTimeOffset OccurredAt, string Description)
{
    public override string ToString() => $"[{OccurredAt:HH:mm:ss}] {Description}";
}
