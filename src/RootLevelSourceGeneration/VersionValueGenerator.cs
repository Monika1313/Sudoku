namespace RootLevelSourceGeneration;

/// <summary>
/// Defines the incremental source generator that is used for the generation on sync the solution version.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class VersionValueGenerator : IIncrementalGenerator
{
	/// <inheritdoc/>
	public void Initialize(IncrementalGeneratorInitializationContext context)
		=> context.RegisterSourceOutput(context.AdditionalTextsProvider.Where(FileNameFilter).Select(Selector), Output);

	private static bool FileNameFilter(AdditionalText file) => file.Path.EndsWith("Directory.Build.props", StringComparison.Ordinal);

	private static string Selector(AdditionalText text, CancellationToken _)
		=> new XmlDocument()
			.OnLoading(text.Path)
			.DocumentElement
			.SelectNodes("descendant::PropertyGroup")
			.Cast<XmlNode>()
			.FirstOrDefault()
			.ChildNodes
			.OfType<XmlNode>()
			.Where(static element => element.Name == "Version")
			.Select(static element => element.InnerText)
			.First()
			.ToString();

	private static void Output(SourceProductionContext spc, string v)
		=> spc.AddSource(
			"SolutionVersion.g.cs",
			$$"""
			// <auto-generated/>

			#nullable enable
			namespace RootLevelSourceGeneration;

			/// <summary>
			/// Represents the data that describes the version used in the project or solution.
			/// </summary>
			[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
			[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{nameof(VersionValueGenerator)}}", "1.1")]
			internal static class SolutionVersion
			{
				/// <summary>
				/// Indicates the version value represented as a <see cref="string"/> value.
				/// </summary>
				[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
				[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{nameof(VersionValueGenerator)}}", "1.1")]
				public const string Value = "{{v}}";
			}
			"""
		);
}
