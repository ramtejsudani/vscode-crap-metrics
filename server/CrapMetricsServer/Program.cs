using OmniSharp.Extensions.LanguageServer.Server;
using CrapMetricsServer.LspHandlers;
using CrapMetricsServer.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var logFile = Path.Combine(Path.GetTempPath(), "crap-metrics-server.log");
File.WriteAllText(logFile, $"Server starting at {DateTime.Now}{Environment.NewLine}");

try
{
    var server = await LanguageServer.From(options =>
    {
        options
            .WithInput(Console.OpenStandardInput())
            .WithOutput(Console.OpenStandardOutput())
            .WithHandler<DidOpenHandler>()
            .WithHandler<DidSaveHandler>()
            .WithHandler<CrapCodeLensHandler>()
            .WithHandler<MetricsHandler>()
            .WithServices(services =>
            {
                // Shared singleton so all handlers use the same analyzer instance
                services.AddSingleton<DocumentHandler>();
            })
            .ConfigureLogging(logging =>
            {
                // Clear all providers to prevent any logger accidentally writing to stdout,
                // which would corrupt the LSP JSON-RPC stream.
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
    });

    File.AppendAllText(logFile, $"Server started OK{Environment.NewLine}");
    Console.Error.WriteLine("CRAP Metrics Server started");

    await server.WaitForExit;
}
catch (Exception ex)
{
    var msg = $"CRASH: {ex}{Environment.NewLine}";
    File.AppendAllText(logFile, msg);
    Console.Error.WriteLine(msg);
    Environment.Exit(1);
}
