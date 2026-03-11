using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace CrapMetricsServer.LspHandlers;

public class DidSaveHandler : IDidSaveTextDocumentHandler
{
    private readonly ILanguageServerFacade server;

    public DidSaveHandler(ILanguageServerFacade server)
    {
        this.server = server;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        Console.Error.WriteLine("File saved: " + request.TextDocument.Uri);

        // Notify the client to re-request all CodeLenses
        server.Workspace.SendCodeLensRefresh(new CodeLensRefreshParams());

        return Task.FromResult(Unit.Value);
    }

    public TextDocumentSaveRegistrationOptions GetRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities) => new()
    {
        DocumentSelector = TextDocumentSelector.ForLanguage("csharp"),
        IncludeText = false   // we read from disk, no need for text in the payload
    };
}
