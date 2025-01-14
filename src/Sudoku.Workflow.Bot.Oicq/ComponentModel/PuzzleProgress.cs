namespace Sudoku.Workflow.Bot.Oicq.ComponentModel;

/// <summary>
/// 定义一个类型，包裹的数据是表示用户在某个群里的题库完成的临时产生数据。
/// </summary>
/// <remarks>
/// <para>
/// 按照设计，每一个题库都有一个不同的题库名，毕竟同一个群里的不同题库是放在同一个文件夹下的，按照文件存储的基本规则，
/// 文件名肯定是不能在同一个文件夹里存相同文件名但是内容不同的文件的。
/// </para>
/// <para>
/// 按照这种规则来看，我们将题库缓存数据，一个文件一个题库的数据，这样的方式来存储。比如说有两个题库是叫“a”和“b”，那么
/// 他们对应的缓存文件名就也叫“a”和“b”，不过后缀名不一样。题库本身是 <c>.txt</c> 格式的文件，但缓存是序列化文件，所以后缀名是
/// <c>.json</c>。因为后缀名不同，所以即使名字一致，也不会产生冲突，刚好也可以达到区分不同题库的效果。
/// </para>
/// <para>
/// 实际在使用机器人的时候经常会出现多个题库不同进度的情况。这种设计比较方便理解，也方便读写，不会造成同文件的复杂处理；
/// 而另外一种方案则是将所有题库的缓存数据都放同一个文件里，用数组或 <see cref="List{T}"/> 的方式进行序列化（或反序列化）。
/// 这种办法的好处就是同一个文件夹存储，方便临时改变数据等。但是不好的地方是，这要造成同一个文件的反复读写操作，不是很便于实现。
/// </para>
/// <para>因为使用了多文件分开存储不同题库的进度的关系，该数据下就不包含所谓“群名”、“题库名称”等属性数值。在调用的时候，就必然已经区分开了。</para>
/// <para>
/// 但是可能有人会问，一个属性就定义成一个数据类型，是不是不合理。其实是合理的，便于以后推广。可能以后会额外在这个类型里增加属性，所以先就定义下来。
/// </para>
/// </remarks>
public sealed class PuzzleProgress
{
	/// <summary>
	/// 表示题目本身。
	/// </summary>
	[JsonPropertyName("puzzle")]
	public required Grid Puzzle { get; set; }
}
