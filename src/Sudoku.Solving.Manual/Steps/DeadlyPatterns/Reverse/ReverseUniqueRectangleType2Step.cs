﻿namespace Sudoku.Solving.Manual.Steps;

/// <summary>
/// Provides with a step that is a <b>Reverse Unique Rectangle Type 2</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Cells"><inheritdoc/></param>
/// <param name="DigitsMask"><inheritdoc/></param>
/// <param name="ExtraDigit">The extra digit used.</param>
/// <param name="CellsHavingExtraDigit">Indicates the cells that contains the extra digit in the pattern.</param>
internal sealed record ReverseUniqueRectangleType2Step(
	ConclusionList Conclusions,
	ViewList Views,
	scoped in Cells Cells,
	short DigitsMask,
	int ExtraDigit,
	scoped in Cells CellsHavingExtraDigit
) : ReverseUniqueRectangleStep(Conclusions, Views, Cells, DigitsMask)
{
	/// <inheritdoc/>
	public override decimal Difficulty => base.Difficulty + .1M;

	/// <inheritdoc/>
	public override int Type => 2;
}
