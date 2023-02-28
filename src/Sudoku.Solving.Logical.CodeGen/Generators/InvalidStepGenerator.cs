﻿namespace Sudoku.Diagnostics.CodeGen.Generators;

/// <summary>
/// Defines a source generator that generates the source code for invalid step instance.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class InvalidStepGenerator : IIncrementalGenerator
{
	/// <inheritdoc/>
	public void Initialize(IncrementalGeneratorInitializationContext context)
		=> context.RegisterPostInitializationOutput(
			igpic => igpic.AddSource(
				"IInvalidStep.g.cs",
				$$"""
				// <auto-generated />

				#nullable enable

				namespace Sudoku.Solving.Logical.Steps;

				partial interface IInvalidStep
				{
					[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{{GetType().FullName}}", "{{VersionValue}}")]
					[global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
					static IInvalidStep() => Instance = new InvalidStep();
				}

				/// <summary>
				/// The background type that has implemented the type <see cref="IInvalidStep"/>.
				/// </summary>
				/// <seealso cref="IInvalidStep"/>
				file sealed class InvalidStep : IInvalidStep
				{
					/// <inheritdoc/>
					string global::Sudoku.Solving.Logical.IStep.Name => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					string? global::Sudoku.Solving.Logical.IStep.Format => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					decimal global::Sudoku.Solving.Logical.IStep.Difficulty => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Solving.Logical.Techniques.Technique global::Sudoku.Solving.Logical.IStep.TechniqueCode => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Solving.Logical.Techniques.TechniqueTags global::Sudoku.Solving.Logical.IStep.TechniqueTags => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Solving.Logical.Techniques.TechniqueGroup global::Sudoku.Solving.Logical.IStep.TechniqueGroup => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Runtime.AnalysisServices.DifficultyLevel global::Sudoku.Solving.Logical.IStep.DifficultyLevel => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Runtime.AnalysisServices.Stableness global::Sudoku.Solving.Logical.IStep.Stableness => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					global::Sudoku.Runtime.AnalysisServices.Rarity global::Sudoku.Solving.Logical.IStep.Rarity => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					ConclusionList global::Sudoku.Presentation.IVisual.Conclusions => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					ViewList global::Sudoku.Presentation.IVisual.Views => throw new global::System.NotSupportedException();


					/// <inheritdoc/>
					void global::Sudoku.Solving.Logical.IStep.ApplyTo(scoped ref global::Sudoku.Concepts.Grid grid) => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					string global::Sudoku.Solving.Logical.IStep.Formatize(bool handleEscaping) => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					bool global::Sudoku.Solving.Logical.IStep.HasTag(global::Sudoku.Solving.Logical.Techniques.TechniqueTags flags) => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					string global::Sudoku.Solving.Logical.IStep.ToFullString() => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					string global::Sudoku.Solving.Logical.IStep.ToSimpleString() => throw new global::System.NotSupportedException();

					/// <inheritdoc/>
					string global::Sudoku.Solving.Logical.IStep.ElimStr() => throw new global::System.NotSupportedException();
				}
				"""
			)
		);
}
