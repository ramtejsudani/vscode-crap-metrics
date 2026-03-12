# How CRAP Metrics Calculates CC and CRAP Score

This document explains the algorithms used to compute **Cyclomatic Complexity (CC)** and **CRAP Score** in this extension.

---

## Cyclomatic Complexity (CC)

### What It Is

Cyclomatic Complexity is a measure of the number of independent execution paths through a method. It was introduced by Thomas J. McCabe in 1976.

A method with no branches has CC = 1. Every decision point adds 1.

### Formula

```
CC = E - N + 2P
```

Where:
- `E` = number of edges in the control flow graph
- `N` = number of nodes in the control flow graph
- `P` = number of connected components (always 1 for a single method)

### How This Extension Calculates It

The extension uses **Roslyn's `ControlFlowGraph`** as the primary source, then counts additional decision points that the CFG surfaces as branches.

#### Step 1 — Build the Control Flow Graph

```csharp
var methodOp = model.GetOperation(method) as IMethodBodyOperation;
var cfg = ControlFlowGraph.Create(methodOp);
```

Roslyn builds a graph of **basic blocks** — straight-line sequences of code with no branches. Every block that has a `BranchValue` (a conditional exit) adds 1 to complexity:

```csharp
foreach (var block in cfg.Blocks)
{
    if (block.BranchValue != null)
        complexity++;
}
```

#### Step 2 — Count Additional Decision Points

The CFG alone does not always capture every logical branch as a separate block. The following are also counted explicitly:

| Construct | Example | Why Counted |
|---|---|---|
| `if` statement | `if (x > 0)` | Core decision point |
| `for` loop | `for (int i = 0; ...)` | Loop condition is a branch |
| `foreach` loop | `foreach (var x in list)` | Iterator has a continue/exit branch |
| `while` loop | `while (running)` | Condition evaluated each iteration |
| `do-while` loop | `do { } while (x)` | Condition at end is still a branch |
| Logical AND `&&` | `if (a && b)` | Short-circuit creates extra path |
| Logical OR `\|\|` | `if (a \|\| b)` | Short-circuit creates extra path |
| Ternary `?:` | `x > 0 ? a : b` | Two possible values |
| Null-coalescing `??` | `x ?? defaultValue` | Two possible paths |

```csharp
complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
complexity += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
complexity += method.DescendantNodes().OfType<DoStatementSyntax>().Count();
complexity += method.DescendantNodes()
    .OfType<BinaryExpressionSyntax>()
    .Count(x => x.IsKind(SyntaxKind.LogicalAndExpression)
             || x.IsKind(SyntaxKind.LogicalOrExpression));
complexity += method.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
complexity += method.DescendantNodes()
    .OfType<BinaryExpressionSyntax>()
    .Count(x => x.IsKind(SyntaxKind.CoalesceExpression));
```

### Example

```csharp
public string Classify(int score)          // CC starts at 1
{
    if (score >= 90)                       // +1 → CC = 2
        return "A";
    else if (score >= 80)                  // +1 → CC = 3
        return "B";
    else if (score >= 70)                  // +1 → CC = 4
        return "C";
    else
        return "F";
}
// Final CC = 4
```

```csharp
public bool IsEligible(User user)         // CC starts at 1
{
    return user != null                   // +1 (&&)
        && user.IsActive                  // +1 (&&)
        && user.Age >= 18;
}
// Final CC = 3
```

### CC Interpretation

| CC | Risk |
|---|---|
| 1–5 | Low — simple, easy to test |
| 6–10 | Moderate — manageable |
| 11–20 | High — hard to test thoroughly |
| > 20 | Very high — consider refactoring |

---

## CRAP Score

### What It Is

CRAP (Change Risk Anti-Patterns) was introduced by Alberto Savoia and Bob Evans. It combines cyclomatic complexity with test coverage to produce a single risk score.

The key insight: **complexity alone is not the problem**. Complex code that is thoroughly tested is manageable. Complex code with no tests is dangerous.

### Formula

```
CRAP(m) = CC(m)² × (1 - coverage(m))³ + CC(m)
```

Where:
- `CC(m)` = cyclomatic complexity of the method
- `coverage(m)` = test coverage as a decimal (0.0 = 0%, 1.0 = 100%)

### Implementation

```csharp
public double Calculate(int complexity, double coverage)
{
    var coverageFactor = 1.0 - (coverage / 100.0);

    return Math.Pow(complexity, 2) *
           Math.Pow(coverageFactor, 3)
           + complexity;
}
```

### Understanding the Formula

The formula has two parts:

```
CRAP = CC² × (1 - coverage)³   +   CC
       ───────────────────────      ──
       risk from lack of coverage   base complexity cost
```

**Part 1 — Risk from lack of coverage:**
- Grows with the *square* of complexity — doubling CC quadruples this term
- Shrinks rapidly as coverage increases — at 100% coverage this term becomes 0
- The cubic exponent on `(1 - coverage)` means coverage has a very strong effect

**Part 2 — Base complexity cost:**
- Always present regardless of coverage
- A perfectly tested method still has a CRAP score equal to its CC

### Score Tables

**Worst case (0% coverage):**

| CC | CRAP | Risk |
|---|---|---|
| 1 | 2 | ✅ Minimal |
| 2 | 6 | ✅ Low |
| 3 | 12 | ⚠️ Moderate |
| 5 | 30 | ⚠️ High |
| 10 | 110 | 🔶 Very High |
| 15 | 240 | 🔴 Extreme |
| 20 | 420 | 🔴 Extreme |

**Effect of coverage on a CC=10 method:**

| Coverage | CRAP | Change |
|---|---|---|
| 0% | 110.00 | Worst case |
| 25% | 52.19 | -53% |
| 50% | 22.50 | -80% |
| 75% | 13.13 | -88% |
| 90% | 10.10 | -91% |
| 100% | 10.00 | Best case — equals CC |

Notice that at 100% coverage, CRAP always equals CC. This is the mathematical floor — even perfect tests cannot reduce CRAP below the method's inherent complexity.

### CRAP Score Thresholds

| CRAP | Indicator | Meaning |
|---|---|---|
| ≤ 5 | ✅ | Clean — low complexity or well tested |
| 6–15 | ⚠️ | Acceptable — monitor and improve over time |
| 16–30 | 🔶 | Risky — add tests or reduce complexity |
| > 30 | 🔴 | Very high risk — refactor and/or add tests urgently |

---

## Current Limitation: Coverage Defaults to 0%

**This extension currently assumes 0% coverage for all methods.**

All displayed scores are worst-case values. This is intentional — it highlights methods where complexity is high and test coverage is unknown or absent.

To benefit from real CRAP scores, you would need to run your test suite with a coverage tool:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

This produces a `coverage.cobertura.xml` file. Integration to read this file and pass real coverage values to the calculator is planned for a future release.

Until then, use the displayed scores as a guide to which methods are most in need of either tests or simplification.

---

## Why Both Metrics?

CC and CRAP are complementary:

- **CC alone** tells you about complexity but not about risk — a complex, well-tested method is fine
- **CRAP alone** without knowing CC doesn't tell you *where* the risk comes from
- **Together** they give you a complete picture: CC tells you *how complex*, CRAP tells you *how risky*

A method with CC=15 and CRAP=240 is a priority for both refactoring *and* testing.
A method with CC=15 and CRAP=15 (at 100% coverage) is complex but under control.

---

## References

- McCabe, T.J. (1976). *A Complexity Measure*. IEEE Transactions on Software Engineering
- Savoia, A. & Evans, B. (2007). *CRAP4J* — original CRAP metric implementation
- [Roslyn ControlFlowGraph API](https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.flowanalysis.controlflowgraph)