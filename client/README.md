# CRAP Metrics for VS Code

A VS Code extension that shows Cyclomatic Complexity (CC) and CRAP Score directly above your C# methods using CodeLens — helping you quickly identify risky, untested code.

## What is CRAP Score?

**CRAP (Change Risk Anti-Patterns)** score combines cyclomatic complexity with code coverage to highlight methods that are both complex and insufficiently tested.

```
CRAP(m) = CC(m)² × (1 - coverage(m))³ + CC(m)
```
Higher scores indicate code that is harder to maintain and more likely to break when modified.

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
- 🎯 Quickly identify high-risk methods that need refactoring or tests

## Requirements

- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed and on PATH
- VS Code 1.85+

## Screenshot

![CRAP Metrics Example](./images/example.png)

![CRAP Metrics Example](./images/example1.png)

Shows CRAP score and Cyclomatic Complexity directly above C# methods.

## License

MIT
