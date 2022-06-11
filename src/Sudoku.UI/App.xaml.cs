﻿namespace Sudoku.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default <see cref="Application"/> class.
/// </summary>
/// <seealso cref="Application"/>
public partial class App : Application
{
	/// <summary>
	/// <para>Initializes the singleton application object.</para>
	/// <para>
	/// This is the first line of authored code executed,
	/// and as such is the logical equivalent of <c>main()</c> or <c>WinMain()</c>.
	/// </para>
	/// </summary>
	public App() => InitializeComponent();


	/// <summary>
	/// Indicates the initial information.
	/// </summary>
	internal WindowInitialInfo InitialInfo { get; } = new();

	/// <summary>
	/// Indicates the user preference instance.
	/// </summary>
	internal Preference UserPreference
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => InitialInfo.UserPreference;
	}


	/// <summary>
	/// <para>Invoked when the application is launched normally by the end user.</para>
	/// <para>
	/// Other entry points will be used such as when the application is launched to open a specific file.
	/// </para>
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(MsLaunchActivatedEventArgs args)
	{
		// Binds the resource fetcher on type 'MergedResources'.
		R.AddExternalResourceFetecher(GetType().Assembly, static key => Current.Resources[key] as string);

		// Handle and assign the initial value, to control the initial page information.
		(
			AppInstance.GetCurrent().GetActivatedEventArgs() switch
			{
				{
					Kind: ExtendedActivationKind.File,
					Data: IFileActivatedEventArgs { Files: [StorageFile { FileType: var fileType } file, ..] }
				} => fileType switch
				{
					CommonFileExtensions.Sudoku
						=> async i => i.FirstGrid = Grid.Parse(await FileIO.ReadTextAsync(file)),
					CommonFileExtensions.PreferenceBackup
						=> static i => i.FirstPageTypeName = nameof(SettingsPage),
					_ => default(Action<WindowInitialInfo>?)
				},
				_ => default
			}
		)?.Invoke(InitialInfo);

		// Activate the main window.
		(InitialInfo.MainWindow = new MainWindow()).Activate();
	}
}
