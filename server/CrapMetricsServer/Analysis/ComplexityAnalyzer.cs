using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace CrapMetricsServer.Analysis;

/// <summary>
/// Calculates Cyclomatic Complexity (CC) for a method using Roslyn's
/// ControlFlowGraph as the primary source, supplemented by explicit
/// decision-point counting for cases the CFG doesn't capture alone.
///
/// Formula: CC = E - N + 2P
///   E = edges, N = nodes, P = connected components (1 per method)
/// </summary>
public class ComplexityAnalyzer
{
    public int Compute(MethodDeclarationSyntax method, SemanticModel model)
    {
        int complexity = 1;

        // Primary: use ControlFlowGraph for accurate branch counting
        var methodOp = model.GetOperation(method) as IMethodBodyOperation;
        if (methodOp != null)
        {
            var cfg = ControlFlowGraph.Create(methodOp);
            foreach (var block in cfg.Blocks)
            {
                if (block.BranchValue != null)
                    complexity++;
            }
        }

        // Supplement: count decision points not always captured by CFG blocks
        var descendants = method.DescendantNodes().ToList();

        // Logical operators create additional execution paths
        complexity += descendants
            .OfType<BinaryExpressionSyntax>()
            .Count(x =>
                x.IsKind(SyntaxKind.LogicalAndExpression) ||
                x.IsKind(SyntaxKind.LogicalOrExpression));

        // Null-coalescing is a decision point
        complexity += descendants
            .OfType<BinaryExpressionSyntax>()
            .Count(x => x.IsKind(SyntaxKind.CoalesceExpression));

        // Ternary operators
        complexity += descendants.OfType<ConditionalExpressionSyntax>().Count();

        // if statements
        complexity += descendants.OfType<IfStatementSyntax>().Count();

        // Loops
        complexity += descendants.OfType<ForStatementSyntax>().Count();
        complexity += descendants.OfType<ForEachStatementSyntax>().Count();
        complexity += descendants.OfType<WhileStatementSyntax>().Count();
        complexity += descendants.OfType<DoStatementSyntax>().Count();

        return complexity;
    }
}
