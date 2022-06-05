﻿namespace System.Runtime.CompilerServices;

/// <summary>
/// Provides an unsafe way to handle references.
/// </summary>
public static class ReferenceMarshal
{
	/// <summary>
	/// Throws an exception of type <typeparamref name="TException"/>.
	/// </summary>
	/// <typeparam name="T">The type of the reference value.</typeparam>
	/// <typeparam name="TException">The exception type.</typeparam>
	/// <returns>The return value is a discard.</returns>
	/// <exception cref="Exception">Always throws.</exception>
	[DoesNotReturn]
	public static ref T RefThrow<T, TException>() where TException : Exception, new() => throw new TException();
}
