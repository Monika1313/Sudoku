namespace Sudoku.Analytics.StepSearchers;

partial class UniqueRectangleStepSearcher
{
	/// <summary>
	/// Check type 1.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ cornerCell
	/// (abc) ab
	///  ab   ab
	/// ]]></code>
	/// </remarks>
	partial void CheckType1(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		// Get the summary mask.
		var mask = grid.GetDigitsUnion(otherCellsMap);
		if (mask != comparer)
		{
			return;
		}

		// Type 1 found. Now check elimination.
		var d1Exists = CandidatesMap[d1].Contains(cornerCell);
		var d2Exists = CandidatesMap[d2].Contains(cornerCell);
		if (!d1Exists && d2Exists)
		{
			return;
		}

		using scoped var conclusions = new ValueList<Conclusion>(2);
		if (d1Exists)
		{
			conclusions.Add(new(Elimination, cornerCell, d1));
		}
		if (d2Exists)
		{
			conclusions.Add(new(Elimination, cornerCell, d2));
		}
		if (conclusions.Count == 0)
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>(6);
		foreach (var cell in otherCellsMap)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
			}
		}

		if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Count) != (6, 2))
		{
			return;
		}

		accumulator.Add(
			new UniqueRectangleType1Step(
				conclusions.ToArray(),
				new[]
				{
					View.Empty
						| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
						| (arMode ? null : candidateOffsets)
				},
				d1,
				d2,
				(CellMap)urCells,
				arMode,
				index
			)
		);
	}

	/// <summary>
	/// Check type 2.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (abc) (abc)
	///  ab    ab
	/// ]]></code>
	/// </remarks>
	partial void CheckType2(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		// Get the summary mask.
		var mask = grid.GetDigitsUnion(otherCellsMap);
		if (mask != comparer)
		{
			return;
		}

		// Gets the extra mask.
		// If the mask is the power of 2, the type 2 will be formed.
		var extraMask = (grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) ^ comparer;
		if (extraMask == 0 || (extraMask & extraMask - 1) != 0)
		{
			return;
		}

		// Type 2 or 5 found. Now check elimination.
		var extraDigit = TrailingZeroCount(extraMask);
		var elimMap = (CellsMap[corner1] + corner2).PeerIntersection & CandidatesMap[extraDigit];
		if (!elimMap)
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>();
		foreach (var cell in urCells)
		{
			if (grid.GetStatus(cell) == CellStatus.Empty)
			{
				foreach (var digit in grid.GetCandidates(cell))
				{
					candidateOffsets.Add(
						new(
							digit == extraDigit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
							cell * 9 + digit
						)
					);
				}
			}
		}

		if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
		{
			return;
		}

		var isType5 = !(CellsMap[corner1] + corner2).InOneHouse;
		accumulator.Add(
			new UniqueRectangleType2Step(
				from cell in elimMap select new Conclusion(Elimination, cell, extraDigit),
				new[] { View.Empty | (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null) | candidateOffsets },
				d1,
				d2,
				(arMode, isType5) switch
				{
					(true, true) => Technique.AvoidableRectangleType5,
					(true, false) => Technique.AvoidableRectangleType2,
					(false, true) => Technique.UniqueRectangleType5,
					_ => Technique.UniqueRectangleType2
				},
				(CellMap)urCells,
				arMode,
				extraDigit,
				index
			)
		);
	}

	/// <summary>
	/// Check type 3.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ corner1, corner2
	/// (ab ) (ab )
	///  abx   aby
	/// ]]></code>
	/// </remarks>
	partial void CheckType3(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		var notSatisfiedType3 = false;
		foreach (var cell in otherCellsMap)
		{
			var currentMask = grid.GetCandidates(cell);
			if ((currentMask & comparer) == 0 || currentMask == comparer || arMode && grid.GetStatus(cell) != CellStatus.Empty)
			{
				notSatisfiedType3 = true;
				break;
			}
		}
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer || notSatisfiedType3)
		{
			return;
		}

		var mask = grid.GetDigitsUnion(otherCellsMap);
		if ((mask & comparer) != comparer)
		{
			return;
		}

		var otherDigitsMask = (Mask)(mask ^ comparer);
		foreach (var houseIndex in otherCellsMap.CoveredHouses)
		{
			if ((ValuesMap[d1] || ValuesMap[d2]) && HousesMap[houseIndex])
			{
				return;
			}

			var iterationMap = (HousesMap[houseIndex] & EmptyCells) - otherCellsMap;
			for (var size = PopCount((uint)otherDigitsMask) - 1; size < iterationMap.Count; size++)
			{
				foreach (var iteratedCells in iterationMap & size)
				{
					var tempMask = grid.GetDigitsUnion(iteratedCells);
					if ((tempMask & comparer) != 0 || PopCount((uint)tempMask) - 1 != size || (tempMask & otherDigitsMask) != otherDigitsMask)
					{
						continue;
					}

					var conclusions = new List<Conclusion>(16);
					foreach (var digit in tempMask)
					{
						foreach (var cell in (iterationMap - iteratedCells) & CandidatesMap[digit])
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var cellOffsets = new List<CellViewNode>();
					foreach (var cell in urCells)
					{
						if (grid.GetStatus(cell) != CellStatus.Empty)
						{
							cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
						}
					}

					var candidateOffsets = new List<CandidateViewNode>();
					foreach (var cell in urCells)
					{
						if (grid.GetStatus(cell) == CellStatus.Empty)
						{
							foreach (var digit in grid.GetCandidates(cell))
							{
								candidateOffsets.Add(
									new(
										(tempMask >> digit & 1) != 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
										cell * 9 + digit
									)
								);
							}
						}
					}
					foreach (var cell in iteratedCells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + digit));
						}
					}

					accumulator.Add(
						new UniqueRectangleType3Step(
							conclusions.ToArray(),
							new[]
							{
								View.Empty
									| (arMode ? cellOffsets : null)
									| candidateOffsets
									| new HouseViewNode(WellKnownColorIdentifierKind.Normal, houseIndex)
							},
							d1,
							d2,
							(CellMap)urCells,
							iteratedCells,
							otherDigitsMask,
							houseIndex,
							arMode,
							index
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Check type 4.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ corner1, corner2
	/// (ab ) ab
	///  abx  aby
	/// ]]></code>
	/// </remarks>
	partial void CheckType4(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		foreach (var houseIndex in otherCellsMap.CoveredHouses)
		{
			if (houseIndex < 9)
			{
				// Process the case in lines.
				continue;
			}

			foreach (var digit in stackalloc[] { d1, d2 })
			{
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, otherCellsMap, houseIndex))
				{
					continue;
				}

				// Yes, Type 4 found.
				// Now check elimination.
				var elimDigit = TrailingZeroCount(comparer ^ (1 << digit));
				if ((otherCellsMap & CandidatesMap[elimDigit]) is not (var elimMap and not []))
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(6);
				foreach (var cell in urCells)
				{
					if (grid.GetStatus(cell) != CellStatus.Empty)
					{
						continue;
					}

					if (otherCellsMap.Contains(cell))
					{
						if (d1 != elimDigit && CandidatesMap[d1].Contains(cell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + d1));
						}
						if (d2 != elimDigit && CandidatesMap[d2].Contains(cell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + d2));
						}
					}
					else
					{
						// Corner1 and corner2.
						foreach (var d in grid.GetCandidates(cell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d));
						}
					}
				}

				var conclusions = from cell in elimMap select new Conclusion(Elimination, cell, elimDigit);
				if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Length) != (6, 2))
				{
					continue;
				}

				var offsets = otherCellsMap.ToArray();
				accumulator.Add(
					new UniqueRectangleWithConjugatePairStep(
						conclusions,
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode(WellKnownColorIdentifierKind.Normal, houseIndex)
						},
						Technique.UniqueRectangleType4,
						d1,
						d2,
						(CellMap)urCells,
						arMode,
						new Conjugate[] { new(offsets[0], offsets[1], digit) },
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check type 5.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab ) abc
	///  abc  abc
	/// ]]></code>
	/// </remarks>
	partial void CheckType5(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		// Get the summary mask.
		var otherCellsMask = grid.GetDigitsUnion(otherCellsMap);

		// Degenerate to type 1.
		var extraMask = (Mask)(otherCellsMask ^ comparer);
		if ((extraMask & extraMask - 1) != 0)
		{
			return;
		}

		// Type 5 found. Now check elimination.
		var extraDigit = TrailingZeroCount(extraMask);
		var cellsThatContainsExtraDigit = otherCellsMap & CandidatesMap[extraDigit];

		// Degenerate to type 1.
		if (cellsThatContainsExtraDigit.Count == 1)
		{
			return;
		}

		if ((cellsThatContainsExtraDigit.PeerIntersection & CandidatesMap[extraDigit]) is not (var elimMap and not []))
		{
			return;
		}

		var candidateOffsets = new List<CandidateViewNode>(16);
		foreach (var cell in urCells)
		{
			if (grid.GetStatus(cell) != CellStatus.Empty)
			{
				continue;
			}

			foreach (var digit in grid.GetCandidates(cell))
			{
				candidateOffsets.Add(new(digit == extraDigit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
			}
		}
		if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
		{
			return;
		}

		accumulator.Add(
			new UniqueRectangleType2Step(
				from cell in elimMap select new Conclusion(Elimination, cell, extraDigit),
				new[] { View.Empty | (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null) | candidateOffsets },
				d1,
				d2,
				arMode ? Technique.AvoidableRectangleType5 : Technique.UniqueRectangleType5,
				(CellMap)urCells,
				arMode,
				extraDigit,
				index
			)
		);
	}

	/// <summary>
	/// Check type 6.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ corner1
	/// (ab )  aby
	///  abx  (ab)
	///        ↑corner2
	/// ]]></code>
	/// </remarks>
	partial void CheckType6(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var o1 = otherCellsMap[0];
		var o2 = otherCellsMap[1];
		var r1 = corner1.ToHouseIndex(HouseType.Row);
		var c1 = corner1.ToHouseIndex(HouseType.Column);
		var r2 = corner2.ToHouseIndex(HouseType.Row);
		var c2 = corner2.ToHouseIndex(HouseType.Column);
		foreach (var digit in stackalloc[] { d1, d2 })
		{
			foreach (var (h1, h2) in stackalloc[] { (r1, r2), (c1, c2) })
			{
				gather(grid, otherCellsMap, h1 is >= 9 and < 18, digit, h1, h2);
			}
		}


		void gather(scoped in Grid grid, scoped in CellMap otherCellsMap, bool isRow, Digit digit, House house1, House house2)
		{
			var precheck = isRow
				&& UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[corner1] + o1, house1)
				&& UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[corner2] + o2, house2)
				|| !isRow
				&& UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[corner1] + o2, house1)
				&& UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[corner2] + o1, house2);
			if (!precheck)
			{
				return;
			}

			// Check eliminations.
			if ((otherCellsMap & CandidatesMap[digit]) is not (var elimMap and not []))
			{
				return;
			}

			var candidateOffsets = new List<CandidateViewNode>(6);
			foreach (var cell in urCells)
			{
				if (otherCellsMap.Contains(cell))
				{
					if (d1 != digit && CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d1));
					}
					if (d2 != digit && CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d2));
					}
				}
				else
				{
					foreach (var d in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(d == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cell * 9 + d));
					}
				}
			}

			var conclusions = from cell in elimMap select new Conclusion(Elimination, cell, digit);
			if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Length) != (6, 2))
			{
				return;
			}

			accumulator.Add(
				new UniqueRectangleWithConjugatePairStep(
					conclusions,
					new[]
					{
						View.Empty
							| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
							| candidateOffsets
							| new HouseViewNode[]
							{
								new(WellKnownColorIdentifierKind.Normal, house1),
								new(WellKnownColorIdentifierKind.Normal, house2)
							}
					},
					Technique.UniqueRectangleType6,
					d1,
					d2,
					(CellMap)urCells,
					false,
					new Conjugate[] { new(corner1, isRow ? o1 : o2, digit), new(corner2, isRow ? o2 : o1, digit) },
					index
				)
			);
		}
	}

	/// <summary>
	/// Check hidden UR.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab ) abx
	///  aby  abz
	/// ]]></code>
	/// </remarks>
	partial void CheckHidden(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		var r = abzCell.ToHouseIndex(HouseType.Row);
		var c = abzCell.ToHouseIndex(HouseType.Column);
		foreach (var digit in stackalloc[] { d1, d2 })
		{
			var map1 = CellsMap[abzCell] + abxCell;
			var map2 = CellsMap[abzCell] + abyCell;
			if (map1.CoveredLine is not (var m1cl and not InvalidValidOfTrailingZeroCountMethodFallback)
				|| map2.CoveredLine is not (var m2cl and not InvalidValidOfTrailingZeroCountMethodFallback))
			{
				// There's no common covered line to display.
				continue;
			}

			if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, map1, m1cl)
				|| !UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, map2, m2cl))
			{
				continue;
			}

			// Hidden UR found. Now check eliminations.
			var elimDigit = TrailingZeroCount(comparer ^ (1 << digit));
			if (!CandidatesMap[elimDigit].Contains(abzCell))
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>();
			foreach (var cell in urCells)
			{
				if (grid.GetStatus(cell) != CellStatus.Empty)
				{
					continue;
				}

				if (otherCellsMap.Contains(cell))
				{
					if ((cell != abzCell || d1 != elimDigit) && CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(d1 != elimDigit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cell * 9 + d1));
					}
					if ((cell != abzCell || d2 != elimDigit) && CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(d2 != elimDigit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cell * 9 + d2));
					}
				}
				else
				{
					foreach (var d in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d));
					}
				}
			}

			if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
			{
				continue;
			}

			accumulator.Add(
				new HiddenUniqueRectangleStep(
					new[] { new Conclusion(Elimination, abzCell, elimDigit) },
					new[]
					{
						View.Empty
							| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
							| candidateOffsets
							| new HouseViewNode[] { new(WellKnownColorIdentifierKind.Normal, r), new(WellKnownColorIdentifierKind.Normal, c) }
					},
					d1,
					d2,
					(CellMap)urCells,
					arMode,
					new Conjugate[] { new(abzCell, abxCell, digit), new(abzCell, abyCell, digit) },
					index
				)
			);
		}
	}

	/// <summary>
	/// Check UR + 2D.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )  abx
	///  aby  (ab )  xy  *
	///         ↑ corner2
	/// ]]></code>
	/// </remarks>
	partial void Check2D(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		var o1 = grid.GetCandidates(otherCellsMap[0]);
		var o2 = grid.GetCandidates(otherCellsMap[1]);
		var o = (Mask)(o1 | o2);
		if (PopCount((uint)o) != 4 || PopCount((uint)o1) > 3 || PopCount((uint)o2) > 3
			|| (o1 & comparer) == 0 || (o2 & comparer) == 0
			|| (o & comparer) != comparer)
		{
			return;
		}

		var xyMask = (Mask)(o ^ comparer);
		var x = TrailingZeroCount(xyMask);
		var y = xyMask.GetNextSet(x);
		var inter = otherCellsMap.PeerIntersection - (CellMap)urCells;
		foreach (var possibleXyCell in inter)
		{
			if (grid.GetCandidates(possibleXyCell) != xyMask)
			{
				continue;
			}

			// 'xy' found.
			// Now check eliminations.
			var elimMap = inter & PeersMap[possibleXyCell];
			var conclusions = new List<Conclusion>(10);
			foreach (var cell in elimMap)
			{
				if (CandidatesMap[x].Contains(cell))
				{
					conclusions.Add(new(Elimination, cell, x));
				}
				if (CandidatesMap[y].Contains(cell))
				{
					conclusions.Add(new(Elimination, cell, y));
				}
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>(10);
			foreach (var cell in urCells)
			{
				if (otherCellsMap.Contains(cell))
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new((comparer >> digit & 1) == 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
					}
				}
				else
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
					}
				}
			}
			foreach (var digit in xyMask)
			{
				candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, possibleXyCell * 9 + digit));
			}

			if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
			{
				return;
			}

			accumulator.Add(
				new UniqueRectangle2DOr3XStep(
					conclusions.ToArray(),
					new[] { View.Empty | (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null) | candidateOffsets },
					arMode ? Technique.AvoidableRectangle2D : Technique.UniqueRectangle2D,
					d1,
					d2,
					(CellMap)urCells,
					arMode,
					x,
					y,
					possibleXyCell,
					index
				)
			);
		}
	}

	/// <summary>
	/// Check UR + 2B/1SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (ab )  (ab )
	///  |
	///  | a
	///  |
	///  abx    aby
	/// ]]></code>
	/// </remarks>
	partial void Check2B1SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		scoped ReadOnlySpan<Digit> digits = (stackalloc[] { d1, d2 });
		foreach (var cell in stackalloc[] { corner1, corner2 })
		{
			foreach (var otherCell in otherCellsMap)
			{
				if (!UniqueRectangleStepSearcherHelper.IsSameHouseCell(cell, otherCell, out var houses))
				{
					continue;
				}

				foreach (var house in houses)
				{
					if (house < 9)
					{
						continue;
					}

					for (var digitIndex = 0; digitIndex < 2; digitIndex++)
					{
						var digit = digits[digitIndex];
						if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[cell] + otherCell, house))
						{
							continue;
						}

						var elimCell = (otherCellsMap - otherCell)[0];
						if (!CandidatesMap[digit].Contains(otherCell))
						{
							continue;
						}

						var elimDigit = TrailingZeroCount(comparer ^ (1 << digit));
						var conclusions = new List<Conclusion>(4);
						if (CandidatesMap[elimDigit].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, elimDigit));
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>(10);
						foreach (var urCell in urCells)
						{
							if (urCell == corner1 || urCell == corner2)
							{
								var coveredHouses = (CellsMap[urCell] + otherCell).CoveredHouses;
								if ((coveredHouses >> house & 1) != 0)
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(
											new(d == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, urCell * 9 + d));
									}
								}
								else
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, urCell * 9 + d));
									}
								}
							}
							else if (urCell == otherCell || urCell == elimCell)
							{
								if (CandidatesMap[d1].Contains(urCell))
								{
									if (urCell != elimCell || d1 != elimDigit)
									{
										candidateOffsets.Add(
											new(
												urCell == elimCell
													? WellKnownColorIdentifierKind.Normal
													: d1 == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
												urCell * 9 + d1
											)
										);
									}
								}
								if (CandidatesMap[d2].Contains(urCell))
								{
									if (urCell != elimCell || d2 != elimDigit)
									{
										candidateOffsets.Add(
											new(
												urCell == elimCell
													? WellKnownColorIdentifierKind.Normal
													: d2 == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
												urCell * 9 + d2
											)
										);
									}
								}
							}
						}

						if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
						{
							continue;
						}

						accumulator.Add(
							new UniqueRectangleWithConjugatePairStep(
								conclusions.ToArray(),
								new[]
								{
									View.Empty
										| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
										| candidateOffsets
										| new HouseViewNode(WellKnownColorIdentifierKind.Normal, house)
								},
								Technique.UniqueRectangle2B1,
								d1,
								d2,
								(CellMap)urCells,
								arMode,
								new Conjugate[] { new(cell, otherCell, digit) },
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR + 2D/1SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )   aby
	///  |
	///  | a
	///  |
	///  abx   (ab )
	///          ↑ corner2
	/// ]]></code>
	/// </remarks>
	partial void Check2D1SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		scoped ReadOnlySpan<Digit> digits = (stackalloc[] { d1, d2 });
		foreach (var cell in stackalloc[] { corner1, corner2 })
		{
			foreach (var otherCell in otherCellsMap)
			{
				if (!UniqueRectangleStepSearcherHelper.IsSameHouseCell(cell, otherCell, out var houses))
				{
					continue;
				}

				foreach (var house in houses)
				{
					if (house < 9)
					{
						continue;
					}

					for (var digitIndex = 0; digitIndex < 2; digitIndex++)
					{
						var digit = digits[digitIndex];
						if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(digit, CellsMap[cell] + otherCell, house))
						{
							continue;
						}

						var elimCell = (otherCellsMap - otherCell)[0];
						if (!CandidatesMap[digit].Contains(otherCell))
						{
							continue;
						}

						var conclusions = new List<Conclusion>(4);
						if (CandidatesMap[digit].Contains(elimCell))
						{
							conclusions.Add(new(Elimination, elimCell, digit));
						}
						if (conclusions.Count == 0)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>(10);
						foreach (var urCell in urCells)
						{
							if (urCell == corner1 || urCell == corner2)
							{
								var flag = false;
								foreach (var r in (CellsMap[urCell] + otherCell).CoveredHouses)
								{
									if (r == house)
									{
										flag = true;
										break;
									}
								}

								if (flag)
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(
											new(d == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, urCell * 9 + d));
									}
								}
								else
								{
									foreach (var d in grid.GetCandidates(urCell))
									{
										candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, urCell * 9 + d));
									}
								}
							}
							else if (urCell == otherCell || urCell == elimCell)
							{
								if (CandidatesMap[d1].Contains(urCell) && (urCell != elimCell || d1 != digit))
								{
									candidateOffsets.Add(
										new(
											urCell == elimCell
												? WellKnownColorIdentifierKind.Normal
												: d1 == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
											urCell * 9 + d1
										)
									);
								}
								if (CandidatesMap[d2].Contains(urCell) && (urCell != elimCell || d2 != digit))
								{
									candidateOffsets.Add(
										new(
											urCell == elimCell
												? WellKnownColorIdentifierKind.Normal
												: d2 == digit ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
											urCell * 9 + d2
										)
									);
								}
							}
						}

						if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
						{
							continue;
						}

						accumulator.Add(
							new UniqueRectangleWithConjugatePairStep(
								conclusions.ToArray(),
								new[]
								{
									View.Empty
										| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
										| candidateOffsets
										| new HouseViewNode(WellKnownColorIdentifierKind.Normal, house)
								},
								Technique.UniqueRectangle2D1,
								d1,
								d2,
								(CellMap)urCells,
								arMode,
								new Conjugate[] { new(cell, otherCell, digit) },
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR + 3X.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab )  abx
	///  aby   abz   xy  *
	/// ]]></code>
	/// Note: <c>z</c> is <c>x</c> or <c>y</c>.
	/// </remarks>
	partial void Check3X(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var c1 = otherCellsMap[0];
		var c2 = otherCellsMap[1];
		var c3 = otherCellsMap[2];
		var m1 = grid.GetCandidates(c1);
		var m2 = grid.GetCandidates(c2);
		var m3 = grid.GetCandidates(c3);
		var mask = (Mask)((Mask)(m1 | m2) | m3);

		if (PopCount((uint)mask) != 4
			|| PopCount((uint)m1) > 3 || PopCount((uint)m2) > 3 || PopCount((uint)m3) > 3
			|| (m1 & comparer) == 0 || (m2 & comparer) == 0 || (m3 & comparer) == 0
			|| (mask & comparer) != comparer)
		{
			return;
		}

		var xyMask = (Mask)(mask ^ comparer);
		var x = TrailingZeroCount(xyMask);
		var y = xyMask.GetNextSet(x);
		var inter = otherCellsMap.PeerIntersection - (CellMap)urCells;
		foreach (var possibleXyCell in inter)
		{
			if (grid.GetCandidates(possibleXyCell) != xyMask)
			{
				continue;
			}

			// Possible XY cell found.
			// Now check eliminations.
			var conclusions = new List<Conclusion>(10);
			foreach (var cell in inter & PeersMap[possibleXyCell])
			{
				if (CandidatesMap[x].Contains(cell))
				{
					conclusions.Add(new(Elimination, cell, x));
				}
				if (CandidatesMap[y].Contains(cell))
				{
					conclusions.Add(new(Elimination, cell, y));
				}
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>(10);
			foreach (var cell in urCells)
			{
				if (otherCellsMap.Contains(cell))
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(comparer >> digit & 1) == 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				else
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
					}
				}
			}
			foreach (var digit in xyMask)
			{
				candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, possibleXyCell * 9 + digit));
			}
			if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
			{
				return;
			}

			accumulator.Add(
				new UniqueRectangle2DOr3XStep(
					conclusions.ToArray(),
					new[] { View.Empty | (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null) | candidateOffsets },
					arMode ? Technique.AvoidableRectangle3X : Technique.UniqueRectangle3X,
					d1,
					d2,
					(CellMap)urCells,
					arMode,
					x,
					y,
					possibleXyCell,
					index
				)
			);
		}
	}

	/// <summary>
	/// Check UR + 3X/2SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab )    abx
	///           |
	///           | b
	///       a   |
	///  aby-----abz
	/// ]]></code>
	/// </remarks>
	partial void Check3X2SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		foreach (var (a, b) in stackalloc[] { (d1, d2), (d2, d1) })
		{
			var abxCell = adjacentCellsMap[0];
			var abyCell = adjacentCellsMap[1];
			var map1 = CellsMap[abzCell] + abxCell;
			var map2 = CellsMap[abzCell] + abyCell;
			if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(b, map1, map1.CoveredLine)
				|| !UniqueRectangleStepSearcherHelper.IsConjugatePair(a, map2, map2.CoveredLine))
			{
				continue;
			}

			var conclusions = new List<Conclusion>(2);
			if (CandidatesMap[a].Contains(abxCell))
			{
				conclusions.Add(new(Elimination, abxCell, a));
			}
			if (CandidatesMap[b].Contains(abyCell))
			{
				conclusions.Add(new(Elimination, abyCell, b));
			}
			if (conclusions.Count == 0)
			{
				continue;
			}

			var candidateOffsets = new List<CandidateViewNode>(6);
			foreach (var digit in grid.GetCandidates(abxCell))
			{
				if ((digit == d1 || digit == d2) && digit != a)
				{
					candidateOffsets.Add(new(digit == b ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abxCell * 9 + digit));
				}
			}
			foreach (var digit in grid.GetCandidates(abyCell))
			{
				if ((digit == d1 || digit == d2) && digit != b)
				{
					candidateOffsets.Add(new(digit == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abyCell * 9 + digit));
				}
			}
			foreach (var digit in grid.GetCandidates(abzCell))
			{
				if (digit == a || digit == b)
				{
					candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, abzCell * 9 + digit));
				}
			}
			foreach (var digit in comparer)
			{
				candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cornerCell * 9 + digit));
			}
			if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 6)
			{
				continue;
			}

			accumulator.Add(
				new UniqueRectangleWithConjugatePairStep(
					conclusions.ToArray(),
					new[]
					{
						View.Empty
							| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
							| candidateOffsets
							| new HouseViewNode[]
							{
								new(WellKnownColorIdentifierKind.Normal, map1.CoveredLine),
								new(WellKnownColorIdentifierKind.Auxiliary1, map2.CoveredLine)
							}
					},
					Technique.UniqueRectangle3X2,
					d1,
					d2,
					(CellMap)urCells,
					arMode,
					new Conjugate[] { new(abxCell, abzCell, b), new(abyCell, abzCell, a) },
					index
				)
			);
		}
	}

	/// <summary>
	/// Check UR + 3N/2SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab )-----abx
	///        a   |
	///            | b
	///            |
	///  aby      abz
	/// ]]></code>
	/// </remarks>
	partial void Check3N2SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		// Step 1: Get the diagonal cell of 'cornerCell' and determine the existence of strong link.
		var abzCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		scoped ReadOnlySpan<(Digit, Digit)> digitPairs = (stackalloc[] { (d1, d2), (d2, d1) });
		scoped ReadOnlySpan<Digit> digits = (stackalloc[] { d1, d2 });
		foreach (var (begin, end) in stackalloc[] { (abxCell, abyCell), (abyCell, abxCell) })
		{
			var linkMap = CellsMap[begin] + abzCell;
			foreach (var (a, b) in digitPairs)
			{
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(b, linkMap, linkMap.CoveredLine))
				{
					continue;
				}

				// Step 2: Get the link cell that is adjacent to 'cornerCell' and check the strong link.
				var secondLinkMap = CellsMap[cornerCell] + begin;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, secondLinkMap, secondLinkMap.CoveredLine))
				{
					continue;
				}

				// Step 3: Check eliminations.
				if (!CandidatesMap[a].Contains(end))
				{
					continue;
				}

				// Step 4: Check highlight candidates.
				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cornerCell * 9 + d));
				}
				foreach (var d in digits)
				{
					if (CandidatesMap[d].Contains(abzCell))
					{
						candidateOffsets.Add(new(d == b ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abzCell * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if ((d == d1 || d == d2) && d != a)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, end * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = new Conjugate[] { new(cornerCell, begin, a), new(begin, abzCell, b) };
				accumulator.Add(
					new UniqueRectangleWithConjugatePairStep(
						new[] { new Conclusion(Elimination, end, a) },
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, conjugatePairs[0].Line),
									new(WellKnownColorIdentifierKind.Auxiliary1, conjugatePairs[1].Line)
								}
						},
						Technique.UniqueRectangle3N2,
						d1,
						d2,
						(CellMap)urCells,
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 3U/2SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///  ↓ cornerCell
	/// (ab )-----abx
	///        a
	///
	///        b
	///  aby -----abz
	/// ]]></code>
	/// </remarks>
	partial void Check3U2SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		scoped ReadOnlySpan<(Digit, Digit)> digitPairs = (stackalloc[] { (d1, d2), (d2, d1) });
		foreach (var (begin, end) in stackalloc[] { (abxCell, abyCell), (abyCell, abxCell) })
		{
			var linkMap = CellsMap[begin] + abzCell;
			foreach (var (a, b) in digitPairs)
			{
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(b, linkMap, linkMap.CoveredLine))
				{
					continue;
				}

				var secondLinkMap = CellsMap[cornerCell] + end;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, secondLinkMap, secondLinkMap.CoveredLine))
				{
					continue;
				}

				if (!CandidatesMap[a].Contains(begin))
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cornerCell * 9 + d));
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if ((d == d1 || d == d2) && d != a)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, end * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(abzCell))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == b ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abzCell * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = new Conjugate[] { new(cornerCell, end, a), new(begin, abzCell, b) };
				accumulator.Add(
					new UniqueRectangleWithConjugatePairStep(
						new[] { new Conclusion(Elimination, begin, a) },
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, conjugatePairs[0].Line),
									new(WellKnownColorIdentifierKind.Auxiliary1, conjugatePairs[1].Line)
								}
						},
						Technique.UniqueRectangle3U2,
						d1,
						d2,
						(CellMap)urCells,
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 3E/2SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="cornerCell">The corner cell.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ cornerCell
	/// (ab )-----abx
	///        a
	///
	///        a
	///  aby -----abz
	/// ]]></code>
	/// </remarks>
	partial void Check3E2SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell cornerCell,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetCandidates(cornerCell) != comparer)
		{
			return;
		}

		var abzCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, cornerCell);
		var adjacentCellsMap = otherCellsMap - abzCell;
		var abxCell = adjacentCellsMap[0];
		var abyCell = adjacentCellsMap[1];
		scoped ReadOnlySpan<(Digit, Digit)> digitPairs = (stackalloc[] { (d1, d2), (d2, d1) });
		foreach (var (begin, end) in stackalloc[] { (abxCell, abyCell), (abyCell, abxCell) })
		{
			var linkMap = CellsMap[begin] + abzCell;
			foreach (var (a, b) in digitPairs)
			{
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, linkMap, linkMap.CoveredLine))
				{
					continue;
				}

				var secondLinkMap = CellsMap[cornerCell] + end;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, secondLinkMap, secondLinkMap.CoveredLine))
				{
					continue;
				}

				if (!CandidatesMap[b].Contains(abzCell))
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(7);
				foreach (var d in comparer)
				{
					candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, cornerCell * 9 + d));
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, end * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(abzCell))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abzCell * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
				{
					continue;
				}

				var conjugatePairs = new Conjugate[] { new(cornerCell, end, a), new(begin, abzCell, a) };
				accumulator.Add(
					new UniqueRectangleWithConjugatePairStep(
						new[] { new Conclusion(Elimination, abzCell, b) },
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, conjugatePairs[0].Line),
									new(WellKnownColorIdentifierKind.Auxiliary1, conjugatePairs[1].Line)
								}
						},
						Technique.UniqueRectangle3E2,
						d1,
						d2,
						(CellMap)urCells,
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4X/3SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (abx)-----(aby)
	///        a    |
	///             | b
	///        a    |
	///  abz ----- abw
	/// ]]></code>
	/// </remarks>
	partial void Check4X3SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		var link1Map = CellsMap[corner1] + corner2;
		foreach (var (a, b) in stackalloc[] { (d1, d2), (d2, d1) })
		{
			if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, link1Map, link1Map.CoveredLine))
			{
				continue;
			}

			var abwCell = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, corner1);
			var abzCell = (otherCellsMap - abwCell)[0];
			foreach (var (head, begin, end, extra) in stackalloc[] { (corner2, corner1, abzCell, abwCell), (corner1, corner2, abwCell, abzCell) })
			{
				var link2Map = CellsMap[begin] + end;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(b, link2Map, link2Map.CoveredLine))
				{
					continue;
				}

				var link3Map = CellsMap[end] + extra;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, link3Map, link3Map.CoveredLine))
				{
					continue;
				}

				var conclusions = new List<Conclusion>(2);
				if (CandidatesMap[b].Contains(head))
				{
					conclusions.Add(new(Elimination, head, b));
				}
				if (CandidatesMap[b].Contains(extra))
				{
					conclusions.Add(new(Elimination, extra, b));
				}
				if (conclusions.Count == 0)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(6);
				foreach (var d in grid.GetCandidates(head))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, head * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(extra))
				{
					if ((d == d1 || d == d2) && d != b)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, extra * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(begin))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, begin * 9 + d));
					}
				}
				foreach (var d in grid.GetCandidates(end))
				{
					if (d == d1 || d == d2)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, end * 9 + d));
					}
				}
				if (!AllowIncompleteUniqueRectangles && (candidateOffsets.Count, conclusions.Count) != (6, 2))
				{
					continue;
				}

				var conjugatePairs = new Conjugate[] { new(head, begin, a), new(begin, end, b), new(end, extra, a) };
				accumulator.Add(
					new UniqueRectangleWithConjugatePairStep(
						conclusions.ToArray(),
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, conjugatePairs[0].Line),
									new(WellKnownColorIdentifierKind.Auxiliary1, conjugatePairs[1].Line),
									new(WellKnownColorIdentifierKind.Normal, conjugatePairs[2].Line)
								}
						},
						Technique.UniqueRectangle4X3,
						d1,
						d2,
						(CellMap)urCells,
						arMode,
						conjugatePairs,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + 4C/3SL.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The structures:</para>
	/// <para>
	/// Subtype 1:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (abx)-----(aby)
	///        a    |
	///             | a
	///        b    |
	///  abz ----- abw
	/// ]]></code>
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	///   ↓ corner1, corner2
	/// (abx)-----(aby)
	///   |    a    |
	///   | b       | a
	///   |         |
	///  abz       abw
	/// ]]></code>
	/// </para>
	/// </remarks>
	partial void Check4C3SL(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		var link1Map = CellsMap[corner1] + corner2;
		scoped var innerMaps = (stackalloc CellMap[2]);
		foreach (var (a, b) in stackalloc[] { (d1, d2), (d2, d1) })
		{
			if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, link1Map, link1Map.CoveredLine))
			{
				continue;
			}

			var end = UniqueRectangleStepSearcherHelper.GetDiagonalCell(urCells, corner1);
			var extra = (otherCellsMap - end)[0];
			foreach (var (abx, aby, abw, abz) in stackalloc[] { (corner2, corner1, extra, end), (corner1, corner2, end, extra) })
			{
				var link2Map = CellsMap[aby] + abw;
				if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(a, link2Map, link2Map.CoveredLine))
				{
					continue;
				}

				var link3Map1 = CellsMap[abw] + abz;
				var link3Map2 = CellsMap[abx] + abz;
				innerMaps[0] = link3Map1;
				innerMaps[1] = link3Map2;
				for (var i = 0; i < 2; i++)
				{
					var linkMap = innerMaps[i];
					if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(b, link3Map1, link3Map1.CoveredLine))
					{
						continue;
					}

					if (!CandidatesMap[b].Contains(aby))
					{
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>(7);
					foreach (var d in grid.GetCandidates(abx))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(
								new(
									i == 0 ? d == a ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal : WellKnownColorIdentifierKind.Auxiliary1,
									abx * 9 + d
								)
							);
						}
					}
					foreach (var d in grid.GetCandidates(abz))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(new(d == b ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal, abz * 9 + d));
						}
					}
					foreach (var d in grid.GetCandidates(aby))
					{
						if ((d == d1 || d == d2) && d != b)
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, aby * 9 + d));
						}
					}
					foreach (var d in grid.GetCandidates(abw))
					{
						if (d == d1 || d == d2)
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, abw * 9 + d));
						}
					}
					if (!AllowIncompleteUniqueRectangles && candidateOffsets.Count != 7)
					{
						continue;
					}

					var conjugatePairs = new Conjugate[] { new(abx, aby, a), new(aby, abw, a), new(linkMap[0], linkMap[1], b) };
					accumulator.Add(
						new UniqueRectangleWithConjugatePairStep(
							new[] { new Conclusion(Elimination, aby, b) },
							new[]
							{
								View.Empty
									| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
									| candidateOffsets
									| new HouseViewNode[]
									{
										new(WellKnownColorIdentifierKind.Normal, conjugatePairs[0].Line),
										new(WellKnownColorIdentifierKind.Normal, conjugatePairs[1].Line),
										new(WellKnownColorIdentifierKind.Auxiliary1, conjugatePairs[2].Line)
									}
							},
							Technique.UniqueRectangle4C3,
							d1,
							d2,
							(CellMap)urCells,
							arMode,
							conjugatePairs,
							index
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Check UR-XY-Wing, UR-XYZ-Wing, UR-WXYZ-Wing and AR-XY-Wing, AR-XYZ-Wing and AR-WXYZ-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="size">The size of the wing to search.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The structures:</para>
	/// <para>
	/// Subtype 1:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )  abxy  yz  xz
	/// (ab )  abxy  *
	///   ↑ corner2
	/// ]]></code>
	/// Note that the pair of cells <c>abxy</c> should be in the same house.
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	///   ↓ corner1
	/// (ab )  abx   xz
	///  aby  (ab )  *   yz
	///         ↑ corner2
	/// ]]></code>
	/// </para>
	/// </remarks>
	partial void CheckRegularWing(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int size,
		int index
	)
	{
		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer)
		{
			return;
		}

		if ((CellsMap[corner1] + corner2).AllSetsAreInOneHouse(out var house) && house < 9)
		{
			// Subtype 1.
			var offsets = otherCellsMap.ToArray();
			var otherCell1 = offsets[0];
			var otherCell2 = offsets[1];
			var mask1 = grid.GetCandidates(otherCell1);
			var mask2 = grid.GetCandidates(otherCell2);
			var mask = (Mask)(mask1 | mask2);

			if (PopCount((uint)mask) != 2 + size || (mask & comparer) != comparer || mask1 == comparer || mask2 == comparer)
			{
				return;
			}

			var map = (PeersMap[otherCell1] | PeersMap[otherCell2]) & BivalueCells;
			if (map.Count < size)
			{
				return;
			}

			var testMap = (CellsMap[otherCell1] + otherCell2).PeerIntersection;
			var extraDigitsMask = (Mask)(mask ^ comparer);
			var cells = map.ToArray();
			for (var (i1, length) = (0, cells.Length); i1 < length - size + 1; i1++)
			{
				var c1 = cells[i1];
				var m1 = grid.GetCandidates(c1);
				if ((m1 & ~extraDigitsMask) == 0)
				{
					continue;
				}

				for (var i2 = i1 + 1; i2 < length - size + 2; i2++)
				{
					var c2 = cells[i2];
					var m2 = grid.GetCandidates(c2);
					if ((m2 & ~extraDigitsMask) == 0)
					{
						continue;
					}

					if (size == 2)
					{
						// Check XY-Wing.
						var m = (Mask)((Mask)(m1 | m2) ^ extraDigitsMask);
						if ((PopCount((uint)m), PopCount((uint)(m1 & m2))) != (1, 1))
						{
							continue;
						}

						// Now check whether all cells found should see their corresponding
						// cells in UR structure ('otherCells1' or 'otherCells2').
						var flag = true;
						foreach (var cell in stackalloc[] { c1, c2 })
						{
							var extraDigit = TrailingZeroCount(grid.GetCandidates(cell) & ~m);
							if (!(testMap & CandidatesMap[extraDigit]).Contains(cell))
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}

						// Now check eliminations.
						var elimDigit = TrailingZeroCount(m);
						var elimMap = (CellsMap[c1] + c2).PeerIntersection & CandidatesMap[elimDigit];
						if (!elimMap)
						{
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>(12);
						foreach (var cell in urCells)
						{
							if (grid.GetStatus(cell) == CellStatus.Empty)
							{
								foreach (var digit in grid.GetCandidates(cell))
								{
									candidateOffsets.Add(
										new(
											digit == elimDigit
												? otherCellsMap.Contains(cell) ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Normal
												: (extraDigitsMask >> digit & 1) != 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
											cell * 9 + digit
										)
									);
								}
							}
						}
						foreach (var digit in grid.GetCandidates(c1))
						{
							candidateOffsets.Add(
								new(digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1, c1 * 9 + digit));
						}
						foreach (var digit in grid.GetCandidates(c2))
						{
							candidateOffsets.Add(
								new(digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1, c2 * 9 + digit));
						}
						if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
						{
							return;
						}

						accumulator.Add(
							new UniqueRectangleWithWingStep(
								from cell in elimMap select new Conclusion(Elimination, cell, elimDigit),
								new[]
								{
									View.Empty
										| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
										| candidateOffsets
								},
								arMode ? Technique.AvoidableRectangleXyWing : Technique.UniqueRectangleXyWing,
								d1,
								d2,
								(CellMap)urCells,
								arMode,
								CellsMap[c1] + c2,
								otherCellsMap,
								extraDigitsMask,
								index
							)
						);
					}
					else // size > 2
					{
						for (var (i3, lengthMinusSizePlus3) = (i2 + 1, length - size + 3); i3 < lengthMinusSizePlus3; i3++)
						{
							var c3 = cells[i3];
							var m3 = grid.GetCandidates(c3);
							if ((m3 & ~extraDigitsMask) == 0)
							{
								continue;
							}

							if (size == 3)
							{
								// Check XYZ-Wing.
								var m = (Mask)(((Mask)(m1 | m2) | m3) ^ extraDigitsMask);
								if ((PopCount((uint)m), PopCount((uint)(m1 & m2 & m3))) != (1, 1))
								{
									continue;
								}

								// Now check whether all cells found should see their corresponding
								// cells in UR structure ('otherCells1' or 'otherCells2').
								var flag = true;
								foreach (var cell in stackalloc[] { c1, c2, c3 })
								{
									var extraDigit = TrailingZeroCount(grid.GetCandidates(cell) & ~m);
									if (!(testMap & CandidatesMap[extraDigit]).Contains(cell))
									{
										flag = false;
										break;
									}
								}
								if (!flag)
								{
									continue;
								}

								// Now check eliminations.
								var elimDigit = TrailingZeroCount(m);
								var elimMap = (CellsMap[c1] + c2 + c3).PeerIntersection & CandidatesMap[elimDigit];
								if (!elimMap)
								{
									continue;
								}

								var candidateOffsets = new List<CandidateViewNode>();
								foreach (var cell in urCells)
								{
									if (grid.GetStatus(cell) == CellStatus.Empty)
									{
										foreach (var digit in grid.GetCandidates(cell))
										{
											candidateOffsets.Add(
												new(
													(extraDigitsMask >> digit & 1) != 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
													cell * 9 + digit
												)
											);
										}
									}
								}
								foreach (var digit in grid.GetCandidates(c1))
								{
									candidateOffsets.Add(
										new(digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1, c1 * 9 + digit));
								}
								foreach (var digit in grid.GetCandidates(c2))
								{
									candidateOffsets.Add(
										new(digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1, c2 * 9 + digit));
								}
								foreach (var digit in grid.GetCandidates(c3))
								{
									candidateOffsets.Add(
										new(digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1, c3 * 9 + digit));
								}
								if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
								{
									return;
								}

								accumulator.Add(
									new UniqueRectangleWithWingStep(
										from cell in elimMap select new Conclusion(Elimination, cell, elimDigit),
										new[]
										{
											View.Empty
												| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
												| candidateOffsets
										},
										arMode ? Technique.AvoidableRectangleXyzWing : Technique.UniqueRectangleXyzWing,
										d1,
										d2,
										(CellMap)urCells,
										arMode,
										CellsMap[c1] + c2 + c3,
										otherCellsMap,
										extraDigitsMask,
										index
									)
								);
							}
							else // size == 4
							{
								for (var i4 = i3 + 1; i4 < length; i4++)
								{
									var c4 = cells[i4];
									var m4 = grid.GetCandidates(c4);
									if ((m4 & ~extraDigitsMask) == 0)
									{
										continue;
									}

									// Check WXYZ-Wing.
									var m = (Mask)((Mask)((Mask)((Mask)(m1 | m2) | m3) | m4) ^ extraDigitsMask);
									if ((PopCount((uint)m), PopCount((uint)(m1 & m2 & m3 & m4))) != (1, 1))
									{
										continue;
									}

									// Now check whether all cells found should see their corresponding
									// cells in UR structure ('otherCells1' or 'otherCells2').
									var flag = true;
									foreach (var cell in stackalloc[] { c1, c2, c3, c4 })
									{
										var extraDigit = TrailingZeroCount(grid.GetCandidates(cell) & ~m);
										if (!(testMap & CandidatesMap[extraDigit]).Contains(cell))
										{
											flag = false;
											break;
										}
									}
									if (!flag)
									{
										continue;
									}

									// Now check eliminations.
									var elimDigit = TrailingZeroCount(m);
									var elimMap = (CellsMap[c1] + c2 + c3 + c4).PeerIntersection & CandidatesMap[elimDigit];
									if (!elimMap)
									{
										continue;
									}

									var candidateOffsets = new List<CandidateViewNode>();
									foreach (var cell in urCells)
									{
										if (grid.GetStatus(cell) == CellStatus.Empty)
										{
											foreach (var digit in grid.GetCandidates(cell))
											{
												candidateOffsets.Add(
													new(
														(extraDigitsMask >> digit & 1) != 0
															? WellKnownColorIdentifierKind.Auxiliary1
															: WellKnownColorIdentifierKind.Normal,
														cell * 9 + digit
													)
												);
											}
										}
									}
									foreach (var digit in grid.GetCandidates(c1))
									{
										candidateOffsets.Add(
											new(
												digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1,
												c1 * 9 + digit
											)
										);
									}
									foreach (var digit in grid.GetCandidates(c2))
									{
										candidateOffsets.Add(
											new(
												digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1,
												c2 * 9 + digit
											)
										);
									}
									foreach (var digit in grid.GetCandidates(c3))
									{
										candidateOffsets.Add(
											new(
												digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1,
												c3 * 9 + digit
											)
										);
									}
									foreach (var digit in grid.GetCandidates(c4))
									{
										candidateOffsets.Add(
											new(
												digit == elimDigit ? WellKnownColorIdentifierKind.Auxiliary2 : WellKnownColorIdentifierKind.Auxiliary1,
												c4 * 9 + digit
											)
										);
									}
									if (UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets))
									{
										return;
									}

									accumulator.Add(
										new UniqueRectangleWithWingStep(
											from cell in elimMap select new Conclusion(Elimination, cell, elimDigit),
											new[]
											{
												View.Empty
													| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
													| candidateOffsets
											},
											arMode ? Technique.AvoidableRectangleWxyzWing : Technique.UniqueRectangleWxyzWing,
											d1,
											d2,
											(CellMap)urCells,
											arMode,
											CellsMap[c1] + c2 + c3 + c4,
											otherCellsMap,
											extraDigitsMask,
											index
										)
									);
								}
							}
						}
					}
				}
			}
		}
		else
		{
			// TODO: Check for subtype 2 on UR/AR Wings.
		}
	}

	/// <summary>
	/// Check UR + SdC.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="arMode">Indicates whether the current mode is AR mode.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">The map of other cells during the current UR searching.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// The structure:
	/// <code><![CDATA[
	///           |   xyz
	///  ab+ ab+  | abxyz abxyz
	///           |   xyz
	/// ----------+------------
	/// (ab)(ab)  |
	///  ↑ corner1, corner2
	/// ]]></code>
	/// </remarks>
	partial void CheckSueDeCoq(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		bool arMode,
		Mask comparer,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		var notSatisfiedType3 = false;
		var mergedMaskInOtherCells = (Mask)0;
		foreach (var cell in otherCellsMap)
		{
			var currentMask = grid.GetCandidates(cell);
			mergedMaskInOtherCells |= currentMask;
			if ((currentMask & comparer) == 0 || currentMask == comparer || arMode && grid.GetStatus(cell) != CellStatus.Empty)
			{
				notSatisfiedType3 = true;
				break;
			}
		}

		if ((grid.GetCandidates(corner1) | grid.GetCandidates(corner2)) != comparer || notSatisfiedType3
			|| (mergedMaskInOtherCells & comparer) != comparer)
		{
			return;
		}

		// Check whether the corners spanned two blocks. If so, UR + SdC can't be found.
		var blockMaskInOtherCells = otherCellsMap.BlockMask;
		if (!IsPow2(blockMaskInOtherCells))
		{
			return;
		}

		var otherDigitsMask = (Mask)(mergedMaskInOtherCells & ~comparer);
		var line = (byte)otherCellsMap.CoveredLine;
		var block = (byte)TrailingZeroCount(otherCellsMap.CoveredHouses & ~(1 << line));
		var (a, _, _, d) = IntersectionMaps[(line, block)];
		using scoped var list = new ValueList<CellMap>(4);
		foreach (var cannibalMode in stackalloc[] { false, true })
		{
			foreach (var otherBlock in d)
			{
				var emptyCellsInInterMap = HousesMap[otherBlock] & HousesMap[line] & EmptyCells;
				if (emptyCellsInInterMap.Count < 2)
				{
					// The intersection needs at least two empty cells.
					continue;
				}

				var b = HousesMap[otherBlock] - HousesMap[line];
				var c = a & b;

				list.Clear();
				switch (emptyCellsInInterMap)
				{
					case { Count: 2 }:
					{
						list.Add(emptyCellsInInterMap);

						break;
					}
					case [var i, var j, var k]:
					{
						list.Add(CellsMap[i] + j);
						list.Add(CellsMap[j] + k);
						list.Add(CellsMap[i] + k);
						list.Add(emptyCellsInInterMap);

						break;
					}
				}

				// Iterate on each intersection combination.
				foreach (var currentInterMap in list)
				{
					var selectedInterMask = grid.GetDigitsUnion(currentInterMap);
					if (PopCount((uint)selectedInterMask) <= currentInterMap.Count + 1)
					{
						// The intersection combination is an ALS or a normal subset,
						// which is invalid in SdCs.
						continue;
					}

					var blockMap = (b | c - currentInterMap) & EmptyCells;
					var lineMap = (a & EmptyCells) - otherCellsMap;

					// Iterate on the number of the cells that should be selected in block.
					for (var i = 1; i <= blockMap.Count - 1; i++)
					{
						// Iterate on each combination in block.
						foreach (var selectedCellsInBlock in blockMap & i)
						{
							var flag = false;
							foreach (var digit in otherDigitsMask)
							{
								foreach (var cell in selectedCellsInBlock)
								{
									if (CandidatesMap[digit].Contains(cell))
									{
										flag = true;
										break;
									}
								}
							}
							if (flag)
							{
								continue;
							}

							var currentBlockMap = selectedCellsInBlock;
							var elimMapBlock = CellMap.Empty;
							var elimMapLine = CellMap.Empty;

							// Get the links of the block.
							var blockMask = grid.GetDigitsUnion(selectedCellsInBlock);

							// Get the elimination map in the block.
							foreach (var digit in blockMask)
							{
								elimMapBlock |= CandidatesMap[digit];
							}
							elimMapBlock &= blockMap - currentBlockMap;

							foreach (var digit in otherDigitsMask)
							{
								elimMapLine |= CandidatesMap[digit];
							}
							elimMapLine &= lineMap - currentInterMap;

							checkGeneralizedSdc(
								accumulator, grid, arMode, cannibalMode, d1, d2, urCells,
								line, otherBlock, otherDigitsMask, blockMask, selectedInterMask,
								otherDigitsMask, elimMapLine, elimMapBlock, otherCellsMap, currentBlockMap,
								currentInterMap, i, 0, index
							);
						}
					}
				}
			}
		}


		static void checkGeneralizedSdc(
			ICollection<UniqueRectangleStep> accumulator,
			scoped in Grid grid,
			bool arMode,
			bool cannibalMode,
			Digit digit1,
			Digit digit2,
			Cell[] urCells,
			House line,
			House block,
			Mask lineMask,
			Mask blockMask,
			Mask selectedInterMask,
			Mask otherDigitsMask,
			scoped in CellMap elimMapLine,
			scoped in CellMap elimMapBlock,
			scoped in CellMap currentLineMap,
			scoped in CellMap currentBlockMap,
			scoped in CellMap currentInterMap,
			int i,
			int j,
			int index
		)
		{
			var maskOnlyInInter = (Mask)(selectedInterMask & ~(blockMask | lineMask));
			var maskIsolated = (Mask)(cannibalMode ? (lineMask & blockMask & selectedInterMask) : maskOnlyInInter);
			if (!cannibalMode && ((blockMask & lineMask) != 0 || maskIsolated != 0 && !IsPow2(maskIsolated))
				|| cannibalMode && !IsPow2(maskIsolated))
			{
				return;
			}

			var elimMapIsolated = CellMap.Empty;
			var digitIsolated = TrailingZeroCount(maskIsolated);
			if (digitIsolated != InvalidValidOfTrailingZeroCountMethodFallback)
			{
				elimMapIsolated = (cannibalMode ? currentBlockMap | currentLineMap : currentInterMap)
					% CandidatesMap[digitIsolated]
					& EmptyCells;
			}

			if (currentInterMap.Count + i + j + 1 == PopCount((uint)blockMask) + PopCount((uint)lineMask) + PopCount((uint)maskOnlyInInter)
				&& (elimMapBlock | elimMapLine | elimMapIsolated) is not [])
			{
				// Check eliminations.
				var conclusions = new List<Conclusion>(10);
				foreach (var cell in elimMapBlock)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if ((blockMask >> digit & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
				}
				foreach (var cell in elimMapLine)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if ((lineMask >> digit & 1) != 0)
						{
							conclusions.Add(new(Elimination, cell, digit));
						}
					}
				}
				foreach (var cell in elimMapIsolated)
				{
					conclusions.Add(new(Elimination, cell, digitIsolated));
				}
				if (conclusions.Count == 0)
				{
					return;
				}

				// Record highlight candidates and cells.
				var candidateOffsets = new List<CandidateViewNode>();
				foreach (var cell in urCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								(otherDigitsMask >> digit & 1) != 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Normal,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in currentBlockMap)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								!cannibalMode && digit == digitIsolated ? WellKnownColorIdentifierKind.Auxiliary3 : WellKnownColorIdentifierKind.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}
				foreach (var cell in currentInterMap)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digitIsolated == digit
									? WellKnownColorIdentifierKind.Auxiliary3
									: (otherDigitsMask >> digit & 1) != 0 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}

				accumulator.Add(
					new UniqueRectangleWithSueDeCoqStep(
						conclusions.ToArray(),
						new[]
						{
							View.Empty
								| (arMode ? UniqueRectangleStepSearcherHelper.GetHighlightCells(urCells) : null)
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, block),
									new(WellKnownColorIdentifierKind.Auxiliary2, line)
								}
						},
						digit1,
						digit2,
						(CellMap)urCells,
						arMode,
						block,
						line,
						blockMask,
						lineMask,
						selectedInterMask,
						cannibalMode,
						maskIsolated,
						currentBlockMap,
						currentLineMap,
						currentInterMap,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR + baba grouping.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1.</param>
	/// <param name="d2">The digit 2.</param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The structures:</para>
	/// <para>
	/// Subtype 1:
	/// <code><![CDATA[
	///      ↓urCellInSameBlock
	/// ab  abc      abc  ←anotherCell
	///
	///     abcx-----abcy ←resultCell
	///           c
	///      ↑targetCell
	/// ]]></code>
	/// Where the digit <c>a</c> and <c>b</c> in the bottom-left cell <c>abcx</c> can be removed.
	/// </para>
	/// <para>
	/// Subtype 2:
	/// <code><![CDATA[
	/// abcx   | ab  abc
	///  |     |
	///  | c   |
	///  |     |
	/// abcy   |     abc
	/// ]]></code>
	/// </para>
	/// </remarks>
	partial void CheckBabaGroupingUnique(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index
	)
	{
		checkType1(grid);
#if IMPLEMENTED
		checkType2(grid);
#endif


		void checkType1(scoped in Grid grid)
		{
			var cells = (CellMap)urCells;

			// Check all cells are empty.
			var containsValueCells = false;
			foreach (var cell in cells)
			{
				if (grid.GetStatus(cell) != CellStatus.Empty)
				{
					containsValueCells = true;
					break;
				}
			}
			if (containsValueCells)
			{
				return;
			}

			// Iterate on each cell.
			foreach (var targetCell in cells)
			{
				var block = targetCell.ToHouseIndex(HouseType.Block);
				var bivalueCellsToCheck = (PeersMap[targetCell] & HousesMap[block] & BivalueCells) - cells;
				if (!bivalueCellsToCheck)
				{
					continue;
				}

				// Check all bi-value cells.
				foreach (var bivalueCellToCheck in bivalueCellsToCheck)
				{
					if ((CellsMap[bivalueCellToCheck] + targetCell).CoveredLine != InvalidValidOfTrailingZeroCountMethodFallback)
					{
						// 'targetCell' and 'bivalueCellToCheck' can't lie on a same line.
						continue;
					}

					if (grid.GetCandidates(bivalueCellToCheck) != comparer)
					{
						// 'bivalueCell' must contain both 'd1' and 'd2'.
						continue;
					}

					var urCellInSameBlock = ((HousesMap[block] & cells) - targetCell)[0];
					var coveredLine = (CellsMap[bivalueCellToCheck] + urCellInSameBlock).CoveredLine;
					if (coveredLine == InvalidValidOfTrailingZeroCountMethodFallback)
					{
						// The bi-value cell 'bivalueCellToCheck' should be lie on a same house
						// as 'urCellInSameBlock'.
						continue;
					}

					var anotherCell = (cells - urCellInSameBlock & HousesMap[coveredLine])[0];
					foreach (var extraDigit in (Mask)(grid.GetCandidates(targetCell) & ~comparer))
					{
						var abcMask = (Mask)(comparer | (Mask)(1 << extraDigit));
						if (grid.GetCandidates(anotherCell) != abcMask)
						{
							continue;
						}

						// Check the conjugate pair of the extra digit.
						var resultCell = (cells - urCellInSameBlock - anotherCell - targetCell)[0];
						var map = CellsMap[targetCell] + resultCell;
						var line = map.CoveredLine;
						if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(extraDigit, map, line))
						{
							continue;
						}

						if (grid.GetCandidates(urCellInSameBlock) != abcMask)
						{
							goto SubType2;
						}

						// Here, is the basic sub-type having passed the checking.
						// Gather conclusions.
						var conclusions = new List<Conclusion>();
						foreach (var digit in grid.GetCandidates(targetCell))
						{
							if (digit == d1 || digit == d2)
							{
								conclusions.Add(new(Elimination, targetCell, digit));
							}
						}
						if (conclusions.Count == 0)
						{
							goto SubType2;
						}

						// Gather views.
						var candidateOffsets = new List<CandidateViewNode>
						{
							new(WellKnownColorIdentifierKind.Auxiliary1, targetCell * 9 + extraDigit)
						};
						if (CandidatesMap[d1].Contains(resultCell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, resultCell * 9 + d1));
						}
						if (CandidatesMap[d2].Contains(resultCell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, resultCell * 9 + d2));
						}
						if (CandidatesMap[extraDigit].Contains(resultCell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, resultCell * 9 + extraDigit));
						}

						foreach (var digit in (Mask)(grid.GetCandidates(urCellInSameBlock) & abcMask))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, urCellInSameBlock * 9 + digit));
						}
						foreach (var digit in grid.GetCandidates(anotherCell))
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, anotherCell * 9 + digit));
						}
						var _xOr_yMask = grid.GetCandidates(bivalueCellToCheck);
						foreach (var digit in _xOr_yMask)
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, bivalueCellToCheck * 9 + digit));
						}

						// Add into the list.
						var extraDigitId = (byte)(char)(extraDigit + '1');
						var extraDigitMask = (Mask)(1 << extraDigit);
						accumulator.Add(
							new UniqueRectangleWithBabaGroupingStep(
								conclusions.ToArray(),
								new[]
								{
									View.Empty
										| new CellViewNode(WellKnownColorIdentifierKind.Normal, targetCell)
										| candidateOffsets
										| new HouseViewNode[]
										{
											new(WellKnownColorIdentifierKind.Normal, block),
											new(WellKnownColorIdentifierKind.Auxiliary1, line)
										},
									View.Empty
										| new CandidateViewNode[]
										{
											new(WellKnownColorIdentifierKind.Auxiliary1, resultCell * 9 + extraDigit),
											new(WellKnownColorIdentifierKind.Auxiliary1, targetCell * 9 + extraDigit)
										}
										| new BabaGroupViewNode[]
										{
											new(WellKnownColorIdentifierKind.Normal, bivalueCellToCheck, (byte)'y', _xOr_yMask),
											new(WellKnownColorIdentifierKind.Normal, targetCell, (byte)'x', _xOr_yMask),
											new(WellKnownColorIdentifierKind.Normal, urCellInSameBlock, extraDigitId, extraDigitMask),
											new(WellKnownColorIdentifierKind.Normal, anotherCell, (byte)'x', _xOr_yMask),
											new(WellKnownColorIdentifierKind.Normal, resultCell, extraDigitId, extraDigitMask)
										}
								},
								d1,
								d2,
								(CellMap)urCells,
								targetCell,
								extraDigit,
								index
							)
						);

					SubType2:
						// Sub-type 2.
						// The extra digit should form a conjugate pair in that line.
						var anotherMap = CellsMap[urCellInSameBlock] + anotherCell;
						var anotherLine = anotherMap.CoveredLine;
						if (!UniqueRectangleStepSearcherHelper.IsConjugatePair(extraDigit, anotherMap, anotherLine))
						{
							continue;
						}

						// Gather conclusions.
						var conclusionsAnotherSubType = new List<Conclusion>();
						foreach (var digit in grid.GetCandidates(targetCell))
						{
							if (digit == d1 || digit == d2)
							{
								conclusionsAnotherSubType.Add(new(Elimination, targetCell, digit));
							}
						}
						if (conclusionsAnotherSubType.Count == 0)
						{
							continue;
						}

						// Gather views.
						var candidateOffsetsAnotherSubtype = new List<CandidateViewNode>
						{
							new(WellKnownColorIdentifierKind.Auxiliary1, targetCell * 9 + extraDigit)
						};
						if (CandidatesMap[d1].Contains(resultCell))
						{
							candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Normal, resultCell * 9 + d1));
						}
						if (CandidatesMap[d2].Contains(resultCell))
						{
							candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Normal, resultCell * 9 + d2));
						}
						if (CandidatesMap[extraDigit].Contains(resultCell))
						{
							candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Auxiliary1, resultCell * 9 + extraDigit));
						}

						var candidateOffsetsAnotherSubtypeLighter = new List<CandidateViewNode>
						{
							new(WellKnownColorIdentifierKind.Auxiliary1, resultCell * 9 + extraDigit),
							new(WellKnownColorIdentifierKind.Auxiliary1, targetCell * 9 + extraDigit)
						};
						foreach (var digit in (Mask)(grid.GetCandidates(urCellInSameBlock) & abcMask))
						{
							if (digit == extraDigit)
							{
								candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Auxiliary1, urCellInSameBlock * 9 + digit));
								candidateOffsetsAnotherSubtypeLighter.Add(new(WellKnownColorIdentifierKind.Auxiliary1, urCellInSameBlock * 9 + digit));
							}
							else
							{
								candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Normal, urCellInSameBlock * 9 + digit));
							}
						}
						foreach (var digit in grid.GetCandidates(anotherCell))
						{
							if (digit == extraDigit)
							{
								candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Auxiliary1, anotherCell * 9 + digit));
								candidateOffsetsAnotherSubtypeLighter.Add(new(WellKnownColorIdentifierKind.Auxiliary1, anotherCell * 9 + digit));
							}
							else
							{
								candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Normal, anotherCell * 9 + digit));
							}
						}
						var _xOr_yMask2 = grid.GetCandidates(bivalueCellToCheck);
						foreach (var digit in _xOr_yMask2)
						{
							candidateOffsetsAnotherSubtype.Add(new(WellKnownColorIdentifierKind.Auxiliary2, bivalueCellToCheck * 9 + digit));
						}

						// Add into the list.
						var extraDigitId2 = (byte)(char)(extraDigit + '1');
						var extraDigitMask2 = (Mask)(1 << extraDigit);
						accumulator.Add(
							new UniqueRectangleWithBabaGroupingStep(
								conclusionsAnotherSubType.ToArray(),
								new[]
								{
									View.Empty
										| new CellViewNode(WellKnownColorIdentifierKind.Normal, targetCell)
										| candidateOffsetsAnotherSubtype
										| new HouseViewNode[]
										{
											new(WellKnownColorIdentifierKind.Normal, block),
											new(WellKnownColorIdentifierKind.Auxiliary1, line),
											new(WellKnownColorIdentifierKind.Auxiliary1, anotherLine)
										},
									View.Empty
										| candidateOffsetsAnotherSubtypeLighter
										| new BabaGroupViewNode[]
										{
											new(WellKnownColorIdentifierKind.Normal, bivalueCellToCheck, (byte)'y', _xOr_yMask2),
											new(WellKnownColorIdentifierKind.Normal, targetCell, (byte)'x', _xOr_yMask2),
											new(WellKnownColorIdentifierKind.Normal, urCellInSameBlock, extraDigitId2, extraDigitMask2),
											new(WellKnownColorIdentifierKind.Normal, anotherCell, (byte)'x', _xOr_yMask2),
											new(WellKnownColorIdentifierKind.Normal, resultCell, extraDigitId2, extraDigitMask2)
										}
								},
								d1,
								d2,
								(CellMap)urCells,
								targetCell,
								extraDigit,
								index
							)
						);
					}
				}
			}
		}

#if false
		void checkType2(scoped in Grid grid)
		{
			// TODO: Check type 2.
		}
#endif
	}

	/// <summary>
	/// <para>Check UR/AR + Guardian (i.e. UR External Type 2) and UR External Type 1.</para>
	/// <para>
	/// A UR external type 1 is a special case for type 2, which means only one guardian cell will be used.
	/// </para>
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalType1Or2(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var houseCells = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((houseCells & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardian1 = houseCells - cells & CandidatesMap[d1];
			var guardian2 = houseCells - cells & CandidatesMap[d2];
			if (!guardian1 ^ !guardian2)
			{
				var guardianDigit = -1;
				var targetElimMap = default(CellMap?);
				var targetGuardianMap = default(CellMap?);
				if (guardian1 is not [] && (guardian1.PeerIntersection & CandidatesMap[d1]) is var a and not [])
				{
					targetElimMap = a;
					guardianDigit = d1;
					targetGuardianMap = guardian1;
				}
				else if (guardian2 is not [] && (guardian2.PeerIntersection & CandidatesMap[d2]) is var b and not [])
				{
					targetElimMap = b;
					guardianDigit = d2;
					targetGuardianMap = guardian2;
				}

				if (targetElimMap is not { } elimMap || targetGuardianMap is not { } guardianMap || guardianDigit == -1)
				{
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>(16);
				var cellOffsets = new List<CellViewNode>();
				foreach (var cell in urCells)
				{
					if (CandidatesMap[d1].Contains(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d1));
					}
					if (CandidatesMap[d2].Contains(cell))
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d2));
					}

					if (grid.GetStatus(cell) == CellStatus.Modifiable)
					{
						cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
					}
				}
				foreach (var cell in guardianMap)
				{
					candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + guardianDigit));
				}

				accumulator.Add(
					new UniqueRectangleExternalType1Or2Step(
						from cell in elimMap select new Conclusion(Elimination, cell, guardianDigit),
						new[]
						{
							View.Empty
								| cellOffsets
								| candidateOffsets
								| new HouseViewNode[]
								{
									new(WellKnownColorIdentifierKind.Normal, houseCombination[0]),
									new(WellKnownColorIdentifierKind.Normal, houseCombination[1])
								}
						},
						d1,
						d2,
						(CellMap)urCells,
						guardianMap,
						guardianDigit,
						UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
						arMode,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external subset (i.e. UR External Type 3).
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalType3(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap - cells & EmptyCells;
			foreach (var guardianCellPair in guardianCells & 2)
			{
				var c1 = guardianCellPair[0];
				var c2 = guardianCellPair[1];
				if (!UniqueRectangleStepSearcherHelper.IsSameHouseCell(c1, c2, out var houses))
				{
					// Those two cells must lie in a same house.
					continue;
				}

				var mask = (Mask)(grid.GetCandidates(c1) | grid.GetCandidates(c2));
				if ((mask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((grid.GetCandidates(c1) & comparer) == 0 || (grid.GetCandidates(c2) & comparer) == 0)
				{
					// Both two cells chosen must contain at least one of two UR digits.
					continue;
				}

				if ((guardianCells & (CandidatesMap[d1] | CandidatesMap[d2])) != guardianCellPair)
				{
					// The current map must be equal to the whole guardian full map.
					continue;
				}

				foreach (var house in houses)
				{
					var houseCells = HousesMap[house] - cells - guardianCellPair & EmptyCells;
					for (var size = 2; size <= houseCells.Count; size++)
					{
						foreach (var otherCells in houseCells & size - 1)
						{
							var subsetDigitsMask = (Mask)(grid.GetDigitsUnion(otherCells) | comparer);
							if (PopCount((uint)subsetDigitsMask) != size)
							{
								// The subset cannot formed.
								continue;
							}

							// UR Guardian External Subsets found. Now check eliminations.
							var elimMap = (houseCells | guardianCellPair) - otherCells;
							var conclusions = new List<Conclusion>();
							foreach (var cell in elimMap)
							{
								var elimDigitsMask = guardianCellPair.Contains(cell) ? (Mask)(subsetDigitsMask & ~comparer) : subsetDigitsMask;
								foreach (var digit in elimDigitsMask)
								{
									if (CandidatesMap[digit].Contains(cell))
									{
										conclusions.Add(new(Elimination, cell, digit));
									}
								}
							}
							if (conclusions.Count == 0)
							{
								continue;
							}

							var candidateOffsets = new List<CandidateViewNode>();
							var cellOffsets = new List<CellViewNode>();
							foreach (var cell in urCells)
							{
								if (CandidatesMap[d1].Contains(cell))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d1));
								}
								if (CandidatesMap[d2].Contains(cell))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d2));
								}

								if (grid.GetStatus(cell) == CellStatus.Modifiable)
								{
									cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
								}
							}
							foreach (var cell in guardianCellPair)
							{
								if (CandidatesMap[d1].Contains(cell))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, cell * 9 + d1));
								}
								if (CandidatesMap[d2].Contains(cell))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, cell * 9 + d2));
								}
							}
							foreach (var cell in otherCells)
							{
								foreach (var digit in grid.GetCandidates(cell))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + digit));
								}
							}

							accumulator.Add(
								new UniqueRectangleExternalType3Step(
									conclusions.ToArray(),
									new[]
									{
										View.Empty
											| cellOffsets
											| candidateOffsets
											| new HouseViewNode[]
											{
												new(WellKnownColorIdentifierKind.Normal, house),
												new(WellKnownColorIdentifierKind.Auxiliary2, houseCombination[0]),
												new(WellKnownColorIdentifierKind.Auxiliary2, houseCombination[1])
											}
									},
									d1,
									d2,
									cells,
									guardianCellPair,
									otherCells,
									subsetDigitsMask,
									UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
									arMode,
									index
								)
							);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external conjugate pair (i.e. UR External Type 4).
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The mask comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalType4(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap - cells & EmptyCells;
			foreach (var guardianCellPair in guardianCells & 2)
			{
				var c1 = guardianCellPair[0];
				var c2 = guardianCellPair[1];
				if (!UniqueRectangleStepSearcherHelper.IsSameHouseCell(c1, c2, out var houses))
				{
					// Those two cells must lie in a same house.
					continue;
				}

				var mask = (Mask)(grid.GetCandidates(c1) | grid.GetCandidates(c2));
				if ((mask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((grid.GetCandidates(c1) & comparer) == 0 || (grid.GetCandidates(c2) & comparer) == 0)
				{
					// Both two cells chosen must contain at least one of two UR digits.
					continue;
				}

				if ((guardianCells & (CandidatesMap[d1] | CandidatesMap[d2])) != guardianCellPair)
				{
					// The current map must be equal to the whole guardian full map.
					continue;
				}

				var possibleConjugatePairDigitsMask = (Mask)(grid.GetDigitsUnion(guardianCellPair) & ~comparer);
				foreach (var house in houses)
				{
					foreach (var conjugatePairDigit in possibleConjugatePairDigitsMask)
					{
						if (!CandidatesMap[conjugatePairDigit].Contains(c1) || !CandidatesMap[conjugatePairDigit].Contains(c2))
						{
							// The conjugate pair can be formed if and only if both guardian cells
							// must contain that digit.
							continue;
						}

						if ((CandidatesMap[conjugatePairDigit] & HousesMap[house]) != guardianCellPair)
						{
							// The house cannot contain any other cells containing that digit.
							continue;
						}

						var conclusions = new List<Conclusion>();
						var elimDigitsMask = (Mask)(possibleConjugatePairDigitsMask & ~(1 << conjugatePairDigit));
						foreach (var elimDigit in elimDigitsMask)
						{
							foreach (var cell in CandidatesMap[elimDigit] & guardianCellPair)
							{
								conclusions.Add(new(Elimination, cell, elimDigit));
							}
						}
						if (conclusions.Count == 0)
						{
							// No eliminations found.
							continue;
						}

						var candidateOffsets = new List<CandidateViewNode>();
						var cellOffsets = new List<CellViewNode>();
						foreach (var cell in urCells)
						{
							if (CandidatesMap[d1].Contains(cell))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d1));
							}
							if (CandidatesMap[d2].Contains(cell))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + d2));
							}

							if (grid.GetStatus(cell) == CellStatus.Modifiable)
							{
								cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
							}
						}
						foreach (var cell in guardianCellPair)
						{
							if (CandidatesMap[d1].Contains(cell))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, cell * 9 + d1));
							}
							if (CandidatesMap[d2].Contains(cell))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, cell * 9 + d2));
							}

							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + conjugatePairDigit));
						}

						accumulator.Add(
							new UniqueRectangleExternalType4Step(
								conclusions.ToArray(),
								new[]
								{
									View.Empty
										| cellOffsets
										| candidateOffsets
										| new HouseViewNode[]
										{
											new(WellKnownColorIdentifierKind.Normal, house),
											new(WellKnownColorIdentifierKind.Auxiliary2, houseCombination[0]),
											new(WellKnownColorIdentifierKind.Auxiliary2, houseCombination[1])
										}
								},
								d1,
								d2,
								cells,
								guardianCellPair,
								new(guardianCellPair, conjugatePairDigit),
								UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
								arMode,
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external turbot fish.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalTurbotFish(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap - cells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (guardianCells is { Count: not 1, CoveredHouses: 0 })
			{
				// All guardian cells must lie in one house.
				continue;
			}

			var digit1IntersectionMap = guardianCells & CandidatesMap[d1];
			var digit2IntersectionMap = guardianCells & CandidatesMap[d2];
			if (digit1IntersectionMap && ~digit2IntersectionMap || ~digit1IntersectionMap && digit2IntersectionMap)
			{
				// For this type guardian cells can only hold one digit appeared in UR.
				continue;
			}

			var guardianDigit = digit1IntersectionMap ? d1 : d2;
			if (guardianCells.Count != 2)
			{
				continue;
			}

			foreach (var coveredHouse in guardianCells.CoveredHouses)
			{
				for (var house = 0; house < 27; house++)
				{
					if (house == coveredHouse)
					{
						continue;
					}

					if ((cells.Houses >> house & 1) != 0)
					{
						// It's unnecessary to construct another strong link on houses that UR has already covered.
						continue;
					}

					var potentialConjugatePairMap = HousesMap[house] & CandidatesMap[guardianDigit];
					if (potentialConjugatePairMap.Count != 2)
					{
						// There is no conjugate pairs in the house.
						continue;
					}

					if ((potentialConjugatePairMap.ExpandedPeers & guardianCells) is not [var guardianConnectedCell])
					{
						// They cannot be connected.
						continue;
					}

					// Possible external turbot fish found.
					var anotherHead = (potentialConjugatePairMap - (PeersMap[guardianConnectedCell] & potentialConjugatePairMap)[0])[0];
					var guardianHead = (guardianCells - guardianConnectedCell)[0];
					var elimMap = PeersMap[guardianHead] & PeersMap[anotherHead] & CandidatesMap[guardianDigit];
					if (!elimMap)
					{
						// No eliminations found.
						continue;
					}

					var candidateOffsets = new List<CandidateViewNode>();
					var cellOffsets = new List<CellViewNode>();
					foreach (var cell in urCells)
					{
						switch (grid.GetStatus(cell))
						{
							case CellStatus.Empty:
							{
								foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
								}

								break;
							}
							case CellStatus.Modifiable:
							{
								cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
								break;
							}
						}
					}
					foreach (var cell in guardianCells)
					{
						candidateOffsets.Add(
							new(
								cell == guardianHead ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Auxiliary2,
								cell * 9 + guardianDigit
							)
						);
					}
					foreach (var cell in potentialConjugatePairMap)
					{
						candidateOffsets.Add(
							new(
								cell == anotherHead ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Auxiliary2,
								cell * 9 + guardianDigit
							)
						);
					}

					accumulator.Add(
						new UniqueRectangleExternalTurbotFishStep(
							from cell in elimMap select new Conclusion(Elimination, cell, guardianDigit),
							new[] { View.Empty | cellOffsets | candidateOffsets },
							d1,
							d2,
							cells,
							guardianCells,
							potentialConjugatePairMap,
							guardianDigit,
							coveredHouse,
							house,
							UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
							arMode,
							index
						)
					);
				}
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external XY-Wing.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalXyWing(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap - cells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (!(guardianCells & CandidatesMap[d1]) || !(guardianCells & CandidatesMap[d2]))
			{
				// Guardian cells must contain both two digits; otherwise, skip the current case.
				continue;
			}

			var cellsToEnumerate = guardianCells.ExpandedPeers - guardianCells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (cellsToEnumerate.Count < 2)
			{
				// No valid combinations.
				continue;
			}

			foreach (var cellPair in cellsToEnumerate & 2)
			{
				var cell1 = cellPair[0];
				var cell2 = cellPair[1];
				var mask1 = grid.GetCandidates(cell1);
				var mask2 = grid.GetCandidates(cell2);
				var intersectionMask = (Mask)(mask1 & mask2);
				if (!IsPow2(intersectionMask))
				{
					// No eliminations can be found in this structure.
					continue;
				}

				var unionMask = (Mask)(mask1 | mask2);
				if ((unionMask & comparer) != comparer)
				{
					// The two cells must contain both two digits.
					continue;
				}

				if ((unionMask & ~(comparer | intersectionMask)) != 0)
				{
					// These two cells may contain extra cells, which is disallowed in this pattern.
					continue;
				}

				var cell1UrDigit = TrailingZeroCount((Mask)(mask1 & ~intersectionMask));
				var cell2UrDigit = TrailingZeroCount((Mask)(mask2 & ~intersectionMask));
				var guardianCellsThatContainsDigit1 = guardianCells & CandidatesMap[cell1UrDigit];
				var guardianCellsThatContainsDigit2 = guardianCells & CandidatesMap[cell2UrDigit];
				if ((PeersMap[cell1] & guardianCellsThatContainsDigit1) != guardianCellsThatContainsDigit1
					|| (PeersMap[cell2] & guardianCellsThatContainsDigit2) != guardianCellsThatContainsDigit2)
				{
					// Two cells must see all guardian cells.
					continue;
				}

				// UR External XY-Wing found. Now check for eliminations.
				var elimDigit = TrailingZeroCount(intersectionMask);
				var elimMap = cellPair.PeerIntersection & CandidatesMap[elimDigit];
				if (!elimMap)
				{
					// No elimination cell.
					continue;
				}

				var candidateOffsets = new List<CandidateViewNode>();
				var cellOffsets = new List<CellViewNode>();
				foreach (var cell in urCells)
				{
					switch (grid.GetStatus(cell))
					{
						case CellStatus.Empty:
						{
							foreach (var digit in (Mask)(grid.GetCandidates(cell) & comparer))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
							}

							break;
						}
						case CellStatus.Modifiable:
						{
							cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));

							break;
						}
					}
				}
				foreach (var cell in guardianCells)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						if (digit != d1 && digit != d2)
						{
							continue;
						}

						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, cell * 9 + digit));
					}
				}
				foreach (var cell in cellPair)
				{
					foreach (var digit in grid.GetCandidates(cell))
					{
						candidateOffsets.Add(
							new(
								digit != d1 && digit != d2 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.Auxiliary2,
								cell * 9 + digit
							)
						);
					}
				}

				accumulator.Add(
					new UniqueRectangleExternalXyWingStep(
						from cell in elimMap select new Conclusion(Elimination, cell, elimDigit),
						new[] { View.Empty | cellOffsets | candidateOffsets },
						d1,
						d2,
						cells,
						guardianCells,
						cellPair,
						UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
						arMode,
						index
					)
				);
			}
		}
	}

	/// <summary>
	/// Check UR/AR + Guardian, with external ALS-XZ rule.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="alses">The ALS structures.</param>
	/// <param name="comparer">The comparer.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="index">The mask index.</param>
	/// <param name="arMode"></param>
	partial void CheckExternalAlmostLockedSetsXz(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		AlmostLockedSet[] alses,
		Mask comparer,
		Digit d1,
		Digit d2,
		int index,
		bool arMode
	)
	{
		var cells = (CellMap)urCells;

		if (!UniqueRectangleStepSearcherHelper.CheckPreconditionsOnIncomplete(grid, urCells, d1, d2))
		{
			return;
		}

		if (!arMode && (EmptyCells & cells) != cells)
		{
			return;
		}

		// Iterate on two houses used.
		foreach (var houseCombination in cells.Houses.GetAllSets().GetSubsets(2))
		{
			var guardianMap = HousesMap[houseCombination[0]] | HousesMap[houseCombination[1]];
			if ((guardianMap & cells) != cells)
			{
				// The houses must contain all 4 UR cells.
				continue;
			}

			var guardianCells = guardianMap - cells & (CandidatesMap[d1] | CandidatesMap[d2]);
			if (guardianCells is { Count: not 1, CoveredHouses: 0 })
			{
				// All guardian cells must lie in one house.
				continue;
			}

			if (!(guardianCells & CandidatesMap[d1]) || !(guardianCells & CandidatesMap[d2]))
			{
				// Guardian cells must contain both two digits; otherwise, skip the current case.
				continue;
			}

			foreach (var (zDigit, xDigit) in stackalloc[] { (d1, d2), (d2, d1) })
			{
				var xDigitGuardianCells = CandidatesMap[xDigit] & guardianCells;
				var zDigitGuardianCells = CandidatesMap[zDigit] & guardianCells;
				foreach (var connectedXDigitHouse in xDigitGuardianCells.CoveredHouses)
				{
					foreach (var als in alses)
					{
						var alsHouse = als.House;
						var alsMask = als.DigitsMask;
						var alsMap = als.Cells;
						if ((cells.Houses >> alsHouse & 1) != 0)
						{
							// The current ALS cannot lie in the house that UR cells covered.
							continue;
						}

						if ((alsMask >> d1 & 1) == 0 || (alsMask >> d2 & 1) == 0)
						{
							// The ALS must uses both two digits.
							continue;
						}

						if ((alsMap & xDigitGuardianCells) is not [] || zDigitGuardianCells == (alsMap & CandidatesMap[zDigit]))
						{
							// The ALS cannot only use X or Z digits that all appears in guardian cells.
							continue;
						}

						var elimMap = (alsMap | zDigitGuardianCells) % CandidatesMap[zDigit];
						if (!elimMap)
						{
							// No eliminations found.
							continue;
						}

						var xDigitMap = (alsMap | xDigitGuardianCells) & CandidatesMap[xDigit];
						if (!xDigitMap.InOneHouse)
						{
							// The X digit must be connected.
							continue;
						}

						// ALS-XZ formed.
						var candidateOffsets = new List<CandidateViewNode>();
						var cellOffsets = new List<CellViewNode>();
						foreach (var urCell in urCells)
						{
							switch (grid.GetStatus(urCell))
							{
								case CellStatus.Empty:
								{
									foreach (var digit in comparer)
									{
										if (CandidatesMap[digit].Contains(urCell))
										{
											candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, urCell * 9 + digit));
										}
									}

									break;
								}
								case CellStatus.Modifiable:
								{
									cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, urCell));
									break;
								}
							}
						}
						foreach (var xDigitCell in xDigitGuardianCells)
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, xDigitCell * 9 + xDigit));
						}
						foreach (var zDigitCell in zDigitGuardianCells)
						{
							candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary2, zDigitCell * 9 + zDigit));
						}
						foreach (var alsCell in alsMap)
						{
							foreach (var digit in grid.GetCandidates(alsCell))
							{
								candidateOffsets.Add(
									new(
										digit == d1 || digit == d2 ? WellKnownColorIdentifierKind.Auxiliary1 : WellKnownColorIdentifierKind.AlmostLockedSet1,
										alsCell * 9 + digit
									)
								);
							}
						}

						accumulator.Add(
							new UniqueRectangleExternalAlmostLockedSetsXzStep(
								from cell in elimMap select new Conclusion(Elimination, cell, zDigit),
								new[]
								{
									View.Empty
										| candidateOffsets
										| cellOffsets
										| new HouseViewNode(WellKnownColorIdentifierKind.AlmostLockedSet1, alsHouse)
								},
								d1,
								d2,
								cells,
								guardianCells,
								als,
								UniqueRectangleStepSearcherHelper.IsIncomplete(AllowIncompleteUniqueRectangles, candidateOffsets),
								arMode,
								index
							)
						);
					}
				}
			}
		}
	}

	/// <summary>
	/// Check AR + Hidden single.
	/// </summary>
	/// <param name="accumulator">The technique accumulator.</param>
	/// <param name="grid">The grid.</param>
	/// <param name="urCells">All UR cells.</param>
	/// <param name="d1">The digit 1 used in UR.</param>
	/// <param name="d2">The digit 2 used in UR.</param>
	/// <param name="corner1">The corner cell 1.</param>
	/// <param name="corner2">The corner cell 2.</param>
	/// <param name="otherCellsMap">
	/// The map of other cells during the current UR searching.
	/// </param>
	/// <param name="index">The index.</param>
	/// <remarks>
	/// <para>The structure:</para>
	/// <para>
	/// <code><![CDATA[
	/// ↓corner1
	/// a   | aby  -  -
	/// abx | a    -  b
	///     | -    -  -
	///       ↑corner2(cell 'a')
	/// ]]></code>
	/// There's only one cell can be filled with the digit 'b' besides the cell 'aby'.
	/// </para>
	/// </remarks>
	partial void CheckHiddenSingleAvoidable(
		ICollection<UniqueRectangleStep> accumulator,
		scoped in Grid grid,
		Cell[] urCells,
		Digit d1,
		Digit d2,
		Cell corner1,
		Cell corner2,
		scoped in CellMap otherCellsMap,
		int index
	)
	{
		if (grid.GetStatus(corner1) != CellStatus.Modifiable || grid.GetStatus(corner2) != CellStatus.Modifiable
			|| grid[corner1] != grid[corner2] || grid[corner1] != d1 && grid[corner1] != d2)
		{
			return;
		}

		// Get the base digit ('a') and the other digit ('b').
		// Here 'b' is the digit that we should check the possible hidden single.
		var baseDigit = grid[corner1];
		var otherDigit = baseDigit == d1 ? d2 : d1;
		var cellsThatTwoOtherCellsBothCanSee = otherCellsMap.PeerIntersection & CandidatesMap[otherDigit];

		// Iterate on two cases (because we holds two other cells,
		// and both those two cells may contain possible elimination).
		for (var i = 0; i < 2; i++)
		{
			var (baseCell, anotherCell) = i == 0 ? (otherCellsMap[0], otherCellsMap[1]) : (otherCellsMap[1], otherCellsMap[0]);

			// Iterate on each house type.
			foreach (var houseType in HouseTypes)
			{
				var houseIndex = baseCell.ToHouseIndex(houseType);

				// If the house doesn't overlap with the specified house, just skip it.
				if (!(cellsThatTwoOtherCellsBothCanSee & HousesMap[houseIndex]))
				{
					continue;
				}

				var otherCells = HousesMap[houseIndex] & CandidatesMap[otherDigit] & PeersMap[anotherCell];
				var sameHouses = (otherCells + anotherCell).CoveredHouses;
				foreach (var sameHouse in sameHouses)
				{
					// Check whether all possible positions of the digit 'b' in this house only
					// lies in the given cells above ('cellsThatTwoOtherCellsBothCanSee').
					if ((HousesMap[sameHouse] - anotherCell & CandidatesMap[otherDigit]) != otherCells)
					{
						continue;
					}

					// Possible hidden single found.
					// If the elimination doesn't exist, just skip it.
					if (!CandidatesMap[otherDigit].Contains(baseCell))
					{
						continue;
					}

					var cellOffsets = new List<CellViewNode>();
					foreach (var cell in urCells)
					{
						cellOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell));
					}

					var candidateOffsets = new List<CandidateViewNode>
					{
						new(WellKnownColorIdentifierKind.Normal, anotherCell * 9 + otherDigit)
					};
					foreach (var cell in otherCells)
					{
						candidateOffsets.Add(new(WellKnownColorIdentifierKind.Auxiliary1, cell * 9 + otherDigit));
					}

					accumulator.Add(
						new AvoidableRectangleWithHiddenSingleStep(
							new[] { new Conclusion(Elimination, baseCell, otherDigit) },
							new[] { View.Empty | cellOffsets | candidateOffsets | new HouseViewNode(WellKnownColorIdentifierKind.Normal, sameHouse) },
							d1,
							d2,
							(CellMap)urCells,
							baseCell,
							anotherCell,
							sameHouse,
							index
						)
					);
				}
			}
		}
	}
}
