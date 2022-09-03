﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates the source code that are options used in manual solver type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ManualSolverOperationsGenerator : IIncrementalGenerator
{
	/// <inheritdoc/>
	public void Initialize(IncrementalGeneratorInitializationContext context)
		=> context.RegisterSourceOutput(context.CompilationProvider, CreateSourceGeneration);

	private void CreateSourceGeneration(SourceProductionContext spc, Compilation compilation)
	{
		if (compilation is not
			{
				Assembly: { Name: Projects.ManualSolving, GlobalNamespace: var @namespace } assemblySymbol
			})
		{
			return;
		}

		var manualSolverTypeSymbol = compilation.GetTypeByMetadataName($"{Projects.ManualSolving}.ManualSolver");
		if (manualSolverTypeSymbol is not { TypeKind: Kind.Class, IsRecord: false, IsSealed: true })
		{
			// The core type cannot be found.
			// This failure will be triggered when you moved the type 'ManualSolver'
			// to another place not in the assembly 'Sudoku.Solving.Manual'.
			return;
		}

		var stepSearcherType = compilation.GetTypeByMetadataName($"{Projects.ManualSolving}.Searchers.IStepSearcher");
		if (stepSearcherType is not { TypeKind: Kind.Interface })
		{
			// Same reason as above.
			return;
		}

		var attributeType = compilation.GetTypeByMetadataName("Sudoku.Solving.Data.Binding.StepSearcherPropertyAttribute");
		if (attributeType is not { TypeKind: Kind.Class, IsSealed: true })
		{
			// Same reason as above.
			return;
		}

		// Iterates on all possible types derived from this interface.
		var allTypes = @namespace.GetAllNestedTypes();
		var foundResultInfos = new List<TypeLocalType_FoundResultInfo>();
		foreach (var searcherType in
			from typeSymbol in allTypes
			where typeSymbol is
			{
				TypeKind: Kind.Class,
				AllInterfaces: var implementedInterfaces and not []
			} && implementedInterfaces.Contains(stepSearcherType, SymbolEqualityComparer.Default)
			select typeSymbol)
		{
			foreach (var property in searcherType.GetMembers().OfType<IPropertySymbol>())
			{
				if (!property.ContainsAttribute(attributeType))
				{
					continue;
				}

				if (property is not
					{
						ExplicitInterfaceImplementations: [],
						ContainingType.Name: var searcherTypeName,
						Name: var propertyName
					})
				{
					continue;
				}

				string searcherFullTypeName = $"{Projects.ManualSolving}.Searchers.I{searcherTypeName}";
				var interfaceType = compilation.GetTypeByMetadataName(searcherFullTypeName);
				if (interfaceType is not { AllInterfaces: var interfaceBaseInterfaces })
				{
					continue;
				}

				if (interfacePropertyMatcher(interfaceType))
				{
					foundResultInfos.Add(new(property, interfaceType, interfaceType));
				}
				else if (interfaceBaseInterfaces.FirstOrDefault(interfacePropertyMatcher) is { } baseInterfaceType)
				{
					foundResultInfos.Add(new(property, interfaceType, baseInterfaceType));
				}


				bool interfacePropertyMatcher(INamedTypeSymbol e)
					=> e.GetMembers().OfType<IPropertySymbol>().Any(p => p.Name == property.Name);
			}
		}

		string targetPropertiesCode = string.Join(
			"\r\n\r\n\t",
			from info in foundResultInfos
			let typeStr = info.DerivedInterfaceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			let propertyContainedInterfaceTypeStr = info.PropertyContainedInterfaceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			let typeStrWithoutInterfacePrefix = info.Property.ContainingType.Name
			let propertyStr = info.Property.Name
			let propertyTypeStr = info.Property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
			select $$"""
			/// <inheritdoc cref="{{propertyContainedInterfaceTypeStr}}.{{propertyStr}}"/>
				[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
				[global::System.Runtime.CompilerServices.CompilerGenerated]
				public {{propertyTypeStr}} {{typeStrWithoutInterfacePrefix}}_{{propertyStr}}
				{
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					set
					{
						var searcher = TargetSearcherCollection.GetOfType<{{typeStr}}>();
						if (searcher is not null)
						{
							searcher.{{propertyStr}} = value;
						}
					}
				}
			"""
		);

		spc.AddSource(
			$"ManualSolver.g.{Shortcuts.ManualSolverOptions}.cs",
			$$"""
			// <auto-generated/>

			#nullable enable

			namespace Sudoku.Solving.Manual;

			partial class ManualSolver
			{
				{{targetPropertiesCode}}
			}
			"""
		);
	}
}

internal readonly record struct TypeLocalType_FoundResultInfo(
	IPropertySymbol Property,
	INamedTypeSymbol DerivedInterfaceType,
	INamedTypeSymbol PropertyContainedInterfaceType
);
