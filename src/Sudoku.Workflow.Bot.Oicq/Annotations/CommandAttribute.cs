namespace Sudoku.Workflow.Bot.Oicq.Annotations;

/// <summary>
/// 提供一个特性，用于标记到指令模块的类型本身上面，表示该模块是启用状态。在运行期间会被反射给识别到。
/// </summary>
/// <param name="name">表示触发该指令的名称。</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed partial class CommandAttribute([PrimaryConstructorParameter] string name) : CommandAnnotationAttribute
{
	/// <summary>
	/// 实例化一个 <see cref="CommandAttribute"/> 类型的实例，无需名称。
	/// </summary>
	internal CommandAttribute() : this(null!)
	{
	}
}
