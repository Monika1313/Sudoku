﻿using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sudoku.Diagnostics.CodeAnalysis.Analyzers
{
	/// <summary>
	/// Indicates an analyzer that analyzes the code for the empty-judged positional pattern.
	/// The pattern is like <c>(<see langword="_"/>, <see langword="_"/>)</c>.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class DiscardedPositionalPatternAnalyzer : DiagnosticAnalyzer
	{
		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, new[] { SyntaxKind.PositionalPatternClause });
		}


		private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			if (
				context.Node is not PositionalPatternClauseSyntax
				{
					Subpatterns: { Count: >= 2 } subpatterns
				} node
			)
			{
				return;
			}

			/*slice-pattern*/
			if (subpatterns.Any(static subpattern => subpattern.Pattern is not DiscardPatternSyntax))
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor: SS0610,
					location: node.GetLocation(),
					messageArgs: new[] { node.ToString() }
				)
			);
		}
	}
}
