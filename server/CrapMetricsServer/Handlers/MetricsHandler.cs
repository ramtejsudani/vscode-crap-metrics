using System.Linq;
using CrapMetricsServer.Handlers;
using MediatR;
using OmniSharp.Extensions.JsonRpc;

namespace CrapMetricsServer.LspHandlers;

/// <summary>
/// Custom JSON-RPC handler for on-demand metric queries.
/// Kept for potential future use (e.g. hover tooltips, diagnostics).
/// Primary display is via CrapCodeLensHandler.
/// </summary>
[Method("crapMetrics/metrics")]
public class MetricsHandler : IJsonRpcRequestHandler<MetricsParams, MetricsResult>
{
    private readonly DocumentHandler analyzer;

    public MetricsHandler(DocumentHandler analyzer)
    {
        this.analyzer = analyzer;
    }

    public Task<MetricsResult> Handle(MetricsParams request, CancellationToken cancellationToken)
    {
        var results = analyzer.Analyze(request.Text);
        var match = results.FirstOrDefault(r => r.line == request.Line);

        return Task.FromResult(match == default
            ? new MetricsResult { cc = 0, crap = 0 }
            : new MetricsResult { cc = match.cc, crap = match.crap });
    }
}

public class MetricsParams : IRequest<MetricsResult>
{
    public string Text { get; set; } = "";
    public int Line { get; set; }
}

public class MetricsResult
{
    public int cc { get; set; }
    public double crap { get; set; }
}
