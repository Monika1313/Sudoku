namespace SudokuStudio.Views.Pages;

/// <summary>
/// Defines a new page that stores a set of controls to analyze a sudoku grid.
/// </summary>
[DependencyProperty<bool>("IsAnalyzerLaunched", Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates whether the analyzer is launched.")]
[DependencyProperty<bool>("IsGathererLaunched", Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates whether the gatherer is launched.")]
[DependencyProperty<bool>("GeneratorIsNotRunning", DefaultValue = true, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates whether the generator is not running currently.")]
[DependencyProperty<double>("ProgressPercent", Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the progress percent value.")]
[DependencyProperty<int>("CurrentViewIndex", DefaultValue = -1, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the current index of the view of property <see cref=\"IRenderable.Views\"/> displayed.")]
[DependencyProperty<int>("SelectedColorIndex", DefaultValue = -1, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the selected color index.")]
[DependencyProperty<string>("BabaGroupNameInput", IsNullable = true, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the input character that is used as a baba group variable.")]
[DependencyProperty<DrawingMode>("SelectedMode", DefaultValue = DrawingMode.Cell, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the selected drawing mode.")]
[DependencyProperty<Inference>("LinkKind", DefaultValue = Inference.Strong, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the link type.")]
[DependencyProperty<AnalyzerResult>("AnalysisResultCache", IsNullable = true, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the analysis result cache.")]
[DependencyProperty<ColorPalette>("UserDefinedPalette", Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the user-defined colors.")]
[DependencyProperty<IRenderable>("VisualUnit", IsNullable = true, Accessibility = GeneralizedAccessibility.Internal, DocSummary = "Indicates the visual unit.")]
public sealed partial class AnalyzePage : Page
{
	[DefaultValue]
	private static readonly ColorPalette UserDefinedPaletteDefaultValue = ((App)Application.Current).Preference.UIPreferences.UserDefinedColorPalette;


	/// <summary>
	/// <para>Indicates the previously selected candidate.</para>
	/// <para>
	/// This field can record the previous data in a pair of clicking operation.
	/// For example, constructing a chain, we should click twice, for the head and tail of the chain.
	/// If we click at the second time, this field will be set the candidate having clicked at the first time.
	/// </para>
	/// </summary>
	internal Candidate? _previousSelectedCandidate;

	/// <summary>
	/// Defines a local view.
	/// </summary>
	internal ViewUnitBindableSource? _localView = new() { Conclusions = Array.Empty<Conclusion>(), View = View.Empty };

	/// <summary>
	/// Indicates the tab routing data.
	/// </summary>
	private List<AnalyzeTabPageBindableSource> _tabsRoutingData;

	/// <summary>
	/// Defines a key-value pair of functions that is used for routing hotkeys.
	/// </summary>
	private Dictionary<Hotkey, Action> _hotkeyFunctions;

	/// <summary>
	/// The navigating data.
	/// </summary>
	private Dictionary<Predicate<NavigationViewItemBase>, Type> _navigatingData;


	/// <summary>
	/// Initializes an <see cref="AnalyzePage"/> instance.
	/// </summary>
	public AnalyzePage()
	{
		InitializeComponent();
		InitializeFields();
		LoadInitialGrid();
	}


	/// <summary>
	/// Provides with an event that is triggered when failed to open a file.
	/// </summary>
	public event OpenFileFailedEventHandler? OpenFileFailed;

	/// <summary>
	/// Provides with an event that is triggered when failed to save a file.
	/// </summary>
	public event SaveFileFailedEventHandler? SaveFileFailed;


	/// <summary>
	/// To clear all possible data from the analyze tabs.
	/// </summary>
	internal void ClearAnalyzeTabsData()
	{
		foreach (var pageData in (IEnumerable<AnalyzeTabPageBindableSource>)AnalyzeTabs.TabItemsSource)
		{
			if (pageData is { Page: IAnalyzeTabPage subTabPage })
			{
				subTabPage.AnalysisResult = null;
			}
		}
	}

	/// <summary>
	/// To update values via the specified <see cref="AnalyzerResult"/> instance.
	/// </summary>
	/// <param name="analyzerResult">The analysis result instance.</param>
	/// <seealso cref="AnalyzerResult"/>
	internal void UpdateAnalysisResult(AnalyzerResult analyzerResult)
	{
		foreach (var pageData in (IEnumerable<AnalyzeTabPageBindableSource>)AnalyzeTabs.TabItemsSource)
		{
			if (pageData is { Page: IAnalyzeTabPage subTabPage })
			{
				subTabPage.AnalysisResult = analyzerResult;
			}
		}
	}

	/// <summary>
	/// Copies for sudoku puzzle text.
	/// </summary>
	internal void CopySudokuGridText()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (SudokuPane.Puzzle is var puzzle and ({ IsUndefined: true } or { IsEmpty: true }))
		{
			return;
		}

		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		dataPackage.SetText(puzzle.ToString(SusserFormat.Full));

		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// To determine whether the current application view is in an unsnapped state.
	/// </summary>
	/// <returns>The <see cref="bool"/> value indicating that.</returns>
	internal bool EnsureUnsnapped(bool isFileSaving)
	{
		// 'FileOpenPicker' APIs will not work if the application is in a snapped state.
		// If an app wants to show a 'FileOpenPicker' while snapped, it must attempt to unsnap first.
		var unsnapped = ApplicationView.Value != ApplicationViewState.Snapped || ApplicationView.TryUnsnap();
		if (!unsnapped)
		{
			if (isFileSaving)
			{
				SaveFileFailed?.Invoke(this, new(SaveFileFailedReason.UnsnappingFailed));
			}
			else
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.UnsnappingFailed));
			}
		}

		return unsnapped;
	}

	/// <summary>
	/// Copy the snapshot of the sudoku grid control, to the clipboard.
	/// </summary>
	/// <returns>
	/// The typical <see langword="await"/>able instance that holds the task to copy the snapshot.
	/// </returns>
	/// <remarks>
	/// The code is referenced from
	/// <see href="https://github.com/microsoftarchive/msdn-code-gallery-microsoft/blob/21cb9b6bc0da3b234c5854ecac449cb3bd261f29/Official%20Windows%20Platform%20Sample/XAML%20render%20to%20bitmap%20sample/%5BC%23%5D-XAML%20render%20to%20bitmap%20sample/C%23/Scenario2.xaml.cs#L120">here</see>
	/// and
	/// <see href="https://github.com/microsoftarchive/msdn-code-gallery-microsoft/blob/21cb9b6bc0da3b234c5854ecac449cb3bd261f29/Official%20Windows%20Platform%20Sample/XAML%20render%20to%20bitmap%20sample/%5BC%23%5D-XAML%20render%20to%20bitmap%20sample/C%23/Scenario2.xaml.cs#L182">here</see>.
	/// </remarks>
	internal async Task CopySudokuGridControlAsSnapshotAsync()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		// Creates the stream to store the output image data.
		var stream = new InMemoryRandomAccessStream();
		await OnSavingOrCopyingSudokuPanePictureAsync(stream);

		// Copies the data to the data package.
		var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
		var streamRef = RandomAccessStreamReference.CreateFromStream(stream);
		dataPackage.SetBitmap(streamRef);

		// Copies to the clipboard.
		Clipboard.SetContent(dataPackage);
	}

	/// <summary>
	/// Pastes the text, to the clipboard.
	/// </summary>
	/// <returns>
	/// The typical <see langword="await"/>able instance that holds the task to paste the text.
	/// </returns>
	internal async Task PasteCodeToSudokuGridAsync()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		var dataPackageView = Clipboard.GetContent();
		if (dataPackageView.Contains(StandardDataFormats.Text) && Grid.TryParse(await dataPackageView.GetTextAsync(), out var grid))
		{
			SudokuPane.Puzzle = grid;
		}
	}

	/// <summary>
	/// Open a file.
	/// </summary>
	/// <returns>A task that handles the operation.</returns>
	internal async Task OpenFileInternalAsync()
	{
		if (!EnsureUnsnapped(false))
		{
			return;
		}

		var fop = new FileOpenPicker();

		var window = ((App)Application.Current).WindowManager.GetWindowForElement(this);
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(fop, hWnd);

		fop.ViewMode = PickerViewMode.Thumbnail;
		fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fop.FileTypeFilter.Add(CommonFileExtensions.Text);
		fop.FileTypeFilter.Add(CommonFileExtensions.PlainText);

		if (await fop.PickSingleFileAsync() is not { } file)
		{
			return;
		}

		await LoadPuzzleCoreAsync(file);
	}

	/// <summary>
	/// Save a file.
	/// </summary>
	/// <returns>A task that handles the operation.</returns>
	internal async Task SaveFileInternalAsync()
	{
		if (!EnsureUnsnapped(true))
		{
			return;
		}

		var fsp = new FileSavePicker();

		var window = ((App)Application.Current).WindowManager.GetWindowForElement(this);
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(fsp, hWnd);

		fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fsp.SuggestedFileName = GetString("Sudoku");
		fsp.FileTypeChoices.Add(GetString("FileExtension_TextDescription"), new[] { CommonFileExtensions.Text });
		fsp.FileTypeChoices.Add(GetString("FileExtension_PlainTextDescription"), new[] { CommonFileExtensions.PlainText });
		fsp.FileTypeChoices.Add(GetString("FileExtension_Picture"), new[] { CommonFileExtensions.PortablePicture });

		if (await fsp.PickSaveFileAsync() is not { Path: var filePath } file)
		{
			return;
		}

		switch (SystemPath.GetExtension(filePath))
		{
			case CommonFileExtensions.PlainText:
			{
				SudokuPlainTextFileHandler.Write(filePath, SudokuPane.Puzzle);
				break;
			}
			case CommonFileExtensions.Text when SudokuPane is { Puzzle: var puzzle, ViewUnit: var viewUnit }:
			{
				SudokuFileHandler.Write(
					filePath,
					new GridInfo[]
					{
						new()
						{
							GridString = puzzle.ToString(SusserFormat.Full),
							RenderableData = viewUnit switch
							{
								{ Conclusions: var conclusions, View: var view } => new() { Conclusions = conclusions, Views = new[] { view } },
								_ => null
							}
						}
					}
				);
				break;
			}
			case CommonFileExtensions.PortablePicture:
			{
				await OnSavingOrCopyingSudokuPanePictureAsync(file);
				break;
			}
		}
	}

	/// <summary>
	/// Save a file via grid formatters.
	/// </summary>
	/// <param name="gridFormatters">The grid formatters.</param>
	/// <returns>A task that handles the operation.</returns>
	internal async Task<bool> SaveFileInternalAsync(ArrayList gridFormatters)
	{
		if (!EnsureUnsnapped(true))
		{
			return false;
		}

		var fsp = new FileSavePicker();

		var window = ((App)Application.Current).WindowManager.GetWindowForElement(this);
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(fsp, hWnd);

		fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fsp.SuggestedFileName = GetString("Sudoku");
		fsp.FileTypeChoices.Add(GetString("FileExtension_TextDescription"), new[] { CommonFileExtensions.Text });
		fsp.FileTypeChoices.Add(GetString("FileExtension_PlainTextDescription"), new[] { CommonFileExtensions.PlainText });
		fsp.FileTypeChoices.Add(GetString("FileExtension_Picture"), new[] { CommonFileExtensions.PortablePicture });

		if (await fsp.PickSaveFileAsync() is not { Path: var filePath } file)
		{
			return false;
		}

		var grid = SudokuPane.Puzzle;
		switch (SystemPath.GetExtension(filePath))
		{
			case CommonFileExtensions.PlainText:
			{
				await File.WriteAllTextAsync(
					filePath,
					string.Join("\r\n\r\n", from gridFormatter in gridFormatters select ((IGridFormatter)gridFormatter).ToString(grid))
				);

				break;
			}
			case CommonFileExtensions.Text:
			{
				var renderableData = SudokuPane.ViewUnit switch
				{
					{ Conclusions: var conclusions, View: var view } => new UserDefinedRenderable { Conclusions = conclusions, Views = new[] { view } },
					_ => default(UserDefinedRenderable?)
				};

				SudokuFileHandler.Write(
					filePath,
					(
						from gridFormatter in gridFormatters
						select ((IGridFormatter)gridFormatter).ToString(grid) into gridString
						select new GridInfo { GridString = gridString, RenderableData = renderableData }
					).ToArray()
				);

				break;
			}
			case CommonFileExtensions.PortablePicture:
			{
				await OnSavingOrCopyingSudokuPanePictureAsync(file);
				break;
			}
		}

		return true;
	}

	/// <summary>
	/// The internal logic to load a puzzle. This operation can be called after dropping files
	/// or picking files from <see cref="FileOpenPicker"/>.
	/// </summary>
	/// <param name="file">The <see cref="StorageFile"/> instance.</param>
	/// <returns>The task instance that handles this operation.</returns>
	/// <seealso cref="FileOpenPicker"/>
	internal async Task LoadPuzzleCoreAsync(StorageFile? file)
	{
		if (file is null)
		{
			return;
		}

		var filePath = file.Path;
		switch (new FileInfo(filePath).Length)
		{
			case 0:
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsEmpty));
				return;
			}
			case > 1024 * 64:
			{
				OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsTooLarge));
				return;
			}
			default:
			{
				switch (SystemPath.GetExtension(filePath))
				{
					case CommonFileExtensions.PlainText:
					{
						var content = await FileIO.ReadTextAsync(file);
						if (string.IsNullOrWhiteSpace(content))
						{
							OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileIsEmpty));
							return;
						}

						if (!Grid.TryParse(content, out var g))
						{
							OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileCannotBeParsed));
							return;
						}

						SudokuPane.Puzzle = g;
						break;
					}
					case CommonFileExtensions.Text:
					{
						switch (SudokuFileHandler.Read(filePath))
						{
							case [{ GridString: var str, RenderableData: var nullableRenderableData }]:
							{
								if (!Grid.TryParse(str, out var g))
								{
									OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileCannotBeParsed));
									return;
								}

								SudokuPane.Puzzle = g;

								if (nullableRenderableData is { } renderableData)
								{
									VisualUnit = renderableData;
								}

								break;
							}
							default:
							{
								OpenFileFailed?.Invoke(this, new(OpenFileFailedReason.FileCannotBeParsed));
								return;
							}
						}

						break;
					}
				}

				break;
			}
		}
	}

	/// <inheritdoc/>
	protected override void OnKeyDown(KeyRoutedEventArgs e)
	{
		base.OnKeyDown(e);

		// This method routes the hotkeys.
		var modifierStatus = Keyboard.GetModifierStatusForCurrentThread();
		foreach (var ((modifiers, key), action) in _hotkeyFunctions)
		{
			if (modifierStatus == modifiers && e.Key == key)
			{
				action();
				break;
			}
		}

		e.Handled = false;
	}

	/// <summary>
	/// Try to initialize field <see cref="_hotkeyFunctions"/> and <see cref="_navigatingData"/>.
	/// </summary>
	/// <seealso cref="_hotkeyFunctions"/>
	/// <seealso cref="_navigatingData"/>
	[MemberNotNull(nameof(_hotkeyFunctions), nameof(_navigatingData), nameof(_tabsRoutingData))]
	private void InitializeFields()
	{
		var thickness = new Thickness(10);
		_tabsRoutingData = new()
		{
			new()
			{
				Header = GetString("AnalyzePage_TechniquesTable"),
				IconSource = new SymbolIconSource { Symbol = Symbol.Flag },
				Page = new Summary { Margin = thickness, BasePage = this }
			},
			new()
			{
				Header = GetString("AnalyzePage_StepDetail"),
				IconSource = new SymbolIconSource { Symbol = Symbol.ShowResults },
				Page = new SolvingPath { Margin = thickness, BasePage = this }
			},
			new()
			{
				Header = GetString("AnalyzePage_AllStepsInCurrentGrid"),
				IconSource = new SymbolIconSource { Symbol = Symbol.Shuffle },
				Page = new GridGathering { Margin = thickness, BasePage = this }
			},
			new()
			{
				Header = GetString("AnalyzePage_Drawing"),
				IconSource = new SymbolIconSource { Symbol = Symbol.Edit },
				Page = new Drawing { Margin = thickness, BasePage = this }
			}
		};

		_hotkeyFunctions = new()
		{
			{ new(VirtualKeyModifiers.Control, VirtualKey.Z), SudokuPane.UndoStep },
			{ new(VirtualKeyModifiers.Control, VirtualKey.Y), SudokuPane.RedoStep },
			{ new(VirtualKeyModifiers.Control, VirtualKey.C), CopySudokuGridText },
			{
				new(VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift, VirtualKey.C),
				async () => await CopySudokuGridControlAsSnapshotAsync()
			},
			{ new(VirtualKeyModifiers.Control, VirtualKey.V), async () => await PasteCodeToSudokuGridAsync() },
			{ new(VirtualKeyModifiers.Control, VirtualKey.O), async () => await OpenFileInternalAsync() },
			{ new(VirtualKeyModifiers.Control, VirtualKey.S), async () => await SaveFileInternalAsync() },
			{ new((VirtualKey)189), SetPreviousView }, // Minus sign
			{ new((VirtualKey)187), SetNextView }, // Equals sign
			{ new(VirtualKey.Home), SetHomeView },
			{ new(VirtualKey.End), SetEndView },
			{ new(VirtualKey.Escape), ClearView }
		};

		_navigatingData = new()
		{
			{ container => container == BasicOperationBar, typeof(BasicOperation) },
			{ container => container == AttributeCheckingOperationBar, typeof(AttributeCheckingOperation) },
			{ container => container == PrintingOperationBar, typeof(PrintingOperation) },
			{ container => container == ShuffleOperationBar, typeof(ShuffleOperation) }
		};
	}

	/// <summary>
	/// Load initial grid.
	/// </summary>
	private void LoadInitialGrid()
	{
		if (((App)Application.Current).FirstGrid is { } grid)
		{
			SudokuPane.Puzzle = grid;
			((App)Application.Current).FirstGrid = null; // Maybe not necessary...
		}
	}

	/// <summary>
	/// An outer-layered method to switching pages. This method can be used by both
	/// <see cref="CommandBarView_ItemInvoked"/> and <see cref="CommandBarView_SelectionChanged"/>.
	/// </summary>
	/// <param name="container">The container.</param>
	/// <seealso cref="CommandBarView_ItemInvoked"/>
	/// <seealso cref="CommandBarView_SelectionChanged"/>
	private void SwitchingPage(NavigationViewItemBase container)
	{
		foreach (var (predicate, type) in _navigatingData)
		{
			if (predicate(container))
			{
				NavigateToPage(type);
			}
		}
	}

	/// <summary>
	/// Try to navigate to the target page.
	/// </summary>
	/// <param name="pageType">The target page type.</param>
	private void NavigateToPage(Type pageType)
	{
		if (CommandBarFrame.SourcePageType != pageType)
		{
			CommandBarFrame.Navigate(pageType, this, DefaultNavigationTransitionInfo);
		}
	}

	/// <summary>
	/// Sets the previous view.
	/// </summary>
	private void SetPreviousView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 } && CurrentViewIndex - 1 >= 0)
		{
			CurrentViewIndex--;
		}
	}

	/// <summary>
	/// Sets the next view.
	/// </summary>
	private void SetNextView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: var length and not 0 } && CurrentViewIndex + 1 < length)
		{
			CurrentViewIndex++;
		}
	}

	/// <summary>
	/// Sets the home view.
	/// </summary>
	private void SetHomeView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 })
		{
			CurrentViewIndex = 0;
		}
	}

	/// <summary>
	/// Sets the end view.
	/// </summary>
	private void SetEndView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: var length and not 0 })
		{
			CurrentViewIndex = length - 1;
		}
	}

	/// <summary>
	/// Skips to the specified index of the view.
	/// </summary>
	/// <param name="viewIndex">The view index.</param>
	private void SkipToSpecifiedViewIndex(int viewIndex)
	{
		if (VisualUnit is not { Views.Length: var length } || length <= viewIndex)
		{
			return;
		}

		CurrentViewIndex = viewIndex;
	}

	/// <summary>
	/// Clear the visual unit data.
	/// </summary>
	private void ClearView()
	{
		if (!IsSudokuPaneFocused())
		{
			return;
		}

		if (VisualUnit is { Views.Length: not 0 })
		{
			VisualUnit = null;
			SudokuPane.ViewUnit = null;
		}
	}

	/// <summary>
	/// Checks whether the pane is focused.
	/// </summary>
	/// <returns>A <see cref="bool"/> result.</returns>
	private bool IsSudokuPaneFocused() => SudokuPane.FocusState != FocusState.Unfocused;

	/// <summary>
	/// Try to update view unit.
	/// </summary>
	private void UpdateViewUnit()
	{
		SudokuPane.ViewUnit = null; // Change the reference to update view.
		SudokuPane.ViewUnit = _localView;
	}

	private bool CheckCellNode(int index, GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (e)
		{
			case { Cell: var cell, MouseButton: MouseButton.Left }:
			{
				if (view.View.Find(node => node is CellViewNode { Cell: var c } && c == cell) is { } foundNode)
				{
					view.View.Remove(foundNode);
				}
				else
				{
					var id = UserDefinedPalette[index].GetIdentifier();
					view.View.Add(new CellViewNode(id, cell));
				}

				UpdateViewUnit();

				break;
			}
		}

		return true;
	}

	private bool CheckCandidateNode(int index, GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (e)
		{
			case { Candidate: var candidate, MouseButton: MouseButton.Left }:
			{
				if (view.View.Find(node => node is CandidateViewNode { Candidate: var c } && c == candidate) is { } foundNode)
				{
					view.View.Remove(foundNode);
				}
				else
				{
					var id = UserDefinedPalette[index].GetIdentifier();
					view.View.Add(new CandidateViewNode(id, candidate));
				}

				UpdateViewUnit();

				break;
			}
		}

		return true;
	}

	private bool CheckHouseNode(int index, GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (e)
		{
			case { Candidate: var candidate2 }:
			{
				if (_previousSelectedCandidate is not { } candidate1)
				{
					_previousSelectedCandidate = candidate2;
					return false;
				}

				var cell1 = candidate1 / 9;
				var cell2 = candidate2 / 9;
				if ((CellsMap[cell1] + cell2).CoveredHouses is not (var coveredHouses and not 0))
				{
					_previousSelectedCandidate = null;
					return true;
				}

				var house = coveredHouses.GetAllSets()[^1];
				if (view.View.Find(node => node is HouseViewNode { House: var h } && h == house) is { } foundNode)
				{
					view.View.Remove(foundNode);
				}
				else
				{
					var id = UserDefinedPalette[index].GetIdentifier();
					view.View.Add(new HouseViewNode(id, house));
				}

				UpdateViewUnit();

				break;
			}
		}

		return true;
	}

	private bool CheckChuteNode(int index, GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (e)
		{
			case { Candidate: var candidate2 }:
			{
				if (_previousSelectedCandidate is not { } candidate1)
				{
					_previousSelectedCandidate = candidate2;
					return false;
				}

				var (mr1, mc1) = GridClickedEventArgs.GetChute(candidate1);
				var (mr2, mc2) = GridClickedEventArgs.GetChute(candidate2);
				if (mr1 == mr2)
				{
					if (view.View.Find(node => node is ChuteViewNode { ChuteIndex: var c } && c == mr1) is { } foundNode)
					{
						view.View.Remove(foundNode);
					}
					else
					{
						var id = UserDefinedPalette[index].GetIdentifier();
						view.View.Add(new ChuteViewNode(id, mr1));
					}

					UpdateViewUnit();

					break;
				}

				if (mc1 == mc2)
				{
					if (view.View.Find(node => node is ChuteViewNode { ChuteIndex: var c } && c - 3 == mc1) is { } foundNode)
					{
						view.View.Remove(foundNode);
					}
					else
					{
						var id = UserDefinedPalette[index].GetIdentifier();
						view.View.Add(new ChuteViewNode(id, mc1 + 3));
					}

					UpdateViewUnit();

					break;
				}

				break;
			}
		}

		return true;
	}

	private bool CheckLinkNode(GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (e)
		{
			case { Candidate: var candidate2 }:
			{
				if (_previousSelectedCandidate is not { } candidate1)
				{
					_previousSelectedCandidate = candidate2;
					return false;
				}

				var cell1 = candidate1 / 9;
				var cell2 = candidate2 / 9;
				var digit1 = candidate1 % 9;
				var digit2 = candidate2 % 9;
				if (view.View.Find(predicate) is { } foundNode)
				{
					view.View.Remove(foundNode);
				}
				else
				{
					var lt1 = new LockedTarget(candidate1 % 9, CellsMap[candidate1 / 9]);
					var lt2 = new LockedTarget(candidate2 % 9, CellsMap[candidate2 / 9]);
					view.View.Add(new LinkViewNode(default!, lt1, lt2, LinkKind)); // Link nodes don't use identifier to display colors.
				}

				UpdateViewUnit();

				break;


				bool predicate(ViewNode element)
					=> element switch
					{
						LinkViewNode { Start.Cells: [var c1], End.Cells: [var c2], Inference: Inference.Default }
							=> c1 == cell1 && c2 == cell2 || c2 == cell1 && c1 == cell2,
						LinkViewNode { Start: { Cells: [var c1], Digit: var d1 }, End: { Cells: [var c2], Digit: var d2 } }
							=> c1 == cell1 && c2 == cell2 && d1 == digit1 && d2 == digit2
							|| c2 == cell1 && c1 == cell2 && d2 == digit1 && d1 == digit2
					};
			}
		}

		return true;
	}

	private bool CheckBabaGroupingNode(int index, GridClickedEventArgs e, ViewUnitBindableSource view)
	{
		switch (BabaGroupNameInput)
		{
			case null or []:
			{
				return true;
			}
			case [var character]:
			{
				switch (e)
				{
					case { Candidate: var candidate }:
					{
						var cell = candidate / 9;
						if (view.View.Find(node => node is BabaGroupViewNode { Cell: var c } && c == cell) is { } foundNode)
						{
							view.View.Remove(foundNode);
						}
						else
						{
							var id = UserDefinedPalette[index].GetIdentifier();
							view.View.Add(new BabaGroupViewNode(id, cell, (Utf8Char)character, Grid.MaxCandidatesMask));
						}

						UpdateViewUnit();

						break;
					}
				}

				break;
			}
			default:
			{
				((Drawing)AnalyzeTabs.SelectedItem).InvalidInputInfoDisplayer.Visibility = Visibility.Visible;
				break;
			}
		}

		return true;
	}

	/// <summary>
	/// Produces a copying/saving operation for pictures from sudoku pane <see cref="SudokuPane"/>.
	/// </summary>
	/// <typeparam name="T">
	/// The type of the handling argument. This type should be <see cref="IRandomAccessStream"/> or <see cref="StorageFile"/>.
	/// </typeparam>
	/// <param name="obj">The argument.</param>
	/// <returns>A <see cref="Task"/> instance that contains details for the current asynchronous operation.</returns>
	/// <seealso cref="IRandomAccessStream"/>
	/// <seealso cref="StorageFile"/>
	private async Task OnSavingOrCopyingSudokuPanePictureAsync<T>(T obj) where T : class
	{
		if (((App)Application.Current).Preference.UIPreferences.TransparentBackground)
		{
			SudokuPane.MainGrid.Background = new SolidColorBrush(Colors.White);
		}

		var desiredSize = ((App)Application.Current).Preference.UIPreferences.DesiredPictureSizeOnSaving;
		var (originalWidth, originalHeight) = (SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height);
		(SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height) = (desiredSize, desiredSize);

		await SudokuPaneOutsideViewBox.RenderToAsync(obj);

		(SudokuPaneOutsideViewBox.Width, SudokuPaneOutsideViewBox.Height) = (originalWidth, originalHeight);

		if (((App)Application.Current).Preference.UIPreferences.TransparentBackground)
		{
			SudokuPane.MainGrid.Background = null;
		}
	}


	/// <summary>
	/// Try to change the value of the property <see cref="CurrentViewIndex"/>.
	/// </summary>
	/// <param name="page">The triggering page.</param>
	/// <param name="value">The index value set.</param>
	/// <seealso cref="CurrentViewIndex"/>
	private static void ChangeCurrentViewIndex(AnalyzePage page, int value)
	{
		var visualUnit = page.VisualUnit;

		if (value == -1)
		{
			page.VisualUnit = null;
			return;
		}

		page.SudokuPane.ViewUnit = visualUnit switch
		{
			{ Conclusions: var conclusions, Views: null or [] } => new() { Conclusions = conclusions, View = View.Empty },
			{ Conclusions: var conclusions, Views: var views } => new() { Conclusions = conclusions, View = views[value] },
			_ => null
		};
	}

	[Callback]
	private static void CurrentViewIndexPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is not (AnalyzePage page, { NewValue: int value }))
		{
			return;
		}

		ChangeCurrentViewIndex(page, value);
	}

	[Callback]
	private static void VisualUnitPropertyCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((d, e) is not (AnalyzePage page, { NewValue: var value and (null or IRenderable) }))
		{
			return;
		}

		var currentViewIndex = value is IRenderable ? 0 : -1;
		page.CurrentViewIndex = currentViewIndex;

		ChangeCurrentViewIndex(page, currentViewIndex);

		// A rescue. The code snippet is used for manually updating the pips pager and text block.
		page.ViewsSwitcher.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
		page.ViewsCountDisplayer.Visibility = value is null ? Visibility.Collapsed : Visibility.Visible;
	}


	private void CommandBarView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		=> SwitchingPage(args.InvokedItemContainer);

	private void CommandBarView_Loaded(object sender, RoutedEventArgs e) => BasicOperationBar.IsSelected = true;

	private void CommandBarView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
		=> SwitchingPage(args.SelectedItemContainer);

	private void CommandBarFrame_Navigated(object sender, NavigationEventArgs e)
	{
		if (e is not { Content: IOperationProviderPage operationProvider, Parameter: AnalyzePage @this })
		{
			return;
		}

		operationProvider.BasePage = @this;
	}

	private void FixGridButton_Click(object sender, RoutedEventArgs e)
	{
		var modified = SudokuPane.Puzzle;
		modified.Fix();

		SudokuPane.Puzzle = modified;
	}

	private void UnfixGridButton_Click(object sender, RoutedEventArgs e)
	{
		var modified = SudokuPane.Puzzle;
		modified.Unfix();

		SudokuPane.Puzzle = modified;
	}

	private void SudokuPane_MouseWheelChanged(SudokuPane sender, SudokuPaneMouseWheelChangedEventArgs e)
	{
		if (VisualUnit is { Views.Length: var length and not 0 })
		{
			((Action)(e.IsClockwise ? SetNextView : SetPreviousView))();
		}
	}

	private void SudokuPane_GridUpdated(SudokuPane sender, GridUpdatedEventArgs e)
	{
		if (e.Behavior is GridUpdatedBehavior.Assignment or GridUpdatedBehavior.Elimination
			or GridUpdatedBehavior.EliminationMultiple or GridUpdatedBehavior.Clear)
		{
			ClearAnalyzeTabsData();
		}
	}

	private async void AnalyzeButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var puzzle = SudokuPane.Puzzle;
		if (!puzzle.IsValid)
		{
			return;
		}

		AnalyzeButton.IsEnabled = false;
		ClearAnalyzeTabsData();
		IsAnalyzerLaunched = true;

		var textFormat = GetString("AnalyzePage_AnalyzerProgress");

		var disallowHighTimeComplexity = ((App)Application.Current).Preference.AnalysisPreferences.LogicalSolverIgnoresSlowAlgorithms;
		var disallowSpaceTimeComplexity = ((App)Application.Current).Preference.AnalysisPreferences.LogicalSolverIgnoresHighAllocationAlgorithms;

		var solver = ((App)Application.Current)
			.Analyzer
			.WithStepSearchers(
				(
					from data in ((App)Application.Current).Preference.StepSearcherOrdering.StepSearchersOrder
					where data.IsEnabled
					select data.CreateStepSearchers() into stepSearchers
					from s in stepSearchers
					let highTimeComplexity = s.IsConfiguredSlow
					let highSpaceComplexity = s.IsConfiguredHighAllocation
					where !highTimeComplexity || highTimeComplexity && !disallowHighTimeComplexity
						|| !highSpaceComplexity || highSpaceComplexity && !disallowSpaceTimeComplexity
					select s
				).ToArray()
			);

		var analyzerResult = await Task.Run(analyze);

		AnalyzeButton.IsEnabled = true;
		IsAnalyzerLaunched = false;

		switch (analyzerResult)
		{
			case { IsSolved: true }:
			{
				UpdateAnalysisResult(analyzerResult);
				AnalysisResultCache = analyzerResult;

				break;
			}
			case
			{
				WrongStep: { Views: var views, Conclusions: var conclusions } wrongStep,
				FailedReason: AnalyzerFailedReason.WrongStep,
				UnhandledException: WrongStepException { CurrentInvalidGrid: var invalidGrid }
			}:
			{
				await new ContentDialog
				{
					XamlRoot = XamlRoot,
					Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]!,
					Title = GetString("AnalyzePage_ErrorStepEncounteredTitle"),
					CloseButtonText = GetString("AnalyzePage_ErrorStepDialogCloseButtonText"),
					DefaultButton = ContentDialogButton.Close,
					Content = new ErrorStepDialogContent
					{
						ErrorStepGrid = invalidGrid,
						ErrorStepText = string.Format(GetString("AnalyzePage_ErrorStepDescription"), wrongStep),
						ViewUnit = new() { View = views?[0] ?? View.Empty, Conclusions = conclusions }
					}
				}.ShowAsync();

				break;
			}
			case { FailedReason: AnalyzerFailedReason.ExceptionThrown, UnhandledException: { } ex }:
			{
				await new ContentDialog
				{
					XamlRoot = XamlRoot,
					Style = (Style)Application.Current.Resources["DefaultContentDialogStyle"]!,
					Title = GetString("AnalyzePage_ExceptionThrownTitle"),
					CloseButtonText = GetString("AnalyzePage_ErrorStepDialogCloseButtonText"),
					DefaultButton = ContentDialogButton.Close,
					Content = new ExceptionThrownOnAnalyzingContent { ThrownException = ex }
				}.ShowAsync();

				break;
			}
		}


		AnalyzerResult analyze()
		{
			lock (StepSearchingOrGatheringSyncRoot)
			{
				return solver.Analyze(
					puzzle,
					new Progress<double>(
						percent => DispatcherQueue.TryEnqueue(
							() =>
							{
								ProgressPercent = percent * 100;
								AnalyzeProgressLabel.Text = string.Format(textFormat, percent);
							}
						)
					)
				);
			}
		}
	}

	private void SudokuPane_Clicked(SudokuPane sender, GridClickedEventArgs e)
	{
		if (e.MouseButton is MouseButton.Right or MouseButton.Middle)
		{
			return;
		}

		if (_localView is null)
		{
			return;
		}

		var shouldClearValue = this switch
		{
			{ SelectedMode: DrawingMode.Cell, SelectedColorIndex: var index and not -1 } => CheckCellNode(index, e, _localView),
			{ SelectedMode: DrawingMode.Candidate, SelectedColorIndex: var index and not -1 } => CheckCandidateNode(index, e, _localView),
			{ SelectedMode: DrawingMode.House, SelectedColorIndex: var index and not -1 } => CheckHouseNode(index, e, _localView),
			{ SelectedMode: DrawingMode.Chute, SelectedColorIndex: var index and not -1 } => CheckChuteNode(index, e, _localView),
			{ SelectedMode: DrawingMode.Link } => CheckLinkNode(e, _localView),
			{ SelectedMode: DrawingMode.BabaGrouping, SelectedColorIndex: var index and not -1 } => CheckBabaGroupingNode(index, e, _localView),
			_ => true
		};
		if (shouldClearValue)
		{
			_previousSelectedCandidate = null;
		}
	}
}
