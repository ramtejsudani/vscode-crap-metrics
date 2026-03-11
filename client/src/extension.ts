import * as path from 'path'
import * as vscode from 'vscode'
import * as lc from 'vscode-languageclient/node'
import * as fs from 'fs'

let client: lc.LanguageClient

export async function activate(context: vscode.ExtensionContext) {

    // Diagnostic output channel — visible in Output panel (Ctrl+Shift+U)
    const outputChannel = vscode.window.createOutputChannel('CRAP Metrics')
    outputChannel.appendLine(`Extension path: ${context.extensionPath}`)

    const serverDll = resolveServerPath(context, outputChannel)

    if (!serverDll) {
        outputChannel.appendLine('ERROR: Server DLL not found in any candidate path')
        outputChannel.show()
        vscode.window.showErrorMessage(
            'CRAP Metrics: Server DLL not found. ' +
            'Make sure .NET 10 is installed. ' +
            'See the Output panel (CRAP Metrics) for the paths checked.'
        )
        return
    }

    outputChannel.appendLine(`Using DLL: ${serverDll}`)

    const serverOptions: lc.ServerOptions = {
        command: 'dotnet',
        args: [serverDll],
        options: {
            // Prevent dotnet from writing the startup banner to stdout,
            // which would corrupt the LSP JSON-RPC stream
            env: { ...process.env, DOTNET_NOLOGO: '1' }
        }
    }

    const clientOptions: lc.LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'csharp' }],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.cs')
        },
        // Surfaces server stderr into the Output panel for easy debugging
        outputChannelName: 'CRAP Metrics Server'
    }

    client = new lc.LanguageClient(
        'crapMetricsServer',
        'CRAP Metrics Server',
        serverOptions,
        clientOptions
    )

    try {
        await client.start()
        context.subscriptions.push(client)
        outputChannel.appendLine('Server started successfully')
    } catch (err) {
        outputChannel.appendLine(`ERROR starting server: ${err}`)
        outputChannel.show()
        vscode.window.showErrorMessage(
            `CRAP Metrics: Failed to start server. Check the Output panel for details. Error: ${err}`
        )
        return
    }
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) return undefined
    return client.stop()
}

/**
 * Resolves the path to CrapMetricsServer.dll.
 * Checks multiple candidate paths in order:
 * 1. Packaged .vsix install — server/ is bundled inside the extension folder
 * 2. Local dev Debug build — server lives next to the client folder
 * 3. Local dev Release build — same but Release configuration
 *
 * Returns null if no candidate exists, and logs each checked path.
 */
function resolveServerPath(
    context: vscode.ExtensionContext,
    outputChannel: vscode.OutputChannel
): string | null {
    const candidates = [
        // Packaged install
        path.join(context.extensionPath, 'server', 'CrapMetricsServer.dll'),
        // Local development — Debug
        path.join(context.extensionPath, '..', 'server', 'CrapMetricsServer',
            'bin', 'Debug', 'net10.0', 'CrapMetricsServer.dll'),
        // Local development — Release
        path.join(context.extensionPath, '..', 'server', 'CrapMetricsServer',
            'bin', 'Release', 'net10.0', 'CrapMetricsServer.dll'),
    ]

    for (const candidate of candidates) {
        const exists = fs.existsSync(candidate)
        outputChannel.appendLine(`  ${exists ? '✓' : '✗'} ${candidate}`)
        if (exists) return candidate
    }

    return null
}