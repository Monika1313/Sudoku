﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Extensions;
using System.Linq;
using Sudoku.Data;
using Sudoku.Runtime;
using Sudoku.Windows;
using static System.Linq.Enumerable;
using static System.Math;
using static System.StringComparison;

namespace Sudoku.Solving.BruteForces.Linqing
{
	/// <summary>
	/// Provides a solver using LINQ method.
	/// </summary>
	public sealed class OneLineLinqSolver : ISolver
	{
		/// <inheritdoc/>
		public string SolverName => Resources.GetValue("OneLineLinq");


		/// <inheritdoc/>
		public AnalysisResult Solve(in SudokuGrid grid)
		{
			var stopwatch = new Stopwatch();

			stopwatch.Start();
			var results = SolveStrings(grid.ToString("0"));
			stopwatch.Stop();

			return results.Count switch
			{
				0 => throw new NoSolutionException(grid),
				1 => new(SolverName, grid, true, stopwatch.Elapsed) { Solution = SudokuGrid.Parse(results[0]) },
				_ => throw new MultipleSolutionsException(grid)
			};
		}


		/// <summary>
		/// Internal solving method.
		/// </summary>
		/// <param name="puzzle">The puzzle string, with placeholder character '0'.</param>
		/// <returns>The result strings (i.e. All solutions).</returns>
		private static IReadOnlyList<string> SolveStrings(string puzzle)
		{
			const string digitChars = "123456789";
			static int index(string solution) => solution.IndexOf('0', OrdinalIgnoreCase);

			var result = new List<string> { puzzle };
			while (result.Count > 0 && index(result[0]) != -1)
			{
				result = (
					from solution in result
					let i = index(solution)
					let c = i % 9
					let b = i - i % 27 + c - i % 3
					from @char in digitChars
					where (
						from i in Range(0, 9)
						let inRow = solution[i - c + i] == @char
						let inColumn = solution[c + i * 9] == @char
						let inBlock = solution[b + i % 3 + (int)Floor(i / 3F) * 9] == @char
						where inRow || inColumn || inBlock
						select i
					).None()
					select $"{solution[..i]}{@char}{solution[(i + 1)..]}").ToList();
			}

			return result;
		}
	}
}
