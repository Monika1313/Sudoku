﻿namespace Sudoku.Platforms.QQ.Modules;

/// <summary>
/// Defines a periodic module.
/// </summary>
/// <param name="TriggeringTime">
/// Indicates the <see cref="TimeOnly"/> instance that describes the time that the operation will be triggered daily.
/// </param>
public abstract record PeriodicModule(TimeOnly TriggeringTime)
{
	/// <summary>
	/// Try to execute the task.
	/// </summary>
	/// <returns>The task.</returns>
	public abstract Task ExecuteAsync();
}