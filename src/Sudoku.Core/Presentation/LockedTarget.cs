﻿namespace Sudoku.Presentation;

/// <summary>
/// Defines the data structure that stores a set of cells and a digit, indicating the information
/// about the locked candidate node.
/// </summary>
[JsonConverter(typeof(LockedTargetJsonConverter))]
[AutoOverridesGetHashCode(nameof(Cells), nameof(Digit))]
[AutoOverridesEquals(nameof(Digit), nameof(Cells), UseExplicitImplementation = true)]
[AutoOverridesToString(nameof(Digit), nameof(Cells), Pattern = "Locked target: {[0] + 1}{[1]}")]
public readonly partial struct LockedTarget :
	IEquatable<LockedTarget>,
	IEqualityOperators<LockedTarget, LockedTarget>
{
	/// <summary>
	/// Initializes a <see cref="LockedTarget"/> instance via the specified cells and the specified digit used.
	/// </summary>
	/// <param name="digit">Indicates the digit used.</param>
	/// <param name="cells">Indicates the cells used.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LockedTarget(int digit, in Cells cells) => (Digit, Cells) = (digit, cells);


	/// <summary>
	/// Indicates whether the number of cells is 1.
	/// </summary>
	public bool IsSole => Cells.Count == 1;

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public int Digit { get; }

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public Cells Cells { get; }


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(in LockedTarget left, in LockedTarget right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(in LockedTarget left, in LockedTarget right) => !(left == right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IEqualityOperators<LockedTarget, LockedTarget>.operator ==(LockedTarget left, LockedTarget right)
		=> left == right;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IEqualityOperators<LockedTarget, LockedTarget>.operator !=(LockedTarget left, LockedTarget right)
		=> left != right;
}
