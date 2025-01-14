namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// (<b>Not implemented</b>) Provides with a <b>Senior Exocet</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Senior Exocet</item>
/// </list>
/// </summary>
[StepSearcher]
public sealed partial class SeniorExocetStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates whether the searcher will find advanced eliminations.
	/// </summary>
	public bool CheckAdvanced { get; set; }


	/// <inheritdoc/>
	protected internal override Step? Collect(scoped ref AnalysisContext context)
	{
		// TODO: Re-implement SE.
		return null;
	}
}
