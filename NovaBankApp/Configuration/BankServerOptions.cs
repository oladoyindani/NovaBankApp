namespace NovaBankApp.Configuration;

public record BankServerOptions(int Port)
{
    public static BankServerOptions FromArguments(string[] arguments)
    {
        var portIndex = Array.IndexOf(arguments, "--port");
        return portIndex >= 0 && portIndex + 1 < arguments.Length && int.TryParse(arguments[portIndex + 1], out var port) && port is > 0 and <= 65535
            ? new BankServerOptions(port)
            : new BankServerOptions(5000);
    }
}
