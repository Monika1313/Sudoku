﻿using System.Collections.Generic;

namespace Sudoku.Models
{
	/// <summary>
	/// Represents a event handler that triggered when the data is changed.
	/// </summary>
	/// <typeparam name="T">The type of the data.</typeparam>
	/// <param name="args">(<see langword="in"/> parameter) The event arguments provided.</param>
	public delegate void PresentationDataChangedEventHandler<T>(ICollection<T> args);
}
