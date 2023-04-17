namespace Sudoku.Rendering.Nodes.Grouped;

/// <summary>
/// Defines a lever view node.
/// </summary>
public sealed partial class LeverViewNode(ColorIdentifier identifier, int headCell, int tailCell, int centerCell) :
	GroupedViewNode(identifier, headCell, ImmutableArray<int>.Empty)
{
	/// <summary>
	/// Indicates the tail cell.
	/// </summary>
	public int TailCell { get; } = tailCell;

	/// <summary>
	/// Indicates the center cell.
	/// </summary>
	public int CenterCell { get; } = centerCell;


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> other is LeverViewNode comparer
		&& HeadCell == comparer.HeadCell && TailCell == comparer.TailCell && CenterCell == comparer.CenterCell;

	[GeneratedOverridingMember(GeneratedGetHashCodeBehavior.CallingHashCodeCombine, nameof(TypeIdentifier), nameof(HeadCell), nameof(TailCell), nameof(CenterCell))]
	public override partial int GetHashCode();

	[GeneratedOverridingMember(GeneratedToStringBehavior.RecordLike, nameof(Identifier), nameof(HeadCell), nameof(TailCell), nameof(CenterCell))]
	public override partial string ToString();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override LeverViewNode Clone() => new(Identifier, HeadCell, TailCell, CenterCell);
}