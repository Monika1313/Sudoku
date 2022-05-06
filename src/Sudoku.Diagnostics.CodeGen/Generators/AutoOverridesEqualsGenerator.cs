﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates for the code that is for the overriden of a type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class AutoOverridesEqualsGenerator : ISourceGenerator
{
	/// <inheritdoc/>
	public void Execute(GeneratorExecutionContext context)
	{
		if (context is not { SyntaxContextReceiver: Receiver { Collection: var collection } })
		{
			return;
		}

		foreach (var (type, attributeData) in collection)
		{
			var typeKind = type.TypeKind;

			var members = type.GetAllMembers();
			var (_, _, namespaceName, genericParameterList, _, _, readOnlyKeyword, _, _, _) =
				SymbolOutputInfo.FromSymbol(type);

			var targetSymbolsRawString = new List<string>();
			bool isClass = typeKind == TypeKind.Class, isStruct = typeKind == TypeKind.Struct;
			if (isClass)
			{
				targetSymbolsRawString.Add("other is not null");
			}

			foreach (var typedConstant in attributeData.ConstructorArguments[0].Values)
			{
				string memberName = (string)typedConstant.Value!;

				// Checks whether the specified member is in the target type.
				var selectedMembers = (from member in members where member.Name == memberName select member).ToArray();
				if (selectedMembers is not [var memberSymbol, ..])
				{
					continue;
				}

				switch (memberSymbol)
				{
					case IFieldSymbol { Name: var fieldName, Type: var fieldType }:
					{
						targetSymbolsRawString.Add($"{fieldName} == other.{fieldName}");
						break;
					}
					case IPropertySymbol { GetMethod.ReturnType: var propertyGetterType, Name: var propertyName }:
					{
						targetSymbolsRawString.Add($"{propertyName} == other.{propertyName}");
						break;
					}
					case IMethodSymbol
					{
						Name: var methodName,
						ReturnType: { SpecialType: not SpecialType.System_Void } methodReturnType,
						Parameters: []
					}:
					{
						targetSymbolsRawString.Add($"{methodName}() == other.{methodName}()");
						break;
					}
				}
			}

			var namedArgs = attributeData.NamedArguments;
			string inKeyword = attributeData.GetNamedArgument<bool>("EmitsInKeyword") ? "in " : string.Empty;
			string sealedKeyword = attributeData.GetNamedArgument<bool>("EmitsSealedKeyword") && isClass
				? "sealed "
				: string.Empty;
			bool isExplicitImpl = attributeData.GetNamedArgument<bool>("UseExplicitImplementation");

			string fullTypeName = type.ToDisplayString(TypeFormats.FullName);
			string typeKindString = type.GetTypeKindModifier();
			string nullableAttribute = isClass ? "[NotNullWhen(true)] " : string.Empty;
			string equalsObjectImpl = isStruct
				? $"obj is {fullTypeName} comparer && Equals(comparer)"
				: $"Equals(comparer as {fullTypeName})";
			string objectEquals = type.IsRecord
				? $"\t// The method '{nameof(Equals)}(object?)' exists."
				: type.IsRefLikeType
				? "\t// The method cannot be generated because the type is a ref struct."
				: $$"""
					/// <inheritdoc cref="object.{{nameof(Equals)}}(object?)"/>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					public {{sealedKeyword}}override {{readOnlyKeyword}}bool {{nameof(Equals)}}([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object? obj)
						=> {{equalsObjectImpl}};
				""";

			bool methodPredicate(IMethodSymbol method)
				=> method is
				{
					Name: nameof(object.Equals),
					Parameters: [{ Type: var parameterType }],
					ReturnType.SpecialType: SpecialType.System_Boolean
				} && SymbolEqualityComparer.Default.Equals(parameterType, type) && !type.IsRecord;
			string nullableAnnotation = typeKind == TypeKind.Class ? "?" : string.Empty;
			bool containsGenericEquals = type.GetMembers().OfType<IMethodSymbol>().Any(methodPredicate);
			string genericEquals = (containsGenericEquals, isExplicitImpl) switch
			{
				(true, false) => $"\t// The method '{nameof(Equals)}({fullTypeName}{nullableAnnotation})' exists.",
				(_, true) => $$"""
					/// <inheritdoc/>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					{{readOnlyKeyword}}bool global::System.IEquatable<{{fullTypeName}}>.{{nameof(Equals)}}({{nullableAttribute}}{{fullTypeName}}{{nullableAnnotation}} other)
						=> {{nameof(Equals)}}(other);
				""",
				_ => $$"""
					/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					public {{readOnlyKeyword}}bool {{nameof(Equals)}}({{nullableAttribute}}{{inKeyword}}{{fullTypeName}}{{nullableAnnotation}} other)
						=> {{string.Join(" && ", targetSymbolsRawString)}};
				"""
			};

			context.AddSource(
				type.ToFileName(),
				Shortcuts.AutoOverridesEquals,
				$$"""
				#nullable enable
				
				namespace {{namespaceName}};
				
				partial {{typeKindString}} {{type.Name}}{{genericParameterList}}
				{
				{{objectEquals}}
				
				{{genericEquals}}
				}
				"""
			);
		}
	}

	/// <inheritdoc/>
	public void Initialize(GeneratorInitializationContext context)
		=> context.RegisterForSyntaxNotifications(() => new Receiver(context.CancellationToken));
}