﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Sudoku.CodeGenerating.Extensions;

namespace Sudoku.CodeGenerating
{
	/// <summary>
	/// Indicates the generator that generates the default overriden methods in a <see langword="ref struct"/>.
	/// </summary>
	[Generator]
	public sealed partial class RefStructDefaultImplGenerator : ISourceGenerator
	{
		/// <inheritdoc/>
		public void Execute(GeneratorExecutionContext context)
		{
			var receiver = (SyntaxReceiver)context.SyntaxReceiver!;
			var compilation = context.Compilation;

			foreach (var typeGroup in
#pragma warning disable RS1024
				from type in receiver.Types
				let model = compilation.GetSemanticModel(type.SyntaxTree)
				select (INamedTypeSymbol)model.GetDeclaredSymbol(type)! into type
				group type by type.ContainingType is null
#pragma warning restore RS1024
			)
			{
				Action<GeneratorExecutionContext, INamedTypeSymbol, Compilation> f = typeGroup.Key ? Q : R;
				foreach (var type in typeGroup)
				{
					f(context, type, compilation);
				}
			}
		}

		/// <inheritdoc/>
		public void Initialize(GeneratorInitializationContext context) => context.FastRegister<SyntaxReceiver>();


		private void Q(GeneratorExecutionContext context, INamedTypeSymbol type, Compilation compilation)
		{
			type.DeconstructInfo(
				false, out _, out string namespaceName, out string genericParametersList,
				out _, out _, out string readonlyKeyword, out _
			);

			Func<ISymbol, ISymbol, bool> c = SymbolEqualityComparer.Default.Equals;
			var intSymbol = compilation.GetSpecialType(SpecialType.System_Int32);
			var boolSymbol = compilation.GetSpecialType(SpecialType.System_Boolean);
			var stringSymbol = compilation.GetSpecialType(SpecialType.System_String);
			var objectSymbol = compilation.GetSpecialType(SpecialType.System_Object);

			var methods = type.GetMembers().OfType<IMethodSymbol>().ToArray();
			string equalsMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "Equals", Parameters: { Length: not 0 } parameters }
					&& c(parameters[0].Type, objectSymbol)
					&& c(symbol.ReturnType, boolSymbol)
			)
				? @"// Can't generate 'Equals' because the method is impl'ed by user."
				: $@"/// <inheritdoc cref=""object.Equals(object?)""/>
		/// <exception cref=""NotSupportedException"">Always throws.</exception>
		[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
		[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
		[global::System.Runtime.CompilerServices.CompilerGenerated]
		public override {readonlyKeyword}bool Equals(object? other) => throw new NotSupportedException();";

			string getHashCodeMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "GetHashCode", Parameters: { Length: 0 } parameters }
					&& c(symbol.ReturnType, intSymbol)
			)
				? @"// Can't generate 'GetHashCode' because the method is impl'ed by user."
				: $@"/// <inheritdoc cref=""object.GetHashCode""/>
		/// <exception cref=""NotSupportedException"">Always throws.</exception>
		[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
		[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
		[global::System.Runtime.CompilerServices.CompilerGenerated]
		public override {readonlyKeyword}int GetHashCode() => throw new NotSupportedException();";

			string toStringMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "ToString", Parameters: { Length: 0 } parameters }
					&& c(symbol.ReturnType, stringSymbol)
			)
				? @"// Can't generate 'ToString' because the method is impl'ed by user."
				: $@"/// <inheritdoc cref=""object.ToString""/>
		/// <exception cref=""NotSupportedException"">Always throws.</exception>
		[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
		[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
		[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
		[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
		[global::System.Runtime.CompilerServices.CompilerGenerated]
		public override {readonlyKeyword}string? ToString() => throw new NotSupportedException();";

			context.AddSource(
				type.ToFileName(),
				"RefStructDefaults",
				$@"#pragma warning disable 809, IDE0005

using System;

#nullable enable

namespace {namespaceName}
{{
	partial struct {type.Name}{genericParametersList}
	{{
#line hidden
		{equalsMethod}

		{getHashCodeMethod}

		{toStringMethod}
#line default
	}}
}}"
			);
		}

		private void R(GeneratorExecutionContext context, INamedTypeSymbol type, Compilation compilation)
		{
			type.DeconstructInfo(
				false, out _, out string namespaceName, out string genericParametersList,
				out _, out _, out string readonlyKeyword, out _
			);

			// Get outer types.
			var outerTypes = new List<INamedTypeSymbol>();
			int outerTypesCount = 0;
			for (var outer = type.ContainingType; outer is not null; outerTypesCount++)
			{
				outerTypes.Add(outer);
				outer = outer.ContainingType;
			}

			string methodIndenting = new('\t', outerTypesCount + 2);
			string typeIndenting = new('\t', outerTypesCount + 1);
			StringBuilder outerTypeDeclarationsStart = new(), outerTypeDeclarationsEnd = new();
			foreach (var outerType in outerTypes)
			{
				outerType.DeconstructInfo(
					false, out _, out _, out string outerGenericParametersList,
					out _, out string outerTypeKind, out _, out _
				);

				string indenting = new('\t', outerTypesCount--);

				outerTypeDeclarationsStart
					.Append(indenting)
					.Append("partial ")
					.Append(outerTypeKind)
					.Append(outerType.Name)
					.AppendLine(outerGenericParametersList)
					.Append(indenting)
					.AppendLine("{");

				outerTypeDeclarationsEnd
					.Append(indenting)
					.AppendLine("}");
			}

			// Remove the last new line.
			outerTypeDeclarationsStart.Remove(outerTypeDeclarationsStart.Length - 2, 2);
			outerTypeDeclarationsEnd.Remove(outerTypeDeclarationsEnd.Length - 2, 2);

			Func<ISymbol, ISymbol, bool> c = SymbolEqualityComparer.Default.Equals;
			var intSymbol = compilation.GetSpecialType(SpecialType.System_Int32);
			var boolSymbol = compilation.GetSpecialType(SpecialType.System_Boolean);
			var stringSymbol = compilation.GetSpecialType(SpecialType.System_String);
			var objectSymbol = compilation.GetSpecialType(SpecialType.System_Object);

			var methods = type.GetMembers().OfType<IMethodSymbol>().ToArray();
			string equalsMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "Equals", Parameters: { Length: not 0 } parameters }
					&& c(parameters[0].Type, objectSymbol)
					&& c(symbol.ReturnType, boolSymbol)
			)
				? $"{methodIndenting}// Can't generate 'Equals' because the method is impl'ed by user."
				: $@"{methodIndenting}/// <inheritdoc cref=""object.Equals(object?)""/>
{methodIndenting}/// <exception cref=""NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readonlyKeyword}bool Equals(object? other) => throw new NotSupportedException();";

			string getHashCodeMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "GetHashCode", Parameters: { Length: 0 } parameters }
					&& c(symbol.ReturnType, intSymbol)
			)
				? $"{methodIndenting}// Can't generate 'GetHashCode' because the method is impl'ed by user."
				: $@"{methodIndenting}/// <inheritdoc cref=""object.GetHashCode""/>
{methodIndenting}/// <exception cref=""NotSupportedException"">Always throws.</exception>
{methodIndenting}/// <exception cref=""NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readonlyKeyword}int GetHashCode() => throw new NotSupportedException();";

			string toStringMethod = Array.Exists(
				methods,
				symbol =>
					symbol is { Name: "ToString", Parameters: { Length: 0 } parameters }
					&& c(symbol.ReturnType, stringSymbol)
			)
				? $"{methodIndenting}// Can't generate 'ToString' because the method is impl'ed by user."
				: $@"{methodIndenting}/// <inheritdoc cref=""object.ToString""/>
{methodIndenting}/// <exception cref=""NotSupportedException"">Always throws.</exception>
{methodIndenting}/// <exception cref=""NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""0.3"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readonlyKeyword}string? ToString() => throw new NotSupportedException();";

			context.AddSource(
				type.ToFileName(),
				"RefStructDefaults",
				$@"#pragma warning disable 809, IDE0005

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#nullable enable

namespace {namespaceName}
{{
{outerTypeDeclarationsStart}
{typeIndenting}partial struct {type.Name}{genericParametersList}
{typeIndenting}{{
#line hidden
{equalsMethod}

{getHashCodeMethod}

{toStringMethod}
#line default
{typeIndenting}}}
{outerTypeDeclarationsEnd}
}}"
			);
		}
	}
}
