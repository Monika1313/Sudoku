namespace Sudoku.Analytics.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Loop Type 3</b> technique.
/// </summary>
/// <param name="conclusions"><inheritdoc/></param>
/// <param name="views"><inheritdoc/></param>
/// <param name="digit1"><inheritdoc/></param>
/// <param name="digit2"><inheritdoc/></param>
/// <param name="loop"><inheritdoc/></param>
/// <param name="subsetCells">Indicates the cells that are subset cells.</param>
/// <param name="subsetDigitsMask">Indicates the mask that contains the subset digits used in this instance.</param>
public sealed partial class UniqueLoopType3Step(
	Conclusion[] conclusions,
	View[]? views,
	Digit digit1,
	Digit digit2,
	scoped in CellMap loop,
	[PrimaryConstructorParameter] scoped in CellMap subsetCells,
	[PrimaryConstructorParameter] Mask subsetDigitsMask
) : UniqueLoopStep(conclusions, views, digit1, digit2, loop)
{
	/// <inheritdoc/>
	public override int Type => 3;

	/// <inheritdoc/>
	public override ExtraDifficultyCase[] ExtraDifficultyCases
		=> new[] { base.ExtraDifficultyCases[0], (ExtraDifficultyCaseNames.Size, SubsetCells.Count * .1M) };

	/// <inheritdoc/>
	public override IReadOnlyDictionary<string, string[]?> FormatInterpolatedParts
		=> new Dictionary<string, string[]?>
		{
			{ "en", new[] { Digit1Str, Digit2Str, LoopStr, SubsetName, DigitsStr, SubsetCellsStr } },
			{ "zh", new[] { Digit1Str, Digit2Str, LoopStr, SubsetName, DigitsStr, SubsetCellsStr } }
		};

	private string SubsetCellsStr => SubsetCells.ToString();

	private string DigitsStr => DigitMaskFormatter.Format(SubsetDigitsMask, FormattingMode.Normal);

	private string SubsetName => R[$"SubsetNamesSize{SubsetCells.Count + 1}"]!;
}
