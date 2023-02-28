﻿namespace Sudoku.Rating;

/// <summary>
/// Represents an extra rating case to be recorded into a technique step.
/// </summary>
/// <param name="Name">
/// Indicates the name of the extra difficulty case. This value is an English word stored in type <see cref="PhasedDifficultyRatingKinds"/>,
/// you can use constants in that type to assign to this property.
/// </param>
/// <param name="Value">Indicates the value of the target rating.</param>
/// <seealso cref="PhasedDifficultyRatingKinds"/>
public readonly record struct ExtraDifficultyCase(string Name, decimal Value);
