﻿using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace System
{
	/// <summary>
	/// Encapsulates a system clipboard (calls <see cref="Clipboard"/>).
	/// </summary>
	internal static class SystemClipboard
	{
		/// <summary>
		/// Gets or sets <see cref="DataFormats.UnicodeText"/> data
		/// on the clipboard.
		/// </summary>
		public static string Text
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Clipboard.GetText();

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Clipboard.SetText(value);
		}

		/// <summary>
		/// Sets the data object on the clipboard.
		/// </summary>
		public static object DataObject
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Clipboard.SetDataObject(value);
		}

		/// <summary>
		/// Sets <see cref="DataFormats.Bitmap"/> data on the clipboard.
		/// </summary>
		public static BitmapSource Image
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Clipboard.SetImage(value);
		}
	}
}
