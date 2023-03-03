﻿namespace Sudoku.Solving.Logical.Steps;

/// <summary>
/// Provides with a step that is a <b>Qiu's Deadly Pattern</b> technique.
/// </summary>
/// <param name="Conclusions"><inheritdoc/></param>
/// <param name="Views"><inheritdoc/></param>
/// <param name="Pattern">Indicates the pattern used.</param>
internal abstract record QiuDeadlyPatternStep(Conclusion[] Conclusions, View[]? Views, scoped in QiuDeadlyPattern Pattern) :
	DeadlyPatternStep(Conclusions, Views)
{
	/// <inheritdoc/>
	public sealed override decimal BaseDifficulty => 5.8M;

	/// <summary>
	/// Indicates the type of the current technique.
	/// </summary>
	public abstract int Type { get; }

	/// <inheritdoc/>
	public sealed override string Name => base.Name;

	/// <inheritdoc/>
	public sealed override string? Format => base.Format;

	/// <inheritdoc/>
	public sealed override Technique TechniqueCode
		=> Type == 5 ? Technique.LockedQiuDeadlyPattern : Enum.Parse<Technique>($"QiuDeadlyPatternType{Type}");

	/// <inheritdoc/>
	public sealed override DifficultyLevel DifficultyLevel => DifficultyLevel.Hard;

	/// <inheritdoc/>
	public sealed override TechniqueGroup TechniqueGroup => TechniqueGroup.DeadlyPattern;

	/// <inheritdoc/>
	public sealed override Rarity Rarity => Rarity.OnlyForSpecialPuzzles;

	private protected string PatternStr => Pattern.Map.ToString();
}
