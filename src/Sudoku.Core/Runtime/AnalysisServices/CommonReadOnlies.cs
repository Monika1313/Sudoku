﻿namespace Sudoku.Runtime.AnalysisServices;

/// <summary>
/// Represents a type holding some common read-only fields used by runtime or compiling-time.
/// </summary>
public static partial class CommonReadOnlies
{
	/// <summary>
	/// Indicates the invalid first set value
	/// after called <see cref="TrailingZeroCount(int)"/> and <see cref="TrailingZeroCount(uint)"/>.
	/// </summary>
	/// <remarks>
	/// For more details you want to learn about, please visit
	/// <see href="https://github.com/dotnet/runtime/blob/a67d5680186ead0c9afdab7e004389c979d5fc1f/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs#L467">this link</see>
	/// to get the inner code.
	/// </remarks>
	/// <seealso cref="TrailingZeroCount(int)"/>
	/// <seealso cref="TrailingZeroCount(uint)"/>
	public const int InvalidFirstSet = 32;

	/// <summary>
	/// Indicates the total number of unique square patterns.
	/// </summary>
	public const int UniqueSquareTemplatesCount = 162;

	/// <summary>
	/// Indicates the total number of Qiu's deadly patterns.
	/// </summary>
	public const int QiuDeadlyPatternTemplatesCount = 972;

	/// <summary>
	/// Indicates the total number of exocet patterns.
	/// </summary>
	public const int ExocetTemplatesCount = 1458;

	/// <summary>
	/// Indicates the total number of Unique Polygon (Heptagon) possible templates of size 3.
	/// </summary>
	public const int BdpTemplatesSize3Count = 14580;

	/// <summary>
	/// Indicates the total number of Unique Polygon (Octagon) possible templates of size 4.
	/// </summary>
	public const int BdpTemplatesSize4Count = 11664;

	/// <summary>
	/// Indicates the total number of multi-sector locked sets possible templates.
	/// </summary>
	public const int MultisectorLockedSetsTemplatesCount = 74601;

	/// <summary>
	/// Indicates the mask that means all rows.
	/// </summary>
	public const int AllRowsMask = 0b000_000_000__111_111_111__000_000_000;

	/// <summary>
	/// Indicates the mask that means all columns.
	/// </summary>
	public const int AllColumnsMask = 0b111_111_111__000_000_000__000_000_000;

	/// <summary>
	/// Indicates the mask that means all houses.
	/// </summary>
	public const int AllHousesMask = 0b111_111_111__111_111_111__111_111_111;

	/// <summary>
	/// Indicates the first cell offset for each house.
	/// </summary>
	public static readonly int[] HouseFirst =
	{
		0,  3,  6, 27, 30, 33, 54, 57, 60,
		0,  9, 18, 27, 36, 45, 54, 63, 72,
		0,  1,  2,  3,  4,  5,  6,  7,  8
	};

	/// <summary>
	/// The table of all <b>Unique Rectangle</b> cells.
	/// </summary>
	public static readonly int[][] UniqueRectanglePatterns =
	{
		new[] {  0,  3,  9, 12 }, new[] {  0,  4,  9, 13 }, new[] {  0,  5,  9, 14 }, new[] {  0,  6,  9, 15 }, new[] {  0,  7,  9, 16 }, new[] {  0,  8,  9, 17 }, new[] {  1,  3, 10, 12 }, new[] {  1,  4, 10, 13 }, new[] {  1,  5, 10, 14 },
		new[] {  1,  6, 10, 15 }, new[] {  1,  7, 10, 16 }, new[] {  1,  8, 10, 17 }, new[] {  2,  3, 11, 12 }, new[] {  2,  4, 11, 13 }, new[] {  2,  5, 11, 14 }, new[] {  2,  6, 11, 15 }, new[] {  2,  7, 11, 16 }, new[] {  2,  8, 11, 17 },
		new[] {  3,  6, 12, 15 }, new[] {  3,  7, 12, 16 }, new[] {  3,  8, 12, 17 }, new[] {  4,  6, 13, 15 }, new[] {  4,  7, 13, 16 }, new[] {  4,  8, 13, 17 }, new[] {  5,  6, 14, 15 }, new[] {  5,  7, 14, 16 }, new[] {  5,  8, 14, 17 },
		new[] {  0,  3, 18, 21 }, new[] {  0,  4, 18, 22 }, new[] {  0,  5, 18, 23 }, new[] {  0,  6, 18, 24 }, new[] {  0,  7, 18, 25 }, new[] {  0,  8, 18, 26 }, new[] {  1,  3, 19, 21 }, new[] {  1,  4, 19, 22 }, new[] {  1,  5, 19, 23 },
		new[] {  1,  6, 19, 24 }, new[] {  1,  7, 19, 25 }, new[] {  1,  8, 19, 26 }, new[] {  2,  3, 20, 21 }, new[] {  2,  4, 20, 22 }, new[] {  2,  5, 20, 23 }, new[] {  2,  6, 20, 24 }, new[] {  2,  7, 20, 25 }, new[] {  2,  8, 20, 26 },
		new[] {  3,  6, 21, 24 }, new[] {  3,  7, 21, 25 }, new[] {  3,  8, 21, 26 }, new[] {  4,  6, 22, 24 }, new[] {  4,  7, 22, 25 }, new[] {  4,  8, 22, 26 }, new[] {  5,  6, 23, 24 }, new[] {  5,  7, 23, 25 }, new[] {  5,  8, 23, 26 },
		new[] {  9, 12, 18, 21 }, new[] {  9, 13, 18, 22 }, new[] {  9, 14, 18, 23 }, new[] {  9, 15, 18, 24 }, new[] {  9, 16, 18, 25 }, new[] {  9, 17, 18, 26 }, new[] { 10, 12, 19, 21 }, new[] { 10, 13, 19, 22 }, new[] { 10, 14, 19, 23 },
		new[] { 10, 15, 19, 24 }, new[] { 10, 16, 19, 25 }, new[] { 10, 17, 19, 26 }, new[] { 11, 12, 20, 21 }, new[] { 11, 13, 20, 22 }, new[] { 11, 14, 20, 23 }, new[] { 11, 15, 20, 24 }, new[] { 11, 16, 20, 25 }, new[] { 11, 17, 20, 26 },
		new[] { 12, 15, 21, 24 }, new[] { 12, 16, 21, 25 }, new[] { 12, 17, 21, 26 }, new[] { 13, 15, 22, 24 }, new[] { 13, 16, 22, 25 }, new[] { 13, 17, 22, 26 }, new[] { 14, 15, 23, 24 }, new[] { 14, 16, 23, 25 }, new[] { 14, 17, 23, 26 },
		new[] { 27, 30, 36, 39 }, new[] { 27, 31, 36, 40 }, new[] { 27, 32, 36, 41 }, new[] { 27, 33, 36, 42 }, new[] { 27, 34, 36, 43 }, new[] { 27, 35, 36, 44 }, new[] { 28, 30, 37, 39 }, new[] { 28, 31, 37, 40 }, new[] { 28, 32, 37, 41 },
		new[] { 28, 33, 37, 42 }, new[] { 28, 34, 37, 43 }, new[] { 28, 35, 37, 44 }, new[] { 29, 30, 38, 39 }, new[] { 29, 31, 38, 40 }, new[] { 29, 32, 38, 41 }, new[] { 29, 33, 38, 42 }, new[] { 29, 34, 38, 43 }, new[] { 29, 35, 38, 44 },
		new[] { 30, 33, 39, 42 }, new[] { 30, 34, 39, 43 }, new[] { 30, 35, 39, 44 }, new[] { 31, 33, 40, 42 }, new[] { 31, 34, 40, 43 }, new[] { 31, 35, 40, 44 }, new[] { 32, 33, 41, 42 }, new[] { 32, 34, 41, 43 }, new[] { 32, 35, 41, 44 },
		new[] { 27, 30, 45, 48 }, new[] { 27, 31, 45, 49 }, new[] { 27, 32, 45, 50 }, new[] { 27, 33, 45, 51 }, new[] { 27, 34, 45, 52 }, new[] { 27, 35, 45, 53 }, new[] { 28, 30, 46, 48 }, new[] { 28, 31, 46, 49 }, new[] { 28, 32, 46, 50 },
		new[] { 28, 33, 46, 51 }, new[] { 28, 34, 46, 52 }, new[] { 28, 35, 46, 53 }, new[] { 29, 30, 47, 48 }, new[] { 29, 31, 47, 49 }, new[] { 29, 32, 47, 50 }, new[] { 29, 33, 47, 51 }, new[] { 29, 34, 47, 52 }, new[] { 29, 35, 47, 53 },
		new[] { 30, 33, 48, 51 }, new[] { 30, 34, 48, 52 }, new[] { 30, 35, 48, 53 }, new[] { 31, 33, 49, 51 }, new[] { 31, 34, 49, 52 }, new[] { 31, 35, 49, 53 }, new[] { 32, 33, 50, 51 }, new[] { 32, 34, 50, 52 }, new[] { 32, 35, 50, 53 },
		new[] { 36, 39, 45, 48 }, new[] { 36, 40, 45, 49 }, new[] { 36, 41, 45, 50 }, new[] { 36, 42, 45, 51 }, new[] { 36, 43, 45, 52 }, new[] { 36, 44, 45, 53 }, new[] { 37, 39, 46, 48 }, new[] { 37, 40, 46, 49 }, new[] { 37, 41, 46, 50 },
		new[] { 37, 42, 46, 51 }, new[] { 37, 43, 46, 52 }, new[] { 37, 44, 46, 53 }, new[] { 38, 39, 47, 48 }, new[] { 38, 40, 47, 49 }, new[] { 38, 41, 47, 50 }, new[] { 38, 42, 47, 51 }, new[] { 38, 43, 47, 52 }, new[] { 38, 44, 47, 53 },
		new[] { 39, 42, 48, 51 }, new[] { 39, 43, 48, 52 }, new[] { 39, 44, 48, 53 }, new[] { 40, 42, 49, 51 }, new[] { 40, 43, 49, 52 }, new[] { 40, 44, 49, 53 }, new[] { 41, 42, 50, 51 }, new[] { 41, 43, 50, 52 }, new[] { 41, 44, 50, 53 },
		new[] { 54, 57, 63, 66 }, new[] { 54, 58, 63, 67 }, new[] { 54, 59, 63, 68 }, new[] { 54, 60, 63, 69 }, new[] { 54, 61, 63, 70 }, new[] { 54, 62, 63, 71 }, new[] { 55, 57, 64, 66 }, new[] { 55, 58, 64, 67 }, new[] { 55, 59, 64, 68 },
		new[] { 55, 60, 64, 69 }, new[] { 55, 61, 64, 70 }, new[] { 55, 62, 64, 71 }, new[] { 56, 57, 65, 66 }, new[] { 56, 58, 65, 67 }, new[] { 56, 59, 65, 68 }, new[] { 56, 60, 65, 69 }, new[] { 56, 61, 65, 70 }, new[] { 56, 62, 65, 71 },
		new[] { 57, 60, 66, 69 }, new[] { 57, 61, 66, 70 }, new[] { 57, 62, 66, 71 }, new[] { 58, 60, 67, 69 }, new[] { 58, 61, 67, 70 }, new[] { 58, 62, 67, 71 }, new[] { 59, 60, 68, 69 }, new[] { 59, 61, 68, 70 }, new[] { 59, 62, 68, 71 },
		new[] { 54, 57, 72, 75 }, new[] { 54, 58, 72, 76 }, new[] { 54, 59, 72, 77 }, new[] { 54, 60, 72, 78 }, new[] { 54, 61, 72, 79 }, new[] { 54, 62, 72, 80 }, new[] { 55, 57, 73, 75 }, new[] { 55, 58, 73, 76 }, new[] { 55, 59, 73, 77 },
		new[] { 55, 60, 73, 78 }, new[] { 55, 61, 73, 79 }, new[] { 55, 62, 73, 80 }, new[] { 56, 57, 74, 75 }, new[] { 56, 58, 74, 76 }, new[] { 56, 59, 74, 77 }, new[] { 56, 60, 74, 78 }, new[] { 56, 61, 74, 79 }, new[] { 56, 62, 74, 80 },
		new[] { 57, 60, 75, 78 }, new[] { 57, 61, 75, 79 }, new[] { 57, 62, 75, 80 }, new[] { 58, 60, 76, 78 }, new[] { 58, 61, 76, 79 }, new[] { 58, 62, 76, 80 }, new[] { 59, 60, 77, 78 }, new[] { 59, 61, 77, 79 }, new[] { 59, 62, 77, 80 },
		new[] { 63, 66, 72, 75 }, new[] { 63, 67, 72, 76 }, new[] { 63, 68, 72, 77 }, new[] { 63, 69, 72, 78 }, new[] { 63, 70, 72, 79 }, new[] { 63, 71, 72, 80 }, new[] { 64, 66, 73, 75 }, new[] { 64, 67, 73, 76 }, new[] { 64, 68, 73, 77 },
		new[] { 64, 69, 73, 78 }, new[] { 64, 70, 73, 79 }, new[] { 64, 71, 73, 80 }, new[] { 65, 66, 74, 75 }, new[] { 65, 67, 74, 76 }, new[] { 65, 68, 74, 77 }, new[] { 65, 69, 74, 78 }, new[] { 65, 70, 74, 79 }, new[] { 65, 71, 74, 80 },
		new[] { 66, 69, 75, 78 }, new[] { 66, 70, 75, 79 }, new[] { 66, 71, 75, 80 }, new[] { 67, 69, 76, 78 }, new[] { 67, 70, 76, 79 }, new[] { 67, 71, 76, 80 }, new[] { 68, 69, 77, 78 }, new[] { 68, 70, 77, 79 }, new[] { 68, 71, 77, 80 },
		new[] {  0,  1, 27, 28 }, new[] {  0,  1, 36, 37 }, new[] {  0,  1, 45, 46 }, new[] {  0,  1, 54, 55 }, new[] {  0,  1, 63, 64 }, new[] {  0,  1, 72, 73 }, new[] {  9, 10, 27, 28 }, new[] {  9, 10, 36, 37 }, new[] {  9, 10, 45, 46 },
		new[] {  9, 10, 54, 55 }, new[] {  9, 10, 63, 64 }, new[] {  9, 10, 72, 73 }, new[] { 18, 19, 27, 28 }, new[] { 18, 19, 36, 37 }, new[] { 18, 19, 45, 46 }, new[] { 18, 19, 54, 55 }, new[] { 18, 19, 63, 64 }, new[] { 18, 19, 72, 73 },
		new[] { 27, 28, 54, 55 }, new[] { 27, 28, 63, 64 }, new[] { 27, 28, 72, 73 }, new[] { 36, 37, 54, 55 }, new[] { 36, 37, 63, 64 }, new[] { 36, 37, 72, 73 }, new[] { 45, 46, 54, 55 }, new[] { 45, 46, 63, 64 }, new[] { 45, 46, 72, 73 },
		new[] {  0,  2, 27, 29 }, new[] {  0,  2, 36, 38 }, new[] {  0,  2, 45, 47 }, new[] {  0,  2, 54, 56 }, new[] {  0,  2, 63, 65 }, new[] {  0,  2, 72, 74 }, new[] {  9, 11, 27, 29 }, new[] {  9, 11, 36, 38 }, new[] {  9, 11, 45, 47 },
		new[] {  9, 11, 54, 56 }, new[] {  9, 11, 63, 65 }, new[] {  9, 11, 72, 74 }, new[] { 18, 20, 27, 29 }, new[] { 18, 20, 36, 38 }, new[] { 18, 20, 45, 47 }, new[] { 18, 20, 54, 56 }, new[] { 18, 20, 63, 65 }, new[] { 18, 20, 72, 74 },
		new[] { 27, 29, 54, 56 }, new[] { 27, 29, 63, 65 }, new[] { 27, 29, 72, 74 }, new[] { 36, 38, 54, 56 }, new[] { 36, 38, 63, 65 }, new[] { 36, 38, 72, 74 }, new[] { 45, 47, 54, 56 }, new[] { 45, 47, 63, 65 }, new[] { 45, 47, 72, 74 },
		new[] {  1,  2, 28, 29 }, new[] {  1,  2, 37, 38 }, new[] {  1,  2, 46, 47 }, new[] {  1,  2, 55, 56 }, new[] {  1,  2, 64, 65 }, new[] {  1,  2, 73, 74 }, new[] { 10, 11, 28, 29 }, new[] { 10, 11, 37, 38 }, new[] { 10, 11, 46, 47 },
		new[] { 10, 11, 55, 56 }, new[] { 10, 11, 64, 65 }, new[] { 10, 11, 73, 74 }, new[] { 19, 20, 28, 29 }, new[] { 19, 20, 37, 38 }, new[] { 19, 20, 46, 47 }, new[] { 19, 20, 55, 56 }, new[] { 19, 20, 64, 65 }, new[] { 19, 20, 73, 74 },
		new[] { 28, 29, 55, 56 }, new[] { 28, 29, 64, 65 }, new[] { 28, 29, 73, 74 }, new[] { 37, 38, 55, 56 }, new[] { 37, 38, 64, 65 }, new[] { 37, 38, 73, 74 }, new[] { 46, 47, 55, 56 }, new[] { 46, 47, 64, 65 }, new[] { 46, 47, 73, 74 },
		new[] {  3,  4, 30, 31 }, new[] {  3,  4, 39, 40 }, new[] {  3,  4, 48, 49 }, new[] {  3,  4, 57, 58 }, new[] {  3,  4, 66, 67 }, new[] {  3,  4, 75, 76 }, new[] { 12, 13, 30, 31 }, new[] { 12, 13, 39, 40 }, new[] { 12, 13, 48, 49 },
		new[] { 12, 13, 57, 58 }, new[] { 12, 13, 66, 67 }, new[] { 12, 13, 75, 76 }, new[] { 21, 22, 30, 31 }, new[] { 21, 22, 39, 40 }, new[] { 21, 22, 48, 49 }, new[] { 21, 22, 57, 58 }, new[] { 21, 22, 66, 67 }, new[] { 21, 22, 75, 76 },
		new[] { 30, 31, 57, 58 }, new[] { 30, 31, 66, 67 }, new[] { 30, 31, 75, 76 }, new[] { 39, 40, 57, 58 }, new[] { 39, 40, 66, 67 }, new[] { 39, 40, 75, 76 }, new[] { 48, 49, 57, 58 }, new[] { 48, 49, 66, 67 }, new[] { 48, 49, 75, 76 },
		new[] {  3,  5, 30, 32 }, new[] {  3,  5, 39, 41 }, new[] {  3,  5, 48, 50 }, new[] {  3,  5, 57, 59 }, new[] {  3,  5, 66, 68 }, new[] {  3,  5, 75, 77 }, new[] { 12, 14, 30, 32 }, new[] { 12, 14, 39, 41 }, new[] { 12, 14, 48, 50 },
		new[] { 12, 14, 57, 59 }, new[] { 12, 14, 66, 68 }, new[] { 12, 14, 75, 77 }, new[] { 21, 23, 30, 32 }, new[] { 21, 23, 39, 41 }, new[] { 21, 23, 48, 50 }, new[] { 21, 23, 57, 59 }, new[] { 21, 23, 66, 68 }, new[] { 21, 23, 75, 77 },
		new[] { 30, 32, 57, 59 }, new[] { 30, 32, 66, 68 }, new[] { 30, 32, 75, 77 }, new[] { 39, 41, 57, 59 }, new[] { 39, 41, 66, 68 }, new[] { 39, 41, 75, 77 }, new[] { 48, 50, 57, 59 }, new[] { 48, 50, 66, 68 }, new[] { 48, 50, 75, 77 },
		new[] {  4,  5, 31, 32 }, new[] {  4,  5, 40, 41 }, new[] {  4,  5, 49, 50 }, new[] {  4,  5, 58, 59 }, new[] {  4,  5, 67, 68 }, new[] {  4,  5, 76, 77 }, new[] { 13, 14, 31, 32 }, new[] { 13, 14, 40, 41 }, new[] { 13, 14, 49, 50 },
		new[] { 13, 14, 58, 59 }, new[] { 13, 14, 67, 68 }, new[] { 13, 14, 76, 77 }, new[] { 22, 23, 31, 32 }, new[] { 22, 23, 40, 41 }, new[] { 22, 23, 49, 50 }, new[] { 22, 23, 58, 59 }, new[] { 22, 23, 67, 68 }, new[] { 22, 23, 76, 77 },
		new[] { 31, 32, 58, 59 }, new[] { 31, 32, 67, 68 }, new[] { 31, 32, 76, 77 }, new[] { 40, 41, 58, 59 }, new[] { 40, 41, 67, 68 }, new[] { 40, 41, 76, 77 }, new[] { 49, 50, 58, 59 }, new[] { 49, 50, 67, 68 }, new[] { 49, 50, 76, 77 },
		new[] {  6,  7, 33, 34 }, new[] {  6,  7, 42, 43 }, new[] {  6,  7, 51, 52 }, new[] {  6,  7, 60, 61 }, new[] {  6,  7, 69, 70 }, new[] {  6,  7, 78, 79 }, new[] { 15, 16, 33, 34 }, new[] { 15, 16, 42, 43 }, new[] { 15, 16, 51, 52 },
		new[] { 15, 16, 60, 61 }, new[] { 15, 16, 69, 70 }, new[] { 15, 16, 78, 79 }, new[] { 24, 25, 33, 34 }, new[] { 24, 25, 42, 43 }, new[] { 24, 25, 51, 52 }, new[] { 24, 25, 60, 61 }, new[] { 24, 25, 69, 70 }, new[] { 24, 25, 78, 79 },
		new[] { 33, 34, 60, 61 }, new[] { 33, 34, 69, 70 }, new[] { 33, 34, 78, 79 }, new[] { 42, 43, 60, 61 }, new[] { 42, 43, 69, 70 }, new[] { 42, 43, 78, 79 }, new[] { 51, 52, 60, 61 }, new[] { 51, 52, 69, 70 }, new[] { 51, 52, 78, 79 },
		new[] {  6,  8, 33, 35 }, new[] {  6,  8, 42, 44 }, new[] {  6,  8, 51, 53 }, new[] {  6,  8, 60, 62 }, new[] {  6,  8, 69, 71 }, new[] {  6,  8, 78, 80 }, new[] { 15, 17, 33, 35 }, new[] { 15, 17, 42, 44 }, new[] { 15, 17, 51, 53 },
		new[] { 15, 17, 60, 62 }, new[] { 15, 17, 69, 71 }, new[] { 15, 17, 78, 80 }, new[] { 24, 26, 33, 35 }, new[] { 24, 26, 42, 44 }, new[] { 24, 26, 51, 53 }, new[] { 24, 26, 60, 62 }, new[] { 24, 26, 69, 71 }, new[] { 24, 26, 78, 80 },
		new[] { 33, 35, 60, 62 }, new[] { 33, 35, 69, 71 }, new[] { 33, 35, 78, 80 }, new[] { 42, 44, 60, 62 }, new[] { 42, 44, 69, 71 }, new[] { 42, 44, 78, 80 }, new[] { 51, 53, 60, 62 }, new[] { 51, 53, 69, 71 }, new[] { 51, 53, 78, 80 },
		new[] {  7,  8, 34, 35 }, new[] {  7,  8, 43, 44 }, new[] {  7,  8, 52, 53 }, new[] {  7,  8, 61, 62 }, new[] {  7,  8, 70, 71 }, new[] {  7,  8, 79, 80 }, new[] { 16, 17, 34, 35 }, new[] { 16, 17, 43, 44 }, new[] { 16, 17, 52, 53 },
		new[] { 16, 17, 61, 62 }, new[] { 16, 17, 70, 71 }, new[] { 16, 17, 79, 80 }, new[] { 25, 26, 34, 35 }, new[] { 25, 26, 43, 44 }, new[] { 25, 26, 52, 53 }, new[] { 25, 26, 61, 62 }, new[] { 25, 26, 70, 71 }, new[] { 25, 26, 79, 80 },
		new[] { 34, 35, 61, 62 }, new[] { 34, 35, 70, 71 }, new[] { 34, 35, 79, 80 }, new[] { 43, 44, 61, 62 }, new[] { 43, 44, 70, 71 }, new[] { 43, 44, 79, 80 }, new[] { 52, 53, 61, 62 }, new[] { 52, 53, 70, 71 }, new[] { 52, 53, 79, 80 }
	};

	/// <summary>
	/// <para>Indicates a table for each cell's peers.</para>
	/// </summary>
	/// <example>
	/// '<c>Peers[0]</c>': the array of peers for the cell 0 (row 1 column 1).
	/// </example>
	public static readonly int[][] Peers =
	{
		new[] {  1,  2,  9, 10, 11, 18, 19, 20,  3,  4,  5,  6,  7,  8, 27, 36, 45, 54, 63, 72, },
		new[] {  2,  9, 10, 11, 18, 19, 20,  3,  4,  5,  6,  7,  8, 28, 37, 46, 55, 64, 73,  0, },
		new[] {  1,  9, 10, 11, 18, 19, 20,  3,  4,  5,  6,  7,  8, 29, 38, 47, 56, 65, 74,  0, },
		new[] {  4,  5, 12, 13, 14, 21, 22, 23,  1,  2,  6,  7,  8, 30, 39, 48, 57, 66, 75,  0, },
		new[] {  3,  5, 12, 13, 14, 21, 22, 23,  1,  2,  6,  7,  8, 31, 40, 49, 58, 67, 76,  0, },
		new[] {  3,  4, 12, 13, 14, 21, 22, 23,  1,  2,  6,  7,  8, 32, 41, 50, 59, 68, 77,  0, },
		new[] {  7,  8, 15, 16, 17, 24, 25, 26,  1,  2,  3,  4,  5, 33, 42, 51, 60, 69, 78,  0, },
		new[] {  6,  8, 15, 16, 17, 24, 25, 26,  1,  2,  3,  4,  5, 34, 43, 52, 61, 70, 79,  0, },
		new[] {  6,  7, 15, 16, 17, 24, 25, 26,  1,  2,  3,  4,  5, 35, 44, 53, 62, 71, 80,  0, },
		new[] {  1,  2, 10, 11, 18, 19, 20, 12, 13, 14, 15, 16, 17, 27, 36, 45, 54, 63, 72,  0, },
		new[] {  1,  2,  9, 11, 18, 19, 20, 12, 13, 14, 15, 16, 17, 28, 37, 46, 55, 64, 73,  0, },
		new[] {  1,  2,  9, 10, 18, 19, 20, 12, 13, 14, 15, 16, 17, 29, 38, 47, 56, 65, 74,  0, },
		new[] {  3,  4,  5, 13, 14, 21, 22, 23,  9, 10, 11, 15, 16, 17, 30, 39, 48, 57, 66, 75, },
		new[] {  3,  4,  5, 12, 14, 21, 22, 23,  9, 10, 11, 15, 16, 17, 31, 40, 49, 58, 67, 76, },
		new[] {  3,  4,  5, 12, 13, 21, 22, 23,  9, 10, 11, 15, 16, 17, 32, 41, 50, 59, 68, 77, },
		new[] {  6,  7,  8, 16, 17, 24, 25, 26,  9, 10, 11, 12, 13, 14, 33, 42, 51, 60, 69, 78, },
		new[] {  6,  7,  8, 15, 17, 24, 25, 26,  9, 10, 11, 12, 13, 14, 34, 43, 52, 61, 70, 79, },
		new[] {  6,  7,  8, 15, 16, 24, 25, 26,  9, 10, 11, 12, 13, 14, 35, 44, 53, 62, 71, 80, },
		new[] {  1,  2,  9, 10, 11, 19, 20, 21, 22, 23, 24, 25, 26, 27, 36, 45, 54, 63, 72,  0, },
		new[] {  1,  2,  9, 10, 11, 18, 20, 21, 22, 23, 24, 25, 26, 28, 37, 46, 55, 64, 73,  0, },
		new[] {  1,  2,  9, 10, 11, 18, 19, 21, 22, 23, 24, 25, 26, 29, 38, 47, 56, 65, 74,  0, },
		new[] {  3,  4,  5, 12, 13, 14, 22, 23, 18, 19, 20, 24, 25, 26, 30, 39, 48, 57, 66, 75, },
		new[] {  3,  4,  5, 12, 13, 14, 21, 23, 18, 19, 20, 24, 25, 26, 31, 40, 49, 58, 67, 76, },
		new[] {  3,  4,  5, 12, 13, 14, 21, 22, 18, 19, 20, 24, 25, 26, 32, 41, 50, 59, 68, 77, },
		new[] {  6,  7,  8, 15, 16, 17, 25, 26, 18, 19, 20, 21, 22, 23, 33, 42, 51, 60, 69, 78, },
		new[] {  6,  7,  8, 15, 16, 17, 24, 26, 18, 19, 20, 21, 22, 23, 34, 43, 52, 61, 70, 79, },
		new[] {  6,  7,  8, 15, 16, 17, 24, 25, 18, 19, 20, 21, 22, 23, 35, 44, 53, 62, 71, 80, },
		new[] { 28, 29, 36, 37, 38, 45, 46, 47, 30, 31, 32, 33, 34, 35,  9, 18, 54, 63, 72,  0, },
		new[] { 27, 29, 36, 37, 38, 45, 46, 47, 30, 31, 32, 33, 34, 35,  1, 10, 19, 55, 64, 73, },
		new[] { 27, 28, 36, 37, 38, 45, 46, 47, 30, 31, 32, 33, 34, 35,  2, 11, 20, 56, 65, 74, },
		new[] { 31, 32, 39, 40, 41, 48, 49, 50, 27, 28, 29, 33, 34, 35,  3, 12, 21, 57, 66, 75, },
		new[] { 30, 32, 39, 40, 41, 48, 49, 50, 27, 28, 29, 33, 34, 35,  4, 13, 22, 58, 67, 76, },
		new[] { 30, 31, 39, 40, 41, 48, 49, 50, 27, 28, 29, 33, 34, 35,  5, 14, 23, 59, 68, 77, },
		new[] { 34, 35, 42, 43, 44, 51, 52, 53, 27, 28, 29, 30, 31, 32,  6, 15, 24, 60, 69, 78, },
		new[] { 33, 35, 42, 43, 44, 51, 52, 53, 27, 28, 29, 30, 31, 32,  7, 16, 25, 61, 70, 79, },
		new[] { 33, 34, 42, 43, 44, 51, 52, 53, 27, 28, 29, 30, 31, 32,  8, 17, 26, 62, 71, 80, },
		new[] { 27, 28, 29, 37, 38, 45, 46, 47, 39, 40, 41, 42, 43, 44,  9, 18, 54, 63, 72,  0, },
		new[] { 27, 28, 29, 36, 38, 45, 46, 47, 39, 40, 41, 42, 43, 44,  1, 10, 19, 55, 64, 73, },
		new[] { 27, 28, 29, 36, 37, 45, 46, 47, 39, 40, 41, 42, 43, 44,  2, 11, 20, 56, 65, 74, },
		new[] { 30, 31, 32, 40, 41, 48, 49, 50, 36, 37, 38, 42, 43, 44,  3, 12, 21, 57, 66, 75, },
		new[] { 30, 31, 32, 39, 41, 48, 49, 50, 36, 37, 38, 42, 43, 44,  4, 13, 22, 58, 67, 76, },
		new[] { 30, 31, 32, 39, 40, 48, 49, 50, 36, 37, 38, 42, 43, 44,  5, 14, 23, 59, 68, 77, },
		new[] { 33, 34, 35, 43, 44, 51, 52, 53, 36, 37, 38, 39, 40, 41,  6, 15, 24, 60, 69, 78, },
		new[] { 33, 34, 35, 42, 44, 51, 52, 53, 36, 37, 38, 39, 40, 41,  7, 16, 25, 61, 70, 79, },
		new[] { 33, 34, 35, 42, 43, 51, 52, 53, 36, 37, 38, 39, 40, 41,  8, 17, 26, 62, 71, 80, },
		new[] { 27, 28, 29, 36, 37, 38, 46, 47, 48, 49, 50, 51, 52, 53,  9, 18, 54, 63, 72,  0, },
		new[] { 27, 28, 29, 36, 37, 38, 45, 47, 48, 49, 50, 51, 52, 53,  1, 10, 19, 55, 64, 73, },
		new[] { 27, 28, 29, 36, 37, 38, 45, 46, 48, 49, 50, 51, 52, 53,  2, 11, 20, 56, 65, 74, },
		new[] { 30, 31, 32, 39, 40, 41, 49, 50, 45, 46, 47, 51, 52, 53,  3, 12, 21, 57, 66, 75, },
		new[] { 30, 31, 32, 39, 40, 41, 48, 50, 45, 46, 47, 51, 52, 53,  4, 13, 22, 58, 67, 76, },
		new[] { 30, 31, 32, 39, 40, 41, 48, 49, 45, 46, 47, 51, 52, 53,  5, 14, 23, 59, 68, 77, },
		new[] { 33, 34, 35, 42, 43, 44, 52, 53, 45, 46, 47, 48, 49, 50,  6, 15, 24, 60, 69, 78, },
		new[] { 33, 34, 35, 42, 43, 44, 51, 53, 45, 46, 47, 48, 49, 50,  7, 16, 25, 61, 70, 79, },
		new[] { 33, 34, 35, 42, 43, 44, 51, 52, 45, 46, 47, 48, 49, 50,  8, 17, 26, 62, 71, 80, },
		new[] { 55, 56, 63, 64, 65, 72, 73, 74, 57, 58, 59, 60, 61, 62,  9, 18, 27, 36, 45,  0, },
		new[] { 54, 56, 63, 64, 65, 72, 73, 74, 57, 58, 59, 60, 61, 62,  1, 10, 19, 28, 37, 46, },
		new[] { 54, 55, 63, 64, 65, 72, 73, 74, 57, 58, 59, 60, 61, 62,  2, 11, 20, 29, 38, 47, },
		new[] { 58, 59, 66, 67, 68, 75, 76, 77, 54, 55, 56, 60, 61, 62,  3, 12, 21, 30, 39, 48, },
		new[] { 57, 59, 66, 67, 68, 75, 76, 77, 54, 55, 56, 60, 61, 62,  4, 13, 22, 31, 40, 49, },
		new[] { 57, 58, 66, 67, 68, 75, 76, 77, 54, 55, 56, 60, 61, 62,  5, 14, 23, 32, 41, 50, },
		new[] { 61, 62, 69, 70, 71, 78, 79, 80, 54, 55, 56, 57, 58, 59,  6, 15, 24, 33, 42, 51, },
		new[] { 60, 62, 69, 70, 71, 78, 79, 80, 54, 55, 56, 57, 58, 59,  7, 16, 25, 34, 43, 52, },
		new[] { 60, 61, 69, 70, 71, 78, 79, 80, 54, 55, 56, 57, 58, 59,  8, 17, 26, 35, 44, 53, },
		new[] { 54, 55, 56, 64, 65, 72, 73, 74, 66, 67, 68, 69, 70, 71,  9, 18, 27, 36, 45,  0, },
		new[] { 54, 55, 56, 63, 65, 72, 73, 74, 66, 67, 68, 69, 70, 71,  1, 10, 19, 28, 37, 46, },
		new[] { 54, 55, 56, 63, 64, 72, 73, 74, 66, 67, 68, 69, 70, 71,  2, 11, 20, 29, 38, 47, },
		new[] { 57, 58, 59, 67, 68, 75, 76, 77, 63, 64, 65, 69, 70, 71,  3, 12, 21, 30, 39, 48, },
		new[] { 57, 58, 59, 66, 68, 75, 76, 77, 63, 64, 65, 69, 70, 71,  4, 13, 22, 31, 40, 49, },
		new[] { 57, 58, 59, 66, 67, 75, 76, 77, 63, 64, 65, 69, 70, 71,  5, 14, 23, 32, 41, 50, },
		new[] { 60, 61, 62, 70, 71, 78, 79, 80, 63, 64, 65, 66, 67, 68,  6, 15, 24, 33, 42, 51, },
		new[] { 60, 61, 62, 69, 71, 78, 79, 80, 63, 64, 65, 66, 67, 68,  7, 16, 25, 34, 43, 52, },
		new[] { 60, 61, 62, 69, 70, 78, 79, 80, 63, 64, 65, 66, 67, 68,  8, 17, 26, 35, 44, 53, },
		new[] { 54, 55, 56, 63, 64, 65, 73, 74, 75, 76, 77, 78, 79, 80,  9, 18, 27, 36, 45,  0, },
		new[] { 54, 55, 56, 63, 64, 65, 72, 74, 75, 76, 77, 78, 79, 80,  1, 10, 19, 28, 37, 46, },
		new[] { 54, 55, 56, 63, 64, 65, 72, 73, 75, 76, 77, 78, 79, 80,  2, 11, 20, 29, 38, 47, },
		new[] { 57, 58, 59, 66, 67, 68, 76, 77, 72, 73, 74, 78, 79, 80,  3, 12, 21, 30, 39, 48, },
		new[] { 57, 58, 59, 66, 67, 68, 75, 77, 72, 73, 74, 78, 79, 80,  4, 13, 22, 31, 40, 49, },
		new[] { 57, 58, 59, 66, 67, 68, 75, 76, 72, 73, 74, 78, 79, 80,  5, 14, 23, 32, 41, 50, },
		new[] { 60, 61, 62, 69, 70, 71, 79, 80, 72, 73, 74, 75, 76, 77,  6, 15, 24, 33, 42, 51, },
		new[] { 60, 61, 62, 69, 70, 71, 78, 80, 72, 73, 74, 75, 76, 77,  7, 16, 25, 34, 43, 52, },
		new[] { 60, 61, 62, 69, 70, 71, 78, 79, 72, 73, 74, 75, 76, 77,  8, 17, 26, 35, 44, 53, },
	};

	/// <summary>
	/// <para>
	/// The map of all cell offsets in its specified house.
	/// The indices is between 0 and 26, where:
	/// <list type="table">
	/// <item>
	/// <term><c>0..9</c></term>
	/// <description>Block 1 to 9.</description>
	/// </item>
	/// <item>
	/// <term><c>9..18</c></term>
	/// <description>Row 1 to 9.</description>
	/// </item>
	/// <item>
	/// <term><c>18..27</c></term>
	/// <description>Column 1 to 9.</description>
	/// </item>
	/// </list>
	/// </para>
	/// </summary>
	/// <example>
	/// '<c>HouseCells[0]</c>': all cell offsets in the house 0 (block 1).
	/// </example>
	public static readonly int[][] HouseCells =
	{
		new[] {  0,  1,  2,  9, 10, 11, 18, 19, 20, },
		new[] {  3,  4,  5, 12, 13, 14, 21, 22, 23, },
		new[] {  6,  7,  8, 15, 16, 17, 24, 25, 26, },
		new[] { 27, 28, 29, 36, 37, 38, 45, 46, 47, },
		new[] { 30, 31, 32, 39, 40, 41, 48, 49, 50, },
		new[] { 33, 34, 35, 42, 43, 44, 51, 52, 53, },
		new[] { 54, 55, 56, 63, 64, 65, 72, 73, 74, },
		new[] { 57, 58, 59, 66, 67, 68, 75, 76, 77, },
		new[] { 60, 61, 62, 69, 70, 71, 78, 79, 80, },
		new[] {  0,  1,  2,  3,  4,  5,  6,  7,  8, },
		new[] {  9, 10, 11, 12, 13, 14, 15, 16, 17, },
		new[] { 18, 19, 20, 21, 22, 23, 24, 25, 26, },
		new[] { 27, 28, 29, 30, 31, 32, 33, 34, 35, },
		new[] { 36, 37, 38, 39, 40, 41, 42, 43, 44, },
		new[] { 45, 46, 47, 48, 49, 50, 51, 52, 53, },
		new[] { 54, 55, 56, 57, 58, 59, 60, 61, 62, },
		new[] { 63, 64, 65, 66, 67, 68, 69, 70, 71, },
		new[] { 72, 73, 74, 75, 76, 77, 78, 79, 80, },
		new[] {  0,  9, 18, 27, 36, 45, 54, 63, 72, },
		new[] {  1, 10, 19, 28, 37, 46, 55, 64, 73, },
		new[] {  2, 11, 20, 29, 38, 47, 56, 65, 74, },
		new[] {  3, 12, 21, 30, 39, 48, 57, 66, 75, },
		new[] {  4, 13, 22, 31, 40, 49, 58, 67, 76, },
		new[] {  5, 14, 23, 32, 41, 50, 59, 68, 77, },
		new[] {  6, 15, 24, 33, 42, 51, 60, 69, 78, },
		new[] {  7, 16, 25, 34, 43, 52, 61, 70, 79, },
		new[] {  8, 17, 26, 35, 44, 53, 62, 71, 80, },
	};

	/// <summary>
	/// <para>The names of all subsets by their sizes.</para>
	/// <para>
	/// For example, if you want to get the name of the size 3, the code will be
	/// <code><![CDATA[string name = SubsetNames[3];]]></code>
	/// Here the digit <c>3</c> is the size you want to get.
	/// </para>
	/// </summary>
	public static readonly string[] SubsetNames =
	{
		string.Empty, "Single", "Pair", "Triple", "Quadruple",
		"Quintuple", "Sextuple", "Septuple"
	};

	/// <summary>
	/// The names of all fishes by their sizes.
	/// </summary>
	public static readonly string[] FishNames =
	{
		string.Empty, "Cyclopsfish", "X-Wing", "Swordfish", "Jellyfish",
		"Squirmbag", "Whale", "Leviathan"
	};

	/// <summary>
	/// <para>The names of all regular wings by their sizes.</para>
	/// <para>
	/// For example, if you want to get the name of the size 3, the code will be
	/// <code><![CDATA[string name = RegularWingNames[3];]]></code>
	/// Here the digit <c>3</c> is the size you want to get.
	/// </para>
	/// </summary>
	public static readonly string[] RegularWingNames =
	{
		string.Empty, string.Empty, string.Empty, string.Empty, "WXYZ-Wing", "VWXYZ-Wing",
		"UVWXYZ-Wing", "TUVWXYZ-Wing", "STUVWXYZ-Wing", "RSTUVWXYZ-Wing"
	};

	/// <summary>
	/// Indicates all grid maps that a grid contains.
	/// </summary>
	/// <example>
	/// '<c>HouseMaps[0]</c>': The map containing all cells in the block 1.
	/// </example>
	public static readonly Cells[] HouseMaps;

	/// <summary>
	/// Indicates the peer maps using <see cref="Peers"/> table.
	/// </summary>
	/// <seealso cref="Peers"/>
	public static readonly Cells[] PeerMaps;

	/// <summary>
	/// Indicates the possible house types to iterate.
	/// </summary>
	public static readonly HouseType[] Houses = { HouseType.Block, HouseType.Row, HouseType.Column };

	/// <summary>
	/// <para>
	/// Indicates all maps that forms the each intersection. The pattern will be like:
	/// <code><![CDATA[
	/// .-------.-------.-------.
	/// | C C C | A A A | A A A |
	/// | B B B | . . . | . . . |
	/// | B B B | . . . | . . . |
	/// '-------'-------'-------'
	/// ]]></code>
	/// </para>
	/// <para>
	/// In addition, in this data structure, a <b>CoverSet</b> is a block and a <b>BaseSet</b> is a line.
	/// </para>
	/// </summary>
	public static readonly IReadOnlyDictionary<(byte Line, byte Block), (Cells LineMap, Cells BlockMap, Cells IntersectionMap, byte[] OtherBlocks)> IntersectionMaps;

	/// <summary>
	/// <para>The table of all blocks to iterate for each blocks.</para>
	/// <para>
	/// This field is only used for providing the data for another field <see cref="IntersectionMaps"/>.
	/// </para>
	/// </summary>
	/// <seealso cref="IntersectionMaps"/>
	internal static readonly byte[][] IntersectionBlockTable =
	{
		new byte[] {  1,  2 }, new byte[] {  0,  2 }, new byte[] {  0,  1 },
		new byte[] {  1,  2 }, new byte[] {  0,  2 }, new byte[] {  0,  1 },
		new byte[] {  1,  2 }, new byte[] {  0,  2 }, new byte[] {  0,  1 },
		new byte[] {  4,  5 }, new byte[] {  3,  5 }, new byte[] {  3,  4 },
		new byte[] {  4,  5 }, new byte[] {  3,  5 }, new byte[] {  3,  4 },
		new byte[] {  4,  5 }, new byte[] {  3,  5 }, new byte[] {  3,  4 },
		new byte[] {  7,  8 }, new byte[] {  6,  8 }, new byte[] {  6,  7 },
		new byte[] {  7,  8 }, new byte[] {  6,  8 }, new byte[] {  6,  7 },
		new byte[] {  7,  8 }, new byte[] {  6,  8 }, new byte[] {  6,  7 },
		new byte[] {  3,  6 }, new byte[] {  0,  6 }, new byte[] {  0,  3 },
		new byte[] {  3,  6 }, new byte[] {  0,  6 }, new byte[] {  0,  3 },
		new byte[] {  3,  6 }, new byte[] {  0,  6 }, new byte[] {  0,  3 },
		new byte[] {  4,  7 }, new byte[] {  1,  7 }, new byte[] {  1,  4 },
		new byte[] {  4,  7 }, new byte[] {  1,  7 }, new byte[] {  1,  4 },
		new byte[] {  4,  7 }, new byte[] {  1,  7 }, new byte[] {  1,  4 },
		new byte[] {  5,  8 }, new byte[] {  2,  8 }, new byte[] {  2,  5 },
		new byte[] {  5,  8 }, new byte[] {  2,  8 }, new byte[] {  2,  5 },
		new byte[] {  5,  8 }, new byte[] {  2,  8 }, new byte[] {  2,  5 },
	};

	/// <summary>
	/// Indicates the combinatorial numbers from <c>C(1, 1)</c> to <c>C(30, 30)</c>.
	/// </summary>
	internal static readonly int[,] Combinatorials =
	{
		{  1,  -1,   -1,    -1,     -1,     -1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  2,   1,   -1,    -1,     -1,     -1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  3,   3,    1,    -1,     -1,     -1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  4,   6,    4,     1,     -1,     -1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  5,  10,   10,     5,      1,     -1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  6,  15,   20,    15,      6,      1,      -1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  7,  21,   35,    35,     21,      7,       1,      -1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  8,  28,   56,    70,     56,     28,       8,       1,       -1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{  9,  36,   84,   126,    126,     84,      36,       9,        1,       -1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 10,  45,  120,   210,    252,    210,     120,      45,       10,        1,       -1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 11,  55,  165,   330,    462,    462,     330,     165,       55,       11,        1,       -1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 12,  66,  220,   495,    792,    924,     792,     495,      220,       66,       12,        1,        -1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 13,  78,  286,   715,   1287,   1716,    1716,    1287,      715,      286,       78,       13,         1,        -1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 14,  91,  364,  1001,   2002,   3003,    3432,    3003,     2002,     1001,      364,       91,        14,         1,        -1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 15, 105,  455,  1365,   3003,   5005,    6435,    6435,     5005,     3003,     1365,      455,       105,        15,         1,        -1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 16, 120,  560,  1820,   4368,   8008,   11440,   12870,    11440,     8008,     4368,     1820,       560,       120,        16,         1,        -1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 17, 136,  680,  2380,   6188,  12376,   19448,   24310,    24310,    19448,    12376,     6188,      2380,       680,       136,        17,         1,       -1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 18, 153,  816,  3060,   8568,  18564,   31824,   43758,    48620,    43758,    31824,    18564,      8568,      3060,       816,       153,        18,        1,       -1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 19, 171,  969,  3876,  11628,  27132,   50388,   75582,    92378,    92378,    75582,    50388,     27132,     11628,      3876,       969,       171,       19,        1,       -1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 20, 190, 1140,  4845,  15504,  38760,   77520,  125970,   167960,   184756,   167960,   125970,     77520,     38760,     15504,      4845,      1140,      190,       20,        1,       -1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 21, 210, 1330,  5985,  20349,  54264,  116280,  203490,   293930,   352716,   352716,   293930,    203490,    116280,     54264,     20349,      5985,     1330,      210,       21,        1,      -1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 22, 231, 1540,  7315,  26334,  74613,  170544,  319770,   497420,   646646,   705432,   646646,    497420,    319770,    170544,     74613,     26334,     7315,     1540,      231,       22,       1,      -1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 23, 253, 1771,  8855,  33649, 100947,  245157,  490314,   817190,  1144066,  1352078,  1352078,   1144066,    817190,    490314,    245157,    100947,    33649,     8855,     1771,      253,      23,       1,     -1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 24, 276, 2024, 10626,  42504, 134596,  346104,  735471,  1307504,  1961256,  2496144,  2704156,   2496144,   1961256,   1307504,    735471,    346104,   134596,    42504,    10626,     2024,     276,      24,      1,     -1,    -1,   -1,  -1, -1, -1 },
		{ 25, 300, 2300, 12650,  53130, 177100,  480700, 1081575,  2042975,  3268760,  4457400,  5200300,   5200300,   4457400,   3268760,   2042975,   1081575,   480700,   177100,    53130,    12650,    2300,     300,     25,      1,    -1,   -1,  -1, -1, -1 },
		{ 26, 325, 2600, 14950,  65780, 230230,  657800, 1562275,  3124550,  5311735,  7726160,  9657700,  10400600,   9657700,   7726160,   5311735,   3124550,  1562275,   657800,   230230,    65780,   14950,    2600,    325,     26,     1,   -1,  -1, -1, -1 },
		{ 27, 351, 2925, 17550,  80730, 296010,  888030, 2220075,  4686825,  8436285, 13037895, 17383860,  20058300,  20058300,  17383860,  13037895,   8436285,  4686825,  2220075,   888030,   296010,   80730,   17550,   2925,    351,    27,    1,  -1, -1, -1 },
		{ 28, 378, 3276, 20475,  98280, 376740, 1184040, 3108105,  6906900, 13123110, 21474180, 30421755,  37442160,  40116600,  37442160,  30421755,  21474180, 13123110,  6906900,  3108105,  1184040,  376740,   98280,  20475,   3276,   378,   28,   1, -1, -1 },
		{ 29, 406, 3654, 23751, 118755, 475020, 1560780, 4292145, 10015005, 20030010, 34597290, 51895935,  67863915,  77558760,  77558760,  67863915,  51895935, 34597290, 20030010, 10015005,  4292145, 1560780,  475020, 118755,  23751,  3654,  406,  29,  1, -1 },
		{ 30, 435, 4060, 27405, 142506, 593775, 2035800, 5852925, 14307150, 30045015, 54627300, 86493225, 119759850, 145422675, 155117520, 145422675, 119759850, 86493225, 54627300, 30045015, 14307150, 5852925, 2035800, 593775, 142506, 27405, 4060, 435, 30,  1 },
	};


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static CommonReadOnlies()
	{
		PeerMaps = new Cells[81];
		for (int i = 0; i < 81; i++)
		{
			PeerMaps[i] = Peers[i];
		}

		HouseMaps = new Cells[27];
		for (int i = 0; i < 27; i++)
		{
			HouseMaps[i] = HouseCells[i];
		}

		var r = (stackalloc[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
		var c = (stackalloc[] { 0, 3, 6, 1, 4, 7, 2, 5, 8 });
		var dic = new Dictionary<(byte, byte), (Cells, Cells, Cells, byte[])>(new ValueTupleComparer());
		for (byte bs = 9; bs < 27; bs++)
		{
			for (byte j = 0; j < 3; j++)
			{
				byte cs = (byte)(bs < 18 ? r[(bs - 9) / 3 * 3 + j] : c[(bs - 18) / 3 * 3 + j]);
				ref readonly var bm = ref HouseMaps[bs];
				ref readonly var cm = ref HouseMaps[cs];
				var i = bm & cm;
				dic.Add((bs, cs), (bm - i, cm - i, i, IntersectionBlockTable[(bs - 9) * 3 + j]));
			}
		}

		IntersectionMaps = dic;
	}
}
