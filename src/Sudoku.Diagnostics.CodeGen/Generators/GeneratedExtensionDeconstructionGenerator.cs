﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates the source code for extension deconstruction methods.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class GeneratedExtensionDeconstructionGenerator : IIncrementalGenerator
{
	/// <inheritdoc/>
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterSourceOutput(
			context.SyntaxProvider
				.ForAttributeWithMetadataName(
					"System.Diagnostics.CodeGen.GeneratedDeconstructionAttribute",
					static (node, _) => node is MethodDeclarationSyntax,
					transform
				)
				.Where(static d => d is not null)
				.Collect(),
			action
		);


		static Data? transform(GeneratorAttributeSyntaxContext gasc, CancellationToken _)
		{
			switch (gasc)
			{
				case
				{
					Attributes.Length: 1,
					TargetNode: MethodDeclarationSyntax
					{
						Modifiers: var modifiers,
						ParameterList.Parameters: [{ Modifiers: var thisParameterModifiers and not [] }, ..]
					} node,
					TargetSymbol: IMethodSymbol
					{
						Name: "Deconstruct",
						TypeParameters: var typeParameters,
						Parameters: { Length: > 1 } parameters,
						IsStatic: true,
						IsExtensionMethod: true,
						ReturnsVoid: true,
						ContainingType: { ContainingType: null, IsFileLocal: false, IsStatic: true, IsGenericType: false } type
					} symbol,
					SemanticModel: { Compilation: var compilation } semanticModel
				}
				when parameters.RemoveAt(0) is var lastParameters and not [] && lastParameters.All(static p => p.RefKind == RefKind.Out):
				{
					var attributeType = compilation.GetTypeByMetadataName("System.Diagnostics.CodeGen.GeneratedDeconstructionArgumentAttribute");
					if (attributeType is null)
					{
						goto default;
					}

					return new(type, symbol, thisParameterModifiers, lastParameters, typeParameters, modifiers, attributeType);
				}
				default:
				{
					return null;
				}
			}
		}

		void action(SourceProductionContext spc, ImmutableArray<Data?> data)
		{
			_ = spc is { CancellationToken: var ct };

			foreach (var tuple in data.CastToNotNull())
			{
#pragma warning disable format
				if (tuple is not (
					{ ContainingNamespace: var @namespace, Name: var typeName } containingType,
					{
						Parameters: [{ Type: INamedTypeSymbol thisParameterType, Name: var thisParameterName }, ..],
						DeclaredAccessibility: var methodAccessibility
					} method,
					var thisParameterModifiers,
					{ Length: var parameterLength } parameters,
					var typeParameters,
					var modifiers,
					var attributeType
				))
#pragma warning restore format
				{
					continue;
				}

				if (thisParameterName.IsKeyword())
				{
					thisParameterName = $"@{thisParameterName}";
				}

				var membersData = (
					from m in thisParameterType.GetAllMembers()
					where m.DeclaredAccessibility != Accessibility.Private && m switch
					{
						IFieldSymbol { RefKind: RefKind.None } => true,
						IPropertySymbol { ReturnsByRef: false, ReturnsByRefReadonly: false } => true,
						IMethodSymbol { ReturnsVoid: false, Parameters: [] } => true,
						_ => false
					}
					let name = standardizeIdentifierName(m.Name)
					select (CheckId: true, Member: m, Name: name)
				).ToArray();

				var selection = (
					from param in parameters
					let index = Array.FindIndex(membersData, member => memberDataSelector(member, param, attributeType))
					where index != -1
					let correspondingData = membersData[index]
					where correspondingData.CheckId // If none found, this field will be set 'false' by default because of 'default(T)'.
					let paramName = param.Name
					let isDirect = standardizeIdentifierName(paramName) == correspondingData.Name
					let correspondingMember = correspondingData.Member
					select new LocalParameterInfo(isDirect, thisParameterName, correspondingMember, correspondingMember.Name, paramName)
				).ToArray();

				if (selection.Length != parameterLength)
				{
					// The method is invalid to generate source code, because some parameters are invalid to be matched.
					continue;
				}

				var assignmentsCode = string.Join("\r\n\t\t", from t in selection select getAssignmentStatementCode(t, ct));

				var argsStr = string.Join(
					", ",
					from parameter in parameters
					let parameterType = parameter.Type
					let name = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
					let annotation = parameterType.NullableAnnotation == NullableAnnotation.Annotated ? "?" : string.Empty
					select $"out {name}{annotation} {parameter.Name}"
				);

				var namespaceStr = @namespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) switch
				{
					{ } s => $"namespace {s["global::".Length..]};\r\n\r\n",
					_ => string.Empty
				};

				var genericTypeParameters = typeParameters switch
				{
					[] => string.Empty,
					_ => $"<{string.Join(", ", from typeParameter in typeParameters select typeParameter.Name)}>"
				};

				var thisParameterTypeStr = thisParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
				var thisParameterStr = $"{thisParameterModifiers} {thisParameterTypeStr} {thisParameterName}";

				spc.AddSource(
					$"{containingType.ToFileName()}_p{parameters.Length}.g.{Shortcuts.GeneratedExtensionDeconstruction}.cs",
					$$"""
					// <auto-generated/>

					#nullable enable

					{{namespaceStr}}partial {{containingType.GetTypeKindModifier()}} {{typeName}}
					{
						/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
						[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{GetType().FullName}}", "{{VersionValue}}")]
						[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
						{{modifiers}} void Deconstruct{{genericTypeParameters}}({{thisParameterStr}}, {{argsStr}})
						{
							{{assignmentsCode}}
						}
					}
					"""
				);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static string standardizeIdentifierName(string name)
			=> name switch
			{
				['_', .. var slice] => standardizeIdentifierName(slice),
				[>= 'A' and <= 'Z', ..] => name,
				[var ch and >= 'a' and <= 'z', .. var slice] => $"{char.ToUpper(ch)}{slice}",
				_ => name
			};

		static bool memberDataSelector((bool, ISymbol, string) memberData, IParameterSymbol parameter, INamedTypeSymbol attributeType)
		{
			return (memberData, parameter) is ((_, { Name: var rawName }, var name), { Name: var paramName })
				&& (
					name == standardizeIdentifierName(paramName)
						|| parameter.GetAttributes() is var attributes and not []
						&& attributes.FirstOrDefault(a) is { ConstructorArguments: [{ Value: string targetPropertyExpression }] }
						&& targetPropertyExpression == rawName
				);


			bool a(AttributeData a) => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType);
		}

		static string getAssignmentStatementCode(LocalParameterInfo t, CancellationToken ct)
			=> t switch
			{
				// Field reference.
				(_, var thisParameterName, IFieldSymbol, var name, var parameterName) => $"{parameterName} = {thisParameterName}.{name};",

				// Property reference. The property is indirectly referenced by attributes.
				(true, var thisParameterName, IPropertySymbol, var name, var paramName) => $"{paramName} = {thisParameterName}.{name};",

				// Property reference. The property is directly referenced.
				(false, var thisParameterName, IPropertySymbol { DeclaringSyntaxReferences: [var r] }, var name, var paramName)
					=> (PropertyDeclarationSyntax)r.GetSyntax(ct)! switch
					{
						// public int Property { get => 42; }
						{ AccessorList.Accessors: [{ Keyword.RawKind: (int)SyntaxKind.GetKeyword, ExpressionBody.Expression: var expr }] }
							=> $"{paramName} = {expr};",

						// public int Property => 42;
						{ ExpressionBody.Expression: var expr } => $"{paramName} = {expr};",

						// public int Property { get { return 42; } }
						{
							AccessorList.Accessors:
							[
								{
									Keyword.RawKind: (int)SyntaxKind.GetKeyword,
									Body.Statements: [ReturnStatementSyntax { Expression: var expr }]
								}
							]
						} => $"{paramName} = {expr};",

						// public int Property { get { <block> } }
						_ => $"{paramName} = {thisParameterName}.{name};"
					},

				// Parameterless method reference. The method is indirectly referenced by attributes.
				(true, var thisParameterName, IMethodSymbol, var name, var paramName) => $"{paramName} = {thisParameterName}.{name}();",

				// Parameterless method reference. The method is directly referenced.
				(false, var thisParameterName, IMethodSymbol { DeclaringSyntaxReferences: [var r] }, var name, var paramName)
					=> (MethodDeclarationSyntax)r.GetSyntax(ct)! switch
					{
						{ ExpressionBody.Expression: var expr } => $"{paramName} = {thisParameterName}.{expr};",
						_ => $"{paramName} = {thisParameterName}.{name}();"
					}
			};
	}
}

/// <summary>
/// The internal output data.
/// </summary>
file readonly record struct Data(
	INamedTypeSymbol StaticClassType,
	IMethodSymbol Method,
	SyntaxTokenList ThisParameterModifiers,
	ImmutableArray<IParameterSymbol> Parameters,
	ImmutableArray<ITypeParameterSymbol> TypeParameters,
	SyntaxTokenList Modifiers,
	INamedTypeSymbol AttributeType
);

file readonly record struct LocalParameterInfo(bool IsDirect, string ThisParameterName, ISymbol Member, string Name, string ParameterName);
