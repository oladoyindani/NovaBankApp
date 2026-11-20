using NovaBankApp.Application;
using NovaBank.ApiClient;

var apiUrl = Environment.GetEnvironmentVariable("NOVABANK_API_URL")
    ?? args.FirstOrDefault(argument => argument.StartsWith("--api-url=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1]
    ?? "http://localhost:5150";

using var apiClient = new BankApiClient(apiUrl);
var application = new NovaBankApplication(apiClient);
await application.RunAsync();
