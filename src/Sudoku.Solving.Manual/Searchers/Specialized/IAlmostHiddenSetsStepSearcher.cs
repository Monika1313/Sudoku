﻿namespace Sudoku.Solving.Manual.Searchers.Specialized;

/// <summary>
/// Provides with an <b>Almost Hidden Sets</b> step searcher.
/// </summary>
public interface IAlmostHiddenSetsStepSearcher : IStepSearcher
{
	/// <inheritdoc cref="AlmostHiddenSet.Gather(in Grid)"/>
	/// <remarks>
	/// Different with the original method <see cref="AlmostHiddenSet.Gather(in Grid)"/>,
	/// this method will only uses the buffer to determine the info, which is unsafe
	/// when calling the method without having initialized those maps in the buffer type,
	/// <see cref="FastProperties"/>.
	/// </remarks>
	/// <seealso cref="AlmostHiddenSet"/>
	/// <seealso cref="AlmostHiddenSet.Gather(in Grid)"/>
	/// <seealso cref="FastProperties"/>
	protected internal static sealed AlmostHiddenSet[] Gather(scoped in Grid grid)
	{
		var result = new List<AlmostHiddenSet>();

		for (int house = 0; house < 27; house++)
		{
			if ((HouseMaps[house] & EmptyCells) is not { Count: >= 3 } tempMap)
			{
				continue;
			}

			short digitsMask = grid.GetDigitsUnion(tempMap);
			for (int size = 2; size < tempMap.Count - 1; size++)
			{
				foreach (int[] digitCombination in digitsMask.GetAllSets().GetSubsets(size))
				{
					var cells = Cells.Empty;
					foreach (int digit in digitCombination)
					{
						cells |= CandidatesMap[digit] & HouseMaps[house];
					}
					if (cells.Count - 1 != size)
					{
						continue;
					}

					short finalDigitsMask = 0;
					foreach (int digit in digitCombination)
					{
						finalDigitsMask |= (short)(1 << digit);
					}

					short allDigitsMask = grid.GetDigitsUnion(cells);
					var finalMaps = new Cells?[9];
					for (int digit = 0; digit < 9; digit++)
					{
						if ((finalDigitsMask >> digit & 1) != 0 || (allDigitsMask >> digit & 1) != 0)
						{
							finalMaps[digit] = CandidatesMap[digit] & cells;
						}
					}

					result.Add(new(finalDigitsMask, allDigitsMask, cells, finalMaps));
				}
			}
		}

		return result.ToArray();
	}
}