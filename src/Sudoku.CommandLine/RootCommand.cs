﻿namespace System.CommandLine;

/// <summary>
/// Provides with the entry for the parsing.
/// </summary>
internal static class RootCommand
{
	/// <summary>
	/// Routes the command line arguments to the specified <see cref="IRootCommand"/> instances.
	/// </summary>
	/// <param name="args">The arguments.</param>
	/// <exception cref="CommandLineParserException">
	/// Throws when the command line arguments is <see langword="null"/> or empty currently,
	/// or the command name is invalid, or the command line arguments is empty.
	/// </exception>
	/// <exception cref="CommandLineException">Throws when an error has been encountered.</exception>
	public static void Route(string[] args)
	{
		if (args is not [var rootCommand, ..])
		{
			throw new CommandLineParserException(ParserError.ArgumentIsEmpty);
		}

		bool e(string s) => s.Equals(rootCommand, StringComparison.OrdinalIgnoreCase);
		const BindingFlags staticProp = BindingFlags.Public | BindingFlags.Static;
		var type = (
			from t in typeof(Program).Assembly.GetTypes()
			where t.IsAssignableTo(typeof(IRootCommand)) && t is { IsClass: true, IsAbstract: false }
			let parameterlessConstructorInfo = t.GetConstructor(Array.Empty<Type>())
			where parameterlessConstructorInfo is not null
			let propertyInfo = t.GetProperty(nameof(IRootCommand.SupportedCommands), staticProp)
			where propertyInfo is { CanRead: true, CanWrite: false }
			let propertyValue = propertyInfo.GetValue(null) as string[]
			where propertyValue is not null && propertyValue.Any(e)
			select t
		).Single();

		var rootCommandInstance = (IRootCommand)Activator.CreateInstance(type)!;
		Parser.ParseAndApplyTo(args, rootCommandInstance);
		rootCommandInstance.Execute();
	}
}
