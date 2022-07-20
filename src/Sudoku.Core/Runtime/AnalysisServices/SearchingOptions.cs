﻿namespace Sudoku.Runtime.AnalysisServices;

/// <summary>
/// Indicates the options for a step searcher used while searching.
/// </summary>
/// <param name="Priority">Indicates the priority of this technique.</param>
/// <param name="EnabledArea">
/// Indicates which areas the step searcher is enabled and works well.
/// The default value is both <see cref="EnabledArea.Default"/> and <see cref="EnabledArea.Gathering"/>.
/// </param>
/// <param name="DisplayingLevel">
/// <para>Indicates the displaying level of this technique.</para>
/// <para>
/// The display level means the which level the technique is at. All higher leveled techniques
/// won't display on the screen when the searchers at the current level have found technique
/// instances.
/// </para>
/// <para>
/// In order to enhance the performance, this attribute is used on a step gatherer.
/// For example, if Alternating Inference Chain (AIC) is at the level <see cref="DisplayingLevel.D"/>
/// but Forcing Chains (FC) is at the level <see cref="DisplayingLevel.E"/>,
/// when we find any AIC technique instances, FC won't be checked at the same time.
/// </para>
/// <para>
/// This attribute is also used for grouping those the searchers, especially in Sudoku Explainer mode.
/// </para>
/// </param>
/// <param name="DisabledReason">
/// <para>
/// Indicates whether the current searcher has bug to fix, or something else to describe why
/// the searcher is (or should be) disabled.
/// </para>
/// <para>
/// The property <b>must</b> contain a value that differs with <see cref="DisabledReason.None"/>
/// when the property <see cref="EnabledArea"/> isn't <see cref="EnabledArea.Default"/>.
/// </para>
/// </param>
public readonly record struct SearchingOptions(
	int Priority,
	DisplayingLevel DisplayingLevel,
	EnabledArea EnabledArea = EnabledArea.Default | EnabledArea.Gathering,
	DisabledReason DisabledReason = DisabledReason.None)
{
	/// <summary>
	/// Indicates the custom priority value. This property is used for a comparison between two step searchers
	/// when they hold a same priority value.
	/// </summary>
	public int SeparatedStepSearcherPriority { get; init; }
}
