﻿namespace Sudoku.Analytics;

/// <summary>
/// Represents a searcher that can creates <see cref="Step"/> instances for the specified technique.
/// </summary>
/// <param name="priority"><inheritdoc cref="Priority" path="/summary"/></param>
/// <param name="level"><inheritdoc cref="Level" path="/summary"/></param>
/// <param name="runningArea"><inheritdoc cref="RunningArea" path="/summary"/></param>
/// <seealso cref="Step"/>
public abstract class StepSearcher(
	int priority,
	StepSearcherLevel level,
	StepSearcherRunningArea runningArea = StepSearcherRunningArea.Searching | StepSearcherRunningArea.Gathering
) :
	IComparable<StepSearcher>,
	IComparisonOperators<StepSearcher, StepSearcher, bool>,
	IEquatable<StepSearcher>,
	IEqualityOperators<StepSearcher, StepSearcher, bool>
{
	/// <summary>
	/// Determines whether the current step searcher is separated one, which mean it can be created
	/// as many possible instances in a same step searchers pool.
	/// </summary>
	public bool IsSeparated => GetType().GetCustomAttribute<SeparatedAttribute>() is not null;

	/// <summary>
	/// Determines whether the current step searcher is a direct one.
	/// </summary>
	/// <remarks>
	/// If you don't know what is a direct step searcher, please visit the property
	/// <see cref="DirectAttribute"/> to learn more information.
	/// </remarks>
	/// <seealso cref="DirectAttribute"/>
	public bool IsDirect => GetType().IsDefined(typeof(DirectAttribute));

	/// <summary>
	/// Determines whether we can adjust the ordering of the current step searcher
	/// as a customized configuration option before solving a puzzle.
	/// </summary>
	/// <remarks>
	/// If you don't know what is a direct step searcher, please visit the property <see cref="FixedAttribute"/> to learn more information.
	/// </remarks>
	/// <seealso cref="FixedAttribute"/>
	public bool IsOptionsFixed => GetType().IsDefined(typeof(FixedAttribute));

	/// <summary>
	/// Determines whether the current step searcher is not supported for sukaku solving mode.
	/// </summary>
	public bool IsNotSupportedForSukaku
		=> GetType().GetCustomAttribute<ConditionalCasesAttribute>() is { Cases: var cases } && cases.Flags(ConditionalCase.Standard);

	/// <summary>
	/// Determines whether the current step searcher is disabled
	/// by option <see cref="ConditionalCase.UnlimitedTimeComplexity"/> being configured.
	/// </summary>
	/// <seealso cref="ConditionalCase.UnlimitedTimeComplexity"/>
	public bool IsConfiguredSlow
		=> GetType().GetCustomAttribute<ConditionalCasesAttribute>() is { Cases: var cases }
		&& cases.Flags(ConditionalCase.UnlimitedTimeComplexity);

	/// <summary>
	/// Determines whether the current step searcher is disabled
	/// by option <see cref="ConditionalCase.UnlimitedSpaceComplexity"/> being configured.
	/// </summary>
	/// <seealso cref="ConditionalCase.UnlimitedSpaceComplexity"/>
	public bool IsConfiguredHighAllocation
		=> GetType().GetCustomAttribute<ConditionalCasesAttribute>() is { Cases: var cases }
		&& cases.Flags(ConditionalCase.UnlimitedSpaceComplexity);

	/// <summary>
	/// Indicates the priority value of the current step searcher.
	/// This property is used for sorting multiple <see cref="StepSearcher"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Please note that the set value cannot be same for different <see cref="StepSearcher"/> types;
	/// otherwise, <see cref="InvalidOperationException"/> will be thrown while comparing with two <see cref="StepSearcher"/>s.
	/// </para>
	/// <para>
	/// This property may be automatically generated by source generator. Therefore, you may not care about implementation of this property.
	/// </para>
	/// </remarks>
	public int Priority { get; } = priority;

	/// <summary>
	/// Indicates the level that the current step searcher belongs to.
	/// </summary>
	/// <remarks>
	/// This property indicates how difficult the step searcher can be enabled. For more information,
	/// please visit type <see cref="StepSearcherLevel"/>.
	/// </remarks>
	/// <seealso cref="StepSearcherLevel"/>
	public StepSearcherLevel Level { get; } = level;

	/// <summary>
	/// Indicates the running area which describes a function where the current step searcher can be invoked.
	/// </summary>
	/// <remarks>
	/// By default, the step searcher will support both <see cref="StepSearcherRunningArea.Searching"/>
	/// and <see cref="StepSearcherRunningArea.Gathering"/>.
	/// </remarks>
	public StepSearcherRunningArea RunningArea { get; } = runningArea;

	/// <summary>
	/// The qualified type name of this instance.
	/// </summary>
	protected string TypeName => EqualityContract.Name;

	/// <summary>
	/// Indicates the <see cref="Type"/> instance that represents the reflection data for the current instance.
	/// This property is used as type checking to distinct with multiple <see cref="StepSearcher"/>s.
	/// </summary>
	protected Type EqualityContract => GetType();


	/// <summary>
	/// Determines whether the specified object has same type as the current instance.
	/// </summary>
	/// <param name="obj">The object to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sealed override bool Equals([NotNullWhen(true)] object? obj)
		=> obj switch { StepSearcher s => Equals(s), _ => EqualityContract == obj?.GetType() };

	/// <inheritdoc cref="Equals(object?)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals([NotNullWhen(true)] StepSearcher? other) => EqualityContract == other?.EqualityContract;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sealed override int GetHashCode() => EqualityContract.GetHashCode();

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(StepSearcher? other)
	{
		ArgumentNullException.ThrowIfNull(other);

		return Priority != other.Priority
			? Priority.CompareTo(other.Priority)
			: throw new InvalidOperationException("Two step searchers cannot contain a same priority value.");
	}

	/// <summary>
	/// Returns the real name of this instance.
	/// </summary>
	/// <returns>Real name of this instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sealed override string ToString() => R[$"StepSearcherName_{TypeName}"] ?? TypeName;

	/// <summary>
	/// Try to search for <see cref="Step"/> results for the current technique rule.
	/// </summary>
	/// <param name="context">
	/// <para>
	/// The analysis context. This argument offers you some elementary data configured or assigned, for the current loop of step searching.
	/// </para>
	/// <para>
	/// All available <see cref="Step"/> results will be stored in property <see cref="AnalysisContext.Accumulator"/>
	/// of this argument, if property <see cref="AnalysisContext.OnlyFindOne"/> returns <see langword="false"/>;
	/// otherwise, the property won't be used, and this method will return the first found step.
	/// </para>
	/// </param>
	/// <returns>
	/// Returns the first found step. The nullability of the return value are as belows:
	/// <list type="bullet">
	/// <item>
	/// <see langword="null"/>:
	/// <list type="bullet">
	/// <item><c><paramref name="context"/>.OnlyFindOne == <see langword="false"/></c>.</item>
	/// <item><c><paramref name="context"/>.OnlyFindOne == <see langword="true"/></c>, but nothing found.</item>
	/// </list>
	/// </item>
	/// <item>
	/// Not <see langword="null"/>:
	/// <list type="bullet">
	/// <item>
	/// <c><paramref name="context"/>.OnlyFindOne == <see langword="true"/></c>,
	/// and found <b>at least one step</b>. In this case the return value is the first found step.
	/// </item>
	/// </list>
	/// </item>
	/// </list>
	/// </returns>
	/// <seealso cref="Step"/>
	/// <seealso cref="AnalysisContext"/>
	protected internal abstract Step? GetAll(scoped ref AnalysisContext context);


	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(StepSearcher? left, StepSearcher? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(StepSearcher? left, StepSearcher? right) => !(left == right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >(StepSearcher left, StepSearcher right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator >=(StepSearcher left, StepSearcher right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <(StepSearcher left, StepSearcher right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator <=(StepSearcher left, StepSearcher right) => left.CompareTo(right) <= 0;
}