namespace Sudoku.Workflow.Bot.Oicq.RootCommands;

/// <summary>
/// 表示从题库里抽取题目的指令。
/// </summary>
[Command("抽题")]
[Usage("！抽题 题库名 sdc", "抽取本群题库“SDC”的题目。")]
internal sealed class PuzzleLibraryExtractCommand : Command
{
	/// <summary>
	/// 表示你需要抽取的题库的所在群（即跨群抽取题目）。
	/// </summary>
	[DoubleArgument("群名")]
	[Hint("表示你需要抽取的题库的所在群（即跨群抽取题目）。")]
	[DisplayingIndex(0)]
	[ArgumentDisplayer("QQ")]
	public string? GroupName { get; set; }

	/// <summary>
	/// 表示你需要抽取的题库的所在群的 QQ 号码。
	/// </summary>
	[DoubleArgument("群号")]
	[Hint("表示你需要抽取的题库的所在群的 QQ 号码。")]
	[DisplayingIndex(0)]
	public string? GroupId { get; set; }

	/// <summary>
	/// 表示你需要抽取的题库的题库名称。
	/// </summary>
	[DoubleArgument("题库名")]
	[Hint("表示你需要抽取的题库的题库名称。")]
	[DisplayingIndex(1)]
	public string? LibraryName { get; set; }


	/// <inheritdoc/>
	protected override async Task ExecuteCoreAsync(GroupMessageReceiver messageReceiver)
	{
		if (messageReceiver is not { GroupId: var groupId })
		{
			return;
		}

		var puzzleLibraries = PuzzleLibraryOperations.GetLibraries(groupId);
		switch (GroupName, GroupId, LibraryName)
		{
			// 抽取此群里唯一一个题库的题目。
			case (null, null, null):
			{
				if (puzzleLibraries is not [{ Name: var libName, Path: var libPath, FinishedPuzzlesCount: var finishedCount } lib])
				{
					// 列表模式 [lib] 表示变量里只有一个成员，意味着这个序列只有一个元素。
					// 取反则表示，要么变量为 null，要么变量为空列表，要么变量包含至少两个及以上的元素，反正就是除了“只有一个元素”以外的所有情况。
					await messageReceiver.SendMessageAsync("当前群存在多个题库，因此机器人无法确定你需要抽取哪一个题库的题目。请给出参数“名称”。");
					return;
				}

				if (!File.Exists(libPath))
				{
					// 前面查询的是本群里本来已经存在的题库，但是由于我自身的原因，在挪动了题库文件后忘记重置数据或同步更新群题库数据的文件，就会出现此错误。
					// 做得很好，下回别这么做了。
					await messageReceiver.SendMessageAsync("虽然我很不想这么说，但是题库确实不见了。请直接联系程序设计者询问该错误。");
					return;
				}

				if (PuzzleLibraryOperations.GetPuzzlesCount(lib) == finishedCount)
				{
					// 说明已经完成这个题库了，所有题目已经全部完成。
					await messageReceiver.SendMessageAsync(
						$"""
						恭喜你，唯一的题库“{libName}”的题目已经全部被完成。
						如果你还想重新做这个题库的题，请联系管理员重置数据后方可重新开始。
						""".RemoveLineEndings()
					);
					return;
				}

				await getPuzzleCore(messageReceiver, groupId, lib);
				break;
			}

			// 抽取此群里指定名称的题目。
			case (null, null, { } name):
			{
				var lib = PuzzleLibraryOperations.GetLibrary(groupId, name);
				if (lib is not { Name: var libName, Path: var libPath, FinishedPuzzlesCount: var finishedCount })
				{
					await messageReceiver.SendMessageAsync($"请检查输入。指定的题库名称“{name}”尚不能找到。");
					return;
				}

				if (!File.Exists(libPath))
				{
					// 前面查询的是本群里本来已经存在的题库，但是由于我自身的原因，在挪动了题库文件后忘记重置数据或同步更新群题库数据的文件，就会出现此错误。
					// 做得很好，下回别这么做了。
					await messageReceiver.SendMessageAsync("虽然我很不想这么说，但是题库确实不见了。请直接联系程序设计者询问该错误。");
					return;
				}

				if (PuzzleLibraryOperations.GetPuzzlesCount(lib) == finishedCount)
				{
					// 说明已经完成这个题库了，所有题目已经全部完成。
					await messageReceiver.SendMessageAsync(
						$"""
						恭喜你，题库“{libName}”的题目已经全部被完成。
						如果你还想重新做这个题库的题，请联系管理员重置数据后方可重新开始。
						""".RemoveLineEndings()
					);
					return;
				}

				await getPuzzleCore(messageReceiver, groupId, lib);
				break;
			}

			// 跨群抽取题目。这个进度尤其难记录。因为程序设计就是将一个群的题库放进一个群的对应文件夹里的。
			// 取出跨群的题库倒是简单，只需要传入那个对应群的数据进去就行了；但是非常不好弄的是记录进度。
			// 因为 PuzzleLibrary 的数据类型设计上就只跟一个群的数据有关系；跨群的题目是走别的群取出的题，得到的 PuzzleLibrary 首先群号就会冲突；
			// 其次也没有办法用这个数据类型表达出来。
			// 目前能想到的唯一办法就是，跨群题库需要使用 PuzzleLibrary 的一个新的派生类型来完成。派生类型因为可以从 PuzzleLibrary 本身类型继承下来，
			// 所以赋值到题库集合里是不影响什么的；而且又区分开了普通题库和跨群题库。
			// 但是，.NET 7 及以前版本的 JSON API 暂时对派生类型的序列化/反序列化不太友好。
			// 虽说这一段可能在 .NET 8 会有所改善，例如使用 [JsonDerivedType] 之类的特性来改变行为。但是这里我们就暂时不考虑这个了。
			case (not null, { } id, _):
			{
#if false
				await messageReceiver.SendMessageAsync("群名和 QQ 同时都有赋值，那么参数“群名”会被忽略。");
				await Task.Delay(1000);
#endif

				// TODO: 之后我再来做。
				break;
			}

			case (_, { } id, { } name):
			{
				// TODO: 之后我再来做。
				break;
			}

			case ({ } groupName, _, { } name):
			{
				// TODO: 之后我再来做。
				break;
			}

			// 其他情况。
			default:
			{
				await messageReceiver.SendMessageAsync(
					"""
					传入的参数有误或缺少，比如抽取跨群题目至少需要同时有“群名”或“QQ”的其中一个参数，
					以及“名称”参数这两个参数同时包含的时候才可以正确读取题目。请检查参数传入正确之后重试。
					""".RemoveLineEndings()
				);
				return;
			}
		}


		static async Task getPuzzleCore(GroupMessageReceiver messageReceiver, string groupId, PuzzleLibrary lib)
		{
			await messageReceiver.SendMessageAsync("正在解析题库，请稍候……");

			// 获取题目。
			var grid = PuzzleLibraryOperations.GetPuzzleFor(lib);

			// 把得到的题目拿去分析，并得到分析结果（看一下题目是否唯一解之类的）。然后打印一下一共使用了什么技巧。
			var analyzerResult = PuzzleAnalyzer.Analyze(grid);
			if (analyzerResult is not { IsSolved: true, DifficultyLevel: var difficultyLevel })
			{
				await messageReceiver.SendMessageAsync("抽取到的题目无法被解出，即多解或无解。请联系题目发布者询问是否题库存在问题。");
				return;
			}

			// 填充一下前面没有必要的步骤（如排除、唯余之类的）。
			// 这里由于题目可能会比较难的关系，我们可以故意先完成一些相对于链和一些复杂结构来说，不太重要的步骤。
			grid = AutoFiller.Fill(analyzerResult);

			// 根据绘图对象直接创建图片，然后发送出去。
			await messageReceiver.SendPictureThenDeleteAsync(
				ISudokuPainter.Create(1000)
					.WithGrid(grid)
					.WithRenderingCandidates(difficultyLevel >= DifficultyLevel.Hard)
					.WithFooterText($"@{lib.Name} #{lib.FinishedPuzzlesCount + 1}")
					.WithPreferenceSettings(static pref => pref.CandidateScale = 0.4M)
			);

			await Task.Delay(200);

			// 显示题目的分析结果（使用的技巧）。
			// 这里只显示技巧，题目的其他要素（比如卡点、题目的终盘等）都不应该显示出来。
			await messageReceiver.SendMessageAsync(analyzerResult.ToString(AnalyzerResultFormattingOptions.ShowElapsedTime));

			// 目前为了考虑代码简便，暂时直接完成题目。如果用户抽了题目，这个题就自动完成了。
			// 之后有空我们再来考虑回答题目的事情。
			lib.FinishedPuzzlesCount++;
			PuzzleLibraryOperations.UpdateLibrary(groupId, lib);
		}
	}
}

/// <summary>
/// 定义一个临时用的、自动完成前面排除、唯一余数、区块等技巧的实例。
/// </summary>
file static class AutoFiller
{
	/// <summary>
	/// 通过指定的 <see cref="AnalyzerResult"/> 实例来获得题目的基本解题信息，并通过该实例，自动完成前面一些不重要的题目步骤。
	/// </summary>
	/// <param name="result"><see cref="AnalyzerResult"/> 类型的实例。</param>
	/// <returns>返回一个 <see cref="Grid"/> 结果。</returns>
	/// <exception cref="InvalidOperationException">如果题目无解或多解、题目难度未知或过于复杂时，就会产生此异常。</exception>
	public static Grid Fill(AnalyzerResult result)
	{
		if (result is not { Puzzle: var grid, IsSolved: true, DifficultyLevel: var diffLevel, SolvingPath: var path })
		{
			throw new InvalidOperationException("The target grid is not unique.");
		}

		switch (diffLevel)
		{
			case DifficultyLevel.Unknown:
			{
				throw new InvalidOperationException("The difficulty level of the target grid is unknown.");
			}
			case DifficultyLevel.Easy or DifficultyLevel.Moderate:
			{
				return grid;
			}
			case >= DifficultyLevel.Hard and <= DifficultyLevel.Nightmare or DifficultyLevel.LastResort:
			{
				foreach (var (stepGrid, step) in path)
				{
					if (step.DifficultyLevel is not (DifficultyLevel.Easy or DifficultyLevel.Moderate))
					{
						return stepGrid;
					}
				}

				throw new InvalidOperationException("Generally program cannot reach here.");
			}
			default:
			{
				throw new InvalidOperationException("The target grid is invalid - its difficulty level is too complex to be checked.");
			}
		}
	}
}
