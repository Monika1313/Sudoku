﻿namespace Sudoku.Diagnostics.CodeAnalysis.Analyzers;

/// <summary>
/// Indicates the analyzer that checks for CRTP (Curiously Recursive Template Pattern) validity.
/// For C#, the syntax should be <c><![CDATA[class A<[Self] T> where T : A<T>]]></c>, where <c>[Self]</c>
/// is target to type <see cref="SelfAttribute"/>.
/// </summary>
/// <seealso cref="SelfAttribute"/>
[Generator(LanguageNames.CSharp)]
public sealed class ThisConstraintAnalyzer : ISourceGenerator
{
	/// <inheritdoc/>
	public void Execute(GeneratorExecutionContext context) =>
		((ThisConstraintSyntaxChecker)context.SyntaxContextReceiver!).Diagnostics.ForEach(context.ReportDiagnostic);

	/// <inheritdoc/>
	public void Initialize(GeneratorInitializationContext context) =>
		context.RegisterForSyntaxNotifications(() => new ThisConstraintSyntaxChecker(context.CancellationToken));
}
