namespace Sudoku.SourceGeneration.Handlers;

/// <summary>
/// The generator handler for instance deconstruction methods.
/// </summary>
internal sealed class InstanceDeconstructionMethodHandler : IIncrementalGeneratorAttributeHandler<InstanceDeconstructionMethodCollectedResult>
{
	private const string DeconstructionMethodArgumentAttributeName = "System.SourceGeneration.DeconstructionMethodArgumentAttribute";


	/// <inheritdoc/>
	public void Output(SourceProductionContext spc, ImmutableArray<InstanceDeconstructionMethodCollectedResult> values)
	{
		static INamedTypeSymbol containingTypeSelector(InstanceDeconstructionMethodCollectedResult data) => data.ContainingType;
		var types = new List<string>();
		foreach (var group in values.GroupBy(containingTypeSelector, (IEqualityComparer<INamedTypeSymbol>)SymbolEqualityComparer.Default))
		{
			var containingType = group.Key;
			var typeName = containingType.Name;
			var @namespace = containingType.ContainingNamespace;
			var typeParameters = containingType.TypeParameters;
			var namespaceStr = @namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)["global::".Length..];
			var typeParametersStr = typeParameters switch
			{
				[] => string.Empty,
				_ => $"<{string.Join(", ", from typeParameter in typeParameters select typeParameter.Name)}>"
			};

			var codeSnippets = new List<string>();
			foreach (var element in group)
			{
				if (element is not
					{
						Method: { DeclaredAccessibility: var methodAccessibility } method,
						Parameters: var parameters,
						Modifiers: var modifiers,
						AttributeType: var attributeType,
						AssemblyName: var assemblyName
					})
				{
					continue;
				}

				var parameterNameData = new List<(string Parameter, string Member)>();
				foreach (var parameter in parameters)
				{
					var name = parameter.Name;
					bool predicate(AttributeData a) => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeType);
					parameterNameData.Add(
						parameter.GetAttributes().FirstOrDefault(predicate) switch
						{
							{ ConstructorArguments: [{ Value: string s }] } => (name, s),
							_ => (name, localToPascalCasing(name))
						}
					);
				}

				var assignmentsCode = string.Join("\r\n\t\t\t", from t in parameterNameData select $"{t.Parameter} = {t.Member};");
				var argsStr = string.Join(
					", ",
					from parameter in parameters
					let parameterType = parameter.Type
					let name = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
					let annotation = parameterType is { NullableAnnotation: Annotated, IsReferenceType: true } ? "?" : string.Empty
					select $"out {name}{annotation} {parameter.Name}"
				);

				var includingReferenceLevel = assemblyName.StartsWith("SudokuStudio") ? "../../../" : "../../";
				codeSnippets.Add(
					$$"""
					/// <include file="{{includingReferenceLevel}}global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
							[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
							[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{GetType().FullName}}", "{{Value}}")]
							[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
							{{modifiers}} void Deconstruct({{argsStr}})
							{
								{{assignmentsCode}}
							}
					"""
				);


				static string localToPascalCasing(string name)
					=> name switch
					{
						['_', .. var slice] => localToPascalCasing(slice),
						[>= 'A' and <= 'Z', ..] => name,
						[var ch and >= 'a' and <= 'z', .. var slice] => $"{char.ToUpper(ch)}{slice}",
						_ => name
					};
			}

			types.Add(
				$$"""
				namespace {{namespaceStr}}
				{
					partial {{containingType.GetTypeKindModifier()}} {{typeName}}{{typeParametersStr}}
					{
						{{string.Join("\r\n\r\n\t\t", codeSnippets)}}
					}
				}
				"""
			);
		}

		spc.AddSource(
			$"DeconstructionMethods.g.{Shortcuts.InstanceDeconstructionMethods}.cs",
			$$"""
			// <auto-generated/>
			
			#nullable enable

			{{string.Join("\r\n\r\n", types)}}
			"""
		);
	}

	/// <inheritdoc/>
	public InstanceDeconstructionMethodCollectedResult? Transform(GeneratorAttributeSyntaxContext gasc, CancellationToken cancellationToken)
		=> gasc switch
		{
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
				SemanticModel.Compilation: { AssemblyName: { } assemblyName } compilation
			}
			when parameters.AllOutParameters() => compilation.GetTypeByMetadataName(DeconstructionMethodArgumentAttributeName) switch
			{
				{ } argumentAttributeType => new(type, symbol, parameters, modifiers, argumentAttributeType, assemblyName),
				_ => null
			},
			_ => null
		};
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Determines whether all parameters are <see langword="out"/> ones.
	/// </summary>
	/// <param name="this">A list of parameters.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool AllOutParameters(this ImmutableArray<IParameterSymbol> @this)
		=> @this.All(static parameter => parameter.RefKind == RefKind.Out);
}
