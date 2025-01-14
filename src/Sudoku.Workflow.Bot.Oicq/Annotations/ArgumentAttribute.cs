namespace Sudoku.Workflow.Bot.Oicq.Annotations;

/// <summary>
/// 提供一个特性，用来表示属性作为命令行解析的参数信息。
/// </summary>
/// <param name="name">表示该参数的名称，即该参数序列最左边这个明确下来的数值。</param>
/// <param name="requiredValuesCount">表示该指令模块的参数需要多少个子参数信息。</param>
public abstract partial class ArgumentAttribute(
	[PrimaryConstructorParameter] string name,
	[PrimaryConstructorParameter(Accessibility = "protected")] int requiredValuesCount
) : CommandAnnotationAttribute;
