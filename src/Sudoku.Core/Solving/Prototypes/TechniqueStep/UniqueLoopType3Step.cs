﻿namespace Sudoku.Solving.Manual.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 3</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Digit1"><inheritdoc/></param>
/// <param name="Digit2"><inheritdoc/></param>
/// <param name="Loop"><inheritdoc/></param>
/// <param name="SubsetDigitsMask">
/// Indicates the mask that contains the subset digits used in this instance.
/// </param>
/// <param name="SubsetCells">Indicates the subset cells.</param>
[StepDisplayingFeature(StepDisplayingFeature.DifficultyRatingNotStable)]
internal sealed partial record UniqueLoopType3Step(
	ConclusionList Conclusions,
	ViewList Views,
	int Digit1,
	int Digit2,
	scoped in CellMap Loop,
	short SubsetDigitsMask,
	scoped in CellMap SubsetCells
) : UniqueLoopStep(Conclusions, Views, Digit1, Digit2, Loop), IStepWithPhasedDifficulty
{
	/// <inheritdoc/>
	public override decimal Difficulty => ((IStepWithPhasedDifficulty)this).TotalDifficulty;

	/// <inheritdoc/>
	public new (string Name, decimal Value)[] ExtraDifficultyValues
		=> new[] { (PhasedDifficultyRatingKinds.Size, SubsetCells.Count * .1M) };

	/// <inheritdoc/>
	public override int Type => 3;

	[ResourceTextFormatter]
	private partial string SubsetCellsStr() => SubsetCells.ToString();

	[ResourceTextFormatter]
	private partial string DigitsStr() => DigitMaskFormatter.Format(SubsetDigitsMask, FormattingMode.Normal);

	[ResourceTextFormatter]
	private partial string SubsetName() => R[$"SubsetNamesSize{SubsetCells.Count + 1}"]!;

	/// <inheritdoc/>
	public override Rarity Rarity => Rarity.Seldom;
}