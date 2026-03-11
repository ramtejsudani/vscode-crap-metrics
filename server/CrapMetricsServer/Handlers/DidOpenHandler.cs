using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;

namespace CrapMetricsServer.LspHandlers;

public class DidOpenHandler : IDidOpenTextDocumentHandler
{
    // The client automatically requests CodeLens on open via textDocument/codeLens,
    // so no action is needed here. The handler must still be registered so the
    // server correctly advertises textDocument/didOpen support during initialization.
    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        Console.Error.WriteLine("File opened: " + request.TextDocument.Uri);
        return Task.FromResult(Unit.Value);
    }

    public TextDocumentOpenRegistrationOptions GetRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities) => new()
    {
        DocumentSelector = TextDocumentSelector.ForLanguage("csharp")
    };
}
