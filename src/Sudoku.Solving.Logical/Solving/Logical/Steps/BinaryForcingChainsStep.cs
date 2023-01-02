﻿namespace Sudoku.Solving.Logical.Steps;

/// <summary>
/// Provides with a step that is a <b>Binary Forcing Chains</b> technique that is compatible with program <b>Sudoku Explainer</b>.
/// The technique contains two kinds of forcing chains:
/// <list type="bullet">
/// <item>Dynamic Contradiction Forcing Chains</item>
/// <item>Dynamic Double Forcing Chains</item>
/// </list>
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="SourcePotential">Indicates the source potential of the chain.</param>
/// <param name="FromOnPotential">Indicates the "on" branch.</param>
/// <param name="FromOffPotential">Indicates the "off" branch.</param>
/// <param name="IsAbsurd">Indicates whether the forcing chains kind is contradiction.</param>
/// <param name="IsNishio"><inheritdoc/></param>
/// <param name="DynamicNestingLevel"><inheritdoc/></param>
internal sealed record BinaryForcingChainsStep(
	ConclusionList Conclusions,
	Potential SourcePotential,
	Potential FromOnPotential,
	Potential FromOffPotential,
	bool IsAbsurd,
	bool IsNishio,
	int DynamicNestingLevel
) : ChainingStep(Conclusions, IsMultiple: true, IsDynamic: true, IsNishio: IsNishio, DynamicNestingLevel: DynamicNestingLevel)
{
	/// <inheritdoc/>
	protected override Candidates GetGreenPotentials(int viewIndex)
		=> viewIndex >= FlatViewsCount ? GetNestedGreenPotentials(viewIndex) : GetColorCandidates(viewIndex, true);

	/// <inheritdoc/>
	protected override Candidates GetRedPotentials(int viewIndex)
		=> viewIndex >= FlatViewsCount ? GetNestedRedPotentials(viewIndex) : GetColorCandidates(viewIndex, false);

	/// <inheritdoc/>
	protected override List<LinkViewNode> GetLinks(int viewIndex)
		=> viewIndex >= FlatViewsCount ? GetNestedLinks(viewIndex) : GetLinks(viewIndex == 0 ? FromOnPotential : FromOffPotential);

	/// <summary>
	/// Gets all colored candidates with the specified state.
	/// </summary>
	/// <param name="viewIndex">The view index.</param>
	/// <param name="state">The state of the candidate you want to color.</param>
	/// <returns>All colored candidates with a same state.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Candidates GetColorCandidates(int viewIndex, bool state)
		=> GetColorCandidates(viewIndex == 0 ? FromOnPotential : FromOffPotential, state, state);
}