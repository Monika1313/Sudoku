﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Sudoku.Forms
{
	partial class MainWindow
	{
		private void MenuItemFileUndo_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				if (_grid.HasUndoSteps)
				{
					_grid.Undo();
					UpdateImageGrid();
				}

				menuItem.IsEnabled = _grid.HasUndoSteps;
			}
		}

		private void MenuItemFileRedo_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				if (_grid.HasRedoSteps)
				{
					_grid.Redo();
					UpdateImageGrid();
				}

				menuItem.IsEnabled = _grid.HasRedoSteps;
			}
		}

		private void MenuItemFileFix_Click(object sender, RoutedEventArgs e)
		{
			_grid.Fix();

			UpdateImageGrid();
		}

		private void MenuItemFileUnfix_Click(object sender, RoutedEventArgs e)
		{
			_grid.Unfix();

			UpdateImageGrid();
		}

		private void MenuItemFileGetSnapshot_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.SetImage((BitmapSource)_imageGrid.Source);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Save failed due to:{Environment.NewLine}{ex.Message}.", "Warning");
			}
		}

		private void MenuItemFileQuit_Click(object sender, RoutedEventArgs e) =>
			Close();

		private void MenuItemAboutMe_Click(object sender, RoutedEventArgs e) =>
			new AboutMe().Show();
	}
}
