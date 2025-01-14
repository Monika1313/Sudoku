namespace Sudoku.Analytics.Metadata;

/// <summary>
/// Provides an attribute type that can be marked onto a <see cref="StepSearcher"/> type,
/// indicating the <see cref="StepSearcher"/> instance is unavailable on partial cases.
/// For example, Deadly Patterns are unavailable for Sukaku puzzles because we cannot determine
/// whether a candidate is having been removed before.
/// </summary>
/// <param name="cases">Indicates the cases that the current <see cref="StepSearcher"/> instance can only be running on.</param>
/// <seealso cref="StepSearcher"/>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed partial class ConditionalCasesAttribute([PrimaryConstructorParameter] ConditionalCase cases) : StepSearcherMetadataAttribute;
