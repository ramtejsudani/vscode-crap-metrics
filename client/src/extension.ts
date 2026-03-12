import * as path from 'path'
import * as vscode from 'vscode'
import * as lc from 'vscode-languageclient/node'
import * as fs from 'fs'

let client: lc.LanguageClient

export async function activate(context: vscode.ExtensionContext) {

    const outputChannel = vscode.window.createOutputChannel('CRAP Metrics')
    outputChannel.appendLine(`Extension path: ${context.extensionPath}`)

    const serverDll = resolveServerPath(context, outputChannel)

    if (!serverDll) {
        outputChannel.appendLine('ERROR: Server DLL not found')
        outputChannel.show()

        vscode.window.showErrorMessage(
            'CRAP Metrics: Server DLL not found. Ensure .NET is installed.'
        )

        return
    }

    outputChannel.appendLine(`Using DLL: ${serverDll}`)

    const dotnetCommand = resolveDotnetCommand()

    const serverOptions: lc.ServerOptions = {
        command: dotnetCommand,
        args: [serverDll],
        options: {
            env: {
                ...process.env,
                DOTNET_NOLOGO: '1'
            }
        }
    }

    const clientOptions: lc.LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'csharp' }],
        synchronize: {
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.cs')
        },
        outputChannelName: 'CRAP Metrics'
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
    }
    catch (err) {

        outputChannel.appendLine(`ERROR starting server: ${err}`)
        outputChannel.show()

        vscode.window.showErrorMessage(
            `CRAP Metrics: Failed to start server. ${err}`
        )
    }
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) return undefined
    return client.stop()
}

function resolveServerPath(
    context: vscode.ExtensionContext,
    outputChannel: vscode.OutputChannel
): string | null {

    const candidates = [

        // packaged extension
        path.join(context.extensionPath, 'server', 'CrapMetricsServer.dll'),

        // dev debug
        path.join(
            context.extensionPath,
            '..',
            'server',
            'CrapMetricsServer',
            'bin',
            'Debug',
            'net8.0',
            'CrapMetricsServer.dll'
        ),

        // dev release
        path.join(
            context.extensionPath,
            '..',
            'server',
            'CrapMetricsServer',
            'bin',
            'Release',
            'net8.0',
            'CrapMetricsServer.dll'
        )
    ]

    for (const candidate of candidates) {

        const normalized = path.normalize(candidate)
        const exists = fs.existsSync(normalized)

        outputChannel.appendLine(` ${exists ? '✓' : '✗'} ${normalized}`)

        if (exists)
            return normalized
    }

    return null
}

function resolveDotnetCommand(): string {

    // Windows uses dotnet.exe
    if (process.platform === 'win32') {
        return 'dotnet'
    }

    // macOS / Linux
    return 'dotnet'
}