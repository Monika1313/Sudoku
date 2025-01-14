namespace Sudoku.Workflow.Bot.Oicq.RootCommands;

/// <summary>
/// 表示绘图指令。
/// </summary>
/// <remarks>
/// 该指令也是执行时间较长的指令，和“！开始游戏”类似。不过，具体的绘图操作都是在别的指令里完成的。这里只记录一下绘图的基本环境，以及反馈空盘的信息。
/// </remarks>
[Command("开始绘图")]
[RequiredUserLevel(30)]
internal sealed class DrawingCommand : Command
{
	/// <inheritdoc/>
	protected override async Task ExecuteCoreAsync(GroupMessageReceiver messageReceiver)
	{
		if (!RunningContexts.TryGetValue(messageReceiver.GroupId, out var context))
		{
			return;
		}

		// 优先设置环境，避免用户触发其他指令。
		context.ExecutingCommand = Name;

		// 列举基本操作指令。
		// 考虑到用户使用手机打字特别不方便，所以等号之类的、什么字母输入啥的，就不考虑了。
		// 为了简化一些操作，我们有时候还是适当“违背”数独的一些基本条例，比如 K9 和 RCB 的坐标表达。
		// 而 Hodoku 有一个约定俗成的表达规则，是用“dxy”的表达，它等价于 RxCy(d) 或者老外比较习惯的 dRxCy 的记号。
		// 但是这个写法的 d 是放开头的，也不太适合用户输入。经过一再简化，最后干脆就直接使用 xyd 的写法表示 RxCy(d) 了。
		await messageReceiver.SendMessageAsync(
			"""
			欢迎使用绘图功能！请艾特机器人，发送如下指令的其中一个就可以开始各种绘图操作了。
			---
			基本命令：
			@机器人 清空：清空所有的绘图内容
			@机器人 添加 132：往 r1c3 添加候选数 2
			@机器人 反添加 132：将 r1c3（即 A3 格）里的候选数 2 去除
			@机器人 132：往 r1c3（即 A3 格）填入 2
			@机器人 130：将 r1c3（即 A3 格）的填数去掉
			---
			其他还有更多的指令，因为写在这里就太长了，所以请私下询问管理员、群主和程序设计者，
			以及等待机器人使用手册发布之后进行参考。
			"""
		);

		// QQ 的发送推送消息机制的设计，我们需要稍微延时来避免消息先后发送的顺序变动。
		// 有些时候后面这个图片消息在一定的情况下，会跑到前面文本消息之前，所以为了确保一定的顺序，这里我们故意让线程等待 200 毫秒，
		// 这样可以前面和后面有一定的时间差。
		// 注意，要用 await，不然就不等了。
		await Task.Delay(200);

		// 初始化盘面。
		await DrawingOperations.ClearAsync(messageReceiver, context);
	}
}
