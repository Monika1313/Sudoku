﻿namespace Sudoku.Diagnostics.CodeGen;

/// <summary>
/// Provides with some commonly-used methods.
/// </summary>
internal static class XamlBinding
{
	public static string? GetDocumentationComment(string propertyName, DocumentationCommentData data, bool isDependencyProperty)
		=> (propertyName, data, isDependencyProperty) switch
		{
			(_, ({ } summary, { } remarks, _, _), _)
				=> $"""
				/// <summary>
					/// {summary}
					/// </summary>
					/// <remarks>
					/// {remarks}
					/// </remarks>
				""",
			(_, ({ } summary, _, _, _), _)
				=> $"""
				/// <summary>
					/// {summary}
					/// </summary>
				""",
			(_, (_, _, { } docCref, { } docPath), _)
				=> $"""
				/// <inheritdoc cref="{docCref}" path="{docPath}"/>
				""",
			(_, (_, _, { } docCref, _), _)
				=> $"""
				/// <inheritdoc cref="{docCref}"/>
				""",
			(_, _, true)
				=> $"""
				/// <summary>
					/// Indicates the interactive property that uses dependency property <see cref="{propertyName}Property"/> to get or set value.
					/// </summary>
					/// <seealso cref="{propertyName}Property" />
				""",
			(_, _, false)
				=> $"""
				/// <summary>
					/// Indicates the interactive setter or getter methods that uses attached property <see cref="{propertyName}Property"/> to get or set value.
					/// </summary>
					/// <seealso cref="{propertyName}Property" />
				"""
		};

	public static string? GetPropertyMetadataString(
		object? defaultValue,
		string? generatorMemberName,
		DefaultValueGeneratingMemberKind? generatorMemberKind,
		string? callbackMethodName,
		string? propertyTypeStr
	) => (defaultValue, generatorMemberName, generatorMemberKind, callbackMethodName) switch
	{
		(char c, _, _, null) => $"new('{c}')",
		(char c, _, _, _) => $"new('{c}', {callbackMethodName})",
		(string s, _, _, null) => $"""new("{s}")""",
		(string s, _, _, _) => $"""new("{s}", {callbackMethodName})""",
		(not null, _, _, null) => $"new({defaultValue.ToString().ToLower()})", // true -> "True"
		(not null, _, _, _) => $"new({defaultValue.ToString().ToLower()}, {callbackMethodName})", // true -> "True"
		(_, null, _, null) => $"new(default({propertyTypeStr}))",
		(_, null, _, _) => $"new(default({propertyTypeStr}), {callbackMethodName})",
		(_, not null, { } kind, _) => kind switch
		{
			DefaultValueGeneratingMemberKind.Field or DefaultValueGeneratingMemberKind.Property
				=> callbackMethodName switch
				{
					null => $"new({generatorMemberName})",
					_ => $"new({generatorMemberName}, {callbackMethodName})"
				},
			DefaultValueGeneratingMemberKind.ParameterlessMethod
				=> callbackMethodName switch
				{
					null => $"new({generatorMemberName}())",
					_ => $"new({generatorMemberName}(), {callbackMethodName})"
				},
			_ => null
		},
		_ => null
	};
}