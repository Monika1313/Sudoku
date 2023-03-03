﻿namespace Sudoku.Solving.Logical.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Polygon</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Map">The map that contains the cells used for this technique.</param>
/// <param name="DigitsMask">The mask that contains all digits used.</param>
internal abstract record UniquePolygonStep(Conclusion[] Conclusions, View[]? Views, scoped in CellMap Map, short DigitsMask) :
	DeadlyPatternStep(Conclusions, Views)
{
	/// <inheritdoc/>
	public override decimal BaseDifficulty => 5.3M;

	/// <summary>
	/// Indicates the type of the technique.
	/// </summary>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override string? Format => base.Format;

	/// <inheritdoc/>
	public sealed override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

	/// <inheritdoc/>
	public sealed override TechniqueGroup TechniqueGroup => TechniqueGroup.DeadlyPattern;

	/// <inheritdoc/>
	public sealed override Rarity Rarity => Rarity.HardlyEver;

	/// <inheritdoc/>
	public sealed override Technique TechniqueCode => Enum.Parse<Technique>($"UniquePolygonType{Type}");

	private protected string DigitsStr => DigitMaskFormatter.Format(DigitsMask, FormattingMode.Normal);

	private protected string CellsStr => Map.ToString();
}
