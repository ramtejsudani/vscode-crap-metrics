namespace CrapMetricsServer.Analysis;

/// <summary>
/// Calculates the CRAP (Change Risk Anti-Patterns) score.
///
/// Formula: CRAP(m) = CC(m)² × (1 - coverage(m))³ + CC(m)
///
/// The score combines cyclomatic complexity with test coverage:
/// - High complexity + low coverage = very high CRAP score
/// - High complexity + high coverage = acceptable CRAP score
/// - Low complexity always yields a low CRAP score regardless of coverage
///
/// Interpretation:
///   ≤  5  — Clean, well-tested
///   6–15  — Acceptable, consider improving
///  16–30  — Risky, add tests
///   > 30  — Very high risk, refactor and test
/// </summary>
public class CrapCalculator
{
    /// <param name="complexity">Cyclomatic complexity of the method (CC ≥ 1)</param>
    /// <param name="coverage">Test coverage as a percentage (0–100)</param>
    public double Calculate(int complexity, double coverage)
    {
        var coverageFactor = 1.0 - (coverage / 100.0);

        return Math.Pow(complexity, 2) *
               Math.Pow(coverageFactor, 3)
               + complexity;
    }
}
