namespace Sudoku.Concepts;

/// <summary>
/// Defines the data structure that stores a set of cells and a digit, indicating the information
/// about the locked candidate node.
/// </summary>
/// <param name="digit">Indicates the digit used.</param>
/// <param name="cells">Indicates the cells used.</param>
/// <remarks>
/// <include file="../../global-doc-comments.xml" path="/g/large-structure"/>
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public readonly partial struct LockedTarget([PrimaryConstructorParameter] Digit digit, [PrimaryConstructorParameter] scoped in CellMap cells) :
	IEquatable<LockedTarget>,
	IEqualityOperators<LockedTarget, LockedTarget, bool>
{
	/// <summary>
	/// Initializes a <see cref="LockedTarget"/> instance via the specified cell and the specified digit used.
	/// </summary>
	/// <param name="digit">Indicates the digit used.</param>
	/// <param name="cell">Indicates the cell used.</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public LockedTarget(Digit digit, Cell cell) : this(digit, CellsMap[cell])
	{
	}


	/// <summary>
	/// Indicates whether the number of cells is 1.
	/// </summary>
	[JsonIgnore]
	public bool IsSole => Cells.Count == 1;

	/// <summary>
	/// The digit string value.
	/// </summary>
	[GeneratedDisplayName(nameof(Digit))]
	[JsonIgnore]
	private string DigitString => (Digit + 1).ToString();


	[DeconstructionMethod]
	public partial void Deconstruct(out CellMap cells, out Digit digit);

	[GeneratedOverridingMember(GeneratedEqualsBehavior.TypeCheckingAndCallingOverloading)]
	public override partial bool Equals(object? obj);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(scoped in LockedTarget other) => Digit == other.Digit && Cells == other.Cells;

	[GeneratedOverridingMember(GeneratedGetHashCodeBehavior.CallingHashCodeCombine, "Cells", "Digit")]
	public override partial int GetHashCode();

	[GeneratedOverridingMember(GeneratedToStringBehavior.RecordLike, nameof(DigitString), "Cells")]
	public override partial string ToString();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	bool IEquatable<LockedTarget>.Equals(LockedTarget other) => Equals(other);


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(scoped in LockedTarget left, scoped in LockedTarget right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(scoped in LockedTarget left, scoped in LockedTarget right) => !left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IEqualityOperators<LockedTarget, LockedTarget, bool>.operator ==(LockedTarget left, LockedTarget right) => left == right;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool IEqualityOperators<LockedTarget, LockedTarget, bool>.operator !=(LockedTarget left, LockedTarget right) => left != right;
}
