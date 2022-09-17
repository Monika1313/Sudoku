﻿namespace Sudoku.Solving.Logics.Implementations.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Rectangle with Guardians</b>
/// (i.e. Unique Rectangle External Type 2) technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Digit1"><inheritdoc/></param>
/// <param name="Digit2"><inheritdoc/></param>
/// <param name="Cells"><inheritdoc/></param>
/// <param name="GuardianCells">Indicates the cells that the guardians lie in.</param>
/// <param name="GuardianDigit">Indicates the digit that the guardians are used.</param>
/// <param name="IsIncomplete">Indicates whether the rectangle is incomplete.</param>
/// <param name="AbsoluteOffset"><inheritdoc/></param>
[StepDisplayingFeature(StepDisplayingFeature.DifficultyRatingNotStable | StepDisplayingFeature.ConstructedTechnique)]
internal sealed record UniqueRectangleWithGuardianStep(
	ConclusionList Conclusions,
	ViewList Views,
	int Digit1,
	int Digit2,
	scoped in CellMap Cells,
	scoped in CellMap GuardianCells,
	int GuardianDigit,
	bool IsIncomplete,
	int AbsoluteOffset
) :
	UniqueRectangleStep(
		Conclusions,
		Views,
		GuardianCells.Count == 1 ? Technique.UniqueRectangleExternalType1 : Technique.UniqueRectangleExternalType2,
		Digit1,
		Digit2,
		Cells,
		false,
		AbsoluteOffset
	),
	IStepWithPhasedDifficulty
{
	/// <inheritdoc/>
	public override decimal Difficulty => ((IStepWithPhasedDifficulty)this).TotalDifficulty;

	/// <inheritdoc/>
	public decimal BaseDifficulty => 4.5M;

	/// <inheritdoc/>
	public (string Name, decimal Value)[] ExtraDifficultyValues
		=> new[]
		{
			(PhasedDifficultyRatingKinds.Guardian, A004526(GuardianCells.Count) * .1M),
			(PhasedDifficultyRatingKinds.Incompleteness, IsIncomplete ? .1M : 0)
		};

	/// <inheritdoc/>
	public override DifficultyLevel DifficultyLevel => DifficultyLevel.Fiendish;

	/// <inheritdoc/>
	public override TechniqueGroup TechniqueGroup => TechniqueGroup.UniqueRectanglePlus;

	/// <inheritdoc/>
	public override Rarity Rarity => Rarity.Often;

	[ResourceTextFormatter]
	internal string GuardianDigitStr() => (GuardianDigit + 1).ToString();

	[ResourceTextFormatter]
	internal string GuardianCellsStr() => GuardianCells.ToString();
}