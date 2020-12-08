﻿using System;
using System.Diagnostics;
using System.Extensions;
using System.Runtime.CompilerServices;
using System.Text;
using Sudoku.DocComments;
using static Sudoku.Constants.Processings;
using Formatter = Sudoku.Data.SudokuGrid.GridFormatter;
using Parser = Sudoku.Data.SudokuGrid.GridParser;
using ParsingOption = Sudoku.Data.GridParsingOption;

namespace Sudoku.Data
{
	/// <summary>
	/// Encapsulates a sudoku grid using value type instead of reference type.
	/// </summary>
#if DEBUG
	[DebuggerDisplay("{" + nameof(ToString) + "(\".+:\"),nq}")]
#endif
	public unsafe partial struct SudokuGrid : IValueEquatable<SudokuGrid>, IFormattable
	{
		/// <summary>
		/// Indicates the default mask of a cell (an empty cell, with all 9 candidates left).
		/// </summary>
		public const short DefaultMask = EmptyMask | MaxCandidatesMask;

		/// <summary>
		/// Indicates the maximum candidate mask that used.
		/// </summary>
		public const short MaxCandidatesMask = 0b111_111_111;

		/// <summary>
		/// Indicates the empty mask.
		/// </summary>
		public const short EmptyMask = (int)CellStatus.Empty << 9;

		/// <summary>
		/// Indicates the modifiable mask.
		/// </summary>
		public const short ModifiableMask = (int)CellStatus.Modifiable << 9;

		/// <summary>
		/// Indicates the given mask.
		/// </summary>
		public const short GivenMask = (int)CellStatus.Given << 9;

		/// <summary>
		/// Indicates the size of each grid.
		/// </summary>
		private const byte Length = 81;


		/// <summary>
		/// Indicates the empty grid string.
		/// </summary>
		public static readonly string EmptyString = new('0', Length);

		/// <summary>
		/// Indicates the default grid that all values are initialized 0, which is same as
		/// <see cref="SudokuGrid()"/>.
		/// </summary>
		/// <remarks>
		/// We recommend you should use this static field instead of the default constructor
		/// to reduce object creation.
		/// </remarks>
		/// <seealso cref="SudokuGrid()"/>
		public static readonly SudokuGrid Undefined;

		/// <summary>
		/// The empty grid that is valid during implementation or running the program
		/// (all values are <see cref="DefaultMask"/>, i.e. empty cells).
		/// </summary>
		/// <remarks>
		/// This field is initialized by the static constructor of this structure.
		/// </remarks>
		/// <seealso cref="DefaultMask"/>
		public static readonly SudokuGrid Empty;


		/// <summary>
		/// Indicates the inner array.
		/// </summary>
		private fixed short _values[Length];

		/// <summary>
		/// Indicates the inner array suggests the initial grid.
		/// </summary>
		private fixed short _initialValues[Length];


		/// <summary>
		/// Initializes an instance with the specified mask list and the length.
		/// </summary>
		/// <param name="masks">The masks.</param>
		/// <param name="length">The length of the <paramref name="masks"/>. The value should be 81.</param>
		/// <exception cref="ArgumentNullException">
		/// Throws when <paramref name="masks"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">Throws when <paramref name="length"/> is not 81.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SudokuGrid(short* masks, int length)
		{
			_ = masks == null ? throw new ArgumentNullException(nameof(masks)) : masks;
			_ = length != Length ? throw new ArgumentException($"The specified argument should be {Length}.", nameof(length)) : length;

			fixed (short* pValues = _values, pInitialValues = _initialValues)
			{
				InternalCopy(pValues, masks);
				InternalCopy(pInitialValues, masks);
			}
		}

		/// <summary>
		/// Initializes an instance with the specified mask array.
		/// </summary>
		/// <param name="masks">The masks.</param>
		/// <exception cref="ArgumentException">Throws when <see cref="Array.Length"/> is not 81.</exception>
		public SudokuGrid(short[] masks)
		{
			_ = masks.Length != Length ? throw new ArgumentException($"The length of the array argument should be {Length}.", nameof(masks)) : masks;

			fixed (short* pArray = masks, pValues = _values, pInitialValues = _initialValues)
			{
				InternalCopy(pValues, pArray);
				InternalCopy(pInitialValues, pArray);
			}
		}


		/// <inheritdoc cref="StaticConstructor"/>
		static SudokuGrid()
		{
			// Initializes the empty grid.
			Empty = new();
			fixed (short* p = Empty._values)
			{
				int i = 0;
				for (short* ptr = p; i < Length; i++, *ptr++ = DefaultMask) ;
			}
			fixed (short* p = Empty._initialValues)
			{
				int i = 0;
				for (short* ptr = p; i < Length; i++, *ptr++ = DefaultMask) ;
			}

			// Initializes events.
			ValueChanged = &OnValueChanged;
			RefreshingCandidates = &OnRefreshingCandidates;
		}


		/// <summary>
		/// Indicates the grid has already solved. If the value is <see langword="true"/>,
		/// the grid is solved; otherwise, <see langword="false"/>.
		/// </summary>
		public readonly bool HasSolved
		{
			get
			{
				for (int i = 0; i < Length; i++)
				{
					if (GetStatus(i) == CellStatus.Empty)
					{
						return false;
					}
				}

				return SimplyValidate();
			}
		}

		/// <summary>
		/// Indicates the number of total candidates.
		/// </summary>
		public readonly int CandidatesCount
		{
			get
			{
				int count = 0;
				for (int i = 0; i < Length; i++)
				{
					if (GetStatus(i) == CellStatus.Empty)
					{
						count += GetCandidateMask(i).PopCount();
					}
				}

				return count;
			}
		}

		/// <summary>
		/// Indicates the pinnable reference of the initial mask list.
		/// </summary>
		public readonly short* InitialMaskPinnableReference
		{
			get
			{
				fixed (short* p = _initialValues)
				{
					return p;
				}
			}
		}

		/// <summary>
		/// Indicates the total number of given cells.
		/// </summary>
		public readonly int GivensCount => Triplet.C;

		/// <summary>
		/// Indicates the total number of modifiable cells.
		/// </summary>
		public readonly int ModifiablesCount => Triplet.B;

		/// <summary>
		/// Indicates the total number of empty cells.
		/// </summary>
		public readonly int EmptiesCount => Triplet.A;

		/// <summary>
		/// The triplet of three main information.
		/// </summary>
		private readonly (int A, int B, int C) Triplet
		{
			get
			{
				int a = 0, b = 0, c = 0;
				for (int i = 0; i < Length; i++)
				{
					var s = GetStatus(i);
					(s == CellStatus.Empty ? ref a : ref s == CellStatus.Modifiable ? ref b : ref c)++;
				}

				return (a, b, c);
			}
		}


		/// <summary>
		/// Gets or sets the value in the specified cell.
		/// </summary>
		/// <param name="cell">The cell you want to get or set a value.</param>
		/// <value>
		/// The value you want to set. The value should be between 0 and 8. If assigning -1,
		/// that means to re-compute all candidates.
		/// </value>
		/// <returns>The value that the cell filled with.</returns>
		[IndexerName("Value")]
		public int this[int cell]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get => GetStatus(cell) switch
			{
				CellStatus.Undefined => -2,
				CellStatus.Empty => -1,
				CellStatus.Modifiable or CellStatus.Given => _values[cell].FindFirstSet()
			};

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				switch (value)
				{
					case -1 when GetStatus(cell) == CellStatus.Modifiable:
					{
						// If 'value' is -1, we should reset the grid.
						// Note that reset candidates may not trigger the event.
						_values[cell] = DefaultMask;
						RefreshingCandidates(ref this);

						break;
					}
					case >= 0 and < 9:
					{
						ref short result = ref _values[cell];
						short copy = result;

						// Set cell status to 'CellStatus.Modifiable'.
						result = (short)(ModifiableMask | 1 << value);

						// To trigger the event, which is used for eliminate
						// all same candidates in peer cells.
						ValueChanged(ref this, new(cell, copy, result, value));

						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets a candidate existence case with a <see cref="bool"/> value.
		/// </summary>
		/// <param name="cell">The cell offset between 0 and 80.</param>
		/// <param name="digit">The digit between 0 and 8.</param>
		/// <value>
		/// The case you want to set. <see langword="false"/> means that this candidate
		/// doesn't exist in this current sudoku grid; otherwise, <see langword="true"/>.
		/// </value>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		[IndexerName("Value")]
		public bool this[int cell, int digit]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get => (_values[cell] >> digit & 1) != 0;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				short copied = _values[cell];
				if (value)
				{
					_values[cell] |= (short)(1 << digit);
				}
				else
				{
					_values[cell] &= (short)~(1 << digit);
				}

				// To trigger the event.
				ValueChanged(ref this, new(cell, copied, _values[cell], -1));
			}
		}


		/// <summary>
		/// Check whether the current grid is valid (no duplicate values on same row, column or block).
		/// </summary>
		/// <returns>The <see cref="bool"/> result.</returns>
		public readonly bool SimplyValidate()
		{
			for (int i = 0; i < Length; i++)
			{
				switch (GetStatus(i))
				{
					case CellStatus.Given or CellStatus.Modifiable:
					{
						int curDigit = this[i];
						foreach (int cell in PeerMaps[i])
						{
							if (curDigit == this[cell])
							{
								return false;
							}
						}

						break;
					}
					case CellStatus.Empty:
					{
						continue;
					}
					default:
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <inheritdoc cref="object.Equals(object?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly bool Equals(object? obj) => obj is SudokuGrid other && Equals(other);

		/// <inheritdoc/>
		public readonly bool Equals(in SudokuGrid other)
		{
			fixed (short* pThis = this, pOther = other)
			{
				int i = 0;
				for (short* l = pThis, r = pOther; i < Length; i++, l++, r++)
				{
					if (*l != *r)
					{
						return false;
					}
				}

				return true;
			}
		}

		/// <inheritdoc cref="object.GetHashCode"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly int GetHashCode() =>
			this == Undefined ? 0 : this == Empty ? 1 : ToString("#").GetHashCode();

		/// <summary>
		/// Serializes this instance to an array, where all digit value will be stored.
		/// </summary>
		/// <returns>
		/// This array. All elements are between 0 to 9, where 0 means the
		/// cell is <see cref="CellStatus.Empty"/> now.
		/// </returns>
		[SkipLocalsInit]
		public readonly int[] ToArray()
		{
			var span = (stackalloc int[Length]);
			for (int i = 0; i < Length; i++)
			{
				// 'this[i]' is always in range -1 to 8 (-1 is empty, and 0 to 8 is 1 to 9 for
				// mankind representation).
				span[i] = this[i] + 1;
			}

			return span.ToArray();
		}

		/// <summary>
		/// Get a mask at the specified cell.
		/// </summary>
		/// <param name="offset">The cell offset you want to get.</param>
		/// <returns>The mask.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly short GetMask(int offset) => _values[offset];

		/// <summary>
		/// Get the candidate mask part of the specified cell.
		/// </summary>
		/// <param name="cell">The cell offset you want to get.</param>
		/// <returns>
		/// <para>
		/// The candidate mask. The return value is a 9-bit <see cref="short"/>
		/// value, where each bit will be:
		/// <list type="table">
		/// <item>
		/// <term><c>0</c></term>
		/// <description>The cell <b>doesn't contain</b> the possibility of the digit.</description>
		/// </item>
		/// <item>
		/// <term><c>1</c></term>
		/// <description>The cell <b>contains</b> the possibility of the digit.</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>
		/// For example, if the result mask is 266(i.e. <c>0b100_001_010</c> in binary),
		/// the value will indicate the cell contains the digit 2, 4 and 9.
		/// </para>
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly short GetCandidateMask(int cell) => (short)(_values[cell] & MaxCandidatesMask);

		/// <summary>
		/// Returns a reference to the element of the <see cref="SudokuGrid"/> at index zero.
		/// </summary>
		/// <returns>A reference to the element of the <see cref="SudokuGrid"/> at index zero.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ref short GetPinnableReference()
		{
			fixed (SudokuGrid* @this = &this)
			{
				return ref @this->_values[0];
			}

			// Don't use this way to get the first value.
			// Following code may cause an error (CS8354: Can't return 'this' by reference).
			//return ref _values[0];
		}

		/// <summary>
		/// Get all masks and print them.
		/// </summary>
		/// <returns>The result.</returns>
		/// <remarks>
		/// Please note that the method cannot be called with a correct behavior using
		/// <see cref="DebuggerDisplayAttribute"/> to output. It seems that Visual Studio
		/// doesn't print correct values when indices of this grid aren't 0. In other words,
		/// when we call this method using <see cref="DebuggerDisplayAttribute"/>, only <c>grid[0]</c>
		/// can be output correctly, and other values will be incorrect: they're always 0.
		/// </remarks>
		public readonly string ToMaskString()
		{
			const string separator = ", ";
			var sb = new StringBuilder();
			foreach (short mask in this)
			{
				sb.Append(mask).Append(separator);
			}

			return sb.RemoveFromEnd(separator.Length).ToString();
		}

		/// <inheritdoc cref="object.ToString"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override readonly string ToString() => ToString(null, null);

		/// <inheritdoc cref="Formattable.ToString(string?)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly string ToString(string? format) => ToString(format, null);

		/// <inheritdoc/>
		public readonly string ToString(string? format, IFormatProvider? formatProvider)
		{
			if (this == Empty)
			{
				return "<Empty>";
			}

			if (this == Undefined)
			{
				return "<Undefined>";
			}

#if DEBUG
			if (debuggerUndefined(this))
			{
				return "<Debugger can't recognize fixed buffer.>";
			}
#endif

			if (formatProvider.HasFormatted(this, format, out string? result))
			{
				return result;
			}

			var f = Formatter.Create(format);
			return format switch
			{
				":" => f.ToString(this).Match(RegularExpressions.ExtendedSusserEliminations) ?? string.Empty,
				"!" => f.ToString(this).Replace("+", string.Empty),
				".!" or "!." or "0!" or "!0" => f.ToString(this).Replace("+", string.Empty),
				".!:" or "!.:" or "0!:" => f.ToString(this).Replace("+", string.Empty),
				_ => f.ToString(this)
			};

#if DEBUG
			static bool debuggerUndefined(in SudokuGrid grid)
			{
				for (int cell = 1; cell < Length; cell++)
				{
					if (grid._values[cell] != 0)
					{
						return false;
					}
				}

				return true;
			}
#endif
		}

		/// <summary>
		/// Get the cell status at the specified cell.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <returns>The cell status.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly CellStatus GetStatus(int cell) => (CellStatus)(_values[cell] >> 9 & (int)CellStatus.All);

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Enumerator GetEnumerator()
		{
			fixed (short* arr = _values)
			{
				return new Enumerator(arr);
			}
		}

		/// <summary>
		/// To fix the current grid (all modifiable values will be changed to given ones).
		/// </summary>
		public void Fix()
		{
			for (int i = 0; i < Length; i++)
			{
				if (GetStatus(i) == CellStatus.Modifiable)
				{
					SetStatus(i, CellStatus.Given);
				}
			}

			UpdateInitialMasks();
		}

		/// <summary>
		/// To unfix the current grid (all given values will be chanegd to modifiable ones).
		/// </summary>
		public void Unfix()
		{
			for (int i = 0; i < Length; i++)
			{
				if (GetStatus(i) == CellStatus.Given)
				{
					SetStatus(i, CellStatus.Modifiable);
				}
			}
		}

		/// <summary>
		/// To reset the grid to initial status.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			fixed (short* pValues = _values, pInitialValues = _initialValues)
			{
				InternalCopy(pValues, pInitialValues);
			}
		}

		/// <summary>
		/// Set the specified cell to the specified status.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <param name="status">The status.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetStatus(int cell, CellStatus status)
		{
			ref short mask = ref _values[cell];
			short copy = mask;
			mask = (short)((int)status << 9 | mask & MaxCandidatesMask);

			ValueChanged(ref this, new(cell, copy, mask, -1));
		}

		/// <summary>
		/// Set the specified cell to the specified mask.
		/// </summary>
		/// <param name="cell">The cell.</param>
		/// <param name="mask">The mask to set.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetMask(int cell, short mask)
		{
			ref short m = ref _values[cell];
			short copy = m;
			m = mask;

			ValueChanged(ref this, new(cell, copy, m, -1));
		}

		/// <summary>
		/// To update initial masks.
		/// </summary>
		internal void UpdateInitialMasks()
		{
			fixed (short* pValues = _values, pInitialValues = _initialValues)
			{
				InternalCopy(pInitialValues, pValues);
			}
		}


		/// <summary>
		/// <para>Parses a string value and converts to this type.</para>
		/// <para>
		/// If you want to parse a PM grid, we recommend you use the method
		/// <see cref="Parse(string, ParsingOption)"/> instead of this method.
		/// </para>
		/// </summary>
		/// <param name="str">(<see langword="in"/> parameter) The string.</param>
		/// <returns>The result instance had converted.</returns>
		/// <seealso cref="Parse(string, ParsingOption)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SudokuGrid Parse(in ReadOnlySpan<char> str) => new Parser(str.ToString()).Parse();

		/// <summary>
		/// <para>
		/// Parses a string value and converts to this type.
		/// </para>
		/// <para>
		/// If you want to parse a PM grid, we recommend you use the method
		/// <see cref="Parse(string, ParsingOption)"/> instead of this method.
		/// </para>
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns>The result instance had converted.</returns>
		/// <seealso cref="Parse(string, ParsingOption)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SudokuGrid Parse(string str) => new Parser(str).Parse();

		/// <summary>
		/// <para>
		/// Parses a string value and converts to this type.
		/// </para>
		/// <para>
		/// If you want to parse a PM grid, you should decide the mode to parse.
		/// If you use compatible mode to parse, all single values will be treated as
		/// given values; otherwise, recommended mode, which uses '<c>&lt;d&gt;</c>'
		/// or '<c>*d*</c>' to represent a value be a given or modifiable one. The decision
		/// will be indicated and passed by the second parameter <paramref name="compatibleFirst"/>.
		/// </para>
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="compatibleFirst">
		/// Indicates whether the parsing operation should use compatible mode to check
		/// PM grid. See <see cref="Parser.CompatibleFirst"/> to learn more.
		/// </param>
		/// <returns>The result instance had converted.</returns>
		/// <seealso cref="Parser.CompatibleFirst"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SudokuGrid Parse(string str, bool compatibleFirst) =>
			new Parser(str, compatibleFirst).Parse();

		/// <summary>
		/// Parses a string value and converts to this type,
		/// using a specified grid parsing type.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="gridParsingOption">The grid parsing type.</param>
		/// <returns>The result instance had converted.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SudokuGrid Parse(string str, ParsingOption gridParsingOption) =>
			new Parser(str).Parse(gridParsingOption);

		/// <summary>
		/// Try to parse a string and converts to this type, and returns a
		/// <see cref="bool"/> value indicating the result of the conversion.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="result">
		/// (<see langword="out"/> parameter) The result parsed. If the conversion is failed,
		/// this argument will be <see cref="Undefined"/>.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <seealso cref="Undefined"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string str, out SudokuGrid result)
		{
			try
			{
				result = Parse(str);
				return true;
			}
			catch
			{
				result = Undefined;
				return false;
			}
		}

		/// <summary>
		/// Try to parse a string and converts to this type, and returns a
		/// <see cref="bool"/> value indicating the result of the conversion.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <param name="option">The grid parsing type.</param>
		/// <param name="result">
		/// (<see langword="out"/> parameter) The result parsed. If the conversion is failed,
		/// this argument will be <see cref="Undefined"/>.
		/// </param>
		/// <returns>A <see cref="bool"/> value indicating that.</returns>
		/// <seealso cref="Undefined"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParse(string str, ParsingOption option, out SudokuGrid result)
		{
			try
			{
				result = Parse(str, option);
				return true;
			}
			catch
			{
				result = Undefined;
				return false;
			}
		}

		/// <summary>
		/// Creates an instance using grid values.
		/// </summary>
		/// <param name="gridValues">The array of grid values.</param>
		/// <param name="creatingOption">
		/// The grid creating option. The default value is <see cref="GridCreatingOption.None"/>.
		/// </param>
		/// <returns>The result instance.</returns>
		public static SudokuGrid CreateInstance(
			int[] gridValues, GridCreatingOption creatingOption = GridCreatingOption.None)
		{
			var result = Empty;
			for (int i = 0; i < Length; i++)
			{
				if (gridValues[i] is var value and not 0)
				{
					// Calls the indexer to trigger the event
					// (Clear the candidates in peer cells).
					result[i] = creatingOption == GridCreatingOption.MinusOne ? value - 1 : value;

					// Set the status to 'CellStatus.Given'.
					result.SetStatus(i, CellStatus.Given);
				}
			}

			return result;
		}

		/// <summary>
		/// Delete or set a value on the specified grid.
		/// </summary>
		/// <param name="this">(<see langword="ref"/> parameter) The grid.</param>
		/// <param name="e">(<see langword="in"/> parameter) The event arguments.</param>
		private static void OnValueChanged(ref SudokuGrid @this, in ValueChangedArgs e)
		{
			if (e is { Cell: var cell, SetValue: var setValue and not -1 })
			{
				foreach (int peerCell in PeerMaps[cell])
				{
					if (@this.GetStatus(peerCell) == CellStatus.Empty)
					{
						// Please don't do this to avoid invoking recursively.
						//@this[peerCell, setValue] = false;

						@this._values[peerCell] &= (short)~(1 << setValue);
					}
				}
			}
		}

		/// <summary>
		/// Re-compute candidates.
		/// </summary>
		/// <param name="this">The grid.</param>
		public static void OnRefreshingCandidates(ref SudokuGrid @this)
		{
			for (int i = 0; i < Length; i++)
			{
				if (@this.GetStatus(i) == CellStatus.Empty)
				{
					// Remove all appeared digits.
					short mask = MaxCandidatesMask;
					foreach (int cell in PeerMaps[i])
					{
						if (@this[cell] is var digit and not -1)
						{
							mask &= (short)~(1 << digit);
						}
					}

					@this._values[i] = (short)(EmptyMask | mask);
				}
			}
		}

		/// <summary>
		/// Internal copy.
		/// </summary>
		/// <param name="dest">The destination pointer.</param>
		/// <param name="src">The source pointer.</param>
		internal static void InternalCopy(short* dest, short* src)
		{
			int i = 0;
			for (short* p = dest, q = src; i < Length; *p++ = *q++, i++) ;
		}


		/// <inheritdoc cref="Operators.operator =="/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in SudokuGrid left, in SudokuGrid right) => left.Equals(right);

		/// <inheritdoc cref="Operators.operator !="/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in SudokuGrid left, in SudokuGrid right) => !(left == right);
	}
}
