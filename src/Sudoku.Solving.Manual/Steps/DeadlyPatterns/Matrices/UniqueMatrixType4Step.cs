﻿namespace Sudoku.Solving.Manual.Steps;

/// <summary>
/// Provides with a step that is a <b>Unique Square Type 4</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Cells"><inheritdoc/></param>
/// <param name="DigitsMask"><inheritdoc/></param>
/// <param name="Digit1">Indicates the digit 1 used.</param>
/// <param name="Digit2">Indicates the digit 2 used.</param>
/// <param name="ConjugateHouse">Indicates the cells used as the conjugate house.</param>
internal sealed record UniqueMatrixType4Step(
	ConclusionList Conclusions,
	ViewList Views,
	scoped in Cells Cells,
	short DigitsMask,
	int Digit1,
	int Digit2,
	scoped in Cells ConjugateHouse
) : UniqueMatrixStep(Conclusions, Views, Cells, DigitsMask)
{
	/// <inheritdoc/>
	public override int Type => 4;

	[FormatItem]
	internal string ConjStr
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ConjugateHouse.ToString();
	}

	[FormatItem]
	internal string Digit1Str
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (Digit1 + 1).ToString();
	}

	[FormatItem]
	internal string Digit2Str
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => (Digit2 + 1).ToString();
	}
}