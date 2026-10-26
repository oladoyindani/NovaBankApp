namespace NovaBankApp.Models;

/// <summary>Describes one timestamped account event shown in the console audit trail.</summary>
public record Transaction(DateTime OccurredAt, string Description)
{
    public override string ToString() => $"[{OccurredAt:HH:mm:ss}] {Description}";
}
