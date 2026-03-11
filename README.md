# CRAP Metrics for VS Code

A VS Code extension that displays **Cyclomatic Complexity (CC)** and **CRAP Score** inline for C# methods using CodeLens.

[![CI](https://github.com/YOUR_USERNAME/vscode-crap-metrics/actions/workflows/ci.yml/badge.svg)](https://github.com/YOUR_USERNAME/vscode-crap-metrics/actions/workflows/ci.yml)
[![Release](https://github.com/YOUR_USERNAME/vscode-crap-metrics/actions/workflows/release.yml/badge.svg)](https://github.com/YOUR_USERNAME/vscode-crap-metrics/actions/workflows/release.yml)
[![VS Code Marketplace](https://img.shields.io/visual-studio-marketplace/v/YOUR_PUBLISHER.vscode-crap-metrics)](https://marketplace.visualstudio.com/items?itemName=YOUR_PUBLISHER.vscode-crap-metrics)

## What is CRAP Score?

**CRAP (Change Risk Anti-Patterns)** score combines cyclomatic complexity with code coverage:

```
CRAP(m) = CC(m)² × (1 - coverage(m))³ + CC(m)
```

| CRAP Score | Meaning |
|---|---|
| ≤ 5 | Clean, well-tested code |
| 6–15 | Acceptable, could improve |
| 16–30 | Risky, add tests |
| > 30 | Very high risk |

> **Note on coverage:** Code coverage requires running your test suite with a coverage tool (e.g. `dotnet test --collect:"XPlat Code Coverage"`). Without coverage data, the extension defaults to 0% coverage, which produces the worst-case CRAP score. This is intentional — it highlights untested complexity.

## Features

- 🔍 Inline CodeLens showing `CRAP: X.XX | CC: Y` above every C# method
- ♻️ Refreshes automatically on file save
- ⚡ Powered by a local LSP server using Roslyn for accurate analysis

## Requirements

- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) installed and on PATH
- VS Code 1.85+

## Installation

Install from the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=YOUR_PUBLISHER.vscode-crap-metrics).

## Development Setup

```bash
git clone https://github.com/YOUR_USERNAME/vscode-crap-metrics
cd vscode-crap-metrics

# Build server
cd server/CrapMetricsServer
dotnet build

# Install client dependencies
cd ../../client
npm install
npm run compile
```

Then press `F5` in VS Code to launch the extension in debug mode.

## Project Structure

```
vscode-crap-metrics/
├── client/                  # VS Code extension (TypeScript)
│   ├── src/extension.ts
│   └── package.json
├── server/                  # LSP server (C# / .NET 10)
│   └── CrapMetricsServer/
│       ├── Analysis/        # CC and CRAP calculators
│       └── Handlers/        # LSP handlers
└── .github/workflows/       # CI/CD pipelines
```

## Contributing

PRs welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting.

## License

MIT
