﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Indicates the generator that generates the default overriden methods in a <see langword="ref struct"/>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class RefStructOverridensGenerator : ISourceGenerator
{
	/// <inheritdoc/>
	public void Execute(GeneratorExecutionContext context)
	{
		if (context is not { SyntaxContextReceiver: Receiver { Collection: var tuples }, Compilation: var compilation })
		{
			return;
		}

		var onTopLevel = OnTopLevel;
		var onNestedLevel = OnNested;
		(
			from tuple in tuples
			group tuple by tuple.Symbol.ContainingType is null into tupleGroupedByIsNested
			from typeTuple in tupleGroupedByIsNested
			select (Action: tupleGroupedByIsNested.Key ? onTopLevel : onNestedLevel, TypeTuple: typeTuple)
		).ForEach(e => e.Action(context, e.TypeTuple, compilation));
	}

	/// <inheritdoc/>
	public void Initialize(GeneratorInitializationContext context)
		=> context.RegisterForSyntaxNotifications(() => new Receiver(context.CancellationToken));

	/// <summary>
	/// Generates for top-levelled <see langword="ref struct"/> types.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="tuple">The tuple.</param>
	/// <param name="compilation">The compilation instance.</param>
	private void OnTopLevel(
		GeneratorExecutionContext context, (INamedTypeSymbol Type, bool GenerateGetHashCode, bool GenerateToString) tuple,
		Compilation compilation)
	{
		var type = tuple.Type;

		var (_, _, namespaceName, genericParameterList, _, _, readOnlyKeyword, _, _, _) =
			SymbolOutputInfo.FromSymbol(type);

		var methods = type.GetMembers().OfType<IMethodSymbol>().ToArray();
		string equalsMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(Equals),
				Parameters: [{ Type.SpecialType: SpecialType.System_Object }],
				ReturnType.SpecialType: SpecialType.System_Boolean
			}
		)
			? $@"// Can't generate '{nameof(Equals)}' because the method is impl'ed by user."
			: $@"/// <inheritdoc cref=""object.Equals(object?)""/>
	/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
	[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER || (NETCOREAPP3_0 || NETCOREAPP3_1)
	[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
#endif
	[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
	[global::System.Runtime.CompilerServices.CompilerGenerated]
	public override {readOnlyKeyword}bool Equals(object? obj) => throw new global::System.NotSupportedException();";

		string getHashCodeMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(GetHashCode),
				Parameters: [],
				ReturnType.SpecialType: SpecialType.System_Int32
			}
		) || !tuple.GenerateGetHashCode
			? $@"// Can't generate '{nameof(GetHashCode)}' because the method is impl'ed by user."
			: $@"/// <inheritdoc cref=""object.GetHashCode""/>
	/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
	[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER || (NETCOREAPP3_0 || NETCOREAPP3_1)
	[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
#endif
	[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
	[global::System.Runtime.CompilerServices.CompilerGenerated]
	public override {readOnlyKeyword}int GetHashCode() => throw new global::System.NotSupportedException();";

		string toStringMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(ToString),
				Parameters: [],
				ReturnType.SpecialType: SpecialType.System_String
			}
		) || !tuple.GenerateToString
			? $@"// Can't generate '{nameof(ToString)}' because the method is impl'ed by user."
			: $@"/// <inheritdoc cref=""object.ToString""/>
	/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
	[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER || (NETCOREAPP3_0 || NETCOREAPP3_1)
	[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
#endif
	[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
	[global::System.Runtime.CompilerServices.CompilerGenerated]
	public override {readOnlyKeyword}string? ToString() => throw new global::System.NotSupportedException();";

		context.AddSource(
			type.ToFileName(),
			"rsd",
			$@"#pragma warning disable CS0809

#nullable enable

namespace {namespaceName};

partial struct {type.Name}{genericParameterList}
{{
	{equalsMethod}

	{getHashCodeMethod}

	{toStringMethod}
}}
"
		);
	}

	/// <summary>
	/// Generates for nested-levelled <see langword="ref struct"/> types.
	/// </summary>
	/// <param name="context">The context.</param>
	/// <param name="tuple">The tuple.</param>
	/// <param name="compilation">The compilation instance.</param>
	private void OnNested(
		GeneratorExecutionContext context, (INamedTypeSymbol Type, bool GenerateGetHashCode, bool GenerateToString) tuple,
		Compilation compilation)
	{
		var type = tuple.Type;

		var (_, _, namespaceName, genericParameterList, _, _, readOnlyKeyword, _, _, _) =
			SymbolOutputInfo.FromSymbol(type);

		// If nested type, the 'genericParametersList' may contain the dot '.' such as
		//
		//     <TKey, TValue>.KeyCollection
		//
		// We should remove the characters before the dot.
		if (!string.IsNullOrEmpty(genericParameterList)
			&& (genericParameterList.IndexOf("where") is var tempIndex and not -1 ? ..tempIndex : ..) is var range
			&& genericParameterList[range].LastIndexOf('.') is var dot and not -1)
		{
			if (dot + 1 >= genericParameterList.Length)
			{
				return;
			}

			genericParameterList = genericParameterList[(dot + 1)..];
			if (genericParameterList.IndexOf('<') == -1)
			{
				genericParameterList = string.Empty;
			}
		}

		// Get outer types.
		var outerTypes = new Stack<(INamedTypeSymbol Type, int Indenting)>();
		int outerTypesCount = 0;
		for (var o = type.ContainingType; o is not null; o = o.ContainingType)
		{
			outerTypesCount++;
		}

		string methodIndenting = new('\t', outerTypesCount + 1);
		string typeIndenting = new('\t', outerTypesCount);
		for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
		{
			outerTypes.Push((outer, outerTypesCount--));
		}

		StringBuilder outerTypeDeclarationsStart = new(), outerTypeDeclarationsEnd = new();
		var indentingStack = new Stack<string>();
		foreach (var (outerType, currentIndenting) in outerTypes)
		{
			var (_, outerFullTypeName, _, _, _, outerTypeKind, _, _, _, _) = SymbolOutputInfo.FromSymbol(outerType);

			string outerGenericParametersList;
			int lastDot = outerFullTypeName.LastIndexOf('.');
			if (lastDot == -1)
			{
				int lt = outerFullTypeName.IndexOf('<'), gt = outerFullTypeName.IndexOf('>');
				if (lt == -1)
				{
					outerGenericParametersList = string.Empty;
				}
				else if (gt < lt)
				{
					continue;
				}
				else
				{
					outerGenericParametersList = outerFullTypeName[lt..gt];
				}
			}
			else
			{
				int start = lastDot + 1;
				if (start >= outerFullTypeName.Length)
				{
					continue;
				}

				string temp = outerFullTypeName[start..];
				int lt = temp.IndexOf('<'), gt = temp.IndexOf('>');
				if (lt == -1)
				{
					outerGenericParametersList = string.Empty;
				}
				else if (gt < lt)
				{
					continue;
				}
				else
				{
					outerGenericParametersList = temp[lt..(gt + 1)];
				}
			}

			string indenting = new('\t', currentIndenting - 1);

			outerTypeDeclarationsStart
				.AppendLine($"{indenting}partial {outerTypeKind}{outerType.Name}{outerGenericParametersList}")
				.AppendLine($"{indenting}{{");

			indentingStack.Push(indenting);
		}

		foreach (string indenting in indentingStack)
		{
			outerTypeDeclarationsEnd.AppendLine($"{indenting}}}");
		}

		// Remove the last new line.
		outerTypeDeclarationsStart.Remove(outerTypeDeclarationsStart.Length - 2, 2);
		outerTypeDeclarationsEnd.Remove(outerTypeDeclarationsEnd.Length - 2, 2);

		var methods = type.GetMembers().OfType<IMethodSymbol>().ToArray();
		string equalsMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(Equals),
				Parameters: [{ Type.SpecialType: SpecialType.System_Object }, ..],
				ReturnType.SpecialType: SpecialType.System_Boolean
			}
		)
			? $"{methodIndenting}// Can't generate '{nameof(Equals)}' because the method is impl'ed by user."
			: $@"{methodIndenting}/// <inheritdoc cref=""object.Equals(object?)""/>
{methodIndenting}/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readOnlyKeyword}bool Equals(object? obj) => throw new global::System.NotSupportedException();";

		string getHashCodeMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(GetHashCode),
				Parameters: [],
				ReturnType.SpecialType: SpecialType.System_Int32
			}
		) || !tuple.GenerateGetHashCode
			? $"{methodIndenting}// Can't generate '{nameof(GetHashCode)}' because the method is impl'ed by user."
			: $@"{methodIndenting}/// <inheritdoc cref=""object.GetHashCode""/>
{methodIndenting}/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readOnlyKeyword}int GetHashCode() => throw new global::System.NotSupportedException();";

		string toStringMethod = Array.Exists(
			methods,
			static symbol => symbol is
			{
				IsStatic: false,
				Name: nameof(ToString),
				Parameters: [],
				ReturnType.SpecialType: SpecialType.System_String
			}
		) || !tuple.GenerateToString
			? $"{methodIndenting}// Can't generate '{nameof(ToString)}' because the method is impl'ed by user."
			: $@"{methodIndenting}/// <inheritdoc cref=""object.ToString""/>
{methodIndenting}/// <exception cref=""global::System.NotSupportedException"">Always throws.</exception>
{methodIndenting}[global::System.CodeDom.Compiler.GeneratedCode(""{GetType().FullName}"", ""{VersionValue}"")]
{methodIndenting}[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
{methodIndenting}[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
{methodIndenting}[global::System.Obsolete(""You can't use or call this method."", true, DiagnosticId = ""BAN"")]
{methodIndenting}[global::System.Runtime.CompilerServices.CompilerGenerated]
{methodIndenting}public override {readOnlyKeyword}string? ToString() => throw new global::System.NotSupportedException();";

		context.AddSource(
			type.ToFileName(),
			"rsd",
			$@"#pragma warning disable CS0809

#nullable enable

namespace {namespaceName};

{outerTypeDeclarationsStart}
{typeIndenting}partial struct {type.Name}{genericParameterList}
{typeIndenting}{{
{equalsMethod}

{getHashCodeMethod}

{toStringMethod}
{typeIndenting}}}
{outerTypeDeclarationsEnd}
"
		);
	}
}
