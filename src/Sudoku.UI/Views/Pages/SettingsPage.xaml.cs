﻿namespace Sudoku.UI.Views.Pages;

/// <summary>
/// A page that can be used on its own or navigated to within a <see cref="Frame"/>.
/// </summary>
/// <seealso cref="Frame"/>
[Page]
public sealed partial class SettingsPage : Page
{
	/// <summary>
	/// Indicates the backing list of <see cref="SettingGroupItem"/>s.
	/// </summary>
	private IList<SettingGroupItem> _settingGroupItems;


	/// <summary>
	/// Initializes a <see cref="SettingsPage"/> instance.
	/// </summary>
	public SettingsPage()
	{
		InitializeComponent();

		InitializeSettingGroupItems();
	}


	/// <summary>
	/// Initializes the field <see cref="_settingGroupItems"/>.
	/// </summary>
	/// <seealso cref="_settingGroupItems"/>
	[MemberNotNull(nameof(_settingGroupItems))]
	private void InitializeSettingGroupItems()
		=> _settingGroupItems = new SettingGroupItem[]
		{
			new(
				R["SettingsPage_GroupItemName_Basic"]!,
				R["SettingsPage_GroupItemDescription_Basic"]!,
				new SettingItem[]
				{
					new ToggleSwitchSettingItem(
						R["SettingsPage_ItemName_ShowCandidates"]!,
						nameof(UserPreference.ShowCandidates)
					),
					new ToggleSwitchSettingItem(
						R["SettingsPage_ItemName_ShowCandidateBorderLines"]!,
						nameof(UserPreference.ShowCandidateBorderLines)
					),
					new ToggleSwitchSettingItem(
						R["SettingsPage_ItemName_EnableDeltaValuesDisplaying"]!,
						R["SettingsPage_ItemDescription_EnableDeltaValuesDisplaying"]!,
						nameof(UserPreference.EnableDeltaValuesDisplaying)
					),
					new FontPickerSettingItem(
						R["SettingsPage_ItemName_ValueFontScale"]!,
						R["SettingsPage_ItemDescription_ValueFontScale"]!,
						nameof(UserPreference.ValueFontName)
					)
					{
						FontScalePropertyName = nameof(UserPreference.ValueFontScale)
					},
					new FontPickerSettingItem(
						R["SettingsPage_ItemName_CandidateFontScale"]!,
						R["SettingsPage_ItemDescription_CandidateFontScale"]!,
						nameof(UserPreference.CandidateFontName)
					)
					{
						FontScalePropertyName = nameof(UserPreference.CandidateFontScale)
					}
				}
			),
			new(R["SettingsPage_GroupItemName_Solving"]!, R["SettingsPage_GroupItemDescription_Solving"]!),
			new(
				R["SettingsPage_GroupItemName_Rendering"]!,
				R["SettingsPage_GroupItemDescription_Rendering"]!,
				new SettingItem[]
				{
					new SliderSettingItem(
						R["SettingsPage_ItemName_OutsideBorderWidth"]!,
						R["SettingsPage_ItemDescription_OutsideBorderWidth"]!,
						nameof(UserPreference.OutsideBorderWidth),
						stepFrequency: .1,
						tickFrequency: .3,
						minValue: 0,
						maxValue: 3
					),
					new SliderSettingItem(
						R["SettingsPage_ItemName_BlockBorderWidth"]!,
						nameof(UserPreference.BlockBorderWidth),
						stepFrequency: .5,
						tickFrequency: .5,
						minValue: 0,
						maxValue: 5
					),
					new SliderSettingItem(
						R["SettingsPage_ItemName_CellBorderWidth"]!,
						nameof(UserPreference.CellBorderWidth),
						stepFrequency: .5,
						tickFrequency: .5,
						minValue: 0,
						maxValue: 5
					),
					new SliderSettingItem(
						R["SettingsPage_ItemName_CandidateBorderWidth"]!,
						R["SettingsPage_ItemDescription_CandidateBorderWidth"]!,
						nameof(UserPreference.CandidateBorderWidth),
						stepFrequency: .1,
						tickFrequency: .3,
						minValue: 0,
						maxValue: 3
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_OutsideBorderColor"]!,
						nameof(UserPreference.OutsideBorderColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_BlockBorderColor"]!,
						nameof(UserPreference.BlockBorderColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_CellBorderColor"]!,
						nameof(UserPreference.CellBorderColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_CandidateBorderColor"]!,
						R["SettingsPage_ItemDescription_CandidateBorderColor"]!,
						nameof(UserPreference.CandidateBorderColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_GivenColor"]!,
						nameof(UserPreference.GivenColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_ModifiableColor"]!,
						nameof(UserPreference.ModifiableColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_CandidateColor"]!,
						nameof(UserPreference.CandidateColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_CellDeltaColor"]!,
						R["SettingsPage_ItemDescription_CellDeltaColor"]!,
						nameof(UserPreference.CellDeltaColor)
					),
					new ColorPickerSettingItem(
						R["SettingsPage_ItemName_CandidateDeltaColor"]!,
						R["SettingsPage_ItemDescription_CandidateDeltaColor"]!,
						nameof(UserPreference.CandidateDeltaColor)
					)
				}
			),
			new(
				R["SettingsPage_GroupItemName_Miscellaneous"]!,
				R["SettingsPage_GroupItemDescription_Miscellaneous"]!,
				new[]
				{
					new ToggleSwitchSettingItem(
						R["SettingsPage_ItemName_DescendingOrderedInfoBarBoard"]!,
						nameof(UserPreference.DescendingOrderedInfoBarBoard)
					)
				}
			)
		};

	/// <summary>
	/// Creates a <see cref="ContentDialog"/> instance.
	/// </summary>
	/// <param name="title">The title.</param>
	/// <param name="message">The message.</param>
	/// <returns>The result instance.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ContentDialog CreateErrorDialog(string title, string message)
		=> new()
		{
			XamlRoot = XamlRoot,
			Title = title,
			Content = message,
			CloseButtonText = R["Close"],
			DefaultButton = ContentDialogButton.Close
		};

	/// <summary>
	/// To backup a preference file.
	/// </summary>
	/// <returns>The task that handles the current operation.</returns>
	private async Task BackupPreferenceFileAsync()
	{
		var fsp = new FileSavePicker
		{
			SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
			SuggestedFileName = R["PreferenceBackup"]!
		};
		fsp.FileTypeChoices.Add(R["FileExtension_Configuration"], new List<string> { CommonFileExtensions.Configuration });
		fsp.AwareHandleOnWin32();

		if (await fsp.PickSaveFileAsync() is not { Name: var fileName } file)
		{
			return;
		}

		// Prevent updates to the remote version of the file until we finish making changes
		// and call CompleteUpdatesAsync.
		CachedFileManager.DeferUpdates(file);

		// Writes to the file.
		await FileIO.WriteTextAsync(
			file,
			JsonSerializer.Serialize(
				((App)Application.Current).UserPreference,
				new JsonSerializerOptions
				{
					WriteIndented = true,
					IncludeFields = true,
					IgnoreReadOnlyProperties = true,
					IgnoreReadOnlyFields = true,
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				}
			)
		);

		// Let Windows know that we're finished changing the file so the other app can update
		// the remote version of the file.
		// Completing updates may require Windows to ask for user input.
		if (await CachedFileManager.CompleteUpdatesAsync(file) == FileUpdateStatus.Complete)
		{
			return;
		}

		// Failed to backup.
		string a = R["SettingsPage_BackupPreferenceFailed1"]!;
		string b = R["SettingsPage_BackupPreferenceFailed2"]!;
		await CreateErrorDialog(R["Info"]!, $"{a}{fileName}{b}").ShowAsync();
	}


	/// <summary>
	/// Triggers when the "backup preference" button is clicked.
	/// </summary>
	/// <param name="sender">The object triggering the event.</param>
	/// <param name="e">The event arguments provided.</param>
	private async void BackupPreference_ClickAsync(object sender, RoutedEventArgs e) => await BackupPreferenceFileAsync();
}
