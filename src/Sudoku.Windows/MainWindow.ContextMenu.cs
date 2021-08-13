﻿namespace Sudoku.Windows;

partial class MainWindow
{
	private void MenuItemImageGridSet1_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 0);

	private void MenuItemImageGridSet2_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 1);

	private void MenuItemImageGridSet3_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 2);

	private void MenuItemImageGridSet4_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 3);

	private void MenuItemImageGridSet5_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 4);

	private void MenuItemImageGridSet6_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 5);

	private void MenuItemImageGridSet7_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 6);

	private void MenuItemImageGridSet8_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 7);

	private void MenuItemImageGridSet9_Click(object sender, RoutedEventArgs e) =>
		SetADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 8);

	private void MenuItemImageGridDelete1_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 0);

	private void MenuItemImageGridDelete2_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 1);

	private void MenuItemImageGridDelete3_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 2);

	private void MenuItemImageGridDelete4_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 3);

	private void MenuItemImageGridDelete5_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 4);

	private void MenuItemImageGridDelete6_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 5);

	private void MenuItemImageGridDelete7_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 6);

	private void MenuItemImageGridDelete8_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 7);

	private void MenuItemImageGridDelete9_Click(object sender, RoutedEventArgs e) =>
		DeleteADigit(_pointConverter.GetCell(_currentRightClickPos.ToDPointF()), 8);

	private void ContextListBoxPathsCopyCurrentStep_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem)
		{
			try
			{
				if (_listBoxPaths.SelectedItem is ListBoxItem { Content: StepTriplet(_, _, var s, _) })
				{
					SystemClipboard.Text = s.ToFullString();
				}
			}
			catch
			{
				Messagings.CannotCopyStep();
			}
		}
	}

	private void ContextListBoxPathsCopyAllSteps_Click(object sender, RoutedEventArgs e)
	{
		if (sender is MenuItem)
		{
			var sb = new ValueStringBuilder(stackalloc char[50]);
			foreach (string step in
				from ListBoxItem item in _listBoxPaths.Items
				let c = item.Content is StepTriplet s ? new StepTriplet?(s) : null
				where c is not null
				select c.Value.Item3.ToFullString())
			{
				sb.AppendLine(step);
			}

			try
			{
				SystemClipboard.Text = sb.ToString();
			}
			catch
			{
				Messagings.CannotCopyStep();
			}
		}
	}

	private void ContextMenuTechniquesApply_Click(object sender, RoutedEventArgs e)
	{
		if (
			sender is MenuItem && _listBoxTechniques is
			{
				SelectedItem: ListBoxItem { Content: InfoTriplet(_, var info, Item3: true, _) triplet }
			}
		)
		{
			ref var valueGrid = ref _puzzle.InnerGrid;
			if (
				!Settings.MainManualSolver.CheckConclusionValidityAfterSearched
				|| CheckConclusionsValidity(
					new UnsafeBitwiseSolver().Solve(valueGrid).Solution!.Value,
					info.Conclusions
				)
			)
			{
				info.ApplyTo(ref valueGrid);
				_currentPainter.Conclusions = null;
				_currentPainter.View = null;

				_listViewSummary.ClearValue(ItemsControl.ItemsSourceProperty);
				_listBoxTechniques.ClearValue(ItemsControl.ItemsSourceProperty);
				_listBoxPaths.ClearValue(ItemsControl.ItemsSourceProperty);

				UpdateImageGrid();
			}
			else
			{
				Messagings.WrongHandling(info, valueGrid);
			}
		}
	}
}
