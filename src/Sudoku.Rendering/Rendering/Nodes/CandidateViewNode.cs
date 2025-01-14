namespace Sudoku.Rendering.Nodes;

/// <summary>
/// Defines a view node that highlights for a candidate.
/// </summary>
//[method: JsonConstructor]
public sealed partial class CandidateViewNode : BasicViewNode//(ColorIdentifier identifier, Candidate candidate) : BasicViewNode(identifier)
{
#pragma warning disable CS1591
	[JsonConstructor]
	public CandidateViewNode(ColorIdentifier identifier, Candidate candidate) : base(identifier) => Candidate = candidate;
#pragma warning restore CS1591


	/// <summary>
	/// Indicates the candidate highlighted.
	/// </summary>
	public Candidate Candidate { get; }// = candidate;

	/// <summary>
	/// Indicates the candidate string.
	/// </summary>
	[GeneratedDisplayName(nameof(Candidate))]
	private string CandidateString => RxCyNotation.ToCandidateString(Candidate);


	[DeconstructionMethod]
	public partial void Deconstruct(out ColorIdentifier identifier, out Candidate candidate);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override bool Equals([NotNullWhen(true)] ViewNode? other) => other is CandidateViewNode comparer && Candidate == comparer.Candidate;

	[GeneratedOverridingMember(GeneratedGetHashCodeBehavior.CallingHashCodeCombine, nameof(TypeIdentifier), nameof(Candidate))]
	public override partial int GetHashCode();

	[GeneratedOverridingMember(GeneratedToStringBehavior.RecordLike, nameof(Identifier), nameof(CandidateString))]
	public override partial string ToString();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override CandidateViewNode Clone() => new(Identifier, Candidate);
}
