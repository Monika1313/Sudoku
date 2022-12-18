﻿namespace Sudoku.Platforms.QQ.Scheduler;

/// <summary>
/// Defines a scheduled service that can stores a list of <see cref="PeriodicOperation"/> instances.
/// </summary>
/// <seealso cref="PeriodicOperation"/>
public sealed class PeriodicOperationPool
{
	/// <summary>
	/// The internal singleton field.
	/// </summary>
	private static PeriodicOperationPool? _instance;


	/// <summary>
	/// The task timers.
	/// </summary>
	private readonly List<Timer> _timers = new();


	/// <summary>
	/// Initializes a <see cref="PeriodicOperationPool"/> instance.
	/// </summary>
	private PeriodicOperationPool()
	{
	}


	/// <summary>
	/// Indicates the singleton instance.
	/// </summary>
	public static PeriodicOperationPool Instance => _instance ??= new PeriodicOperationPool();


	/// <summary>
	/// Try to stack a new scheduled task.
	/// </summary>
	/// <param name="time">The time that the operation will be executed at.</param>
	/// <param name="action">The task.</param>
	public void Add(TimeOnly time, Action action)
	{
		var now = DateTime.Now;
		var firstRun = new DateTime(now.Year, now.Month, now.Day, time.Hour, time.Minute, time.Second, time.Millisecond);
		if (now > firstRun)
		{
			firstRun = firstRun.AddDays(1);
		}

		var timeToGo = firstRun - now;
		if (timeToGo <= TimeSpan.Zero)
		{
			timeToGo = TimeSpan.Zero;
		}

		_timers.Add(new(_ => action(), null, timeToGo, TimeSpan.FromDays(1)));
	}

	/// <summary>
	/// Try to stack a new scheduled task.
	/// </summary>
	/// <param name="operation">The periodic operation instance.</param>
	public void Add(PeriodicOperation operation) => Add(operation.TriggeringTime, async () => await operation.ExecuteAsync());

	/// <summary>
	/// Try to stack a list of new scheduled tasks.
	/// </summary>
	/// <param name="operations">The periodic operation instances.</param>
	public void AddRange(IEnumerable<PeriodicOperation> operations)
	{
		foreach (var operation in operations)
		{
			Add(operation);
		}
	}
}
