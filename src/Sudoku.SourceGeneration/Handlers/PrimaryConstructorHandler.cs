namespace Sudoku.SourceGeneration.Handlers;

/// <summary>
/// The generator handler for primary constructor parameters.
/// </summary>
internal sealed class PrimaryConstructorHandler : IIncrementalGeneratorAttributeHandler<PrimaryConstructorCollectedResult>
{
	/// <inheritdoc/>
	public void Output(SourceProductionContext spc, ImmutableArray<PrimaryConstructorCollectedResult> values)
	{
		var types = new List<string>();
		foreach (var valuesGrouped in from tuple in values group tuple by $"{tuple.Namesapce}.{tuple.Type}" into @group select @group.ToArray())
		{
			var (namespaceName, fieldDeclarations, propertyDeclarations) = (valuesGrouped[0].Namesapce, new List<string>(), new List<string>());
			foreach (
				var (
					parameterName, typeKind, refKind, scopedKind, nullableAnnotation, parameterType, typeSymbol, isReadOnly,
					namespaceString, typeName, isRecord, attributesData, comment
				) in valuesGrouped
			)
			{
				switch (attributesData)
				{
					case [{ ConstructorArguments: [{ Value: LocalMemberKinds.Field }], NamedArguments: var namedArgs }]:
					{
						var targetMemberName = getTargetMemberName(namedArgs, parameterName, "_<@");
						var accessibilityModifiers = getAccessibilityModifiers(namedArgs, "private ");
						var readonlyModifier = getReadOnlyModifier(namedArgs, scopedKind, refKind, typeKind, typeSymbol.IsRefLikeType, isReadOnly, true);
						var refModifiers = getRefModifiers(namedArgs, scopedKind, refKind, typeKind, typeSymbol.IsRefLikeType, isReadOnly, true);
						var docComments = getDocComments(comment);
						var parameterTypeName = getParameterType(parameterType, nullableAnnotation);
						var assigning = getAssigningExpression(refModifiers, parameterName);
						var pragmaWarningDisable = assigning.StartsWith("ref")
							? """
							#pragma warning disable CS9094
									
							"""
							: "\t\t";
						var pragmaWarningRestor = assigning.StartsWith("ref")
							? """

							#pragma warning restore CS9094
							"""
							: string.Empty;
						fieldDeclarations.Add(
							$"""
							/// <summary>
									{docComments ?? $"/// The generated field declaration for parameter <c>{parameterName}</c>."}
									/// </summary>
									[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{GetType().FullName}", "{Value}")]
									[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
							{pragmaWarningDisable}{accessibilityModifiers}{readonlyModifier}{refModifiers}{parameterTypeName}{targetMemberName} = {assigning};{pragmaWarningRestor}
							"""
						);

						break;
					}
					case [{ ConstructorArguments: [{ Value: LocalMemberKinds.Property }], NamedArguments: var namedArgs }]:
					{
						var targetMemberName = getTargetMemberName(namedArgs, parameterName, ">@");
						var accessibilityModifiers = getAccessibilityModifiers(namedArgs, "public ");
						var readonlyModifier = getReadOnlyModifier(namedArgs, scopedKind, refKind, typeKind, typeSymbol.IsRefLikeType, isReadOnly, false);
						var refModifiers = getRefModifiers(namedArgs, scopedKind, refKind, typeKind, typeSymbol.IsRefLikeType, isReadOnly, false);
						var docComments = getDocComments(comment);
						var parameterTypeString = parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
						var parameterTypeName = getParameterType(parameterType, nullableAnnotation);
						var assigning = getAssigningExpression(refModifiers, parameterName);
						var memberNotNullAttribute = namedArgs.TryGetValueOrDefault<string>("MembersNotNull", out var memberNotNullExpr)
							&& (memberNotNullExpr?.Contains(':') ?? false)
							&& memberNotNullExpr.Split(':') is [var booleanExpr, var memberNamesExpr]
							&& booleanExpr.Trim().ToCamelCasing() is var finalBooleanStringValue and ("true" or "false")
							&& (from memberName in memberNamesExpr.Split(',') select memberName.Trim()).ToArray() is var memberNamesArray and not []
							&& (from memberName in memberNamesArray select $"nameof({memberName})") is var nameOfExpressionList
							&& string.Join(", ", nameOfExpressionList) is var nameOfExpressions
							? $"""
							[global::System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute({finalBooleanStringValue}, {nameOfExpressions})]
									
							"""
							: string.Empty;

						propertyDeclarations.Add(
							$$"""
							/// <summary>
									{{docComments ?? $"/// The generated property declaration for parameter <c>{parameterName}</c>."}}
									/// </summary>
									[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{GetType().FullName}}", "{{Value}}")]
									[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
									{{memberNotNullAttribute}}{{accessibilityModifiers}}{{readonlyModifier}}{{refModifiers}}{{parameterTypeName}}{{targetMemberName}} { get; } = {{assigning}};
							"""
						);

						break;
					}
				}
			}

			var typeKindString = valuesGrouped[0].TypeSymbol.GetTypeKindModifier();
			var genericTypeParameters = valuesGrouped[0].TypeSymbol switch
			{
				{ IsGenericType: true, TypeParameters: var typeParameters }
					=> $"<{string.Join(", ", from typeParameter in typeParameters select typeParameter.Name)}>",
				_ => string.Empty
			};

			types.Add(
				$$"""
				namespace {{namespaceName}}
				{
					partial {{typeKindString}} {{valuesGrouped[0].Type}}{{genericTypeParameters}}
					{
						{{(fieldDeclarations.Count == 0 ? "// No field members." : string.Join("\r\n\r\n\t\t", fieldDeclarations))}}
				
				
						{{(propertyDeclarations.Count == 0 ? "// No property members." : string.Join("\r\n\r\n\t\t", propertyDeclarations))}}
					}
				}
				"""
			);

			static string getTargetMemberName(NamedArgs namedArgs, string parameterName, string defaultPattern)
				=> namedArgs.TryGetValueOrDefault<string>("GeneratedMemberName", out var customizedFieldName)
				&& customizedFieldName is not null
					? customizedFieldName
					: namedArgs.TryGetValueOrDefault<string>("NamingRule", out var namingRule) && namingRule is not null
						? namingRule.InternalHandle(parameterName)
						: defaultPattern.InternalHandle(parameterName);

			static string getAccessibilityModifiers(NamedArgs namedArgs, string @default)
				=> namedArgs.TryGetValueOrDefault<string>("Accessibility", out var a) && a is not null ? $"{a.Trim().ToLower()} " : @default;

			static string getReadOnlyModifier(NamedArgs namedArgs, ScopedKind scopedKind, RefKind refKind, TypeKind typeKind, bool isRefStruct, bool isReadOnly, bool isField)
				=> (scopedKind, refKind, typeKind, isReadOnly, isRefStruct, isField) switch
				{
					(0, RefKind.In, TypeKind.Struct, false, true, _) => "readonly ",
					(0, RefKind.Ref or RefKind.RefReadOnly, TypeKind.Struct, false, true, _) => "readonly ",
					(_, _, TypeKind.Struct, _, _, true) => "readonly ",
					(_, _, TypeKind.Struct, false, _, _) => "readonly ",
					_ => string.Empty
				};

			static string getRefModifiers(NamedArgs namedArgs, ScopedKind scopedKind, RefKind refKind, TypeKind typeKind, bool isRefStruct, bool isReadOnly, bool isField)
				=> (namedArgs.TryGetValueOrDefault<string>("RefKind", out var l) && l is not null ? $"{l} " : null)
				?? (scopedKind, refKind, typeKind, isReadOnly, isRefStruct, isField) switch
				{
					(0, RefKind.In, TypeKind.Struct, false, true, _) => "ref readonly ",
					(0, RefKind.In, TypeKind.Struct, true, true, _) => "ref readonly ",
					(0, RefKind.Ref or RefKind.RefReadOnly, TypeKind.Struct, false, true, _) => "ref ",
					(0, RefKind.Ref or RefKind.RefReadOnly, TypeKind.Struct, true, true, _) => "ref ",
					_ => null
				}
				?? string.Empty;

			static string getParameterType(ITypeSymbol parameterType, NullableAnnotation nullableAnnotation)
				=> parameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is var r
				&& parameterType.TypeKind != TypeKind.Struct && nullableAnnotation == Annotated
					? $"{r}? "
					: $"{r} ";

			static string getAssigningExpression(string refModifiers, string parameterName)
				=> refModifiers switch { not "" => $"ref {parameterName}", _ => parameterName };

			static string? getDocComments(string? comment)
				=> comment switch
				{
					null or "" => null,
					_ => string.Join(
						Environment.NewLine,
						from line in comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries) select $"/// {line.Trim()}"
					)
				};
		}

		spc.AddSource(
			"PrimaryConstructorParameters.g.cs",
			$$"""
			// <auto-generated/>

			#nullable enable

			{{string.Join("\r\n\r\n", types)}}
			"""
		);
	}

	/// <inheritdoc/>
	public PrimaryConstructorCollectedResult? Transform(GeneratorAttributeSyntaxContext gasc, CancellationToken cancellationToken)
		=> gasc switch
		{
			{
				TargetNode: ParameterSyntax { Parent.Parent: TypeDeclarationSyntax { Modifiers: var typeModifiers and not [] } },
				TargetSymbol: IParameterSymbol
				{
					Name: var parameterName,
					RefKind: var refKind,
					ScopedKind: var scopedKind,
					NullableAnnotation: var nullableAnnotation,
					Type: var parameterType,
					ContainingSymbol: IMethodSymbol
					{
						ContainingType:
						{
							ContainingNamespace: var @namespace,
							IsReadOnly: var isReadOnly,
							Name: var typeName,
							IsRecord: var isRecord,
							TypeKind: var typeKind
						} typeSymbol
					}
				} parameterSymbol,
				Attributes: { Length: 1 or 2 } attributesData
			}
			when typeModifiers.Any(SyntaxKind.PartialKeyword)
				=> new(
					parameterName,
					typeKind,
					refKind,
					scopedKind,
					nullableAnnotation,
					parameterType,
					typeSymbol,
					isReadOnly,
					@namespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)["global::".Length..],
					typeName,
					isRecord,
					attributesData.ToArray(),
					// Gets the documentation comments.
					// Please note that Roslyn may contain a bug that primary constructor parameters don't contain any documentation comments,
					// so the result value of this variable may be string.Empty ("").
					parameterSymbol.GetDocumentationCommentXml(cancellationToken: cancellationToken)
				),
			_ => null
		};
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Internal handle the naming rule, converting it into a valid identifier via specified parameter name.
	/// </summary>
	/// <param name="this">The naming rule.</param>
	/// <param name="parameterName">The parameter name.</param>
	/// <returns>The final identifier.</returns>
	public static string InternalHandle(this string @this, string parameterName)
		=> @this
			.Replace("<@", parameterName.ToCamelCasing())
			.Replace(">@", parameterName.ToPascalCasing())
			.Replace("@", parameterName);
}
