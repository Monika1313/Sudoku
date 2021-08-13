﻿namespace Sudoku.Diagnostics.CodeAnalysis.Analyzers;

[CodeAnalyzer("SS0702")]
public sealed partial class NullCoalescingAnalyzer : DiagnosticAnalyzer
{
	/// <inheritdoc/>
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, new[] { SyntaxKind.ConditionalExpression });
	}


	private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
	{
		var (semanticModel, _, originalNode) = context;

		// Here we only check for conditional expression that uses the operator '? :'.
		// For the special if statement can be detected by the .NET basic code analyzers,
		// we don't analyze on this case.
		if (
			originalNode is not ConditionalExpressionSyntax
			{
				Condition: var condition and (BinaryExpressionSyntax or IsPatternExpressionSyntax),
				WhenTrue: var whenTrueExpr,
				WhenFalse: var whenFalseExpr
			}
		)
		{
			return;
		}

		switch (condition)
		{
			case BinaryExpressionSyntax
			{
				RawKind: (int)SyntaxKind.EqualsExpression,
				Left: var leftExpr,
				Right: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression }
			}:
			{
				innerCheck(semanticModel, leftExpr);

				break;
			}
			case IsPatternExpressionSyntax
			{
				Expression: var leftExpr,
				Pattern: ConstantPatternSyntax { Expression.RawKind: (int)SyntaxKind.NullLiteralExpression }
			}:
			{
				innerCheck(semanticModel, leftExpr);

				break;
			}
		}


		void innerCheck(SemanticModel semanticModel, ExpressionSyntax leftExpr)
		{
			// ↓ leftExpr      ↓ whenFalseExpr
			// a is null ? b : a
			//             ↑ whenTrueExpr
			if (
				semanticModel.GetOperation(leftExpr) is not (
					(FRef or LRef or PRef) and { Type: (_, _, isNullable: true) leftExprType } referenceOperation
				)
			)
			{
				return;
			}

			if (
				semanticModel.GetOperation(whenFalseExpr) is not (
					var operandReferenceOperation and (FRef or LRef or PRef)
				)
			)
			{
				return;
			}

			switch ((OperandReference: operandReferenceOperation, Reference: referenceOperation))
			{
				case (OperandReference: FRef fr1, Reference: FRef fr2) when fr1.SameReferenceWith(fr2):
				case (OperandReference: PRef pr1, Reference: PRef pr2) when pr1.SameReferenceWith(pr2):
				case (OperandReference: LRef lr1, Reference: LRef lr2) when lr1.SameReferenceWith(lr2):
				{
					break;
				}
				default:
				{
					return;
				}
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: SS0702,
					location: originalNode.GetLocation(),
					messageArgs: new[] { whenFalseExpr.ToString(), whenTrueExpr.ToString() },
					additionalLocations: new[] { whenFalseExpr.GetLocation(), whenTrueExpr.GetLocation() }
				)
			);
		}
	}
}
