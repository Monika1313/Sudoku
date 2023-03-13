﻿namespace Sudoku.Platforms.QQ.Modules.Group;

[GroupModule("排名")]
[RequiredRole(SenderRole = GroupRoleKind.God)]
file sealed class RankingModule : GroupModule
{
	/// <inheritdoc/>
	protected override async Task ExecuteCoreAsync(GroupMessageReceiver messageReceiver)
	{
		if (messageReceiver is not { Sender.Group: var group })
		{
			return;
		}

		// If the number of members are too large, we should only iterate the specified number of elements from top.
		var context = BotRunningContext.GetContext(group);
		var usersData = (await Scorer.GetUserRankingListAsync(group, async () => await messageReceiver.SendMessageAsync("群用户列表为空。")))!.Take(10);

		await messageReceiver.SendMessageAsync(
			$"""
			用户排名：
			{string.Join(
				Environment.NewLine,
				usersData.Select(
					static (pair, i) =>
					{
						var name = pair.Name;
						var qq = pair.Data.QQ;
						var score = pair.Data.ExperiencePoint;
						var coin = pair.Data.Coin;
						var grade = Scorer.GetGrade(score);
						return $"#{i + 1,2} {name}（{qq}） 🚩{score} 💴{coin} 🏅{grade}";
					}
				)
			)}
			---
			排名最多仅列举本群前十名的成绩；想要精确查看用户名次请使用“查询”指令。
			"""
		);
	}
}
