﻿namespace Sudoku.UI.Views.Pages;

/// <summary>
/// A page that can be used on its own or navigated to within a <see cref="Frame"/>.
/// </summary>
/// <seealso cref="Frame"/>
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
				Get("SettingsPage_GroupItemName_Basic"),
				Get("SettingsPage_GroupItemDescription_Basic"),
				new[]
				{
					new ToggleSwitchSettingItem(
						Get("SettingsPage_ItemName_ShowCandidates"),
						nameof(UserPreference.ShowCandidates)
					),
					new ToggleSwitchSettingItem(
						Get("SettingsPage_ItemName_ShowCandidateBorderLines"),
						nameof(UserPreference.ShowCandidateBorderLines)
					),
					new ToggleSwitchSettingItem(
						Get("SettingsPage_ItemName_EnableDeltaValuesDisplaying"),
						Get("SettingsPage_ItemDescription_EnableDeltaValuesDisplaying"),
						nameof(UserPreference.EnableDeltaValuesDisplaying)
					)
				}
			),
			new(Get("SettingsPage_GroupItemName_Solving"), Get("SettingsPage_GroupItemDescription_Solving")),
			new(
				Get("SettingsPage_GroupItemName_Rendering"),
				Get("SettingsPage_GroupItemDescription_Rendering"),
				new SettingItem[]
				{
					new SliderSettingItem(
						Get("SettingsPage_ItemName_OutsideBorderWidth"),
						Get("SettingsPage_ItemDescription_OutsideBorderWidth"),
						nameof(UserPreference.OutsideBorderWidth),
						stepFrequency: .1,
						tickFrequency: .3,
						minValue: 0,
						maxValue: 3
					),
					new SliderSettingItem(
						Get("SettingsPage_ItemName_BlockBorderWidth"),
						nameof(UserPreference.BlockBorderWidth),
						stepFrequency: .5,
						tickFrequency: .5,
						minValue: 0,
						maxValue: 5
					),
					new SliderSettingItem(
						Get("SettingsPage_ItemName_CellBorderWidth"),
						nameof(UserPreference.CellBorderWidth),
						stepFrequency: .5,
						tickFrequency: .5,
						minValue: 0,
						maxValue: 5
					),
					new SliderSettingItem(
						Get("SettingsPage_ItemName_CandidateBorderWidth"),
						Get("SettingsPage_ItemDescription_CandidateBorderWidth"),
						nameof(UserPreference.CandidateBorderWidth),
						stepFrequency: .1,
						tickFrequency: .3,
						minValue: 0,
						maxValue: 3
					),
					new ColorPickerSettingItem(
						Get("SettingsPage_ItemName_OutsideBorderColor"),
						nameof(UserPreference.OutsideBorderColor)
					)
				}
			),
			new(
				Get("SettingsPage_GroupItemName_Miscellaneous"),
				Get("SettingsPage_GroupItemDescription_Miscellaneous"),
				new[]
				{
					new ToggleSwitchSettingItem(
						Get("SettingsPage_ItemName_DescendingOrderedInfoBarBoard"),
						nameof(UserPreference.DescendingOrderedInfoBarBoard)
					)
				}
			)
		};
}
