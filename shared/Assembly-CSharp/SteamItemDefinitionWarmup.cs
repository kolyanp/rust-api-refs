using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class SteamItemDefinitionWarmup
{
	private static IReadOnlyList<IPlayerItemDefinition> warmedDefinitions;

	public static void StartWarmup()
	{
		if (PlatformService.Instance.IsValid && PlatformService.Instance.ItemDefinitions != null)
		{
			IReadOnlyList<IPlayerItemDefinition> itemDefinitions = PlatformService.Instance.ItemDefinitions;
			if (itemDefinitions != warmedDefinitions)
			{
				warmedDefinitions = itemDefinitions;
				WarmupAsync(itemDefinitions);
			}
		}
	}

	private static async Task WarmupAsync(IReadOnlyList<IPlayerItemDefinition> definitions)
	{
		TimeSpan budget = TimeSpan.FromMilliseconds(1.0);
		await ThreadUtil.IterateBudgetUnbuffered(definitions, budget, ThreadUtil.ErrorLogging.LogExceptionAndBreak, (IPlayerItemDefinition item) =>
		{
			if (PlatformService.Instance.IsValid)
			{
				_ = item.ItemShortName;
				_ = item.WorkshopId;
			}
		});
	}
}
