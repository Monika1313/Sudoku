﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates the source code for deconstruction methods.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class GeneratedDeconstructionGenerator : IIncrementalGenerator
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
					TargetNode: MethodDeclarationSyntax { Modifiers: var modifiers } node,
					TargetSymbol: IMethodSymbol
					{
						Name: "Deconstruct",
						TypeParameters: [],
						Parameters: var parameters and not [],
						IsStatic: false,
						ReturnsVoid: true,
						ContainingType: { ContainingType: null, IsFileLocal: false } type
					} symbol,
					SemanticModel.Compilation: var compilation
				}
				when parameters.All(static p => p.RefKind == RefKind.Out):
				{
					var attributeType = compilation.GetTypeByMetadataName("System.Diagnostics.CodeGen.GeneratedDeconstructionArgumentAttribute");
					if (attributeType is null)
					{
						goto default;
					}

					return new(type, symbol, parameters, modifiers, attributeType);
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
					{ ContainingNamespace: var @namespace, Name: var typeName, TypeParameters: var typeParameters } containingType,
					{ DeclaredAccessibility: var methodAccessibility } method,
					{ Length: var parameterLength } parameters,
					var modifiers,
					var attributeType
				))
#pragma warning restore format
				{
					continue;
				}

				var membersData = (
					from m in containingType.GetAllMembers()
					where m switch
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
					from parameter in parameters
					let index = Array.FindIndex(membersData, member => memberDataSelector(member, parameter, attributeType))
					where index != -1
					let correspondingData = membersData[index]
					where correspondingData.CheckId // If none found, this field will be set 'false' by default because of 'default(T)'.
					let parameterName = parameter.Name
					let isDirect = standardizeIdentifierName(parameterName) == correspondingData.Name
					select (IsDirect: isDirect, correspondingData.Member, correspondingData.Member.Name, ParameterName: parameterName)
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

				var typeParametersStr = typeParameters switch
				{
					[] => string.Empty,
					_ => $"<{string.Join(", ", from typeParameter in typeParameters select typeParameter.Name)}>"
				};

				spc.AddSource(
					$"{containingType.ToFileName()}_p{parameters.Length}.g.{Shortcuts.GeneratedDeconstruction}.cs",
					$$"""
					// <auto-generated/>

					#nullable enable

					{{namespaceStr}}partial {{containingType.GetTypeKindModifier()}} {{typeName}}{{typeParametersStr}}
					{
						/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
						[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
						[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{GetType().FullName}}", "{{VersionValue}}")]
						[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
						{{modifiers}} void Deconstruct({{argsStr}})
						{
							{{assignmentsCode}}
						}
					}
					"""
				);
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

			static string getAssignmentStatementCode((bool IsDirect, ISymbol Member, string Name, string ParameterName) t, CancellationToken ct)
				=> t switch
				{
					// Field reference.
					(_, IFieldSymbol, var name, var parameterName) => $"{parameterName} = {name};",

					// Property reference. The property is directly referenced.
					(true, IPropertySymbol, var name, var paramName) => $"{paramName} = {name};",

					// Property reference. The property is indirectly referenced by attributes.
					(false, IPropertySymbol { DeclaringSyntaxReferences: var syntaxRefs }, var name, var paramName) => syntaxRefs switch
					{
						// Declared in metadata.
						{ IsDefaultOrEmpty: true } => $"{paramName} = {name};",

						// Declared in source files.
						[var r] => (PropertyDeclarationSyntax)r.GetSyntax(ct)! switch
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
							_ => $"{paramName} = {name};"
						}
					},

					// Parameterless method reference. The method is directly referenced.
					(true, IMethodSymbol, var name, var paramName) => $"{paramName} = {name}();",

					// Parameterless method reference. The method is indirectly referenced by attributes.
					(false, IMethodSymbol { DeclaringSyntaxReferences: var syntaxRefs }, var name, var paramName) => syntaxRefs switch
					{
						// Declared in metadata.
						{ IsDefaultOrEmpty: true } => $"{paramName} = {name}();",

						// Declared in source files.
						[var r] => (MethodDeclarationSyntax)r.GetSyntax(ct)! switch
						{
							{ ExpressionBody.Expression: var expr } => $"{paramName} = {expr};",
							_ => $"{paramName} = {name}();"
						}
					}
				};
		}
	}
}

/// <summary>
/// The internal output data.
/// </summary>
/// <param name="ContainingType">The containing type.</param>
/// <param name="Method">The deconstruction method.</param>
/// <param name="Parameters">The parameters.</param>
/// <param name="Modifiers">The modifiers of the deconstruction method.</param>
/// <param name="AttributeType">The attribute type used for fetching the arguments' extra data.</param>
file readonly record struct Data(
	INamedTypeSymbol ContainingType,
	IMethodSymbol Method,
	ImmutableArray<IParameterSymbol> Parameters,
	SyntaxTokenList Modifiers,
	INamedTypeSymbol AttributeType
);