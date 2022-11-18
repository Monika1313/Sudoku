﻿namespace Sudoku.Communication.Qicq.Commands;

/// <summary>
/// Indicates the ranking command.
/// </summary>
[Command(Permissions.Owner, Permissions.Administrator)]
internal sealed class RankingCommand : Command
{
	/// <inheritdoc/>
	public override string CommandName => R["_Command_Ranking"]!;

	/// <inheritdoc/>
	public override CommandComparison ComparisonMode => CommandComparison.Prefix;


	/// <inheritdoc/>
	protected override async Task<bool> ExecuteCoreAsync(string args, GroupMessageReceiver e)
	{
		if (e is not { Sender.Group: var group })
		{
			return false;
		}

		var folder = Environment.GetFolderPath(SpecialFolder.MyDocuments);
		if (!Directory.Exists(folder))
		{
			// Error. The computer does not contain "My Documents" folder.
			// This folder is special; if the computer does not contain the folder, we should return directly.
			return true;
		}

		var botDataFolder = $"""{folder}\{R["BotSettingsFolderName"]}""";
		if (!Directory.Exists(botDataFolder))
		{
			await e.SendMessageAsync(R["_MessageFormat_RankingListIsEmpty"]!);
			return true;
		}

		var botUsersDataFolder = $"""{botDataFolder}\{R["UserSettingsFolderName"]}""";
		if (!Directory.Exists(botUsersDataFolder))
		{
			await e.SendMessageAsync(R["_MessageFormat_RankingListIsEmpty"]!);
			return true;
		}

		// If the number of members are too large, we should only iterate top 10 elements.
		var usersData = (
			from file in Directory.GetFiles(botUsersDataFolder, "*.json")
			let ud = Deserialize<UserData>(File.ReadAllText(file))
			where ud is not null
			let qq = ud.QQ
			let nickname = @group.GetMemberFromQQAsync(qq).Result?.Name
			where nickname is not null
			let numericQQ = int.TryParse(qq, out var result) ? result : 0
			orderby ud.Score descending, numericQQ
			select (Name: nickname, Data: ud)
		).Take(10);

		var rankingStr = string.Join(
			"\r\n",
			usersData.Select(
				static (pair, i) =>
				{
					if (pair is not (var name, { QQ: var qq, Score: var score }))
					{
						throw new();
					}

					var openBrace = R["_Token_OpenBrace"]!;
					var closedBrace = R["_Token_ClosedBrace"]!;
					var colon = R["_Token_Colon"]!;
					var comma = R["_Token_Comma"]!;
					var scoreSuffix = R["ExpString"]!;
					return $"#{i + 1}{colon}{name}{openBrace}{qq}{closedBrace}{comma}{score} {scoreSuffix}";
				}
			)
		);

		await e.SendMessageAsync(
			$"""
			{R["_MessageFormat_RankingResult"]}
			---
			{rankingStr}
			"""
		);
		return true;
	}
}