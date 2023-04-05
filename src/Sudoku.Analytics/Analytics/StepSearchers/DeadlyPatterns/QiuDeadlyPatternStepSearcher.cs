﻿namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Qiu's Deadly Pattern</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Qiu's Deadly Pattern Type 1</item>
/// <item>Qiu's Deadly Pattern Type 2</item>
/// <item>Qiu's Deadly Pattern Type 3</item>
/// <item>Qiu's Deadly Pattern Type 4</item>
/// <item>Qiu's Deadly Pattern Locked Type</item>
/// </list>
/// </summary>
[StepSearcher, ConditionalCases(ConditionalCase.Standard)]
public sealed partial class QiuDeadlyPatternStepSearcher : StepSearcher
{
	/// <summary>
	/// All different patterns.
	/// </summary>
	private static readonly QiuDeadlyPattern[] Patterns = new QiuDeadlyPattern[QiuDeadlyPatternTemplatesCount];


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static QiuDeadlyPatternStepSearcher()
	{
		var BaseLineIterator = new[,]
		{
			{ 9, 10 }, { 9, 11 }, { 10, 11 }, { 12, 13 }, { 12, 14 }, { 13, 14 },
			{ 15, 16 }, { 15, 17 }, { 16, 17 }, { 18, 19 }, { 18, 20 }, { 19, 20 },
			{ 21, 22 }, { 21, 23 }, { 22, 23 }, { 24, 25 }, { 24, 26 }, { 25, 26 }
		};
		var StartCells = new[,]
		{
			{ 0, 1 }, { 0, 2 }, { 1, 2 }, { 3, 4 }, { 3, 5 }, { 4, 5 },
			{ 6, 7 }, { 6, 8 }, { 7, 8 }, { 0, 9 }, { 0, 18 }, { 9, 18 },
			{ 27, 36 }, { 27, 45 }, { 36, 45 }, { 54, 63 }, { 54, 72 }, { 63, 72 }
		};

		for (var (i, n, length) = (0, 0, BaseLineIterator.Length); i < length >> 1; i++)
		{
			var isRow = i < length >> 2;
			var houseType = isRow ? HouseType.Column : HouseType.Row;
			var baseLineMap = HousesMap[BaseLineIterator[i, 0]] | HousesMap[BaseLineIterator[i, 1]];
			for (var (j, z) = (isRow ? 0 : 9, 0); z < length >> 2; j++, z++)
			{
				var c1 = StartCells[j, 0];
				var c2 = StartCells[j, 1];
				for (var k = 0; k < 9; k++, c1 += isRow ? 9 : 1, c2 += isRow ? 9 : 1)
				{
					var pairMap = CellsMap[c1] + c2;
					if (baseLineMap && pairMap)
					{
						continue;
					}

					var tempMapBlock = HousesMap[c1.ToHouseIndex(HouseType.Block)] | HousesMap[c2.ToHouseIndex(HouseType.Block)];
					if (baseLineMap && tempMapBlock)
					{
						continue;
					}

					var tempMapLine = HousesMap[c1.ToHouseIndex(houseType)] | HousesMap[c2.ToHouseIndex(houseType)];
					var squareMap = baseLineMap & tempMapLine;
					Patterns[n++] = new(squareMap, baseLineMap - squareMap, pairMap);
				}
			}
		}
	}


	/// <inheritdoc/>
	protected internal override Step? GetAll(scoped ref AnalysisContext context)
	{
		scoped ref readonly var grid = ref context.Grid;
		var accumulator = context.Accumulator!;
		var onlyFindOne = context.OnlyFindOne;
		for (int i = 0, length = Patterns.Length; i < length; i++)
		{
			var isRow = i < length >> 1;
			if (Patterns[i] is not { Pair: [var pairFirst, var pairSecond] pair, Square: var square, BaseLine: var baseLine } pattern)
			{
				continue;
			}

			// To check whether both two pair cells are empty.
			if (!EmptyCells.Contains(pairFirst) || !EmptyCells.Contains(pairSecond))
			{
				continue;
			}

			// Step 1: To determine whether the distinction degree of base line is 1.
			var appearedDigitsMask = (short)0;
			var distinctionMask = (short)0;
			var appearedParts = 0;
			for (int j = 0, house = isRow ? 18 : 9; j < 9; j++, house++)
			{
				var houseMap = HousesMap[house];
				if ((baseLine & houseMap) is var tempMap and not [])
				{
					f(grid, tempMap, ref appearedDigitsMask, ref distinctionMask, ref appearedParts);
				}
				else if ((square & houseMap) is var squareMap and not [])
				{
					// Don't forget to record the square cells.
					f(grid, squareMap, ref appearedDigitsMask, ref distinctionMask, ref appearedParts);
				}


				static void f(
					scoped in Grid grid,
					scoped in CellMap map,
					scoped ref short appearedDigitsMask,
					scoped ref short distinctionMask,
					scoped ref int appearedParts
				)
				{
					var flag = false;
					var offsets = map.ToArray();
					var c1 = offsets[0];
					var c2 = offsets[1];
					if (!EmptyCells.Contains(c1))
					{
						var d1 = grid[c1];
						distinctionMask ^= (short)(1 << d1);
						appearedDigitsMask |= (short)(1 << d1);

						flag = true;
					}
					if (!EmptyCells.Contains(c2))
					{
						var d2 = grid[c2];
						distinctionMask ^= (short)(1 << d2);
						appearedDigitsMask |= (short)(1 << d2);

						flag = true;
					}

					appearedParts += flag ? 1 : 0;
				}
			}

			if (!IsPow2(distinctionMask) || appearedParts != PopCount((uint)appearedDigitsMask))
			{
				continue;
			}

			var pairMask = grid.GetDigitsUnion(pair);

			// Iterate on each combination.
			for (var size = 2; size < PopCount((uint)pairMask); size++)
			{
				foreach (var digits in pairMask.GetAllSets().GetSubsets(size))
				{
					// Step 2: To determine whether the digits in pair cells
					// will only appears in square cells.
					var tempMap = CellMap.Empty;
					foreach (var digit in digits)
					{
						tempMap |= CandidatesMap[digit];
					}
					var appearingMap = tempMap & square;
					if (appearingMap.Count != 4)
					{
						continue;
					}

					var flag = false;
					foreach (var digit in digits)
					{
						if (!(square & CandidatesMap[digit]))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}

					var comparer = (short)0;
					foreach (var digit in digits)
					{
						comparer |= (short)(1 << digit);
					}
					var otherDigitsMask = (short)(pairMask & ~comparer);
					if (appearingMap == (tempMap & HousesMap[TrailingZeroCount(square.BlockMask)]))
					{
						// Qdp forms.
						// Now check each type.
						if (CheckType1(accumulator, grid, isRow, pair, square, baseLine, pattern, comparer, otherDigitsMask, onlyFindOne) is { } type1Step)
						{
							return type1Step;
						}
						if (CheckType2(accumulator, grid, isRow, pair, square, baseLine, pattern, comparer, otherDigitsMask, onlyFindOne) is { } type2Step)
						{
							return type2Step;
						}
						if (CheckType3(accumulator, grid, isRow, pair, square, baseLine, pattern, comparer, otherDigitsMask, onlyFindOne) is { } type3Step)
						{
							return type3Step;
						}
					}
				}
			}

			if (CheckType4(accumulator, isRow, pair, square, baseLine, pattern, pairMask, onlyFindOne) is { } type4Step)
			{
				return type4Step;
			}
			if (CheckLockedType(accumulator, grid, isRow, pair, square, baseLine, pattern, pairMask, onlyFindOne) is { } typeLockedStep)
			{
				return typeLockedStep;
			}
		}

		return null;
	}

	/// <summary>
	/// Check for type 1.
	/// </summary>
	private Step? CheckType1(
		List<Step> accumulator,
		scoped in Grid grid,
		bool isRow,
		scoped in CellMap pair,
		scoped in CellMap square,
		scoped in CellMap baseLine,
		scoped in QiuDeadlyPattern pattern,
		short comparer,
		short otherDigitsMask,
		bool onlyFindOne
	)
	{
		if (!IsPow2(otherDigitsMask))
		{
			return null;
		}

		var extraDigit = TrailingZeroCount(otherDigitsMask);
		var map = pair & CandidatesMap[extraDigit];
		if (map is not [var elimCell])
		{
			return null;
		}

		var mask = (short)(grid.GetCandidates(elimCell) & ~(1 << extraDigit));
		if (mask == 0)
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var digit in mask)
		{
			conclusions.Add(new(Elimination, elimCell, digit));
		}

		var cellMap = square | pair;
		var cellOffsets = new CellViewNode[cellMap.Count];
		var i = 0;
		foreach (var cell in cellMap)
		{
			cellOffsets[i++] = new(DisplayColorKind.Normal, cell);
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in comparer)
		{
			foreach (var cell in square & CandidatesMap[digit])
			{
				candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
			}
		}
		var anotherCellInPair = (pair - map)[0];
		foreach (var digit in grid.GetCandidates(anotherCellInPair))
		{
			candidateOffsets.Add(new(DisplayColorKind.Normal, anotherCellInPair * 9 + digit));
		}

		var lineMask = isRow ? baseLine.RowMask : baseLine.ColumnMask;
		var offset = isRow ? 9 : 18;
		var step = new QiuDeadlyPatternType1Step(
			conclusions.ToArray(),
			new[]
			{
				View.Empty
					| cellOffsets
					| candidateOffsets
					| from pos in lineMask.GetAllSets() select new HouseViewNode(DisplayColorKind.Normal, pos + offset)
			},
			pattern,
			elimCell * 9 + extraDigit
		);
		if (onlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

		return null;
	}

	/// <summary>
	/// Check for type 2.
	/// </summary>
	private Step? CheckType2(
		List<Step> accumulator,
		scoped in Grid grid,
		bool isRow,
		scoped in CellMap pair,
		scoped in CellMap square,
		scoped in CellMap baseLine,
		scoped in QiuDeadlyPattern pattern,
		short comparer,
		short otherDigitsMask,
		bool onlyFindOne
	)
	{
		if (!IsPow2(otherDigitsMask))
		{
			return null;
		}

		var extraDigit = TrailingZeroCount(otherDigitsMask);
		var map = pair & CandidatesMap[extraDigit];
		if ((map.PeerIntersection & CandidatesMap[extraDigit]) is not (var elimMap and not []))
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var cell in elimMap)
		{
			conclusions.Add(new(Elimination, cell, extraDigit));
		}

		var cellMap = square | pair;
		var cellOffsets = new CellViewNode[cellMap.Count];
		var i = 0;
		foreach (var cell in cellMap)
		{
			cellOffsets[i++] = new(DisplayColorKind.Normal, cell);
		}
		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var digit in comparer)
		{
			foreach (var cell in square & CandidatesMap[digit])
			{
				candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
			}
		}
		foreach (var cell in pair)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? DisplayColorKind.Auxiliary1 : DisplayColorKind.Normal, cell * 9 + digit));
			}
		}

		var lineMask = isRow ? baseLine.RowMask : baseLine.ColumnMask;
		var offset = isRow ? 9 : 18;
		var step = new QiuDeadlyPatternType2Step(
			conclusions.ToArray(),
			new[]
			{
				View.Empty
					| cellOffsets
					| candidateOffsets
					| from pos in lineMask.GetAllSets() select new HouseViewNode(DisplayColorKind.Normal, pos + offset)
			},
			pattern,
			extraDigit
		);
		if (onlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

		return null;
	}

	/// <summary>
	/// Check for type 3.
	/// </summary>
	private Step? CheckType3(
		List<Step> accumulator,
		scoped in Grid grid,
		bool isRow,
		scoped in CellMap pair,
		scoped in CellMap square,
		scoped in CellMap baseLine,
		scoped in QiuDeadlyPattern pattern,
		short comparer,
		short otherDigitsMask,
		bool onlyFindOne
	)
	{
		foreach (var houseIndex in pair.CoveredHouses)
		{
			var allCellsMap = (HousesMap[houseIndex] & EmptyCells) - pair;
			for (var (size, length) = (PopCount((uint)otherDigitsMask) - 1, allCellsMap.Count); size < length; size++)
			{
				foreach (var cells in allCellsMap & size)
				{
					var mask = grid.GetDigitsUnion(cells);
					if ((mask & comparer) != comparer || PopCount((uint)mask) != size + 1)
					{
						continue;
					}

					var conclusions = new List<Conclusion>();
					foreach (var digit in mask)
					{
						foreach (var cell in allCellsMap - cells & CandidatesMap[digit])
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var cellMap = square | pair;
					var cellOffsets = new CellViewNode[cellMap.Count];
					var i = 0;
					foreach (var cell in cellMap)
					{
						cellOffsets[i++] = new(DisplayColorKind.Normal, cell);
					}
					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var digit in comparer)
					{
						foreach (var cell in square & CandidatesMap[digit])
						{
							candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
						}
					}
					foreach (var cell in pair)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(
								new(
									(otherDigitsMask >> digit & 1) != 0 ? DisplayColorKind.Auxiliary1 : DisplayColorKind.Normal,
									cell * 9 + digit
								)
							);
						}
					}
					foreach (var cell in cells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
						}
					}

					var lineMask = isRow ? baseLine.RowMask : baseLine.ColumnMask;
					var offset = isRow ? 9 : 18;
					var step = new QiuDeadlyPatternType3Step(
						conclusions.ToArray(),
						new[]
						{
							View.Empty
								| cellOffsets
								| candidateOffsets
								| from pos in lineMask.GetAllSets() select new HouseViewNode(DisplayColorKind.Normal, pos + offset)
						},
						pattern,
						mask,
						cells,
						true
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
	/// Check for type 4.
	/// </summary>
	private Step? CheckType4(
		List<Step> accumulator,
		bool isRow,
		scoped in CellMap pair,
		scoped in CellMap square,
		scoped in CellMap baseLine,
		scoped in QiuDeadlyPattern pattern,
		short comparer,
		bool onlyFindOne
	)
	{
		foreach (var houseIndex in pair.CoveredHouses)
		{
			foreach (var digit in comparer)
			{
				if ((CandidatesMap[digit] & HousesMap[houseIndex]) != pair)
				{
					continue;
				}

				var otherDigitsMask = (short)(comparer & ~(1 << digit));
				var flag = false;
				foreach (var d in otherDigitsMask)
				{
					if ((ValuesMap[d] & HousesMap[houseIndex]) is not [] || (HousesMap[houseIndex] & CandidatesMap[d]) != square)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}

				var elimDigit = TrailingZeroCount(comparer & ~(1 << digit));
				var elimMap = pair & CandidatesMap[elimDigit];
				if (!elimMap)
				{
					continue;
				}

				var conclusions = new List<Conclusion>();
				foreach (var cell in elimMap)
				{
					conclusions.Add(new(Elimination, cell, elimDigit));
				}

				var cellMap = square | pair;
				var cellOffsets = new CellViewNode[cellMap.Count];
				var i = 0;
				foreach (var cell in cellMap)
				{
					cellOffsets[i++] = new(DisplayColorKind.Normal, cell);
				}
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var d in comparer)
				{
					foreach (var cell in square & CandidatesMap[d])
					{
						candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + d));
					}
				}
				foreach (var cell in pair)
				{
					candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + digit));
				}

				var lineMask = isRow ? baseLine.RowMask : baseLine.ColumnMask;
				var offset = isRow ? 9 : 18;
				var step = new QiuDeadlyPatternType4Step(
					conclusions.ToArray(),
					new[]
					{
						View.Empty
							| cellOffsets
							| candidateOffsets
							| from pos in lineMask.GetAllSets() select new HouseViewNode(DisplayColorKind.Normal, pos + offset)
					},
					pattern,
					new(pair, digit)
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
	/// Check for locked type.
	/// </summary>
	private Step? CheckLockedType(
		List<Step> accumulator,
		scoped in Grid grid,
		bool isRow,
		scoped in CellMap pair,
		scoped in CellMap square,
		scoped in CellMap baseLine,
		scoped in QiuDeadlyPattern pattern,
		short comparer,
		bool onlyFindOne
	)
	{
		// Firstly, we should check the cells in the block that the square cells lying on.
		var block = TrailingZeroCount(square.BlockMask);
		var otherCellsMap = (HousesMap[block] & EmptyCells) - square;
		var tempMap = CellMap.Empty;
		scoped var pairDigits = comparer.GetAllSets();

		var flag = false;
		foreach (var digit in pairDigits)
		{
			if (ValuesMap[digit] && HousesMap[block])
			{
				flag = true;
				break;
			}

			tempMap |= CandidatesMap[digit];
		}
		if (flag)
		{
			return null;
		}

		otherCellsMap &= tempMap;
		if (otherCellsMap is [] or { Count: > 5 })
		{
			return null;
		}

		// May be in one house or span two houses. Now we check for this case.
		var candidates = CandidateMap.Empty;
		foreach (var cell in otherCellsMap)
		{
			foreach (var digit in pairDigits)
			{
				if (CandidatesMap[digit].Contains(cell))
				{
					candidates.Add(cell * 9 + digit);
				}
			}
		}

		if (candidates.PeerIntersection is not (var elimMap and not []))
		{
			return null;
		}

		var conclusions = new List<Conclusion>();
		foreach (var candidate in elimMap)
		{
			if (CandidatesMap[candidate % 9].Contains(candidate / 9))
			{
				conclusions.Add(new(Elimination, candidate));
			}
		}
		if (conclusions.Count == 0)
		{
			return null;
		}

		var cellMap = square | pair;
		var cellOffsets = new CellViewNode[cellMap.Count];
		var i = 0;
		foreach (var cell in cellMap)
		{
			cellOffsets[i++] = new(DisplayColorKind.Normal, cell);
		}
		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var d in comparer)
		{
			foreach (var cell in square & CandidatesMap[d])
			{
				candidateOffsets.Add(new(DisplayColorKind.Auxiliary1, cell * 9 + d));
			}
		}
		foreach (var cell in pair)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(DisplayColorKind.Normal, cell * 9 + digit));
			}
		}
		foreach (var candidate in candidates)
		{
			candidateOffsets.Add(new(DisplayColorKind.Auxiliary2, candidate));
		}

		var lineMask = isRow ? baseLine.RowMask : baseLine.ColumnMask;
		var offset = isRow ? 9 : 18;
		var step = new QiuDeadlyPatternLockedTypeStep(
			conclusions.ToArray(),
			new[]
			{
				View.Empty
					| cellOffsets
					| candidateOffsets
					| from pos in lineMask.GetAllSets() select new HouseViewNode(DisplayColorKind.Normal, pos + offset)
			},
			pattern,
			candidates
		);
		if (onlyFindOne)
		{
			return step;
		}

		accumulator.Add(step);

		return null;
	}
}