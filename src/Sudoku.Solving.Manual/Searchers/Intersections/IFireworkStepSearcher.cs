﻿namespace Sudoku.Solving.Manual.Searchers;

/// <summary>
/// Provides with a <b>Firework</b> step searcher. The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>
/// Firework Pair:
/// <list type="bullet">
/// <item>Firework Pair Type 1 (Single Firework + 2 Bi-value cells)</item>
/// <item>Firework Pair Type 2 (Double Fireworks)</item>
/// <item>Firework Pair Type 3 (Single Fireworks + Empty Rectangle)</item>
/// </list>
/// </item>
/// <item>Firework Triple</item>
/// <item>Firework Quadruple</item>
/// </list>
/// </summary>
public interface IFireworkStepSearcher : IIntersectionStepSearcher
{
	/// <summary>
	/// Indicates the patterns used.
	/// </summary>
	protected static readonly FireworkPattern[] Patterns = new FireworkPattern[FireworkSubsetCount];

	/// <summary>
	/// Indicates the pattern pairs. This field is used for checking the technique firework pair type 2.
	/// </summary>
	protected static readonly (FireworkPattern First, FireworkPattern Second, int MeetCell)[] PatternPairs = new (FireworkPattern, FireworkPattern, int)[PairFireworksCount];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static IFireworkStepSearcher()
	{
		int[][] houses =
		{
			new[] { 0, 1, 3, 4 }, new[] { 0, 2, 3, 5 }, new[] { 1, 2, 4, 5 },
			new[] { 0, 1, 6, 7 }, new[] { 0, 2, 6, 8 }, new[] { 1, 2, 7, 8 },
			new[] { 3, 4, 6, 7 }, new[] { 3, 5, 6, 8 }, new[] { 4, 5, 7, 8 }
		};

		int i = 0;
		foreach (int[] houseQuad in houses)
		{
			// Gather triples.
			foreach (int[] triple in houseQuad.GetSubsets(3))
			{
				foreach (int a in HouseMaps[triple[0]])
				{
					foreach (int b in HouseMaps[triple[1]])
					{
						foreach (int c in HouseMaps[triple[2]])
						{
							if (!(Cells.Empty + a + b).InOneHouse || !(Cells.Empty + b + c).InOneHouse)
							{
								continue;
							}

							Patterns[i++] = new(Cells.Empty + a + b + c, b);
						}
					}
				}
			}

			// Gather quadruples.
			foreach (int a in HouseMaps[houseQuad[0]])
			{
				foreach (int b in HouseMaps[houseQuad[1]])
				{
					foreach (int c in HouseMaps[houseQuad[2]])
					{
						foreach (int d in HouseMaps[houseQuad[3]])
						{
							if (!(Cells.Empty + a + b).InOneHouse || !(Cells.Empty + a + c).InOneHouse
								|| !(Cells.Empty + b + d).InOneHouse || !(Cells.Empty + c + d).InOneHouse)
							{
								continue;
							}

							Patterns[i++] = new(Cells.Empty + a + b + c + d, null);
						}
					}
				}
			}
		}

		i = 0;
		for (int firstIndex = 0; firstIndex < Patterns.Length - 1; firstIndex++)
		{
			for (int secondIndex = firstIndex + 1; secondIndex < Patterns.Length; secondIndex++)
			{
				scoped ref readonly var a = ref Patterns[firstIndex];
				scoped ref readonly var b = ref Patterns[secondIndex];
				if ((a, b) is not ((var aMap, { } aPivot), (var bMap, { } bPivot)))
				{
					continue;
				}

				if ((aMap & bMap) is not [])
				{
					continue;
				}

				if (aPivot.ToHouseIndex(HouseType.Block) == bPivot.ToHouseIndex(HouseType.Block))
				{
					continue;
				}

				if ((!(aMap - aPivot) & !(bMap - bPivot)) is not [var meetCell])
				{
					continue;
				}

				PatternPairs[i++] = (a, b, meetCell);
			}
		}
	}


	/// <summary>
	/// <para>Checks for all digits which the cells containing form a firework pattern.</para>
	/// <para>
	/// This method returns the digits that satisfied the condition. If none found,
	/// this method will return 0.
	/// </para>
	/// </summary>
	/// <param name="c1">The cell 1 used in this pattern.</param>
	/// <param name="c2">The cell 2 used in this pattern.</param>
	/// <param name="pivot">The pivot cell.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="house1CellsExcluded">
	/// The excluded cells that is out of the firework structure in the <paramref name="c1"/>'s house.
	/// </param>
	/// <param name="house2CellsExcluded">
	/// The excluded cells that is out of the firework structure in the <paramref name="c2"/>'s house.
	/// </param>
	/// <returns>All digits that satisfied the firework rule. If none found, 0.</returns>
	protected static sealed short GetFireworkDigits(
		int c1, int c2, int pivot, scoped in Grid grid,
		scoped out Cells house1CellsExcluded, scoped out Cells house2CellsExcluded)
	{
		int pivotCellBlock = pivot.ToHouseIndex(HouseType.Block);
		var excluded1 = HouseMaps[(Cells.Empty + c1 + pivot).CoveredLine] - HouseMaps[pivotCellBlock] - c1;
		var excluded2 = HouseMaps[(Cells.Empty + c2 + pivot).CoveredLine] - HouseMaps[pivotCellBlock] - c2;

		short finalMask = 0;
		foreach (int digit in grid.GetDigitsUnion(Cells.Empty + c1 + c2 + pivot))
		{
			if (isFireworkFor(digit, excluded1, grid) && isFireworkFor(digit, excluded2, grid))
			{
				finalMask |= (short)(1 << digit);
			}
		}

		(house1CellsExcluded, house2CellsExcluded) = (excluded1, excluded2);
		return finalMask;


		static bool isFireworkFor(int digit, scoped in Cells houseCellsExcluded, scoped in Grid grid)
		{
			foreach (int cell in houseCellsExcluded)
			{
				switch (grid[cell])
				{
					case -1 when CandidatesMap[digit].Contains(cell):
					case var cellValue when cellValue == digit:
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}

[StepSearcher]
internal sealed partial class FireworkStepSearcher : IFireworkStepSearcher
{
	/// <inheritdoc/>
	public IStep? GetAll(ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne)
	{
		foreach (var pattern in IFireworkStepSearcher.Patterns)
		{
			if ((EmptyCells & pattern.Map) != pattern.Map)
			{
				// At least one cell is not empty cell. Just skip this structure.
				continue;
			}

			// Checks for the number of kinds of digits in three cells.
			short digitsMask = grid.GetDigitsUnion(pattern.Map);
			if (PopCount((uint)digitsMask) < 3)
			{
				continue;
			}

			switch (pattern.Pivot)
			{
				case { } pivot when PopCount((uint)digitsMask) >= 3:
				{
					if (CheckPairType1(accumulator, grid, onlyFindOne, pattern, digitsMask, pivot) is { } stepPairType1)
					{
						return stepPairType1;
					}

					if (CheckPairType3(accumulator, grid, onlyFindOne, pattern, digitsMask, pivot) is { } stepPairType3)
					{
						return stepPairType3;
					}

					if (CheckTriple(accumulator, grid, onlyFindOne, pattern, digitsMask, pivot) is { } stepTriple)
					{
						return stepTriple;
					}

					break;
				}
				case null when PopCount((uint)digitsMask) >= 4:
				{
					if (CheckQuadruple(accumulator, grid, onlyFindOne, pattern) is { } step)
					{
						return step;
					}

					break;
				}
			}
		}

		if (CheckPairType2(accumulator, grid, onlyFindOne) is { } stepPairType2)
		{
			return stepPairType2;
		}

		return null;
	}

	/// <summary>
	/// Checks for firework pair type 1 steps.
	/// </summary>
	private IStep? CheckPairType1(
		ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne,
		scoped in FireworkPattern pattern, short digitsMask, int pivot)
	{
		var map = pattern.Map;
		var nonPivotCells = map - pivot;
		int cell1 = nonPivotCells[0], cell2 = nonPivotCells[1];
		short satisfiedDigitsMask = IFireworkStepSearcher.GetFireworkDigits(
			cell1,
			cell2,
			pivot,
			grid,
			out var house1CellsExcluded,
			out var house2CellsExcluded
		);
		if (PopCount((uint)satisfiedDigitsMask) < 2)
		{
			// No possible digits found as a firework digit.
			return null;
		}

		foreach (int[] digits in digitsMask.GetAllSets().GetSubsets(2))
		{
			short currentDigitsMask = (short)(1 << digits[0] | 1 << digits[1]);
			int cell1TheOtherLine = cell1.ToHouseIndex(
				(Cells.Empty + cell1 + pivot).CoveredLine.ToHouse() == HouseType.Row
					? HouseType.Column
					: HouseType.Row
			);
			int cell2TheOtherLine = cell2.ToHouseIndex(
				(Cells.Empty + cell2 + pivot).CoveredLine.ToHouse() == HouseType.Row
					? HouseType.Column
					: HouseType.Row
			);

			foreach (int extraCell1 in (HouseMaps[cell1TheOtherLine] & EmptyCells) - cell1)
			{
				foreach (int extraCell2 in (HouseMaps[cell2TheOtherLine] & EmptyCells) - cell2)
				{
					if (extraCell1 == extraCell2)
					{
						// Cannot be a same cell.
						continue;
					}

					if (grid.GetCandidates(extraCell1) != currentDigitsMask
						|| grid.GetCandidates(extraCell2) != currentDigitsMask)
					{
						continue;
					}

					// Firework pair type 1 found.
					var elimMap = PeerMaps[extraCell1] & PeerMaps[extraCell2] & EmptyCells;
					if (elimMap is [])
					{
						// No elimination cells.
						continue;
					}

					var conclusions = new List<Conclusion>(4);
					foreach (int cell in elimMap)
					{
						if (CandidatesMap[digits[0]].Contains(cell))
						{
							conclusions.Add(new(Elimination, cell * 9 + digits[0]));
						}
						if (CandidatesMap[digits[1]].Contains(cell))
						{
							conclusions.Add(new(Elimination, cell * 9 + digits[1]));
						}
					}
					if (conclusions.Count == 0)
					{
						// No eliminations found.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>(10);
					foreach (int cell in map)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & currentDigitsMask))
						{
							candidateOffsets.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
						}
					}
					foreach (int digit in grid.GetCandidates(extraCell1))
					{
						candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, extraCell1 * 9 + digit));
					}
					foreach (int digit in grid.GetCandidates(extraCell2))
					{
						candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, extraCell2 * 9 + digit));
					}

					var cellOffsets = new List<CellViewNode>();
					foreach (int cell in house1CellsExcluded)
					{
						cellOffsets.Add(new(DisplayColorKind.Elimination, cell));
					}
					foreach (int cell in house2CellsExcluded)
					{
						cellOffsets.Add(new(DisplayColorKind.Elimination, cell));
					}

					var step = new FireworkPairType1Step(
						conclusions.ToImmutableArray(),
						ImmutableArray.Create(
							View.Empty | candidateOffsets,
							View.Empty | candidateOffsets | cellOffsets
						),
						map,
						currentDigitsMask,
						extraCell1,
						extraCell2
					);
					if (onlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Checks for firework pair type 2 steps.
	/// </summary>
	private IStep? CheckPairType2(ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne)
	{
		foreach (var (a, b, meetCell) in IFireworkStepSearcher.PatternPairs)
		{
			if ((a, b) is not ((var aMap, { } aPivot), (var bMap, { } bPivot)))
			{
				continue;
			}

			var nonPivotCells1 = aMap - aPivot;
			int cell11 = nonPivotCells1[0], cell21 = nonPivotCells1[1];
			short satisfiedDigitsMask1 = IFireworkStepSearcher.GetFireworkDigits(
				cell11,
				cell21,
				aPivot,
				grid,
				out var house1CellsExcluded1,
				out var house2CellsExcluded1
			);

			var nonPivotCells2 = bMap - bPivot;
			int cell12 = nonPivotCells2[0], cell22 = nonPivotCells2[1];
			short satisfiedDigitsMask2 = IFireworkStepSearcher.GetFireworkDigits(
				cell12,
				cell22,
				bPivot,
				grid,
				out var house1CellsExcluded2,
				out var house2CellsExcluded2
			);
			short intersection = (short)(satisfiedDigitsMask1 & satisfiedDigitsMask2);
			if (PopCount((uint)intersection) < 2)
			{
				continue;
			}

			foreach (int[] digits in intersection.GetAllSets().GetSubsets(2))
			{
				short currentDigitsMask = (short)(1 << digits[0] | 1 << digits[1]);
				if (grid.GetCandidates(meetCell) != currentDigitsMask)
				{
					continue;
				}

				// Firework pair type 2 found.
				var untouchedCrossCells = (
					HouseMaps[meetCell.ToHouseIndex(HouseType.Row)]
						| HouseMaps[meetCell.ToHouseIndex(HouseType.Column)]
				) - meetCell - nonPivotCells1 - nonPivotCells2;

				var conclusions = new List<Conclusion>();
				foreach (int cell in
					untouchedCrossCells & EmptyCells & (CandidatesMap[digits[0]] | CandidatesMap[digits[1]]))
				{
					if (CandidatesMap[digits[0]].Contains(cell))
					{
						conclusions.Add(new(Elimination, cell * 9 + digits[0]));
					}
					if (CandidatesMap[digits[1]].Contains(cell))
					{
						conclusions.Add(new(Elimination, cell * 9 + digits[1]));
					}
				}
				foreach (int digit in (short)(grid.GetCandidates(aPivot) & ~currentDigitsMask))
				{
					conclusions.Add(new(Elimination, aPivot, digit));
				}
				foreach (int digit in (short)(grid.GetCandidates(bPivot) & ~currentDigitsMask))
				{
					conclusions.Add(new(Elimination, bPivot, digit));
				}
				if (conclusions.Count == 0)
				{
					// No eliminations found.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(14);
				var candidateOffsetsOnlyContainFirework1 = new List<CandidateViewNode>(6);
				var candidateOffsetsOnlyContainFirework2 = new List<CandidateViewNode>(6);
				foreach (int cell in aMap)
				{
					foreach (int digit in (short)(grid.GetCandidates(cell) & currentDigitsMask))
					{
						var node = new CandidateViewNode(DisplayColorKind.Normal, cell * 9 + digit);
						candidateOffsets.Add(node);
						candidateOffsetsOnlyContainFirework1.Add(node);
					}
				}
				foreach (int cell in bMap)
				{
					foreach (int digit in (short)(grid.GetCandidates(cell) & currentDigitsMask))
					{
						var node = new CandidateViewNode(DisplayColorKind.Auxiliary1, cell * 9 + digit);
						candidateOffsets.Add(node);
						candidateOffsetsOnlyContainFirework2.Add(node);
					}
				}
				foreach (int digit in grid.GetCandidates(meetCell))
				{
					candidateOffsets.Add(new(DisplayColorKind.Auxiliary2, meetCell * 9 + digit));
				}

				var cellOffsetsView2 = new List<CellViewNode>();
				foreach (int cell in house1CellsExcluded1)
				{
					cellOffsetsView2.Add(new(DisplayColorKind.Elimination, cell));
				}
				foreach (int cell in house2CellsExcluded1)
				{
					cellOffsetsView2.Add(new(DisplayColorKind.Elimination, cell));
				}
				var cellOffsetsView3 = new List<CellViewNode>();
				foreach (int cell in house1CellsExcluded2)
				{
					cellOffsetsView3.Add(new(DisplayColorKind.Elimination, cell));
				}
				foreach (int cell in house2CellsExcluded2)
				{
					cellOffsetsView3.Add(new(DisplayColorKind.Elimination, cell));
				}

				var step = new FireworkPairType2Step(
					conclusions.ToImmutableArray(),
					ImmutableArray.Create(
						View.Empty | candidateOffsets,
						View.Empty | candidateOffsetsOnlyContainFirework1 | cellOffsetsView2,
						View.Empty | candidateOffsetsOnlyContainFirework2 | cellOffsetsView3,
						View.Empty
							| candidateOffsetsOnlyContainFirework1
							| candidateOffsetsOnlyContainFirework2
							| new UnknownViewNode[]
							{
								new(DisplayColorKind.Normal, aPivot, (Utf8Char)'x', currentDigitsMask),
								new(DisplayColorKind.Normal, bPivot, (Utf8Char)'x', currentDigitsMask),
								new(DisplayColorKind.Normal, meetCell, (Utf8Char)'x', currentDigitsMask)
							}
					),
					currentDigitsMask,
					a,
					b,
					meetCell
				);
				if (onlyFindOne)
				{
					return step;
				}

				accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Checks for firework pair type 3 steps.
	/// </summary>
	private IStep? CheckPairType3(
		ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne,
		scoped in FireworkPattern pattern, short digitsMask, int pivot)
	{
		var map = pattern.Map;
		var nonPivotCells = map - pivot;
		int cell1 = nonPivotCells[0], cell2 = nonPivotCells[1];
		short satisfiedDigitsMask = IFireworkStepSearcher.GetFireworkDigits(
			cell1,
			cell2,
			pivot,
			grid,
			out var house1CellsExcluded,
			out var house2CellsExcluded
		);
		if (PopCount((uint)satisfiedDigitsMask) < 2)
		{
			// No possible digits found as a firework digit.
			return null;
		}

		foreach (int[] digits in digitsMask.GetAllSets().GetSubsets(2))
		{
			short currentDigitsMask = (short)(1 << digits[0] | 1 << digits[1]);
			int cell1TheOtherLine = cell1.ToHouseIndex(
				(Cells.Empty + cell1 + pivot).CoveredLine.ToHouse() == HouseType.Row
					? HouseType.Column
					: HouseType.Row
			);
			int cell2TheOtherLine = cell2.ToHouseIndex(
				(Cells.Empty + cell2 + pivot).CoveredLine.ToHouse() == HouseType.Row
					? HouseType.Column
					: HouseType.Row
			);
			var fullTwoDigitsMap = CandidatesMap[digits[0]] | CandidatesMap[digits[1]];
			short cell1OtheBlocksMask = (HouseMaps[cell1TheOtherLine] - HouseMaps[cell1.ToHouseIndex(HouseType.Block)]).BlockMask;
			short cell2OtheBlocksMask = (HouseMaps[cell2TheOtherLine] - HouseMaps[cell2.ToHouseIndex(HouseType.Block)]).BlockMask;
			foreach (int emptyRectangleBlock in (short)(cell1OtheBlocksMask | cell2OtheBlocksMask))
			{
				var digit1EmptyRectangleMap = HouseMaps[emptyRectangleBlock] & CandidatesMap[digits[0]];
				var digit2EmptyRectangleMap = HouseMaps[emptyRectangleBlock] & CandidatesMap[digits[1]];
				if (!IEmptyRectangleStepSearcher.IsEmptyRectangle(digit1EmptyRectangleMap, emptyRectangleBlock, out _, out _)
					|| !IEmptyRectangleStepSearcher.IsEmptyRectangle(digit2EmptyRectangleMap, emptyRectangleBlock, out _, out _))
				{
					// The block doesn't form a valid empty rectangle.
					continue;
				}

				var lastCells = HouseMaps[emptyRectangleBlock] & fullTwoDigitsMap;
				if (((HouseMaps[cell1TheOtherLine] | HouseMaps[cell2TheOtherLine]) & lastCells) != lastCells)
				{
					// Those two houses don't include all empty rectangle cells.
					continue;
				}

				// Firework pair type 3 found.
				var elimMap = (HouseMaps[pivot.ToHouseIndex(HouseType.Block)] & fullTwoDigitsMap)
					- HouseMaps[(Cells.Empty + pivot + cell1).CoveredLine]
					- HouseMaps[(Cells.Empty + pivot + cell2).CoveredLine];
				if (elimMap is [])
				{
					// No elimination cells found.
					continue;
				}

				var conclusions = new List<Conclusion>();
				foreach (int cell in elimMap)
				{
					if (CandidatesMap[digits[0]].Contains(cell))
					{
						conclusions.Add(new(Elimination, cell * 9 + digits[0]));
					}
					if (CandidatesMap[digits[1]].Contains(cell))
					{
						conclusions.Add(new(Elimination, cell * 9 + digits[1]));
					}
				}
				if (conclusions.Count == 0)
				{
					// No eliminations found.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(16);
				foreach (int cell in map)
				{
					foreach (int digit in (short)(grid.GetCandidates(cell) & currentDigitsMask))
					{
						candidateOffsets.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
					}
				}
				foreach (int cell in lastCells)
				{
					foreach (int digit in (short)(grid.GetCandidates(cell) & currentDigitsMask))
					{
						candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
					}
				}

				var cellOffsets = new List<CellViewNode>();
				foreach (int cell in house1CellsExcluded)
				{
					cellOffsets.Add(new(DisplayColorKind.Elimination, cell));
				}
				foreach (int cell in house2CellsExcluded)
				{
					cellOffsets.Add(new(DisplayColorKind.Elimination, cell));
				}

				var step = new FireworkPairType3Step(
					conclusions.ToImmutableArray(),
					ImmutableArray.Create(
						View.Empty | candidateOffsets,
						View.Empty | candidateOffsets | cellOffsets
					),
					map,
					currentDigitsMask,
					emptyRectangleBlock
				);
				if (onlyFindOne)
				{
					return step;
				}

				accumulator.Add(step);
			}
		}

		return null;
	}

	/// <summary>
	/// Checks for firework triple steps.
	/// </summary>
	private IStep? CheckTriple(
		ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne,
		scoped in FireworkPattern pattern, short digitsMask, int pivot)
	{
		var nonPivotCells = pattern.Map - pivot;
		int cell1 = nonPivotCells[0], cell2 = nonPivotCells[1];
		short satisfiedDigitsMask = IFireworkStepSearcher.GetFireworkDigits(
			cell1,
			cell2,
			pivot,
			grid,
			out var house1CellsExcluded,
			out var house2CellsExcluded
		);
		if (satisfiedDigitsMask == 0)
		{
			// No possible digits found as a firework digit.
			return null;
		}

		foreach (int[] digits in digitsMask.GetAllSets().GetSubsets(3))
		{
			short currentDigitsMask = (short)(1 << digits[0] | 1 << digits[1] | 1 << digits[2]);
			if ((satisfiedDigitsMask & currentDigitsMask) != currentDigitsMask)
			{
				continue;
			}

			// Firework Triple is found.
			int pivotCellBlock = pivot.ToHouseIndex(HouseType.Block);
			var pivotRowCells = HouseMaps[pivot.ToHouseIndex(HouseType.Row)];
			var pivotColumnCells = HouseMaps[pivot.ToHouseIndex(HouseType.Column)];

			// Now check eliminations.
			var conclusions = new List<Conclusion>(18);
			foreach (int digit in (short)(grid.GetCandidates(pivot) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, pivot, digit));
			}
			foreach (int digit in (short)(grid.GetCandidates(cell1) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, cell1, digit));
			}
			foreach (int digit in (short)(grid.GetCandidates(cell2) & ~currentDigitsMask))
			{
				conclusions.Add(new(Elimination, cell2, digit));
			}
			foreach (int digit in currentDigitsMask)
			{
				var possibleBlockCells = HouseMaps[pivotCellBlock] & EmptyCells & CandidatesMap[digit];
				foreach (int cell in possibleBlockCells - pivotRowCells - pivotColumnCells)
				{
					conclusions.Add(new(Elimination, cell, digit));
				}
			}
			if (conclusions.Count == 0)
			{
				// No eliminations found.
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>();
			foreach (int digit in (short)(grid.GetCandidates(pivot) & currentDigitsMask))
			{
				candidateOffsets.Add(new(DisplayColorKind.Normal, pivot * 9 + digit));
			}
			foreach (int digit in (short)(grid.GetCandidates(cell1) & currentDigitsMask))
			{
				candidateOffsets.Add(new(DisplayColorKind.Normal, cell1 * 9 + digit));
			}
			foreach (int digit in (short)(grid.GetCandidates(cell2) & currentDigitsMask))
			{
				candidateOffsets.Add(new(DisplayColorKind.Normal, cell2 * 9 + digit));
			}

			var cellOffsets = new List<CellViewNode>(12);
			foreach (int house1CellExcluded in house1CellsExcluded)
			{
				cellOffsets.Add(new(DisplayColorKind.Elimination, house1CellExcluded));
			}
			foreach (int house2CellExcluded in house2CellsExcluded)
			{
				cellOffsets.Add(new(DisplayColorKind.Elimination, house2CellExcluded));
			}

			var unknowns = new List<UnknownViewNode>(4);
			int house1 = (Cells.Empty + cell1 + pivot).CoveredLine;
			int house2 = (Cells.Empty + cell2 + pivot).CoveredLine;
			foreach (int cell in (HouseMaps[house1] & HouseMaps[pivotCellBlock] & EmptyCells) - pivot)
			{
				unknowns.Add(new(DisplayColorKind.Normal, cell, (Utf8Char)'y', currentDigitsMask));
			}
			foreach (int cell in (HouseMaps[house2] & HouseMaps[pivotCellBlock] & EmptyCells) - pivot)
			{
				unknowns.Add(new(DisplayColorKind.Normal, cell, (Utf8Char)'x', currentDigitsMask));
			}

			var step = new FireworkTripleStep(
				conclusions.ToImmutableArray(),
				ImmutableArray.Create(
					View.Empty | candidateOffsets,
					View.Empty
						| cellOffsets
						| unknowns
						| new UnknownViewNode[]
						{
							new(
								DisplayColorKind.Normal,
								pivot,
								(Utf8Char)'z',
								(short)(grid.GetCandidates(pivot) & currentDigitsMask)
							),
							new(
								DisplayColorKind.Normal,
								cell1,
								(Utf8Char)'x',
								(short)(grid.GetCandidates(cell1) & currentDigitsMask)
							),
							new(
								DisplayColorKind.Normal,
								cell2,
								(Utf8Char)'y',
								(short)(grid.GetCandidates(cell2) & currentDigitsMask)
							)
						}
				),
				pattern.Map,
				currentDigitsMask
			);
			if (onlyFindOne)
			{
				return step;
			}

			accumulator.Add(step);
		}

		return null;
	}

	/// <summary>
	/// Checks for firework quadruple steps.
	/// </summary>
	private IStep? CheckQuadruple(
		ICollection<IStep> accumulator, scoped in Grid grid, bool onlyFindOne, scoped in FireworkPattern pattern)
	{
		if (pattern is not { Map: [var c1, var c2, var c3, var c4] map })
		{
			return null;
		}

		short digitsMask = grid.GetDigitsUnion(map);
		if (PopCount((uint)digitsMask) < 4)
		{
			return null;
		}

		foreach (int[] digits in digitsMask.GetAllSets().GetSubsets(4))
		{
			var cases = (stackalloc[]
			{
				((digits[0], digits[1]), (digits[2], digits[3])),
				((digits[0], digits[2]), (digits[1], digits[3])),
				((digits[0], digits[3]), (digits[1], digits[2])),
				((digits[1], digits[2]), (digits[0], digits[3])),
				((digits[1], digits[3]), (digits[0], digits[2])),
				((digits[2], digits[3]), (digits[0], digits[1]))
			});

			foreach (var (pivot1, pivot2) in stackalloc[] { (c1, c4), (c2, c3) })
			{
				var nonPivot1Cells = (map - pivot1) & (
					HouseMaps[pivot1.ToHouseIndex(HouseType.Row)]
						| HouseMaps[pivot1.ToHouseIndex(HouseType.Column)]
				);
				var nonPivot2Cells = (map - pivot2) & (
					HouseMaps[pivot2.ToHouseIndex(HouseType.Row)]
						| HouseMaps[pivot2.ToHouseIndex(HouseType.Column)]
				);
				int cell1Pivot1 = nonPivot1Cells[0], cell2Pivot1 = nonPivot1Cells[1];
				int cell1Pivot2 = nonPivot2Cells[0], cell2Pivot2 = nonPivot2Cells[1];

				foreach (var ((d1, d2), (d3, d4)) in cases)
				{
					short pair1DigitsMask = (short)(1 << d1 | 1 << d2);
					short pair2DigitsMask = (short)(1 << d3 | 1 << d4);
					short satisfiedDigitsMaskPivot1 = IFireworkStepSearcher.GetFireworkDigits(
						cell1Pivot1,
						cell2Pivot1,
						pivot1,
						grid,
						out var house1CellsExcludedPivot1,
						out var house2CellsExcludedPivot1
					);
					short satisfiedDigitsMaskPivot2 = IFireworkStepSearcher.GetFireworkDigits(
						cell1Pivot2,
						cell2Pivot2,
						pivot2,
						grid,
						out var house1CellsExcludedPivot2,
						out var house2CellsExcludedPivot2
					);
					if ((satisfiedDigitsMaskPivot1 & pair1DigitsMask) != pair1DigitsMask
						|| (satisfiedDigitsMaskPivot2 & pair2DigitsMask) != pair2DigitsMask)
					{
						continue;
					}

					// Firework quadruple found.
					short fourDigitsMask = (short)(pair1DigitsMask | pair2DigitsMask);

					var conclusions = new List<Conclusion>(20);
					foreach (int digit in (short)(grid.GetCandidates(pivot1) & ~pair1DigitsMask))
					{
						conclusions.Add(new(Elimination, pivot1, digit));
					}
					foreach (int digit in (short)(grid.GetCandidates(pivot2) & ~pair2DigitsMask))
					{
						conclusions.Add(new(Elimination, pivot2, digit));
					}
					foreach (int cell in map - pivot1 - pivot2)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & ~fourDigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					foreach (int cell in
						(
							HouseMaps[pivot1.ToHouseIndex(HouseType.Block)]
								- HouseMaps[pivot1.ToHouseIndex(HouseType.Row)]
								- HouseMaps[pivot1.ToHouseIndex(HouseType.Column)]
						) & EmptyCells)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & pair1DigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					foreach (int cell in
						(
							HouseMaps[pivot2.ToHouseIndex(HouseType.Block)]
								- HouseMaps[pivot2.ToHouseIndex(HouseType.Row)]
								- HouseMaps[pivot2.ToHouseIndex(HouseType.Column)]
						) & EmptyCells)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & pair2DigitsMask))
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						// No eliminations found.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (int digit in (short)(grid.GetCandidates(pivot1) & fourDigitsMask))
					{
						candidateOffsets.Add(new(DisplayColorKind.Normal, pivot1 * 9 + digit));
					}
					foreach (int digit in (short)(grid.GetCandidates(pivot2) & fourDigitsMask))
					{
						candidateOffsets.Add(new(DisplayColorKind.Normal, pivot2 * 9 + digit));
					}
					foreach (int cell in map - pivot1 - pivot2)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & fourDigitsMask))
						{
							candidateOffsets.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
						}
					}

					var candidateOffsetsView2 = new List<CandidateViewNode>();
					foreach (int cell in map - pivot2)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & pair1DigitsMask))
						{
							candidateOffsetsView2.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
						}
					}
					var candidateOffsetsView3 = new List<CandidateViewNode>();
					foreach (int cell in map - pivot1)
					{
						foreach (int digit in (short)(grid.GetCandidates(cell) & pair2DigitsMask))
						{
							candidateOffsetsView3.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
						}
					}

					var cellOffsets1 = new List<CellViewNode>();
					foreach (int cell in house1CellsExcludedPivot1 | house2CellsExcludedPivot1)
					{
						cellOffsets1.Add(new(DisplayColorKind.Elimination, cell));
					}
					var cellOffsets2 = new List<CellViewNode>();
					foreach (int cell in house1CellsExcludedPivot2 | house2CellsExcludedPivot2)
					{
						cellOffsets2.Add(new(DisplayColorKind.Elimination, cell));
					}

					var step = new FireworkQuadrupleStep(
						conclusions.ToImmutableArray(),
						ImmutableArray.Create(
							View.Empty | candidateOffsets,
							View.Empty
								| cellOffsets1
								| candidateOffsetsView2,
							View.Empty
								| cellOffsets2
								| candidateOffsetsView3
						),
						map,
						fourDigitsMask
					);
					if (onlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}
		}

		return null;
	}
}