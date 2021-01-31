﻿using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Shapes;
using Sudoku.Data;
using Sudoku.Models;

namespace Sudoku.Painting
{
	/// <summary>
	/// Indicates the grid painter.
	/// </summary>
	/// <param name="Translator">Indicates the translator.</param>
	/// <param name="Grid">Indicates the grid.</param>
	/// <remarks>
	/// Please note that eliminations will be colored with red, but all assignments won't be colored,
	/// because they will be colored using another method (draw candidates).
	/// </remarks>
	public sealed record GridPainter(in PointTranslator Translator, in SudokuGrid Grid)
	{
		/// <summary>
		/// The square root of 2.
		/// </summary>
		private const float SqrtOf2 = 1.41421356F;

		/// <summary>
		/// The rotate angle (45 degrees, i.e. <c><see cref="Math.PI"/> / 4</c>).
		/// This field is used for rotate the chains if some of them are overlapped.
		/// </summary>
		/// <seealso cref="Math.PI"/>
		private const float RotateAngle = .78539816F;


		/// <summary>
		/// Indicates the drawing width.
		/// </summary>
		public double Width => Translator.ControlSize.Width;

		/// <summary>
		/// Indicates the drawing height.
		/// </summary>
		public double Height => Translator.ControlSize.Height;

		/// <summary>
		/// Indicates the focused cells.
		/// </summary>
		public Cells FocusedCells { get; set; }

		/// <summary>
		/// Indicates the view.
		/// </summary>
		public PresentationData? View { get; set; }

		/// <summary>
		/// Indicates the custom view.
		/// </summary>
		public PresentationData? CustomView { get; set; }

		/// <summary>
		/// Indicates all conclusions.
		/// </summary>
		public IEnumerable<Conclusion>? Conclusions { get; set; }


		/// <summary>
		/// To create a <see cref="Shape"/> collection that draws all elements here.
		/// </summary>
		/// <returns>The <see cref="Shape"/> collection.</returns>
		public IReadOnlyCollection<Shape> Create()
		{
			// TODO: Implement this.
			throw new NotImplementedException();
		}
	}
}
