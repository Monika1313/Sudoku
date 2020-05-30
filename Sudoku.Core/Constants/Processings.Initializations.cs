﻿using System;
using System.Collections.Generic;
using Sudoku.Data;

namespace Sudoku.Constants
{
	partial class Processings
	{
		/// <include file='../GlobalDocComments.xml' path='comments/staticConstructor'/>
		/// <remarks>
		/// The initialization order between static constructor and static fields
		/// may be annoying, so I use static constructor both.
		/// </remarks>
		static Processings()
		{
			#region BlockTable
			BlockTable = new[]
			{
				0, 0, 0, 1, 1, 1, 2, 2, 2,
				0, 0, 0, 1, 1, 1, 2, 2, 2,
				0, 0, 0, 1, 1, 1, 2, 2, 2,
				3, 3, 3, 4, 4, 4, 5, 5, 5,
				3, 3, 3, 4, 4, 4, 5, 5, 5,
				3, 3, 3, 4, 4, 4, 5, 5, 5,
				6, 6, 6, 7, 7, 7, 8, 8, 8,
				6, 6, 6, 7, 7, 7, 8, 8, 8,
				6, 6, 6, 7, 7, 7, 8, 8, 8
			};
			#endregion

			#region RowTable
			RowTable = new[]
			{
				9, 9, 9, 9, 9, 9, 9, 9, 9,
				10, 10, 10, 10, 10, 10, 10, 10, 10,
				11, 11, 11, 11, 11, 11, 11, 11, 11,
				12, 12, 12, 12, 12, 12, 12, 12, 12,
				13, 13, 13, 13, 13, 13, 13, 13, 13,
				14, 14, 14, 14, 14, 14, 14, 14, 14,
				15, 15, 15, 15, 15, 15, 15, 15, 15,
				16, 16, 16, 16, 16, 16, 16, 16, 16,
				17, 17, 17, 17, 17, 17, 17, 17, 17
			};
			#endregion

			#region ColumnTable
			ColumnTable = new[]
			{
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26,
				18, 19, 20, 21, 22, 23, 24, 25, 26
			};
			#endregion

			#region Peers
			{
				Peers = new int[81][]
				{
					new[] { 9, 18, 27, 36, 45, 54, 63, 72, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 19, 20 },
					new[] { 10, 19, 28, 37, 46, 55, 64, 73, 0, 2, 3, 4, 5, 6, 7, 8, 9, 11, 18, 20 },
					new[] { 11, 20, 29, 38, 47, 56, 65, 74, 0, 1, 3, 4, 5, 6, 7, 8, 9, 10, 18, 19 },
					new[] { 12, 21, 30, 39, 48, 57, 66, 75, 0, 1, 2, 4, 5, 6, 7, 8, 13, 14, 22, 23 },
					new[] { 13, 22, 31, 40, 49, 58, 67, 76, 0, 1, 2, 3, 5, 6, 7, 8, 12, 14, 21, 23 },
					new[] { 14, 23, 32, 41, 50, 59, 68, 77, 0, 1, 2, 3, 4, 6, 7, 8, 12, 13, 21, 22 },
					new[] { 15, 24, 33, 42, 51, 60, 69, 78, 0, 1, 2, 3, 4, 5, 7, 8, 16, 17, 25, 26 },
					new[] { 16, 25, 34, 43, 52, 61, 70, 79, 0, 1, 2, 3, 4, 5, 6, 8, 15, 17, 24, 26 },
					new[] { 17, 26, 35, 44, 53, 62, 71, 80, 0, 1, 2, 3, 4, 5, 6, 7, 15, 16, 24, 25 },
					new[] { 0, 18, 27, 36, 45, 54, 63, 72, 10, 11, 12, 13, 14, 15, 16, 17, 1, 2, 19, 20 },
					new[] { 1, 19, 28, 37, 46, 55, 64, 73, 9, 11, 12, 13, 14, 15, 16, 17, 0, 2, 18, 20 },
					new[] { 2, 20, 29, 38, 47, 56, 65, 74, 9, 10, 12, 13, 14, 15, 16, 17, 0, 1, 18, 19 },
					new[] { 3, 21, 30, 39, 48, 57, 66, 75, 9, 10, 11, 13, 14, 15, 16, 17, 4, 5, 22, 23 },
					new[] { 4, 22, 31, 40, 49, 58, 67, 76, 9, 10, 11, 12, 14, 15, 16, 17, 3, 5, 21, 23 },
					new[] { 5, 23, 32, 41, 50, 59, 68, 77, 9, 10, 11, 12, 13, 15, 16, 17, 3, 4, 21, 22 },
					new[] { 6, 24, 33, 42, 51, 60, 69, 78, 9, 10, 11, 12, 13, 14, 16, 17, 7, 8, 25, 26 },
					new[] { 7, 25, 34, 43, 52, 61, 70, 79, 9, 10, 11, 12, 13, 14, 15, 17, 6, 8, 24, 26 },
					new[] { 8, 26, 35, 44, 53, 62, 71, 80, 9, 10, 11, 12, 13, 14, 15, 16, 6, 7, 24, 25 },
					new[] { 0, 9, 27, 36, 45, 54, 63, 72, 19, 20, 21, 22, 23, 24, 25, 26, 1, 2, 10, 11 },
					new[] { 1, 10, 28, 37, 46, 55, 64, 73, 18, 20, 21, 22, 23, 24, 25, 26, 0, 2, 9, 11 },
					new[] { 2, 11, 29, 38, 47, 56, 65, 74, 18, 19, 21, 22, 23, 24, 25, 26, 0, 1, 9, 10 },
					new[] { 3, 12, 30, 39, 48, 57, 66, 75, 18, 19, 20, 22, 23, 24, 25, 26, 4, 5, 13, 14 },
					new[] { 4, 13, 31, 40, 49, 58, 67, 76, 18, 19, 20, 21, 23, 24, 25, 26, 3, 5, 12, 14 },
					new[] { 5, 14, 32, 41, 50, 59, 68, 77, 18, 19, 20, 21, 22, 24, 25, 26, 3, 4, 12, 13 },
					new[] { 6, 15, 33, 42, 51, 60, 69, 78, 18, 19, 20, 21, 22, 23, 25, 26, 7, 8, 16, 17 },
					new[] { 7, 16, 34, 43, 52, 61, 70, 79, 18, 19, 20, 21, 22, 23, 24, 26, 6, 8, 15, 17 },
					new[] { 8, 17, 35, 44, 53, 62, 71, 80, 18, 19, 20, 21, 22, 23, 24, 25, 6, 7, 15, 16 },
					new[] { 0, 9, 18, 36, 45, 54, 63, 72, 28, 29, 30, 31, 32, 33, 34, 35, 37, 38, 46, 47 },
					new[] { 1, 10, 19, 37, 46, 55, 64, 73, 27, 29, 30, 31, 32, 33, 34, 35, 36, 38, 45, 47 },
					new[] { 2, 11, 20, 38, 47, 56, 65, 74, 27, 28, 30, 31, 32, 33, 34, 35, 36, 37, 45, 46 },
					new[] { 3, 12, 21, 39, 48, 57, 66, 75, 27, 28, 29, 31, 32, 33, 34, 35, 40, 41, 49, 50 },
					new[] { 4, 13, 22, 40, 49, 58, 67, 76, 27, 28, 29, 30, 32, 33, 34, 35, 39, 41, 48, 50 },
					new[] { 5, 14, 23, 41, 50, 59, 68, 77, 27, 28, 29, 30, 31, 33, 34, 35, 39, 40, 48, 49 },
					new[] { 6, 15, 24, 42, 51, 60, 69, 78, 27, 28, 29, 30, 31, 32, 34, 35, 43, 44, 52, 53 },
					new[] { 7, 16, 25, 43, 52, 61, 70, 79, 27, 28, 29, 30, 31, 32, 33, 35, 42, 44, 51, 53 },
					new[] { 8, 17, 26, 44, 53, 62, 71, 80, 27, 28, 29, 30, 31, 32, 33, 34, 42, 43, 51, 52 },
					new[] { 0, 9, 18, 27, 45, 54, 63, 72, 37, 38, 39, 40, 41, 42, 43, 44, 28, 29, 46, 47 },
					new[] { 1, 10, 19, 28, 46, 55, 64, 73, 36, 38, 39, 40, 41, 42, 43, 44, 27, 29, 45, 47 },
					new[] { 2, 11, 20, 29, 47, 56, 65, 74, 36, 37, 39, 40, 41, 42, 43, 44, 27, 28, 45, 46 },
					new[] { 3, 12, 21, 30, 48, 57, 66, 75, 36, 37, 38, 40, 41, 42, 43, 44, 31, 32, 49, 50 },
					new[] { 4, 13, 22, 31, 49, 58, 67, 76, 36, 37, 38, 39, 41, 42, 43, 44, 30, 32, 48, 50 },
					new[] { 5, 14, 23, 32, 50, 59, 68, 77, 36, 37, 38, 39, 40, 42, 43, 44, 30, 31, 48, 49 },
					new[] { 6, 15, 24, 33, 51, 60, 69, 78, 36, 37, 38, 39, 40, 41, 43, 44, 34, 35, 52, 53 },
					new[] { 7, 16, 25, 34, 52, 61, 70, 79, 36, 37, 38, 39, 40, 41, 42, 44, 33, 35, 51, 53 },
					new[] { 8, 17, 26, 35, 53, 62, 71, 80, 36, 37, 38, 39, 40, 41, 42, 43, 33, 34, 51, 52 },
					new[] { 0, 9, 18, 27, 36, 54, 63, 72, 46, 47, 48, 49, 50, 51, 52, 53, 28, 29, 37, 38 },
					new[] { 1, 10, 19, 28, 37, 55, 64, 73, 45, 47, 48, 49, 50, 51, 52, 53, 27, 29, 36, 38 },
					new[] { 2, 11, 20, 29, 38, 56, 65, 74, 45, 46, 48, 49, 50, 51, 52, 53, 27, 28, 36, 37 },
					new[] { 3, 12, 21, 30, 39, 57, 66, 75, 45, 46, 47, 49, 50, 51, 52, 53, 31, 32, 40, 41 },
					new[] { 4, 13, 22, 31, 40, 58, 67, 76, 45, 46, 47, 48, 50, 51, 52, 53, 30, 32, 39, 41 },
					new[] { 5, 14, 23, 32, 41, 59, 68, 77, 45, 46, 47, 48, 49, 51, 52, 53, 30, 31, 39, 40 },
					new[] { 6, 15, 24, 33, 42, 60, 69, 78, 45, 46, 47, 48, 49, 50, 52, 53, 34, 35, 43, 44 },
					new[] { 7, 16, 25, 34, 43, 61, 70, 79, 45, 46, 47, 48, 49, 50, 51, 53, 33, 35, 42, 44 },
					new[] { 8, 17, 26, 35, 44, 62, 71, 80, 45, 46, 47, 48, 49, 50, 51, 52, 33, 34, 42, 43 },
					new[] { 0, 9, 18, 27, 36, 45, 63, 72, 55, 56, 57, 58, 59, 60, 61, 62, 64, 65, 73, 74 },
					new[] { 1, 10, 19, 28, 37, 46, 64, 73, 54, 56, 57, 58, 59, 60, 61, 62, 63, 65, 72, 74 },
					new[] { 2, 11, 20, 29, 38, 47, 65, 74, 54, 55, 57, 58, 59, 60, 61, 62, 63, 64, 72, 73 },
					new[] { 3, 12, 21, 30, 39, 48, 66, 75, 54, 55, 56, 58, 59, 60, 61, 62, 67, 68, 76, 77 },
					new[] { 4, 13, 22, 31, 40, 49, 67, 76, 54, 55, 56, 57, 59, 60, 61, 62, 66, 68, 75, 77 },
					new[] { 5, 14, 23, 32, 41, 50, 68, 77, 54, 55, 56, 57, 58, 60, 61, 62, 66, 67, 75, 76 },
					new[] { 6, 15, 24, 33, 42, 51, 69, 78, 54, 55, 56, 57, 58, 59, 61, 62, 70, 71, 79, 80 },
					new[] { 7, 16, 25, 34, 43, 52, 70, 79, 54, 55, 56, 57, 58, 59, 60, 62, 69, 71, 78, 80 },
					new[] { 8, 17, 26, 35, 44, 53, 71, 80, 54, 55, 56, 57, 58, 59, 60, 61, 69, 70, 78, 79 },
					new[] { 0, 9, 18, 27, 36, 45, 54, 72, 64, 65, 66, 67, 68, 69, 70, 71, 55, 56, 73, 74 },
					new[] { 1, 10, 19, 28, 37, 46, 55, 73, 63, 65, 66, 67, 68, 69, 70, 71, 54, 56, 72, 74 },
					new[] { 2, 11, 20, 29, 38, 47, 56, 74, 63, 64, 66, 67, 68, 69, 70, 71, 54, 55, 72, 73 },
					new[] { 3, 12, 21, 30, 39, 48, 57, 75, 63, 64, 65, 67, 68, 69, 70, 71, 58, 59, 76, 77 },
					new[] { 4, 13, 22, 31, 40, 49, 58, 76, 63, 64, 65, 66, 68, 69, 70, 71, 57, 59, 75, 77 },
					new[] { 5, 14, 23, 32, 41, 50, 59, 77, 63, 64, 65, 66, 67, 69, 70, 71, 57, 58, 75, 76 },
					new[] { 6, 15, 24, 33, 42, 51, 60, 78, 63, 64, 65, 66, 67, 68, 70, 71, 61, 62, 79, 80 },
					new[] { 7, 16, 25, 34, 43, 52, 61, 79, 63, 64, 65, 66, 67, 68, 69, 71, 60, 62, 78, 80 },
					new[] { 8, 17, 26, 35, 44, 53, 62, 80, 63, 64, 65, 66, 67, 68, 69, 70, 60, 61, 78, 79 },
					new[] { 0, 9, 18, 27, 36, 45, 54, 63, 73, 74, 75, 76, 77, 78, 79, 80, 55, 56, 64, 65 },
					new[] { 1, 10, 19, 28, 37, 46, 55, 64, 72, 74, 75, 76, 77, 78, 79, 80, 54, 56, 63, 65 },
					new[] { 2, 11, 20, 29, 38, 47, 56, 65, 72, 73, 75, 76, 77, 78, 79, 80, 54, 55, 63, 64 },
					new[] { 3, 12, 21, 30, 39, 48, 57, 66, 72, 73, 74, 76, 77, 78, 79, 80, 58, 59, 67, 68 },
					new[] { 4, 13, 22, 31, 40, 49, 58, 67, 72, 73, 74, 75, 77, 78, 79, 80, 57, 59, 66, 68 },
					new[] { 5, 14, 23, 32, 41, 50, 59, 68, 72, 73, 74, 75, 76, 78, 79, 80, 57, 58, 66, 67 },
					new[] { 6, 15, 24, 33, 42, 51, 60, 69, 72, 73, 74, 75, 76, 77, 79, 80, 61, 62, 70, 71 },
					new[] { 7, 16, 25, 34, 43, 52, 61, 70, 72, 73, 74, 75, 76, 77, 78, 80, 60, 62, 69, 71 },
					new[] { 8, 17, 26, 35, 44, 53, 62, 71, 72, 73, 74, 75, 76, 77, 78, 79, 60, 61, 69, 70 }
				};
			}
			#endregion

			#region RegionCells
			{
				RegionCells = new int[][]
				{
					new[] { 0, 1, 2, 9, 10, 11, 18, 19, 20 },
					new[] { 3, 4, 5, 12, 13, 14, 21, 22, 23 },
					new[] { 6, 7, 8, 15, 16, 17, 24, 25, 26 },
					new[] { 27, 28, 29, 36, 37, 38, 45, 46, 47 },
					new[] { 30, 31, 32, 39, 40, 41, 48, 49, 50 },
					new[] { 33, 34, 35, 42, 43, 44, 51, 52, 53 },
					new[] { 54, 55, 56, 63, 64, 65, 72, 73, 74 },
					new[] { 57, 58, 59, 66, 67, 68, 75, 76, 77 },
					new[] { 60, 61, 62, 69, 70, 71, 78, 79, 80 },
					new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
					new[] { 9, 10, 11, 12, 13, 14, 15, 16, 17 },
					new[] { 18, 19, 20, 21, 22, 23, 24, 25, 26 },
					new[] { 27, 28, 29, 30, 31, 32, 33, 34, 35 },
					new[] { 36, 37, 38, 39, 40, 41, 42, 43, 44 },
					new[] { 45, 46, 47, 48, 49, 50, 51, 52, 53 },
					new[] { 54, 55, 56, 57, 58, 59, 60, 61, 62 },
					new[] { 63, 64, 65, 66, 67, 68, 69, 70, 71 },
					new[] { 72, 73, 74, 75, 76, 77, 78, 79, 80 },
					new[] { 0, 9, 18, 27, 36, 45, 54, 63, 72 },
					new[] { 1, 10, 19, 28, 37, 46, 55, 64, 73 },
					new[] { 2, 11, 20, 29, 38, 47, 56, 65, 74 },
					new[] { 3, 12, 21, 30, 39, 48, 57, 66, 75 },
					new[] { 4, 13, 22, 31, 40, 49, 58, 67, 76 },
					new[] { 5, 14, 23, 32, 41, 50, 59, 68, 77 },
					new[] { 6, 15, 24, 33, 42, 51, 60, 69, 78 },
					new[] { 7, 16, 25, 34, 43, 52, 61, 70, 79 },
					new[] { 8, 17, 26, 35, 44, 53, 62, 71, 80 }
				};
			}
			#endregion

			#region RegionMaps
			{
				RegionMaps = new GridMap[27];
				for (int i = 0; i < 27; i++)
				{
					RegionMaps[i].AddRange((IEnumerable<int>)RegionCells[i]);
				}
			}
			#endregion

			#region IntersectionMaps
			{
				var r = (ReadOnlySpan<byte>)stackalloc byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
				var c = (ReadOnlySpan<byte>)stackalloc byte[] { 0, 3, 6, 1, 4, 7, 2, 5, 8 };
				var dic = new Dictionary<(byte, byte), (GridMap, GridMap, GridMap)>(new ValueTupleComparer());
				for (byte bs = 9; bs < 27; bs++)
				{
					for (byte j = 0; j < 3; j++)
					{
						byte cs = bs < 18 ? r[(bs - 9) / 3 * 3 + j] : c[(bs - 18) / 3 * 3 + j];
						var bm = RegionMaps[bs];
						var cm = RegionMaps[cs];
						var i = bm & cm;
						dic.Add((bs, cs), (bm - i, cm - i, i));
					}
				}

				IntersectionMaps = dic;
			}
			#endregion
		}
	}
}
