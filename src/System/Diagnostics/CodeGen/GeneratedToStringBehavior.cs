﻿namespace System.Diagnostics.CodeGen;

/// <summary>
/// Defines a kind of behavior describing source generator's generated source code on overridding <see cref="object.ToString"/>.
/// </summary>
/// <seealso cref="object.ToString"/>
public enum GeneratedToStringBehavior
{
	/// <summary>
	/// Indicates the generated code will directly invoke <see cref="ISimpleFormattable.ToString(string?)"/> method,
	/// with <see langword="null"/> as target argument.
	/// </summary>
	/// <remarks>
	/// The generated code will be like:
	/// <code><![CDATA[
	/// // <auto-generated/>
	/// 
	/// partial struct Type
	/// {
	///     /// <inheritdoc cref="object.ToString"/>
	///     [CompilerGenerated]
	///     [GeneratedCode("SourceGeneratorName", "1.0.0.0")]
	///     public override readonly partial string ToString() => ToString((string?)null);
	/// }
	/// ]]></code>
	/// </remarks>
	/// <seealso cref="ISimpleFormattable.ToString(string?)"/>
	CallOverloadWithNull,

	/// <summary>
	/// Indicates the generated code will directly returns the expression constructed in the target member.
	/// </summary>
	/// <remarks>
	/// The generated code will be like:
	/// <code><![CDATA[
	/// // <auto-generated/>
	/// 
	/// partial struct Type
	/// {
	///     /// <inheritdoc cref="object.ToString"/>
	///     [CompilerGenerated]
	///     [GeneratedCode("SourceGeneratorName", "1.0.0.0")]
	///     public override readonly partial string ToString() => _field.ToString();
	///     
	///     // Suppose the type contains a field named '_field':
	///     // private readonly int _field = 42;
	/// }
	/// ]]></code>
	/// </remarks>
	SimpleMember,

	/// <summary>
	/// Indicates the generated code will output record-like <c>ToString</c> result.
	/// </summary>
	/// <remarks>
	/// The generated code will be like:
	/// <code><![CDATA[
	/// // <auto-generated/>
	/// 
	/// partial struct Type
	/// {
	///     /// <inheritdoc cref="object.ToString"/>
	///     [CompilerGenerated]
	///     [GeneratedCode("SourceGeneratorName", "1.0.0.0")]
	///     public override readonly partial string ToString()
	///         => $$"""{{nameof(Type)}} { {{nameof(_field)}} = {{_field}}, {{nameof(Property)}} = {{Property}} }""";
	/// }
	/// ]]></code>
	/// </remarks>
	RecordLike
}
