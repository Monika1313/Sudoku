﻿namespace Sudoku.Solving.Logical.Steps;

/// <summary>
/// Provides with a step that is a <b>Bi-value Universal Grave False Candidate Type</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="FalseCandidate">
/// Indicates the false candidate that will cause a BUG deadly pattern if it is true.
/// </param>
internal sealed record BivalueUniversalGraveFalseCandidateTypeStep(Conclusion[] Conclusions, View[]? Views, int FalseCandidate) :
	BivalueUniversalGraveStep(Conclusions, Views)
{
	/// <inheritdoc/>
	public override decimal BaseDifficulty => base.BaseDifficulty + .1M;

	/// <inheritdoc/>
	public override Technique TechniqueCode => Technique.BivalueUniversalGraveFalseCandidateType;

	/// <inheritdoc/>
	public override Rarity Rarity => Rarity.Seldom;

	/// <inheritdoc/>
	public override IReadOnlyDictionary<string, string[]?>? FormatInterpolatedParts
		=> new Dictionary<string, string[]?> { { "en", new[] { FalseCandidateStr } }, { "zh", new[] { FalseCandidateStr } } };

	private string FalseCandidateStr => RxCyNotation.ToCandidateString(FalseCandidate);
}
