﻿namespace Sudoku.Solving.Logical.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 1</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Digit1"><inheritdoc/></param>
/// <param name="Digit2"><inheritdoc/></param>
/// <param name="Loop"><inheritdoc/></param>
internal sealed record UniqueLoopType1Step(Conclusion[] Conclusions, View[]? Views, int Digit1, int Digit2, scoped in CellMap Loop) :
	UniqueLoopStep(Conclusions, Views, Digit1, Digit2, Loop)
{
	/// <inheritdoc/>
	public override int Type => 1;

	/// <inheritdoc/>
	public override Rarity Rarity => Rarity.Sometimes;

	/// <inheritdoc/>
	public override IReadOnlyDictionary<string, string[]?> FormatInterpolatedParts
		=> new Dictionary<string, string[]?>
		{
			{ "en", new[] { Digit1Str, Digit2Str, LoopStr } },
			{ "zh", new[] { Digit1Str, Digit2Str, LoopStr } }
		};
}
