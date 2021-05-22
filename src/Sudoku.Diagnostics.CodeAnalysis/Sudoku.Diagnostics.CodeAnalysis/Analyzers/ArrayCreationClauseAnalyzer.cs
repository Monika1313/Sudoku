﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Sudoku.Diagnostics.CodeAnalysis.Analyzers
{
	/// <summary>
	/// Indicates an analyzer that analyzes the code for array creation clause.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class ArrayCreationClauseAnalyzer : DiagnosticAnalyzer
	{
		/// <inheritdoc/>
		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, new[] { SyntaxKind.VariableDeclaration });
		}


		private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			/*length-pattern*/
			if (
				context.Node is not VariableDeclarationSyntax
				{
					Type: ArrayTypeSyntax { RankSpecifiers: { Count: 1 } },
					Variables: { Count: not 0 } variables
				} node
			)
			{
				return;
			}

			foreach (var variable in variables)
			{
				if (
					variable is not
					{
						Initializer:
						{
							Value: ArrayCreationExpressionSyntax
							{
								Type: var type,
								Initializer: { }
							}
						}
					}
				)
				{
					continue;
				}

				context.ReportDiagnostic(
					Diagnostic.Create(
						descriptor: SS9002,
						location: type.GetLocation(),
						messageArgs: null
					)
				);
			}
		}
	}
}
