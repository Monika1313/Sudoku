namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Borescoper's Deadly Pattern Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="cells"><inheritdoc/></param>
/// <param name="digitsMask"><inheritdoc/></param>
/// <param name="subsetCells">Indicates the cells that the subset used.</param>
/// <param name="subsetDigitsMask">Indicates the mask of subset digits used.</param>
public sealed partial class BorescoperDeadlyPatternType3Step(
	Conclusion[] conclusions,
	View[]? views,
	scoped in CellMap cells,
	Mask digitsMask,
	[PrimaryConstructorParameter] scoped in CellMap subsetCells,
	[PrimaryConstructorParameter] Mask subsetDigitsMask
) : BorescoperDeadlyPatternStep(conclusions, views, cells, digitsMask)
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override ExtraDifficultyCase[] ExtraDifficultyCases => new[] { (ExtraDifficultyCaseNames.Size, SubsetCells.Count * .1M) };

	/// <inheritdoc/>
	public override IReadOnlyDictionary<string, string[]?> FormatInterpolatedParts
		=> new Dictionary<string, string[]?>
		{
			{ "en", new[] { DigitsStr, CellsStr, ExtraDigitsStr, ExtraCellsStr } },
			{ "zh", new[] { DigitsStr, CellsStr, ExtraCellsStr, ExtraDigitsStr } }
		};

	private string ExtraDigitsStr => DigitMaskFormatter.Format(SubsetDigitsMask, FormattingMode.Normal);

	private string ExtraCellsStr => SubsetCells.ToString();
}
