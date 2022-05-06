﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates for the code that is for the overriden of a type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class AutoOverridesGetHashCodeGenerator : ISourceGenerator
{
	/// <inheritdoc/>
	public void Execute(GeneratorExecutionContext context)
	{
		// Check values.
		if (
			context is not
			{
				SyntaxContextReceiver: Receiver { Collection: var collection } receiver,
				Compilation: { Assembly: var assembly } compilation
			}
		)
		{
			return;
		}

		// Iterates on each pair in the collection.
		foreach (var (type, attributeData) in collection)
		{
			if (attributeData.ApplicationSyntaxReference is not { Span: var textSpan, SyntaxTree: var syntaxTree })
			{
				continue;
			}

			var members = type.GetAllMembers();
			var methods = members.OfType<IMethodSymbol>().ToArray();
			if (
				!type.IsRecord && Array.Exists(
					methods,
					static symbol => symbol is
					{
						ContainingType.SpecialType: not (SpecialType.System_Object or SpecialType.System_ValueType),
						IsStatic: false,
						IsAbstract: false,
						Name: nameof(GetHashCode),
						Parameters: [],
						ReturnType.SpecialType: SpecialType.System_Int32,
						IsImplicitlyDeclared: false
					}
				)
			)
			{
				continue;
			}

			var (_, _, namespaceName, genericParameterList, _, _, readOnlyKeyword, _, _, _) =
				SymbolOutputInfo.FromSymbol(type);

			var targetSymbolsRawString = new List<string>();
			var symbolsRawValue = new List<string>();
			var location = Location.Create(syntaxTree, textSpan);
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
					case IFieldSymbol { Name: var fieldName }:
					{
						targetSymbolsRawString.Add(fieldName);
						symbolsRawValue.Add(fieldName);
						break;
					}
					case IPropertySymbol { GetMethod: not null, Name: var propertyName }:
					{
						targetSymbolsRawString.Add(propertyName);
						symbolsRawValue.Add(propertyName);
						break;
					}
					case IMethodSymbol { Name: var methodName, Parameters: [], ReturnsVoid: false }:
					{
						targetSymbolsRawString.Add($"{methodName}()");
						symbolsRawValue.Add($"{methodName}()");
						break;
					}
				}
			}

			bool isNotStruct = type.TypeKind != TypeKind.Struct;
			string? pattern = attributeData.GetNamedArgument<string>("Pattern");
			bool withSealedKeyword = attributeData.GetNamedArgument<bool>("EmitsSealedKeyword");
			string sealedKeyword = withSealedKeyword && isNotStruct ? "sealed " : string.Empty;
			string methodBody = targetSymbolsRawString.Count switch
			{
				<= 8 => pattern switch
				{
					null => $"\t\t=> global::System.HashCode.Combine({string.Join(", ", targetSymbolsRawString)});",
					_ => $"\t\t=> {convert(pattern)};",
				},
				_ => $$"""
					{
						var final = new global::System.HashCode();
						{{string.Join("\r\n\t\t", from e in targetSymbolsRawString select $"final.Add({e});")}}
						return final.ToHashCode();
					}
				"""
			};

			context.AddSource(
				type.ToFileName(),
				Shortcuts.AutoOverridesGetHashCode,
				$$"""
				#nullable enable
				
				namespace {{namespaceName}};
				
				partial {{type.GetTypeKindModifier()}} {{type.Name}}{{genericParameterList}}
				{
					/// <inheritdoc cref="object.GetHashCode"/>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					public {{sealedKeyword}}override {{readOnlyKeyword}}int GetHashCode()
				{{methodBody}}
				}
				"""
			);


			string convert(string pattern)
				=> Regex
					.Replace(pattern, """(\[0\]|\[[1-9]\d*\])""", m => symbolsRawValue[int.Parse(m.Value[1..^1])])
					.Replace("*", $"{nameof(GetHashCode)}()");
		}
	}

	/// <inheritdoc/>
	public void Initialize(GeneratorInitializationContext context)
		=> context.RegisterForSyntaxNotifications(() => new Receiver(context.CancellationToken));
}