﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sudoku.Data.Extensions;
using Sudoku.Data.Meta;
using Sudoku.Drawing;
using Sudoku.Solving.Utils;

namespace Sudoku.Solving.Manual.Fishes.Basic
{
	/// <summary>
	/// Encapsulates a normal fish technique step finder in solving
	/// in <see cref="ManualSolver"/>.
	/// </summary>
	public sealed class NormalFishStepFinder : FishStepFinder
	{
		/// <inheritdoc/>
		public override IReadOnlyList<TechniqueInfo> TakeAll(Grid grid)
		{
			var result = new List<TechniqueInfo>();

			result.AddRange(TakeAllBySizeAndFinChecks(grid, 2, false));
			result.AddRange(TakeAllBySizeAndFinChecks(grid, 2, true));
			result.AddRange(TakeAllBySizeAndFinChecks(grid, 3, false));
			result.AddRange(TakeAllBySizeAndFinChecks(grid, 3, true));
			result.AddRange(TakeAllBySizeAndFinChecks(grid, 4, false));
			result.AddRange(TakeAllBySizeAndFinChecks(grid, 4, true));

			return result;
		}


		/// <summary>
		/// Searches all basic fish of the specified size and
		/// fin checking <see cref="bool"/> value.
		/// </summary>
		/// <param name="grid">The grid.</param>
		/// <param name="size">The size.</param>
		/// <param name="searchRow">
		/// Indicates the solver will searching rows or columns.
		/// </param>
		/// <returns>The result.</returns>
		private static IReadOnlyList<NormalFishTechniqueInfo> TakeAllBySizeAndFinChecks(
			Grid grid, int size, bool searchRow)
		{
			Contract.Requires(size >= 2 && size <= 4);

			var result = new List<NormalFishTechniqueInfo>();

			int baseSetStart = searchRow ? 9 : 18;
			int coverSetStart = searchRow ? 18 : 9;
			for (int digit = 0; digit < 9; digit++)
			{
				for (int bs1 = baseSetStart; bs1 < baseSetStart + 10 - size; bs1++)
				{
					// Get the appearing mask of 'digit' in 'bs1'.
					short baseMask1 = grid.GetDigitAppearingMask(digit, bs1);
					if (baseMask1.CountSet() == 0)
					{
						continue;
					}

					for (int bs2 = bs1 + 1; bs2 < baseSetStart + 11 - size; bs2++)
					{
						// Get the appearing mask of 'digit' in 'bs2'.
						short baseMask2 = grid.GetDigitAppearingMask(digit, bs2);
						if (baseMask2.CountSet() == 0)
						{
							continue;
						}

						if (size == 2)
						{
							short baseMask = (short)(baseMask1 | baseMask2);
							int finAndBodyCount = baseMask.CountSet();
							if (finAndBodyCount >= 5)
							{
								continue;
							}

							// Search (Finned) (Sashimi) X-Wing.
							for (int cs1 = coverSetStart, i = 0; cs1 < coverSetStart + 8; cs1++, i++)
							{
								// Check whether this cover set has 'digit'.
								if ((baseMask >> i & 1) == 0)
								{
									continue;
								}

								for (int cs2 = cs1 + 1, j = 0; cs2 < coverSetStart + 9; cs2++, j++)
								{
									// Check whether this cover set has 'digit'.
									if ((baseMask >> j & 1) == 0)
									{
										continue;
									}

									// Confirm all elimination cells.
									var baseSets = new[] { bs1, bs2 };
									var coverSets = new[] { cs1, cs2 };
									var bodyMap = new NewerGridMap();
									var elimMap = new NewerGridMap();
									GetGridMap(ref bodyMap, baseSets);
									GetGridMap(ref elimMap, coverSets);
									bodyMap &= elimMap;
									elimMap &= ~bodyMap;

									// Check the existence of fin.
									var finCells = (List<int>?)null;
									if (finAndBodyCount == 2) // size == 2
									{
										goto Label_CheckWhetherTheNumberOfIntersectionCellsIsNotZero;
									}

									// Get the fin mask.
									short finMask = (short)(baseMask & ~(1 << i | 1 << j) & 511);

									// Confirm all fin cells.
									finCells = new List<int>();
									foreach (int baseSet in baseSets)
									{
										for (int x = 0, temp = finMask; x < 9; x++, temp >>= 1)
										{
											if ((temp & 1) != 0)
											{
												int possibleFinCellOffset = RegionUtils.GetCellOffset(baseSet, x);
												if (grid.GetCellStatus(possibleFinCellOffset) == CellStatus.Empty
													&& !grid[possibleFinCellOffset, digit])
												{
													finCells.Add(possibleFinCellOffset);
												}
											}
										}
									}

									// Check the number of fins is less than 3.
									// and all fins do not lie on any cover sets.
									if (finCells.Count > 2
										|| !finCells.All(
											c => !coverSets.Contains(searchRow ? c % 9 + 18 : c / 9 + 9)))
									{
										continue;
									}

									// Get intersection.
									foreach (int finCell in finCells)
									{
										elimMap &= new NewerGridMap(finCell);
									}

								Label_CheckWhetherTheNumberOfIntersectionCellsIsNotZero:
									// Check whether the number of intersection cells is not 0.
									if (elimMap.Count != 0)
									{
										// Finned/Sashimi X-Wing found.
										// Check eliminations.
										var elimList = new List<int>();
										foreach (int offset in elimMap.Offsets)
										{
											if (grid.GetCellStatus(offset) == CellStatus.Empty
												&& !grid[offset, digit])
											{
												elimList.Add(offset * 9 + digit);
											}
										}

										if (elimList.Count != 0)
										{
											// Eliminations does exist.
											// Check all highlighted candidates.
											var highlightCandidates = new List<(int, int)>(
												from cellOffset in bodyMap.Offsets
												select (0, cellOffset));

											// Check the fish is sashimi, normal finned or normal.
											bool? isSashimi = null;
											if (!(finCells is null))
											{
												int finCell = finCells[0];
												int block = finCell / 9 / 3 * 3 + finCell % 9 % 3;
												foreach (int offset in bodyMap.Offsets)
												{
													if (offset / 9 / 3 * 3 + offset % 9 % 3 == block)
													{
														isSashimi =
															grid.GetCellStatus(offset) != CellStatus.Empty
															|| grid[offset, digit];

														break;
													}
												}
											}

											// Add to 'result'.
											result.Add(
												new NormalFishTechniqueInfo(
													conclusions: new List<Conclusion>(
														from cand in elimList
														select new Conclusion(ConclusionType.Elimination, cand)),
													views: new List<View>
													{
														new View(
															cellOffsets: null,
															candidateOffsets: highlightCandidates,
															regionOffsets: new List<(int, int)>
															{
																(0, bs1),
																(0, bs2),
																(1, cs1),
																(1, cs2)
															},
															linkMasks: null)
													},
													digit,
													baseSets: new List<int>(baseSets),
													coverSets: new List<int>(coverSets),
													finCellOffsets: finCells,
													isSashimi));
										}
									}
								}
							}
						}
						else // size > 2
						{
							for (int bs3 = bs2 + 1; bs3 < baseSetStart + 12 - size; bs3++)
							{
								// Get the appearing mask of 'digit' in 'bs3'.
								short baseMask3 = grid.GetDigitAppearingMask(digit, bs3);
								if (baseMask3.CountSet() == 0)
								{
									continue;
								}

								if (size == 3)
								{
									short baseMask = (short)((short)(baseMask1 | baseMask2) | baseMask3);
									if (baseMask.CountSet() >= 6)
									{
										continue;
									}

									// TODO: Search (Finned) (Sashimi) Swordfish.
								}
								else // size == 4
								{
									for (int bs4 = bs3 + 1; bs4 < baseSetStart + 9; bs4++)
									{
										// Get the appearing mask of 'digit' in 'bs4'.
										short baseMask4 = grid.GetDigitAppearingMask(digit, bs4);
										if (baseMask4.CountSet() == 0)
										{
											continue;
										}

										short baseMask = (short)((short)((short)(baseMask1 | baseMask2) | baseMask3) | baseMask4);
										if (baseMask.CountSet() >= 7)
										{
											continue;
										}

										// TODO: Search (Finned) (Sashimi) Jellyfish.
									}
								}
							}
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Record all cells in the all regions to a <see cref="NewerGridMap"/> instance.
		/// </summary>
		/// <param name="map">The map.</param>
		/// <param name="regionOffsets">All region offsets.</param>
		private static void GetGridMap(ref NewerGridMap map, int[] regionOffsets)
		{
			foreach (int regionOffset in regionOffsets)
			{
				map |= new NewerGridMap(RegionUtils.GetCellOffsets(regionOffset));
			}
		}
	}
}
