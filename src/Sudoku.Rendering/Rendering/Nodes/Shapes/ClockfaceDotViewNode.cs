namespace Sudoku.Rendering.Nodes.Shapes;

/// <summary>
/// Defines a clockface dot view node.
/// </summary>
public sealed partial class ClockfaceDotViewNode(ColorIdentifier identifier, scoped in CellMap cells, bool isClockwise) :
	QuadrupleCellMarkViewNode(identifier, cells)
{
	/// <summary>
	/// Initializes a <see cref="ClockfaceDotViewNode"/> instance via the specified values.
	/// </summary>
	/// <param name="identifier">The identifier.</param>
	/// <param name="topLeftCell">The top-left cell.</param>
	/// <param name="isClockwise">Indicates whether the dot is marked as clockwise.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ClockfaceDotViewNode(ColorIdentifier identifier, Cell topLeftCell, bool isClockwise) :
		this(identifier, CellsMap[topLeftCell] + (topLeftCell + 1) + (topLeftCell + 9) + (topLeftCell + 10), isClockwise)
	{
	}


	/// <summary>
	/// Indicates whether the dot is marked as clockwise. If <see langword="true"/>, clockwise; otherwise, counterclockwise.
	/// </summary>
	public bool IsClockwise { get; } = isClockwise;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other) => other is ClockfaceDotViewNode comparer && Cells == comparer.Cells;

	[GeneratedOverridingMember(GeneratedGetHashCodeBehavior.CallingHashCodeCombine, nameof(TypeIdentifier), nameof(Cells))]
	public override partial int GetHashCode();

	[GeneratedOverridingMember(GeneratedToStringBehavior.RecordLike, nameof(Identifier), nameof(Cells), nameof(IsClockwise))]
	public override partial string ToString();

	/// <inheritdoc/>
	public override ClockfaceDotViewNode Clone() => new(Identifier, Cells, IsClockwise);
}
