using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CrapMetricsServer.Analysis;

namespace CrapMetricsServer.Handlers;

/// <summary>
/// Parses a C# source file and returns per-method CC and CRAP scores.
/// Registered as a singleton in DI so all LSP handlers share one instance.
/// </summary>
public class DocumentHandler
{
    private readonly ComplexityAnalyzer complexityAnalyzer = new();
    private readonly CrapCalculator crapCalculator = new();

    public List<(int line, int cc, double crap)> Analyze(string code)
    {
        var results = new List<(int, int, double)>();

        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetCompilationUnitRoot();
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

        Console.Error.WriteLine($"Analyzing {methods.Count} method(s)");

        var compilation = CSharpCompilation.Create("Analysis")
            .AddSyntaxTrees(tree)
            .AddReferences(
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location)
            );

        var model = compilation.GetSemanticModel(tree);

        foreach (var method in methods)
        {
            try
            {
                var cc = complexityAnalyzer.Compute(method, model);

                // Coverage defaults to 0 — worst-case CRAP score.
                // Future: integrate with coverlet XML output.
                var crap = crapCalculator.Calculate(cc, coverage: 0);

                var line = method.GetLocation().GetLineSpan().StartLinePosition.Line;

                Console.Error.WriteLine($"  {method.Identifier}: CC={cc}, CRAP={crap:F2}");

                results.Add((line, cc, crap));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  ERROR on {method.Identifier}: {ex.Message}");
            }
        }

        return results;
    }
}
