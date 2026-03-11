using CrapMetricsServer.Handlers;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;

namespace CrapMetricsServer.LspHandlers;

public class CrapCodeLensHandler : CodeLensHandlerBase
{
    private readonly DocumentHandler analyzer;

    public CrapCodeLensHandler(DocumentHandler analyzer)
    {
        this.analyzer = analyzer;
    }

#pragma warning disable CS8609 // OmniSharp base class has inconsistent nullability annotation
    public override Task<CodeLensContainer> Handle(CodeLensParams request, CancellationToken cancellationToken)
#pragma warning restore CS8609
    {
        var filePath = request.TextDocument.Uri.GetFileSystemPath();

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return Task.FromResult(new CodeLensContainer())!;

        var text = File.ReadAllText(filePath);
        var results = analyzer.Analyze(text);

        var lenses = results.Select(r => new CodeLens
        {
            Range = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Range(
                new Position(r.line, 0),
                new Position(r.line, 0)
            ),
            Command = new Command
            {
                Title = FormatTitle(r.crap, r.cc),
                Name = ""
            }
        });

        return Task.FromResult(new CodeLensContainer(lenses))!;
    }

    // Resolve is called lazily if ResolveProvider=true.
    // We pre-populate Command above so just return unchanged.
    public override Task<CodeLens> Handle(CodeLens request, CancellationToken cancellationToken)
        => Task.FromResult(request);

    protected override CodeLensRegistrationOptions CreateRegistrationOptions(
        CodeLensCapability capability,
        ClientCapabilities clientCapabilities) => new()
    {
        DocumentSelector = TextDocumentSelector.ForLanguage("csharp"),
        ResolveProvider = false
    };

    private static string FormatTitle(double crap, int cc)
    {
        var icon = crap switch
        {
            <= 5  => "✅",
            <= 15 => "⚠️",
            <= 30 => "🔶",
            _     => "🔴"
        };
        return $"{icon} CRAP: {crap:F2} | CC: {cc}";
    }
}
