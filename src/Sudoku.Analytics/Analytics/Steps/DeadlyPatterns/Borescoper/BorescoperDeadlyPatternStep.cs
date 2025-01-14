namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="cells">The map that contains the cells used for this technique.</param>
/// <param name="digitsMask">Indicates the mask of used digits.</param>
public abstract partial class BorescoperDeadlyPatternStep(
	Conclusion[] conclusions,
	View[]? views,
	[PrimaryConstructorParameter] scoped in CellMap cells,
	[PrimaryConstructorParameter] Mask digitsMask
) : DeadlyPatternStep(conclusions, views)
{
	/// <inheritdoc/>
	public override decimal BaseDifficulty => 5.3M;

	/// <summary>
	/// Indicates the type of the technique.
	/// </summary>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override string Format => R[$"TechniqueFormat_UniquePolygonType{Type}Step"]!;

	/// <inheritdoc/>
	public sealed override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

	/// <inheritdoc/>
	public sealed override Technique Code => Enum.Parse<Technique>($"BorescoperDeadlyPatternType{Type}");

	private protected string DigitsStr => DigitMaskFormatter.Format(DigitsMask, FormattingMode.Normal);

	private protected string CellsStr => Cells.ToString();
}
