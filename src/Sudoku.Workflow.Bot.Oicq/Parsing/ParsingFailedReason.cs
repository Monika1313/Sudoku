namespace Sudoku.Workflow.Bot.Oicq.Parsing;

/// <summary>
/// 一个本地枚举类型，表示一个在解析期间产生的错误信息。
/// </summary>
internal enum ParsingFailedReason
{
	/// <summary>
	/// 表示没有错误。
	/// </summary>
	None,

	/// <summary>
	/// 表示解析失败，因为目标属性没有找到。
	/// </summary>
	PropertyNotFound,

	/// <summary>
	/// 表示解析失败，因为目标属性缺少 <see langword="get"/> 或 <see langword="set"/> 方法的至少一个。
	/// </summary>
	PropertyMissingAccessor,

	/// <summary>
	/// 表示解析失败，因为目标属性是索引器（有参属性）。
	/// </summary>
	PropertyIsIndexer,

	/// <summary>
	/// 表示解析失败，因为目标属性不是 <see cref="string"/> 类型，却缺少 <see cref="ValueConverterAttribute{T}"/> 的转换指示情况。
	/// </summary>
	/// <seealso cref="ValueConverterAttribute{T}"/>
	PropertyMissingConverter,

	/// <summary>
	/// 表示解析失败，因为用户输入的结果在转换期间失败。比如某处要求输入整数，结果输入了别的无法转为整数数据的结果，例如字母。
	/// </summary>
	InvalidInput
}
