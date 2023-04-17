namespace Sudoku.Rendering;

/// <summary>
/// Defines a <see cref="ColorIdentifier"/> derived type that uses palette ID value to distinct with colors.
/// </summary>
/// <param name="value">The palette color ID value to be assigned. The color palette requires implementation of target projects.</param>
public sealed partial class PaletteIdColorIdentifier(int value) : ColorIdentifier
{
	/// <summary>
	/// Indicates the ID value.
	/// </summary>
	public int Value { get; } = value;


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals([NotNullWhen(true)] ColorIdentifier? other)
		=> other is PaletteIdColorIdentifier comparer && Value == comparer.Value;

	[GeneratedOverridingMember(GeneratedGetHashCodeBehavior.SimpleField, nameof(Value))]
	public override partial int GetHashCode();

	[GeneratedOverridingMember(GeneratedToStringBehavior.RecordLike, nameof(Value))]
	public override partial string ToString();
}