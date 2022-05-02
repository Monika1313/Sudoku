﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates deconstruction methods of a type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed partial class AutoDeconstructionGenerator : ISourceGenerator
{
	/// <summary>
	/// Defines the pattern that matches for an expression.
	/// </summary>
	private static readonly Regex ExpressionPattern = new(
		"""\w+""",
		RegexOptions.ExplicitCapture,
		TimeSpan.FromSeconds(5)
	);


	/// <inheritdoc/>
	public void Execute(GeneratorExecutionContext context)
	{
		// Check values.
		if (
			context is not
			{
				SyntaxContextReceiver: Receiver { Collection: var collection },
				Compilation: { Assembly: var assembly } compilation
			}
		)
		{
			return;
		}

		// Gathers the attributes data that are applied to the whole assembly, which are used for generation
		// of extension deconstruction methods.
		// Due to the design of the source generator and the attribute, the attribute can only applied to an assembly,
		// which cannot be fetched in the syntax context receiver gotten above.
		const string attributeFullNameExtension = "System.Diagnostics.CodeGen.AutoExtensionDeconstructionAttribute";
		var attributeTypeSymbolExtension = compilation.GetTypeByMetadataName(attributeFullNameExtension);
		GatherAssemblyAttributes(attributeTypeSymbolExtension, assembly, collection);

		// Iterates on each value.
		static ITypeSymbol symbolSelector((bool, INamedTypeSymbol Symbol, AttributeData) e) => e.Symbol;
		foreach (var tuplesGroupedByMethodType in from tuple in collection group tuple by tuple.IsExtension)
		{
			// Checks the key, indicating whether the current source generator generates
			// for the extension deconstruction methods.
			if (tuplesGroupedByMethodType.Key)
			{
				// Generates for extension deconstruction methods.
				// The behavior is different: We should gather all attributes, and group them by its real type
				// to which the attribute applied.
				foreach (var tuplesGroupedByType in
					tuplesGroupedByMethodType.GroupBy(symbolSelector, SymbolEqualityComparer.Default))
				{
					var type = (INamedTypeSymbol)tuplesGroupedByType.Key!;

					var attributesData = (from tuple in tuplesGroupedByType select tuple.AttributeData).ToArray();
					if (attributesData is not [{ NamedArguments: var firstNamedArgs }, ..])
					{
						continue;
					}

					var (_, _, namespaceName, _, genericParameterListWithoutConstraint, _, _, _, _, _) =
						SymbolOutputInfo.FromSymbol(type);

					// Gets the namespace applied to.
					// TODO: Check and make diagnostics if same type don't set same namespace;
					// non-first values are ignored.
					static bool keySelector(KeyValuePair<string, TypedConstant> e) => e.Key == "Namespace";
					var selectedKvp = firstNamedArgs.FirstOrDefault(keySelector);
					string namespaceNameResult = ((string?)selectedKvp.Value.Value) ?? namespaceName;

					// The final code.
					string fullTypeNameWithoutConstraint = type.ToDisplayString(TypeFormats.FullNameWithConstraints);
					string finalCode = string.Join(
						"\r\n\r\n\t",
						GetForExtension(attributesData, genericParameterListWithoutConstraint, fullTypeNameWithoutConstraint)
					);

					// Hash code value will be used for the distinction for the different types
					// whose name are same as the current type name.
					// For example, 'System.Hello' and 'Sudoku.Core.Hello' are different types.
					int hashCode =
						0xFACE * assembly.ToDisplayString().GetHashCode()
							^ 0xDEAD * namespaceName.GetHashCode() << 7
							^ 0xC0DE * type.Name.GetHashCode() << 3;

					// Emit the generated source.
					context.AddSource(
						type.ToFileName(),
						"ed",
						$$"""
						#nullable enable
						
						namespace {{namespaceNameResult}};
						
						/// <summary>
						/// Provides with extension deconstruction methods on this type.
						/// </summary>
						[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
						[global::System.Runtime.CompilerServices.CompilerGenerated]
						public static class {{type.Name}}_DE{{hashCode:X2}}
						{
							{{finalCode}}
						}
						"""
					);
				}
			}
			else
			{
				// Generates for instance deconstruction methods.
				// The behavior is simple: Get all possible attributes, and gather the info for the generation,
				// and finally generate.
				foreach (var tuplesGroupedByType in
					tuplesGroupedByMethodType.GroupBy(symbolSelector, SymbolEqualityComparer.Default))
				{
					var type = (INamedTypeSymbol)tuplesGroupedByType.Key!;

					var tupleArray = tuplesGroupedByType.ToArray();
					var (_, _, namespaceName, genericParameterList, _, _, readOnlyKeyword, _, _, _) =
						SymbolOutputInfo.FromSymbol(type);

					// Gets the type name.
					string typeKindName = type.GetTypeKindModifier();

					// The final code.
					string finalCode = string.Join(
						"\r\n\r\n\t",
						GetForInstance(
							type, readOnlyKeyword, (from tuple in tupleArray select tuple.AttributeData).ToArray())
					);

					// Emit the generated source.
					context.AddSource(
						type.ToFileName(),
						"d",
						$$"""
						#nullable enable
						
						namespace {{namespaceName}};
						
						[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
						[global::System.Runtime.CompilerServices.CompilerGenerated]
						partial {{typeKindName}} {{type.Name}}{{genericParameterList}}
						{
							{{finalCode}}
						}
						"""
					);
				}
			}
		}
	}

	/// <inheritdoc/>
	public void Initialize(GeneratorInitializationContext context)
		=> context.RegisterForSyntaxNotifications(() => new Receiver(context.CancellationToken));

	/// <summary>
	/// Get for assembly-targeted attributes on generation.
	/// </summary>
	/// <param name="attributeTypeSymbol">The attribute type symbol.</param>
	/// <param name="assembly">The assembly symbol.</param>
	/// <param name="collection">The target collection to store the results.</param>
	private void GatherAssemblyAttributes(
		INamedTypeSymbol? attributeTypeSymbol, IAssemblySymbol assembly,
		ICollection<(bool, INamedTypeSymbol, AttributeData)> collection)
	{
		// Gather assembly-applied attributes, and group them by the actual type applying to.
		// Here I use the method syntax 'GroupBy(...)' instead of keyword-styled syntax
		// because we should define the customized symbol equality comparer to compare two different symbols.
		static INamedTypeSymbol? typeSelector(AttributeData e) => (INamedTypeSymbol?)e.ConstructorArguments[0].Value;
		foreach (var attributesDataGroupedByType in (
			from attributeData in assembly.GetAttributes()
			let attributeClass = attributeData.AttributeClass
			where SymbolEqualityComparer.Default.Equals(attributeTypeSymbol, attributeClass)
			let constructorArgs = attributeData.ConstructorArguments
			where constructorArgs.Length == 2 && typeSelector(attributeData) is not null
			let syntaxRef = attributeData.ApplicationSyntaxReference
			let location = Location.Create(syntaxRef.SyntaxTree, syntaxRef.Span)
			select (AttributeData: attributeData, Location: location)
		).GroupBy(static pair => typeSelector(pair.AttributeData), SymbolEqualityComparer.Default))
		{
			// Gets the key of the group.
			// The key of the group is the type, which is the primary key to distinct different groups.
			var type = (INamedTypeSymbol)attributesDataGroupedByType.Key!;

			// Adds them into the target collection.
			foreach (var (attributeData, location) in attributesDataGroupedByType)
			{
				collection.Add((true, type, attributeData));
			}
		}
	}

	/// <summary>
	/// Gets the raw code parts for extension deconstruction methods via the specified list of attributes data.
	/// </summary>
	/// <param name="attributesData">The attributes data, with the corresponding location.</param>
	/// <param name="genericParameterListWithoutConstraint">The generic parameter list.</param>
	/// <param name="fullTypeNameWithoutConstraint">The full type name, without the generic constraint.</param>
	/// <returns>The collection of raw code parts for extension deconstruction methods.</returns>
	private IReadOnlyCollection<string> GetForExtension(
		AttributeData[] attributesData, string? genericParameterListWithoutConstraint,
		string fullTypeNameWithoutConstraint)
	{
		string constraint = fullTypeNameWithoutConstraint.IndexOf("where") is var index and not -1
			? fullTypeNameWithoutConstraint[index..]
			: string.Empty;

		// Creates a list that stores the parts of the final code.
		var result = new List<string>();

		// Iterates on each attribute data instance.
		foreach (var attributeData in attributesData)
		{
			// Checks whether the number of constructor arguments is 2.
			// If so, check whether the validity of the first argument (must be 'System.Type')
			// and the second argument (must be 'params string[]').
			if (
#pragma warning disable IDE0055
				attributeData is not
				{
					ConstructorArguments: [
						{ Value: INamedTypeSymbol typeOfResult },
						{ Values: var typedConstants and not [] }
					],
					NamedArguments: var namedArgs
				}
#pragma warning restore IDE0055
			)
			{
				// Invalid case.
				continue;
			}

			// Gets the string segment for the keyword 'in' if necessary.
			// If the type is a large structure, user will set the property value to true,
			// in order to optimize the argument-passing operation.
			static bool isPropertyInKeyword(KeyValuePair<string, TypedConstant> e) => e.Key == "EmitsInKeyword";
			var inKeywordRawValueKvp = namedArgs.FirstOrDefault(isPropertyInKeyword);
			string inKeyword = (bool?)inKeywordRawValueKvp.Value.Value is true ? "in " : string.Empty;

			// Creates a collection that is used for storing a pair of information on the target propeties.
			// Here the target property means the corresponding property searching it through the attribute value
			// (have mentioned above, an array of string elements).
			var pairs = new List<(ITypeSymbol Symbol, string Name)>();

			// Iterates on each key-value pair.
			foreach (var typedConstant in typedConstants)
			{
				// The target value type must be a string.
				if (typedConstant.Value is not string s)
				{
					continue;
				}

				// The expression must be valid.
				// If the input string has other unexpected characters, just report it as a diagnostic warning
				// that can tell the user the argument will be ignored.
				if (!ExpressionPattern.IsMatch(s))
				{
					continue;
				}

				// Gets the target member via the name.
				// If the member is a property, the operation will be successful.
				switch (typeOfResult.GetMembers(s).FirstOrDefault())
				{
					case IFieldSymbol
					{
						Type: var fieldType and not (IPointerTypeSymbol or IFunctionPointerTypeSymbol),
						Name: var fieldName
					}:
					{
						// Add it into the collection.
						pairs.Add((fieldType, fieldName));
						break;
					}
					case IPropertySymbol
					{
						Type: var propertyType and not (IPointerTypeSymbol or IFunctionPointerTypeSymbol),
						Name: var propertyName
					}:
					{
						// Add it into the collection.
						pairs.Add((propertyType, propertyName));
						break;
					}
				}
			}

			// Gather the final information, and emit the generated source.
			string args = string.Join(
				", ",
				from element in pairs
				select $"out {element.Symbol.ToDisplayString(TypeFormats.FullName)} {element.Name.ToCamelCase()}"
			);
			string assignments = string.Join(
				"\r\n\t\t",
				from element in pairs
				select $"{element.Name.ToCamelCase()} = @this.{element.Name};"
			);
			result.Add(
				// Here we should insert an extra indentation, on purpose.
				$$"""
				/// <summary>
					/// Deconstruct the current instance into multiple values, which means you can use
					/// the value-tuple syntax to define your own deconstruction logic.
					/// </summary>
					/// <remarks>
					/// <para>
					/// For example,
					/// if you have defined a <see langword="static"/> deconstruction method <c>Deconstruct</c>
					/// in a <see langword="static class"/>, without any return value:
					/// <code><![CDATA[
					/// public static void Deconstruct(out string name, out int age)
					/// {
					///     name = Name;
					///     age = Age;
					/// }
					/// ]]></code>
					/// The following code will be legal.
					/// <code><![CDATA[
					/// // Use explicitly typed variables to get a deconstruction.
					/// (string name, int age) = student;
					/// 
					/// // Or use the type inferring to omit the type of each variable having been deconstructed.
					/// var (name, age) = student;
					/// 
					/// // Of course, you can also use the instance-method-like invocation to get values.
					/// student.Deconstruct(out string name, out int age);
					/// ]]></code>
					/// </para>
					/// <para>
					/// Static deconstruction methods also allow you using deconstruction patterns, like this:
					/// <code><![CDATA[
					/// if (student is (name: var name, age: >= 18))
					/// {
					///     Console.WriteLine(name);
					/// }
					/// ]]></code>
					/// </para>
					/// </remarks>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					public static void Deconstruct{{genericParameterListWithoutConstraint}}(this {{inKeyword}}{{fullTypeNameWithoutConstraint}}@this, {{args}}){{constraint}}
					{
						{{assignments}}
					}
				"""
			);
		}

		return result;
	}

	/// <summary>
	/// Gets the raw code parts for instance deconstruction methods via the specified list of attributes data.
	/// </summary>
	/// <param name="type">The type to be used.</param>
	/// <param name="readOnlyKeyword">The read-only keyword token.</param>
	/// <param name="attributesData">The attributes data, and the corresponding location.</param>
	/// <returns>The collection of raw code parts for instance deconstruction methods.</returns>
	private IReadOnlyCollection<string> GetForInstance(
		INamedTypeSymbol type, string? readOnlyKeyword, IEnumerable<AttributeData> attributesData)
	{
		var result = new List<string>();

		foreach (var attributeData in attributesData)
		{
			// Checks whether the number of constructor arguments is 1.
			// If so, check whether the validity of the only argument.
			if (attributeData.ConstructorArguments is not [{ Values: var typedConstants and not [] }])
			{
				// Invalid case.
				continue;
			}

			// Creates a collection that is used for storing a pair of information on the target propeties.
			// Here the target property means the corresponding property searching it through the attribute value
			// (have mentioned above, an array of string elements).
			var pairs = new List<(ITypeSymbol Symbol, string Name)>();

			// Iterates on each key-value pair.
			foreach (var typedConstant in typedConstants)
			{
				// The target type must be a string.
				if (typedConstant.Value is not string s)
				{
					continue;
				}

				// The expression must be valid.
				// If the input string has other unexpected characters, just report it as a diagnostic warning
				// that can tell the user the argument will be ignored.
				if (!ExpressionPattern.IsMatch(s))
				{
					continue;
				}

				// Gets the target member via the name.
				// If the member is a property, the operation will be successful.
				switch (type.GetMembers(s).FirstOrDefault())
				{
					case IFieldSymbol
					{
						Type: var fieldType and not (IPointerTypeSymbol or IFunctionPointerTypeSymbol),
						Name: var fieldName
					}:
					{
						// Add it into the collection.
						pairs.Add((fieldType, fieldName));
						break;
					}
					case IPropertySymbol
					{
						Type: var propertyType and not (IPointerTypeSymbol or IFunctionPointerTypeSymbol),
						Name: var propertyName
					}:
					{
						// Add it into the collection.
						pairs.Add((propertyType, propertyName));
						break;
					}
				}
			}

			// Gather the final information, and emit the generated source.
			string args = string.Join(
				", ",
				from element in pairs
				select $"out {element.Symbol.ToDisplayString(TypeFormats.FullName)} {element.Name.ToCamelCase()}"
			);
			string assignments = string.Join(
				"\r\n\t\t",
				from element in pairs
				select $"{element.Name.ToCamelCase()} = {element.Name};"
			);
			result.Add(
				// Here we should insert an extra indentation, on purpose.
				$$"""
				/// <summary>
					/// Deconstruct the current instance into multiple values, which means you can use
					/// the value-tuple syntax to define your own deconstruction logic.
					/// </summary>
					/// <remarks>
					/// <para>
					/// For example,
					/// if you have defined a deconstruction method <c>Deconstruct</c> with no return value:
					/// <code><![CDATA[
					/// public void Deconstruct(out string name, out int age)
					/// {
					///     name = Name;
					///     age = Age;
					/// }
					/// ]]></code>
					/// The following code will be legal.
					/// <code><![CDATA[
					/// // Use explicitly typed variables to get a deconstruction.
					/// (string name, int age) = student;
					/// 
					/// // Or use the type inferring to omit the type of each variable having been deconstructed.
					/// var (name, age) = student;
					/// 
					/// // Of course, you can also use the pure invocation to get values.
					/// student.Deconstruct(out string name, out int age);
					/// ]]></code>
					/// </para>
					/// <para>
					/// Deconstruction methods also allow you using deconstruction patterns, like this:
					/// <code><![CDATA[
					/// if (student is (name: var name, age: >= 18))
					/// {
					///     Console.WriteLine(name);
					/// }
					/// ]]></code>
					/// </para>
					/// </remarks>
					[global::System.Runtime.CompilerServices.CompilerGenerated]
					[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
					[global::System.CodeDom.Compiler.GeneratedCode("{{GetType().FullName}}", "{{VersionValue}}")]
					public {{readOnlyKeyword}}void Deconstruct({{args}})
					{
						{{assignments}}
					}
				"""
			);
		}

		return result;
	}
}
