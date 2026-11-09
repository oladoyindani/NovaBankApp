namespace NovaBankApp.Models;

public record Transaction(DateTime OccurredAt, string Description)
{
    public override string ToString() => $"[{OccurredAt:HH:mm:ss}] {Description}";
}
