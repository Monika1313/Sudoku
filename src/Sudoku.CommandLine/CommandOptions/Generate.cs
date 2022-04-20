﻿namespace Sudoku.CommandLine.CommandOptions;

/// <summary>
/// Represents a generate command.
/// </summary>
public sealed class Generate : IRootCommand<ErrorCode>
{
	/// <summary>
	/// Indicates the range of givens that generated puzzle should be.
	/// </summary>
	[Command('c', "count", "The range of given cells that generated puzzle should be.")]
	[CommandConverter(typeof(CellCountRangeConverter))]
	public (int Min, int Max) Range { get; set; } = (24, 30);

	/// <summary>
	/// Indicates the algorithm to generate the puzzle.
	/// </summary>
	[Command('m', "method", "The method that defines what algorithm used for generating a sudoku puzzle.")]
	[CommandConverter(typeof(EnumTypeConverter<GenerateType>))]
	public GenerateType GenerateType { get; set; } = GenerateType.HardPatternLike;

	/// <inheritdoc/>
	public static string Name => "generate";

	/// <inheritdoc/>
	public static string Description => "To generate a sudoku puzzle.";

	/// <inheritdoc/>
	public static string[] SupportedCommands => new[] { "generate" };

	/// <inheritdoc/>
	public static IEnumerable<IRootCommand<ErrorCode>>? UsageCommands => throw new NotImplementedException();


	/// <inheritdoc/>
	public ErrorCode Execute()
	{
		switch (GenerateType)
		{
			case GenerateType.HardPatternLike:
			{
				var generator = new HardPatternPuzzleGenerator();
				while (true)
				{
					var targetPuzzle = generator.Generate();
					int c = targetPuzzle.GivensCount;
					if (c < Range.Min || c >= Range.Max)
					{
						continue;
					}

					ConsoleExtensions.WriteLine($"""The puzzle generated: '{targetPuzzle:0}'""");

					return ErrorCode.None;
				}
			}
			default:
			{
				return ErrorCode.MethodIsInvalid;
			}
		}
	}
}