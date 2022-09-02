﻿namespace Sudoku.Solving.Manual.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave XZ</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="DigitsMask">Indicates the digits used.</param>
/// <param name="Cells">Indicates the cells used.</param>
/// <param name="ExtraCell">Indicates the XZ cell.</param>
internal sealed partial record BivalueUniversalGraveXzStep(
	ConclusionList Conclusions,
	ViewList Views,
	short DigitsMask,
	scoped in CellMap Cells,
	int ExtraCell
) : BivalueUniversalGraveStep(Conclusions, Views), IStepWithPhasedDifficulty
{
	/// <inheritdoc/>
	public override decimal Difficulty => ((IStepWithPhasedDifficulty)this).TotalDifficulty;

	/// <inheritdoc/>
	public decimal BaseDifficulty => base.Difficulty;

	/// <inheritdoc/>
	public (string Name, decimal Value)[] ExtraDifficultyValues
		=> new[] { (PhasedDifficultyRatingKinds.ExtraDigit, .2M) };

	/// <inheritdoc/>
	public override Technique TechniqueCode => Technique.BivalueUniversalGraveXzRule;

	/// <inheritdoc/>
	public override Rarity Rarity => Rarity.HardlyEver;

	[ResourceTextFormatter]
	private partial string DigitStr() => DigitMaskFormatter.Format(DigitsMask, FormattingMode.Normal);

	[ResourceTextFormatter]
	private partial string CellsStr() => Cells.ToString();

	[ResourceTextFormatter]
	private partial string ExtraCellStr() => RxCyNotation.ToCellString(ExtraCell);
}