﻿namespace Sudoku.Platforms.QQ.Modules.Group;

[BuiltIn]
file sealed class CardUplevelingModule : GroupModule
{
#pragma warning disable CS0414
	private static readonly int CloverLevelDefaultValue = -1;
#pragma warning restore CS0414


	/// <inheritdoc/>
	public override string RaisingCommand => "强化";

	/// <summary>
	/// Indicates the clover level.
	/// </summary>
	[DoubleArgumentCommand("三叶草")]
	[DefaultValue(nameof(CloverLevelDefaultValue))]
	public int CloverLevel { get; set; }

	/// <summary>
	/// Indicates the auxiliary cards.
	/// </summary>
	[DoubleArgumentCommand("辅助")]
	[ValueConverter<NumericArrayConverter<int>>]
	public int[]? AuxiliaryCards { get; set; }


	/// <inheritdoc/>
	protected override async Task ExecuteCoreAsync(GroupMessageReceiver messageReceiver)
	{
		if (messageReceiver is not { Sender.Id: var senderId })
		{
			return;
		}

		if (InternalReadWrite.Read(senderId) is not { ExperiencePoint: var exp, CardLevel: var userCardLevel, Coin: var coin } user)
		{
			await messageReceiver.SendMessageAsync("很抱歉，你尚未使用过机器人。强化系统至少要求用户达到 25 级。");
			return;
		}

		if (Scorer.GetGrade(exp) < 25)
		{
			await messageReceiver.SendMessageAsync("很抱歉，强化系统至少要求用户达到 25 级。");
			return;
		}

		if (coin < 30)
		{
			await messageReceiver.SendMessageAsync("强化一次需消耗 30 金币。金币不足，无法强化。");
			return;
		}

		switch (this)
		{
			case { AuxiliaryCards: null }:
			{
				await messageReceiver.SendMessageAsync("参数有误。辅助卡至少需要一张。请给出强化等级数值。所有的辅助卡片，可使用“！查询”指令查询。");
				break;
			}
			case { AuxiliaryCards: { Length: > 3 } cards }:
			{
				await messageReceiver.SendMessageAsync("参数有误。强化系统最多只能上三张辅助卡。请检查参数输入，如“！强化 辅助 2，2，2”。");
				break;
			}
			case { AuxiliaryCards: { } cards, CloverLevel: var level }:
			{
				if (level is < -1 or > 10)
				{
					await messageReceiver.SendMessageAsync("参数有误。三叶草等级必须介于 0 到 10 之间，且请优先确保你拥有当前等级的三叶草。");
					break;
				}

				if (Array.Exists(cards, card => userCardLevel - card >= 3))
				{
					await messageReceiver.SendMessageAsync($"你的卡片等级为 {userCardLevel} 级，但你强化使用的卡片等级和当前等级相差超过 3 级及以上。不支持这种强化。");
					break;
				}

				if (Array.Exists(cards, card => userCardLevel - card < 0))
				{
					await messageReceiver.SendMessageAsync($"你的卡片等级为 {userCardLevel} 级，但你选取的辅助卡比你的主卡等级还高。不支持这种强化。");
					break;
				}

				var copied = new Dictionary<int, int>(user.UplevelingCards);
				if (user.Items.TryGetValue(ShoppingItem.Card, out var basicCardsCount) && !copied.TryAdd(0, basicCardsCount))
				{
					copied[0] += basicCardsCount;
				}

				for (var trial = 0; trial < Min(3, cards.Length); trial++)
				{
					var currentCard = cards[trial];

					if (!copied.ContainsKey(currentCard))
					{
						await messageReceiver.SendMessageAsync($"你的强化辅助卡不包含 {currentCard} 级别的卡片。请检查输入。");
						return;
					}

					copied[currentCard]--;
				}

				if (copied.Any(lastCardsCountPredicate))
				{
					var (key, value) = copied.First(lastCardsCountPredicate);
					await messageReceiver.SendMessageAsync($"强化辅助卡级别为 {key} 不够使用：原本该级卡片还有 {value} 个。请重新调整卡片等级。");
					return;
				}

				var possibility = Scorer.GetUpLevelingSuccessPossibility(userCardLevel, cards, level);

				user.Coin -= 30;

				var final = Rng.Next(0, 10000);
				var boundary = possibility * 10000;
				if (final < boundary)
				{
					// Success.
					user.CardLevel++;

					user.Items.Remove(ShoppingItem.Card);
					user.UplevelingCards = copied;

					InternalReadWrite.Write(user);

					await messageReceiver.SendMessageAsync(
						$"恭喜你，强化成功！卡片等级变动：{user.CardLevel - 1} -> {user.CardLevel}，倍率：{Scorer.GetGlobalRate(user.CardLevel)}！"
					);

					break;
				}
				else
				{
					// Failed.
					var originalLevel = user.CardLevel;
					if (user.CardLevel > 5)
					{
						user.CardLevel--;
					}

					user.Items.Remove(ShoppingItem.Card);
					user.UplevelingCards = copied;

					InternalReadWrite.Write(user);

					await messageReceiver.SendMessageAsync(
						originalLevel switch
						{
							> 5 => $"不够好运，强化失败。卡片等级降级：{originalLevel} -> {originalLevel - 1}。",
							_ => "不够好运，强化失败。卡片小于 5 级不掉级。"
						}
					);
				}

				break;


				static bool lastCardsCountPredicate(KeyValuePair<int, int> kvp) => kvp.Value < 0;
			}
		}
	}
}
