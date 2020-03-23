﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Sudoku.Data;
using Sudoku.Data.Extensions;
using Sudoku.Data.Stepping;
using Sudoku.Drawing.Layers;
using Sudoku.Solving;
using Sudoku.Solving.Generating;
using Sudoku.Solving.Manual;
using Grid = System.Windows.Controls.Grid;
using SudokuGrid = Sudoku.Data.Grid;
using w = System.Windows;
using d = System.Drawing;

namespace Sudoku.Forms
{
	partial class MainWindow
	{
		private void MenuItemFileOpen_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
			{
				AddExtension = true,
				DefaultExt = "sudoku",
				Filter = "Text file|*.txt|Sudoku file|*.sudoku|All files|*.*",
				Multiselect = false,
				Title = "Open sudoku file from..."
			};

			if (!(dialog.ShowDialog() is true))
			{
				e.Handled = true;
				return;
			}

			using var sr = new StreamReader(dialog.FileName);
			LoadPuzzle(sr.ReadToEnd());
		}

		private void MenuItemFileSave_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				AddExtension = true,
				CheckPathExists = true,
				DefaultExt = "sudoku",
				Filter = "Text file|*.txt|Sudoku file|*.sudoku",
				Title = "Save sudoku file to..."
			};

			if (!(dialog.ShowDialog() is true))
			{
				e.Handled = true;
				return;
			}

			using var sw = new StreamWriter(dialog.FileName);
			sw.Write(Puzzle.ToString("#"));
		}

		private void MenuItemBackupConfig_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new SaveFileDialog
			{
				AddExtension = true,
				CheckPathExists = true,
				DefaultExt = "sudoku",
				Filter = "Configuration file|*.scfg",
				Title = "Save configuration file to..."
			};

			if (!(dialog.ShowDialog() is true))
			{
				e.Handled = true;
				return;
			}

			try
			{
				SaveConfig(dialog.FileName);
			}
			catch (Exception)
			{
				MessageBox.Show("Configuration file is failed to save due to internal error.", "Warning");
			}
		}

		private void MenuItemFileGetSnapshot_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.SetImage((BitmapSource)_imageGrid.Source);
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Save failed due to:{Environment.NewLine}{ex.Message}.",
					"Warning");
			}
		}

		private void MenuItemFileQuit_Click(object sender, RoutedEventArgs e) =>
			Close();

		private void MenuItemOptionsShowCandidates_Click(object sender, RoutedEventArgs e)
		{
			_layerCollection.Add(
				new ValueLayer(
					_pointConverter, Settings.ValueScale, Settings.CandidateScale,
					Settings.GivenColor, Settings.ModifiableColor, Settings.CandidateColor,
					Settings.GivenFontName, Settings.ModifiableFontName,
					Settings.CandidateFontName, Puzzle,
					Settings.ShowCandidates = _menuItemOptionsShowCandidates.IsChecked ^= true));

			UpdateImageGrid();
		}

		private void MenuItemOptionsSettings_Click(object sender, RoutedEventArgs e)
		{
			var settingsWindow = new SettingsWindow(this);
			if (!(settingsWindow.ShowDialog() is true))
			{
				e.Handled = true;
				return;
			}

			Settings.CoverBy(settingsWindow.Settings);
			UpdateControls();
			UpdateImageGrid();
		}

		private void MenuItemEditUndo_Click(object sender, RoutedEventArgs e)
		{
			if (Puzzle.HasUndoSteps)
			{
				Puzzle.Undo();
				UpdateImageGrid();
			}

			_menuItemUndo.IsEnabled = Puzzle.HasUndoSteps;
			_menuItemRedo.IsEnabled = Puzzle.HasRedoSteps;
		}

		private void MenuItemEditRedo_Click(object sender, RoutedEventArgs e)
		{
			if (Puzzle.HasRedoSteps)
			{
				Puzzle.Redo();
				UpdateImageGrid();
			}

			_menuItemUndo.IsEnabled = Puzzle.HasUndoSteps;
			_menuItemRedo.IsEnabled = Puzzle.HasRedoSteps;
		}

		private void MenuItemEditCopy_Click(object sender, RoutedEventArgs e) => InternalCopy(null);

		private void MenuItemEditCopyCurrentGrid_Click(object sender, RoutedEventArgs e) =>
			InternalCopy("#");

		private void MenuItemEditCopyPmGrid_Click(object sender, RoutedEventArgs e) =>
			InternalCopy("@");

		private void MenuItemEditCopyHodokuLibrary_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.SetText(Puzzle.ToString(GridOutputOptions.HodokuCompatible));
			}
			catch (Exception ex)
			{
				MessageBox.Show(
					$"Cannot save text to clipboard due to:{Environment.NewLine}{ex.Message}",
					"Warning");
			}
		}

		private void MenuItemEditPaste_Click(object sender, RoutedEventArgs e)
		{
			string puzzleStr = Clipboard.GetText();
			if (puzzleStr is null)
			{
				// 'value' is not null always.
				e.Handled = true;
				return;
			}

			LoadPuzzle(puzzleStr);
		}

		private void MenuItemEditFix_Click(object sender, RoutedEventArgs e)
		{
			Puzzle.Fix();

			UpdateImageGrid();
		}

		private void MenuItemEditUnfix_Click(object sender, RoutedEventArgs e)
		{
			Puzzle.Unfix();

			UpdateImageGrid();
		}

		private void MenuItemEditReset_Click(object sender, RoutedEventArgs e)
		{
			Puzzle.Reset();

			UpdateImageGrid();
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		private async void MenuItemGenerateHardPattern_Click(object sender, RoutedEventArgs e)
		{
			_textBoxInfo.Text = "Generating...";

			var puzzle = await Task.Run(new HardPatternPuzzleGenerator().Generate);

			_textBoxInfo.Text = string.Empty;

			Puzzle = new UndoableGrid((SudokuGrid)puzzle);
			_gridSummary.Children.Clear();
			_listBoxPaths.Items.Clear();

			UpdateImageGrid();
		}

		[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
		private async void MenuItemModeSolve_Click(object sender, RoutedEventArgs e)
		{
			// Record the backup.
			var initialiGrid = Puzzle;

			// Update status.
			_listBoxPaths.Items.Clear();
			_gridSummary.Children.Clear();
			_textBoxInfo.Text = "Solving, please wait. During solving you can do some other work...";

			// Run the solver asynchronizedly, during solving you can do other work.
			var analysisResult = await Task.Run(() =>
			{
				return new ManualSolver
				{
					FastSearch = Settings.FastSearch,
					AnalyzeDifficultyStrictly = Settings.SeMode
				}.Solve(Puzzle);
			});

			// Solved. Now update the technique summary.
			_gridSummary.RowDefinitions.Clear();
			_textBoxInfo.Text = string.Empty;
			if (analysisResult.HasSolved)
			{
				int i = 0;
				foreach (var step in analysisResult.SolvingSteps!)
				{
					_listBoxPaths.Items.Add(new InfoPair(i++, step));
				}

				// Add handlers.
				_listBoxPaths.SelectionChanged += (o, e) =>
				{
					if (_listBoxPaths.SelectedIndex == -1)
					{
						Puzzle = initialiGrid;
						UpdateImageGrid();

						e.Handled = true;
						return;
					}

					if (!(o is w::Controls.ListBox b && b.SelectedItem is InfoPair pair))
					{
						e.Handled = true;
						return;
					}

					var (n, s) = pair;
					Puzzle = new UndoableGrid((SudokuGrid)analysisResult.StepGrids![n]);
					_layerCollection.Add(
						new ViewLayer(
							_pointConverter, s.Views[0], new Dictionary<int, d::Color>
							{
								[0] = d::Color.FromArgb(128, 255, 192, 89),
								[1] = d::Color.FromArgb(128, 177, 165, 243),
								[2] = d::Color.FromArgb(128, 134, 242, 128),
								[-1] = d::Color.FromArgb(128, 247, 165, 167),
								[-2] = d::Color.FromArgb(128, 134, 232, 208),
							}));

					UpdateImageGrid();
				};
				_listBoxPaths.LostFocus += (_, e) =>
				{
					Puzzle = initialiGrid;
					UpdateImageGrid();
				};

				var collection = new List<(string?, int, decimal?, decimal?)>();
				decimal summary = 0, summaryMax = 0;
				int summaryCount = 0;
				foreach (var techniqueGroup in GetGroupedSteps())
				{
					string name = techniqueGroup.Key;
					int count = techniqueGroup.Count();
					decimal total = 0, maximum = 0;
					foreach (var step in techniqueGroup)
					{
						summary += step.Difficulty;
						summaryCount++;
						total += step.Difficulty;
						maximum = Math.Max(step.Difficulty, maximum);
						summaryMax = Math.Max(step.Difficulty, maximum);
					}

					collection.Add((name, count, total, maximum));
				}

				collection.Add((null, summaryCount, summary, summaryMax));

				_gridSummary.RowDefinitions.Add(
					new w.Controls.RowDefinition
					{
						Height = new GridLength(FontSize, GridUnitType.Auto)
					});
				_gridSummary.Children.Add(
					CreateLabelInGrid(
						"Technique", HorizontalAlignment.Left, VerticalAlignment.Center, 0, 0));
				_gridSummary.Children.Add(
					CreateLabelInGrid(
						"Count", HorizontalAlignment.Center, VerticalAlignment.Center, 0, 1));
				_gridSummary.Children.Add(
					CreateLabelInGrid(
						"Total", HorizontalAlignment.Center, VerticalAlignment.Center, 0, 2));
				_gridSummary.Children.Add(
					CreateLabelInGrid(
						"Max", HorizontalAlignment.Center, VerticalAlignment.Center, 0, 3));

				i = 1;
				foreach (ITuple quadruple in collection)
				{
					_gridSummary.RowDefinitions.Add(
						new w::Controls.RowDefinition
						{
							Height = new GridLength(FontSize, GridUnitType.Auto)
						});
					for (int j = 0; j < 4; j++)
					{
						_gridSummary.Children.Add(
							CreateLabelInGrid(
								quadruple[j].NullableToString(),
								j != 0 ? HorizontalAlignment.Center : HorizontalAlignment.Left,
								VerticalAlignment.Center,
								i,
								j));
					}

					i++;
				}
			}
			else
			{
				MessageBox.Show("The puzzle cannot be solved due to internal error.", "Warning");
			}

			IEnumerable<IGrouping<string, TechniqueInfo>> GetGroupedSteps()
			{
				(_, _, var solvingSteps) = analysisResult;
				return from solvingStep in solvingSteps!
					   orderby solvingStep.Difficulty
					   group solvingStep by solvingStep.Name;
			}

			static w::Controls.Label CreateLabelInGrid(
				string content, HorizontalAlignment horizontalAlignment,
				VerticalAlignment verticalAlignment, int row, int column)
			{
				var z = new w::Controls.Label
				{
					Content = content,
					HorizontalAlignment = horizontalAlignment,
					VerticalAlignment = verticalAlignment
				};
				z.SetValue(Grid.RowProperty, row);
				z.SetValue(Grid.ColumnProperty, column);
				return z;
			}
		}

		private void MenuItemModeSeMode_Click(object sender, RoutedEventArgs e) =>
			_menuItemModeSeMode.IsChecked = Settings.SeMode ^= true;

		private void MenuItemModeFastSearch_Click(object sender, RoutedEventArgs e) =>
			_menuItemModeFastSearch.IsChecked = Settings.FastSearch ^= true;

		private void MenuItemAboutMe_Click(object sender, RoutedEventArgs e) =>
			new AboutMeWindow().Show();


		private readonly struct InfoPair
		{
			public InfoPair(int index, TechniqueInfo techniqueInfo) =>
				(Index, TechniqueInfo) = (index, techniqueInfo);

			public InfoPair((int _index, TechniqueInfo _info) pair) =>
				(Index, TechniqueInfo) = (pair._index, pair._info);


			public int Index { get; }

			public TechniqueInfo TechniqueInfo { get; }


			public void Deconstruct(out int index, out TechniqueInfo techniqueInfo) =>
				(index, techniqueInfo) = (Index, TechniqueInfo);

			public override string ToString() => TechniqueInfo.ToString();
		}
	}
}
