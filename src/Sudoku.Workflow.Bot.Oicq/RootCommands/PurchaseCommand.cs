namespace Sudoku.Workflow.Bot.Oicq.RootCommands;

/// <summary>
/// 购买指令。
/// </summary>
[Command("购买")]
[RequiredUserLevel(20)]
internal sealed class PurchaseCommand : Command
{
	/// <summary>
	/// 表示购买的三叶草的等级。该参数配合“三叶草”使用。对于“物品”参数填入强化卡的时候无效。
	/// </summary>
	[DoubleArgument("级别")]
	[Hint("表示购买的三叶草的等级。该参数配合“三叶草”使用。对于“物品”参数填入强化卡的时候无效。")]
	[ValueConverter<NumericConverter<int>>]
	[DefaultValue<int>(-1)]
	[DisplayingIndex(0)]
	public int Level { get; set; }

	/// <summary>
	/// 表示批量购买的数量。该参数配合“物品”使用，表示当前购买的物品一次性买多少个。
	/// </summary>
	[DoubleArgument("批量")]
	[Hint("表示批量购买的数量。该参数配合“物品”使用，表示当前购买的物品一次性买多少个。")]
	[ValueConverter<NumericConverter<int>>]
	[DefaultValue<int>(1)]
	[DisplayingIndex(2)]
	public int BatchedCount { get; set; }

	/// <summary>
	/// 表示购买的物品。可以是“三叶草”或“强化卡”。
	/// </summary>
	[DoubleArgument("物品")]
	[Hint("表示购买的物品。可以是“三叶草”或“强化卡”。")]
	[DisplayingIndex(1)]
	public string? ItemName { get; set; }


	/// <inheritdoc/>
	protected override async Task ExecuteCoreAsync(GroupMessageReceiver messageReceiver)
	{
		if (messageReceiver is not { Sender.Id: var senderId })
		{
			return;
		}

		if (UserOperations.Read(senderId) is not { Coin: var coin } user)
		{
			await messageReceiver.SendMessageAsync("用户没有使用过机器人。无法购买商品。");
			return;
		}

		if (BatchedCount < 0)
		{
			await messageReceiver.SendMessageAsync("抱歉，批量购买的次数至少为 1。");
			return;
		}

		switch (this)
		{
			case { ItemName: ItemNames.Card, BatchedCount: var count }:
			{
				var price = Item.Card.GetPrice();
				if (coin < price * count)
				{
					goto Failed_CoinNotEnough;
				}

				user.Coin -= price * count;

				if (!user.UplevelingCards.TryAdd(0, count))
				{
					user.UplevelingCards[0] += count;
				}

				UserOperations.Write(user);

				goto Successful;
			}
			case { ItemName: null or ItemNames.Clover, Level: var level, BatchedCount: var count }:
			{
				if (level == 10)
				{
					await messageReceiver.SendMessageAsync("很抱歉。终极三叶草无法通过购买的方式获得。");
					return;
				}

				if (level is < 1 or > 9)
				{
					await messageReceiver.SendMessageAsync("可购买的三叶草卡片等级为 1 到 9。请检查你的输入，如“！购买 物品 三叶草 等级 3”。");
					break;
				}

				var targetItem = Item.CloverLevel1 + (level - 1);
				var price = targetItem.GetPrice();
				if (coin < price * count)
				{
					goto Failed_CoinNotEnough;
				}

				user.Coin -= price * count;

				if (!user.Items.TryAdd(targetItem, count))
				{
					user.Items[targetItem] += count;
				}

				UserOperations.Write(user);

				goto Successful;
			}
			default:
			{
				await messageReceiver.SendMessageAsync(
					"你要购买的物品名称不清楚。可提供购买的商品可以为“三叶草”和“强化卡”两种。请输入合适的指令，如“！购买 物品 强化卡”。"
				);

				break;
			}

		Failed_CoinNotEnough:
			{
				await messageReceiver.SendMessageAsync("购买失败。你的金币不足。");
				break;
			}
		Successful:
			{
				await messageReceiver.SendMessageAsync("恭喜，购买成功！请使用“！查询 内容 物品”指令确认。");
				break;
			}
		}
	}
}

/// <summary>
/// 为属性 <see cref="PurchaseCommand.ItemName"/> 提供枚举数值。
/// </summary>
/// <seealso cref="PurchaseCommand.ItemName"/>
file static class ItemNames
{
	/// <summary>
	/// 表示数值“三叶草”。
	/// </summary>
	public const string Clover = "三叶草";

	/// <summary>
	/// 表示数值“强化卡”。
	/// </summary>
	public const string Card = "强化卡";
}
