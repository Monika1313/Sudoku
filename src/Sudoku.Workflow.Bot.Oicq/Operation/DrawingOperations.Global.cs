namespace Sudoku.Workflow.Bot.Oicq.Operation;

partial class DrawingOperations
{
	/// <summary>
	/// 用于 <see cref="string.Split(char[], StringSplitOptions)"/> 操作的枚举常量值，
	/// 表示既将空白字符片段舍弃（对应 <see cref="StringSplitOptions.RemoveEmptyEntries"/>），
	/// 又将其它满足条件的片段的头尾空白字符自动去除（对应 <see cref="StringSplitOptions.TrimEntries"/>）。
	/// </summary>
	/// <seealso cref="string.Split(char[], StringSplitOptions)"/>
	/// <seealso cref="StringSplitOptions.RemoveEmptyEntries"/>
	/// <seealso cref="StringSplitOptions.TrimEntries"/>
	private const StringSplitOptions SplitOptionsBoth = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;


	/// <summary>
	/// 字符串的分割字符，用于绘图操作输入一系列数据的时候使用。
	/// </summary>
	internal static readonly char[] Separators = { ',', '，', '、' };

	/// <summary>
	/// 用于快捷抛出的 <see cref="InvalidOperationException"/> 异常实例。
	/// </summary>
	private static readonly InvalidOperationException DefaultException = new("Operation failed due to internal exception.");


	/// <summary>
	/// 直接清空整个绘图操作里已经设置了的视图节点，恢复为空盘面。
	/// </summary>
	public static async partial Task ClearAsync(GroupMessageReceiver receiver, BotRunningContext context)
	{
		var painter = ISudokuPainter.Create(1000, 20)
			.WithRenderingCandidates(true)
			.WithPreferenceSettings(
				static pref =>
				{
					pref.CandidateScale = .4M;
					pref.BattenburgSize = 42.666668F;
					pref.BorderBarWidth = 10F;
					pref.CellCornerArrowWidth = 20F;
					pref.CellCornerTriangleSize = 30F;
					pref.ClockfaceDotBorderWidth = 4F;
					pref.ClockfaceDotSize = 30F;
					pref.EmbeddedSkyscraperArrowFont = new("Times New Roman", 24F, FontStyle.Bold);
					pref.GreaterThanSignFont = new("Consolas", 28F, FontStyle.Bold);
					pref.KropkiDotBorderWidth = 4F;
					pref.KropkiDotSize = 30F;
					pref.NeighborSignCellPadding = 10F;
					pref.NeighborSignsWidth = 6F;
					pref.NumberLabelFont = new("Consolas", 28F, FontStyle.Bold);
					pref.PencilmarkFont = new("Segoe UI", 24F, FontStyle.Regular);
					pref.QuadrupleMaxArrowSize = 16F;
					pref.WheelFont = new("MiSans", 24F, FontStyle.Regular);
					pref.XvSignFont = new("Consolas", 28F, FontStyle.Bold);
				}
			)
			.WithGrid(Grid.Undefined);
		context.DrawingContext = new() { Painter = painter };

		// 利用前面创建的 painter 对象，直接产生图片并发送。这里的 painter 是初始化的结果，所以发送出去的图片也是空盘，没有标记，没有候选数，啥都没有。
		// 注意，这个指令到这里就结束了。后面没有代码了。
		// 因为这里随后的操作是艾特机器人，在别的指令里完成，这里我们不处理它（实际上也处理不了，因为 API 是这么设计的）。
		await receiver.SendPictureThenDeleteAsync(painter);
	}

	/// <summary>
	/// 快捷调用 <see cref="string.Split(char[], StringSplitOptions)"/> 操作。
	/// </summary>
	/// <param name="this">需要处理的字符串。</param>
	/// <returns>将字符串进行分割之后的数组序列，去掉了头尾的空白字符，也舍弃掉了只有空白字符的匹配项。</returns>
	/// <seealso cref="string.Split(char[], StringSplitOptions)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static string[] LocalSplit(this string @this) => @this.Split(Separators, SplitOptionsBoth);

	/// <summary>
	/// 将前文传入的两个字符信息直接转为“行”和“列”两个数值，然后直接解析成合适的单元格坐标。
	/// </summary>
	/// <param name="r">一个字符，表示一个所在行的数据。</param>
	/// <param name="c">一个字符，表示一个所在列的数据。</param>
	/// <returns>单元格的索引。范围为 0 到 80。</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Cell GetCellIndex(char r, char c) => (r - '1') * 9 + (c - '1');

	/// <summary>
	/// 将前文传入的三个字符信息直接转为“行”、“列”和“数”三个数值，然后将其解析成合适的单元格坐标和数，以二元组（数对）的形式返回。
	/// </summary>
	/// <param name="r">一个字符，表示一个所在行的数据。</param>
	/// <param name="c">一个字符，表示一个所在列的数据。</param>
	/// <param name="d">一个字符，表示一个数值数据。</param>
	/// <returns>一个二元组（数对），其中一个元素是单元格，第二个元素是数值。</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static (Cell Cell, Digit Digit) GetCandidateIndex(char r, char c, char d) => (GetCellIndex(r, c), d - '1');

	/// <summary>
	/// 将前文传入的两个字符信息直接转为“区域类型名称”和“索引”两个数值，并表达成区域绝对索引值。区域从 0 到 26 编号，分别表示 27 个不同的区域类型
	/// （宫 1-9、行 1-9、列 1-9）。由于 API 基本设计架构，宫被优先计算，所以 0-8 是宫的编号，请尤其注意。
	/// </summary>
	/// <param name="r">表示区域类型的名称标签。支持的值可以是行列宫的基本中文汉字，或是表示同名字的 R、C、B 字母（大小写均可）。</param>
	/// <param name="i">索引。表示属于该区域类型 <paramref name="r"/> 的顺数第几个。比如“行3”就表示第 3 行。</param>
	/// <returns>返回一个绝对索引，取值范围在 0 到 26 之间，包含边界。</returns>
	/// <exception cref="ArgumentException">
	/// 如果 <paramref name="r"/> 输入的数值不合法，就会产生此异常；<paramref name="i"/> 我们这里没有给出处理，因为在调用方基本就处理过了。
	/// 这也是为什么我把这个方法定义为 <see langword="private"/> 的原因。
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static House GetHouseIndex(char r, char i)
		=> r switch
		{
			'行' or 'R' or 'r' => 9,
			'列' or 'C' or 'c' => 18,
			'宫' or 'B' or 'b' => 0,
			_ => throw new ArgumentException("The specified value is invalid.", nameof(r))
		} + (i - '1');

	/// <summary>
	/// 将前文传入的两个字符信息直接转为“大行列类型名称”和“索引”两个数值，并表达成大行类的绝对索引值。大行列从 0 到 5 编号，分别表示 6 个不同的大行列类型
	/// （大行 1-3、大列 1-3）。
	/// </summary>
	/// <param name="r">表示大行列类型的名称标签。支持的值可以是中文汉字“行”和“列”，也可以是表示区域的字母。</param>
	/// <param name="i">索引。表示属于该大行列类型 <paramref name="r"/> 的顺数第几个。比如“大行2”就表示第 2 个大行。</param>
	/// <returns>返回一个绝对索引，取值范围在 0 到 5 之间，包含边界。</returns>
	/// <exception cref="ArgumentException">
	/// <inheritdoc cref="GetHouseIndex(char, char)" path="/exception[@cref='ArgumentException']"/>
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetChuteIndex(char r, char i)
		=> r switch
		{
			'行' or 'R' or 'r' => 0,
			'列' or 'C' or 'c' => 3,
			_ => throw new ArgumentException("The specified value is invalid.", nameof(r))
		} + (i - '1');

	/// <summary>
	/// 通过一个颜色表达出来的字符串，转换为一个实体的 <see cref="ColorIdentifier"/> 类型的实例，用于绘图。
	/// </summary>
	/// <param name="this">颜色字符串。</param>
	/// <returns>一个 <see cref="ColorIdentifier"/> 实例，表示颜色的标识符。</returns>
	/// <remarks>
	/// 该方法支持的输入可以是基本的配色表（配色 #1 到 #15），也可以是 RGB 和 ARGB 的代码。
	/// </remarks>
	/// <seealso cref="ColorIdentifier"/>
	/// <seealso href="https://source.dot.net/#System.Private.CoreLib/src/libraries/Common/src/System/HexConverter.cs,2094caa70f8308bc">
	/// char.IsAsciiHexDigit 方法
	/// </seealso>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ColorIdentifier? GetIdentifier(this string @this)
		=> @this switch
		{
			[>= '1' and <= '9'] or ['1', >= '0' and <= '5'] when int.TryParse(@this, out var v) && v is > 0 and <= 15 => v - 1,
			{ Length: 6 or 8 } when @this.All(char.IsAsciiHexDigit) => ColorTranslator.FromHtml(@this).ToIdentifier(),
			_ => null
		};

	/// <summary>
	/// 使用参数 <paramref name="receiver"/> 创建缓存图片，并发送。
	/// </summary>
	/// <param name="receiver"><see cref="GroupMessageReceiver"/> 实例。</param>
	/// <param name="context">绘图使用的上下文。</param>
	/// <param name="raw">原始数据（字符串）。</param>
	/// <param name="isAdding">表示是否最终是追加到集合之中。</param>
	/// <param name="nodeCreator">一个方法，表示创建一个 <see cref="ViewNode"/> 的实例。</param>
	/// <param name="cellPredicate">一个单元格是否满足条件的判别方法。</param>
	/// <returns>一个 <see cref="Task"/> 类型的实例，表示异步操作期间的基本数据。</returns>
	private static async Task GeneratePictureAsync(
		GroupMessageReceiver receiver,
		DrawingContext context,
		string raw,
		bool isAdding,
		Func<Cell, ViewNode> nodeCreator,
		Func<Cell, bool>? cellPredicate
	)
	{
		var nodes = new HashSet<ViewNode>();
		foreach (var element in raw.LocalSplit())
		{
			if (element is not [var r and >= '1' and <= '9', var c and >= '1' and <= '9'])
			{
				continue;
			}

			var cell = GetCellIndex(r, c);
			if (cellPredicate is not null && !cellPredicate(cell))
			{
				continue;
			}

			nodes.Add(nodeCreator(cell));
		}

		await receiver.SendPictureThenDeleteAsync(isAdding ? context.Painter.AddNodes(nodes) : context.Painter.RemoveNodes(nodes));
	}
}
