﻿namespace Sudoku.Analytics.InternalHelpers;

/// <summary>
/// Used by <see cref="UniqueRectangleStepSearcher"/>.
/// </summary>
/// <seealso cref="UniqueRectangleStepSearcher"/>
internal static class UniqueRectangStepSearcherHelper
{
	/// <summary>
	/// Check preconditions.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is searching for ARs.</param>
	/// <returns>Indicates whether the UR is passed to check.</returns>
	public static bool CheckPreconditions(scoped in Grid grid, int[] urCells, bool arMode)
	{
		var emptyCountWhenArMode = (byte)0;
		var modifiableCount = (byte)0;
		foreach (var urCell in urCells)
		{
			switch (grid.GetStatus(urCell))
			{
				case CellStatus.Given:
				case CellStatus.Modifiable when !arMode:
				{
					return false;
				}
				case CellStatus.Empty when arMode:
				{
					emptyCountWhenArMode++;
					break;
				}
				case CellStatus.Modifiable:
				{
					modifiableCount++;
					break;
				}
			}
		}

		return modifiableCount != 4 && emptyCountWhenArMode != 4;
	}

	/// <summary>
	/// Checks whether the specified UR cells satisfies the precondition of an incomplete UR.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">The UR cells.</param>
	/// <param name="d1">The first digit used.</param>
	/// <param name="d2">The second digit used.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool CheckPreconditionsOnIncomplete(scoped in Grid grid, int[] urCells, int d1, int d2)
	{
		// Same-sided cells cannot contain only one digit of two digits 'd1' and 'd2'.
		foreach (var (a, b) in stackalloc[] { (0, 1), (2, 3), (0, 2), (1, 3) })
		{
			var mask1 = grid.GetCandidates(urCells[a]);
			var mask2 = grid.GetCandidates(urCells[b]);
			var gatheredMask = (short)(mask1 | mask2);
			var intersectedMask = (short)(mask1 & mask2);
			if ((gatheredMask >> d1 & 1) == 0 || (gatheredMask >> d2 & 1) == 0)
			{
				return false;
			}
		}

		// All four cells must contain at least one digit appeared in the UR.
		var comparer = (short)(1 << d1 | 1 << d2);
		foreach (var cell in urCells)
		{
			if ((grid.GetCandidates(cell) & comparer) == 0)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// To determine whether the specified house forms a conjugate pair
	/// of the specified digit, and the cells where they contain the digit
	/// is same as the given map contains.
	/// </summary>
	/// <param name="digit">The digit.</param>
	/// <param name="map">The map.</param>
	/// <param name="houseIndex">The house index.</param>
	/// <returns>A <see cref="bool"/> value.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsConjugatePair(int digit, scoped in CellMap map, int houseIndex)
		=> (HousesMap[houseIndex] & CandidatesMap[digit]) == map;

	/// <summary>
	/// Get a cell that can't see each other.
	/// </summary>
	/// <param name="urCells">The UR cells.</param>
	/// <param name="cell">The current cell.</param>
	/// <returns>The diagonal cell.</returns>
	/// <exception cref="ArgumentException">
	/// Throws when the specified argument <paramref name="cell"/> is invalid.
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetDiagonalCell(int[] urCells, int cell)
		=> cell == urCells[0] ? urCells[3] : cell == urCells[1] ? urCells[2] : cell == urCells[2] ? urCells[1] : urCells[0];

	/// <summary>
	/// Get whether two cells are in a same house.
	/// </summary>
	/// <param name="cell1">The cell 1 to check.</param>
	/// <param name="cell2">The cell 2 to check.</param>
	/// <param name="houses">
	/// The result houses that both two cells lie in. If the cell can't be found, this argument will be 0.
	/// </param>
	/// <returns>
	/// The <see cref="bool"/> value indicating whether the another cell is same house as the current one.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsSameHouseCell(int cell1, int cell2, out int houses)
	{
		var v = (CellsMap[cell1] + cell2).CoveredHouses;
		(var r, houses) = v != 0 ? (true, v) : (false, 0);
		return r;
	}

	/// <summary>
	/// Get all highlight cells.
	/// </summary>
	/// <param name="urCells">The all UR cells used.</param>
	/// <returns>The list of highlight cells.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CellViewNode[] GetHighlightCells(int[] urCells)
		=> new CellViewNode[]
		{
			new(DisplayColorKind.Normal, urCells[0]),
			new(DisplayColorKind.Normal, urCells[1]),
			new(DisplayColorKind.Normal, urCells[2]),
			new(DisplayColorKind.Normal, urCells[3])
		};
}