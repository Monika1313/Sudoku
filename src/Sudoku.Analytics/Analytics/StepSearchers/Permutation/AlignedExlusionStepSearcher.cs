namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with an <b>Aligned Exclusion</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Aligned Exclusion</item>
/// </list>
/// </summary>
[StepSearcher]
[ConditionalCases(ConditionalCase.UnlimitedTimeComplexity)]
public sealed partial class AlignedExclusionStepSearcher : StepSearcher
{
	/// <summary>
	/// Indicates the maximum searching size. This value must be greater than 2 and less than 5. The default value is 3.
	/// </summary>
	public int MaxSearchingSize { get; set; } = 3;


	/// <inheritdoc/>
	protected internal override Step? Collect(scoped ref AnalysisContext context)
	{
		scoped ref readonly var grid = ref context.Grid;
		foreach (var cell in EmptyCells)
		{
			if (IsPow2(grid.GetCandidates(cell)))
			{
				// This technique cannot be used for a grid containing naked singles.
				return null;
			}
		}

		for (var size = 3; size < MaxSearchingSize; size++)
		{
			// Search for "base" cells that can participate to an exclusion set. For each candidate, collect the potentially excluding cells.
			var candidateList = CellMap.Empty;
			var cellExcluders = new Dictionary<Cell, CellMap>();
			foreach (var cell in EmptyCells)
			{
				if (PopCount((uint)grid.GetCandidates(cell)) < 2)
				{
					continue;
				}

				// Look for potentially excluding cells (whose number of candidates is less than size).
				var excludingCells = CellMap.Empty;
				foreach (var excludingCell in Peers[cell])
				{
					var count = PopCount((uint)grid.GetCandidates(excludingCell));
					if (count >= 2 && count < size)
					{
						excludingCells.Add(excludingCell);
					}
				}

				if (excludingCells)
				{
					candidateList.Add(cell);
					cellExcluders.Add(cell, excludingCells);
				}
			}

			if (cellExcluders.Count < size)
			{
				// Not enough elements to be checked.
				return null;
			}

			// Iterate on all permutations of 'size' cells among the possible base cells.
			// To iterate over 'n' cells (n > 2), we first iterate among two cells.
			// Then we retain only the other cells that are visible by at least one of these two cells (the twinArea),
			// and we continue the iteration on these remaining cells.

			// First iterate on the first two cells.
			foreach (var cellPair in candidateList & 2)
			{
				// Setup the first two cells.
				var cell1 = cellPair[0];
				var cell2 = cellPair[1];
				var cell1Count = PopCount((uint)grid.GetCandidates(cell1));
				var cell2Count = PopCount((uint)grid.GetCandidates(cell2));

				// Create the twin area: set of cells visible by one of the two first cells.
				var twinArea = (cellExcluders[cell1] | cellExcluders[cell2]) - candidateList - cell1 - cell2;

				// Check if we have enough cells in the twin Area.
				if (twinArea.Count < size - 2)
				{
					continue;
				}

				var tailCells = twinArea;

				// Iterate on remaining cells using the twinArea.
				foreach (Mask tailCombinationMask in new MaskCombinationsGenerator(tailCells.Count, size - 2))
				{
					var cells = new Cell[size];
					var cadinalities = new int[size];

					// Copy the first two cells.
					cells[0] = cell1;
					cells[1] = cell2;
					cadinalities[0] = cell1Count;
					cadinalities[1] = cell2Count;

					// Add the tail cells.
					scoped var tIndices = tailCombinationMask.GetAllSets();
					for (var i = 0; i < tIndices.Length; i++)
					{
						cells[i + 2] = tailCells[tIndices[i]];
						cadinalities[i + 2] = PopCount((uint)grid.GetCandidates(cells[i + 2]));
					}

					// Build the list of common excluding cells for the base cells 'cells'.
					var commonExcluders = CellMap.Empty;
					for (var i = 0; i < size; i++)
					{
						var excludingCells = cellExcluders[cells[i]];
						if (i == 0)
						{
							commonExcluders |= excludingCells;
						}
						else
						{
							commonExcluders &= excludingCells;
						}
					}

					if (commonExcluders.Count < 2)
					{
						continue;
					}

					var potentialIndices = new int[size];

					// Iterate on combinations of candidates across the base cells.
					var allowedCombinations = new List<Digit[]>();
					var lockedCombinations = new Dictionary<Digit[], Cell>();
					bool isFinished;
					do
					{
						// Get next combination of indices.
						var z = 0;
						bool rollOver;
						do
						{
							if (potentialIndices[z] == 0)
							{
								rollOver = true;
								potentialIndices[z] = cadinalities[z] - 1;
								z++;
							}
							else
							{
								rollOver = false;
								potentialIndices[z]--;
							}
						} while (z < size && rollOver);

						// Build the combination of potentials.
						var potentials = new Digit[size];
						for (var i = 0; i < size; i++)
						{
							var values = grid.GetCandidates(cells[i]);
							var p = values.GetNextSet(0);
							for (var j = 0; j < potentialIndices[i]; j++)
							{
								p = values.GetNextSet(p + 1);
							}

							potentials[i] = p;
						}

						var isAllowed = true;
						var lockingCell = -1;

						// Check if this candidate combination is allowed, using hidden single rule.
						foreach (Mask mask in new MaskCombinationsGenerator(size, 2))
						{
							scoped var cellIndices = mask.GetAllSets();
							var p1 = potentials[cellIndices[0]];
							var p2 = potentials[cellIndices[1]];

							if (p1 == p2)
							{
								// Hidden single: Using the same candidate value for 2 cells of the set is only allowed
								// if they don't share a house.
								var c1 = cells[cellIndices[0]];
								var c2 = cells[cellIndices[1]];
								if (PeersMap[c1].Contains(c2))
								{
									isAllowed = false;
									break;
								}
							}
						}

						// Check if this candidate combination is allowed, using common excluder cells.
						if (isAllowed)
						{
							foreach (var excludingCell in commonExcluders)
							{
								var values = grid.GetCandidates(excludingCell);
								for (var i = 0; i < size; i++)
								{
									values &= (Mask)~(1 << potentials[i]);
								}

								if (values == 0)
								{
									lockingCell = excludingCell;
									isAllowed = false;
									break;
								}
							}
						}

						// Store the combination in the appropriate pattern.
						if (isAllowed)
						{
							allowedCombinations.Add(potentials);
						}
						else
						{
							lockedCombinations.Add(potentials, lockingCell);
						}

						// Check if last combination of candidates from the base cells has been reached.
						isFinished = true;
						for (var i = 0; i < size; i++)
						{
							if (potentialIndices[i] != 0)
							{
								isFinished = false;
								break;
							}
						}
					} while (!isFinished);

					// For all candidates of all base cells, test if the value is possible in at least one allowed combination.
					var conclusions = new List<Conclusion>();
					var conclusionCandidates = CandidateMap.Empty;
					for (var i = 0; i < size; i++)
					{
						var cell = cells[i];
						var values = grid.GetCandidates(cell);
						for (var p = values.GetNextSet(0); p != -1; p = values.GetNextSet(p + 1))
						{
							var isValueAllowed = false;
							foreach (var combination in allowedCombinations)
							{
								if (combination[i] == p)
								{
									isValueAllowed = true; // At least one allowed combination permits this value.
									break;
								}
							}

							if (!isValueAllowed)
							{
								// Yeah, value p can be excluded from cell.
								conclusions.Add(new(Elimination, cell, p));
								conclusionCandidates.Add(cell * 9 + p);
							}
						}
					}

					// Get all highlighted candidates.
					var candidateOffsets = new List<CandidateViewNode>();
					var relaventCandidates = GetRelaventCombinationValues(lockedCombinations, conclusions, cells);
					foreach (var (_, cell) in lockedCombinations)
					{
						if (cell != -1)
						{
							var digits = grid.GetCandidates(cell);
							if ((relaventCandidates & digits) == digits)
							{
								foreach (var digit in digits)
								{
									candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
								}
							}
						}
					}

					foreach (var cell in cells)
					{
						foreach (var digit in grid.GetCandidates(cell))
						{
							if (!conclusionCandidates.Contains(cell * 9 + digit))
							{
								candidateOffsets.Add(new(WellKnownColorIdentifierKind.Normal, cell * 9 + digit));
							}
						}
					}

					// Create hint.
					var step = new AlignedExclusionStep(
						conclusions.ToArray(),
						new[] { View.Empty | candidateOffsets },
						(CellMap)cells,
						lockedCombinations
					);
					if (context.OnlyFindOne)
					{
						return step;
					}

					context.Accumulator.Add(step);
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Test if the given combination of digits for the cells are relavent for this rule.
	/// A combination is relavent if it includes one of the conclusion.
	/// </summary>
	/// <param name="combination">The combination of digits.</param>
	/// <param name="conclusions">All conclusions.</param>
	/// <param name="cells">The cells used.</param>
	/// <returns>Whether this combination is relavent.</returns>
	private bool IsRelavent(Digit[] combination, List<Conclusion> conclusions, Cell[] cells)
	{
		Debug.Assert(combination.Length == cells.Length);
		for (var i = 0; i < combination.Length; i++)
		{
			var cell = cells[i];
			var digit = combination[i];

			var flag = false;
			foreach (var conclusion in conclusions)
			{
				if (conclusion.Cell == cell && conclusion.Digit == digit)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Get all possible relavent combination digits.
	/// </summary>
	/// <param name="lockedCombinations">The all locked combinations.</param>
	/// <param name="conclusions">All conclusions.</param>
	/// <param name="cells">The cells used.</param>
	/// <returns>A mask of digits for relavent ones.</returns>
	private Mask GetRelaventCombinationValues(Dictionary<Digit[], Cell> lockedCombinations, List<Conclusion> conclusions, Cell[] cells)
	{
		var result = (Mask)0;
		foreach (var (combination, _) in lockedCombinations)
		{
			if (IsRelavent(combination, conclusions, cells))
			{
				foreach (var digit in combination)
				{
					result |= (Mask)(1 << digit);
				}
			}
		}

		return result;
	}
}
