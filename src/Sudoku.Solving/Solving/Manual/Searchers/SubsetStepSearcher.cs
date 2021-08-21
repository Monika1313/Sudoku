﻿using Sudoku.Solving.Manual.Steps.Subsets;

namespace Sudoku.Solving.Manual.Searchers;

/// <summary>
/// Provides with a <b>Subset</b> step searcher. The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Locked Pair</item>
/// <item>Locked Triple</item>
/// <item>Naked Pair</item>
/// <item>Naked Triple</item>
/// <item>Naked Quadruple</item>
/// <item>Naked Pair (+)</item>
/// <item>Naked Triple (+)</item>
/// <item>Naked Quadruple (+)</item>
/// <item>Hidden Pair</item>
/// <item>Hidden Triple</item>
/// <item>Hidden Quadruple</item>
/// </list>
/// </summary>
public sealed class SubsetStepSearcher : IStepSearcher
{
	/// <inheritdoc/>
	public SearcherIdentifier Identifier => SearcherIdentifier.Subset;

	/// <inheritdoc/>
	public SearchingOptions Options { get; set; } = new(3, DisplayingLevel.B);


	/// <inheritdoc/>
	public Step? GetAll(ICollection<Step> accumulator, in Grid grid, bool onlyFindOne)
	{
		for (int size = 2; size <= 4; size++)
		{
			// Naked subsets.
			for (int region = 0; region < 27; region++)
			{
				if ((RegionMaps[region] & EmptyMap) is not { Count: >= 2 } currentEmptyMap)
				{
					continue;
				}

				// Iterate on each combination.
				foreach (int[] cells in currentEmptyMap.SubsetOfSize(size))
				{
					short mask = 0;
					foreach (int cell in cells)
					{
						mask |= grid.GetCandidates(cell);
					}
					if (PopCount((uint)mask) != size)
					{
						continue;
					}

					// Naked subset found. Now check eliminations.
					short flagMask = 0;
					var conclusions = new List<Conclusion>();
					foreach (int digit in mask)
					{
						var map = cells % CandMaps[digit];
						flagMask |= (short)(map.InOneRegion ? 0 : (1 << digit));

						foreach (int cell in map)
						{
							conclusions.Add(new(ConclusionType.Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					var candidateOffsets = new List<(int, ColorIdentifier)>();
					foreach (int cell in cells)
					{
						foreach (int digit in grid.GetCandidates(cell))
						{
							candidateOffsets.Add((cell * 9 + digit, (ColorIdentifier)0));
						}
					}

					bool? isLocked = flagMask == mask ? true : flagMask != 0 ? false : null;
					var step = new NakedSubsetStep(
						conclusions.ToImmutableArray(),
						new PresentationData[]
						{
							new()
							{
								Candidates = candidateOffsets,
								Regions = new[] { (region, (ColorIdentifier)0) }
							}
						}.ToImmutableArray(),
						region,
						new(cells),
						mask,
						isLocked
					);

					if (onlyFindOne)
					{
						return step;
					}

					accumulator.Add(step);
				}
			}

			// Hidden subsets.
			for (int region = 0; region < 27; region++)
			{
				var traversingMap = RegionMaps[region] - EmptyMap;
				if (traversingMap.Count >= 8)
				{
					// No available digit (Or hidden single).
					continue;
				}

				short mask = Grid.MaxCandidatesMask;
				foreach (int cell in traversingMap)
				{
					mask &= (short)~(1 << grid[cell]);
				}
				foreach (int[] digits in mask.GetAllSets().GetSubsets(size))
				{
					short tempMask = mask;
					short digitsMask = 0;
					var map = Cells.Empty;
					foreach (int digit in digits)
					{
						tempMask &= (short)~(1 << digit);
						digitsMask |= (short)(1 << digit);
						map |= RegionMaps[region] & CandMaps[digit];
					}
					if (map.Count != size)
					{
						continue;
					}

					// Gather eliminations.
					var conclusions = new List<Conclusion>();
					foreach (int digit in tempMask)
					{
						foreach (int cell in map & CandMaps[digit])
						{
							conclusions.Add(new(ConclusionType.Elimination, cell, digit));
						}
					}
					if (conclusions.Count == 0)
					{
						continue;
					}

					// Gather highlight candidates.
					var candidateOffsets = new List<(int, ColorIdentifier)>();
					foreach (int digit in digits)
					{
						foreach (int cell in map & CandMaps[digit])
						{
							candidateOffsets.Add((cell * 9 + digit, (ColorIdentifier)0));
						}
					}

					var step = new HiddenSubsetStep(
						conclusions.ToImmutableArray(),
						new PresentationData[]
						{
							new()
							{
								Candidates = candidateOffsets,
								Regions = new[] { (region, (ColorIdentifier)0) }
							}
						}.ToImmutableArray(),
						region,
						map,
						digitsMask
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
