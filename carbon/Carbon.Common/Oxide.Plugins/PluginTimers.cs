using System;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;

namespace Oxide.Plugins;

public class PluginTimers
{
	private Oxide.Core.Libraries.Timer timer;

	private Plugin plugin;

	public PluginTimers(Plugin plugin)
	{
		this.plugin = plugin;
		timer = new Oxide.Core.Libraries.Timer(plugin);
	}

	public Timer Once(float seconds, Action callback)
	{
		return new Timer(timer.Once(seconds, callback, plugin));
	}

	public Timer In(float seconds, Action callback)
	{
		return new Timer(timer.Once(seconds, callback, plugin));
	}

	public Timer Every(float interval, Action callback)
	{
		return new Timer(timer.Repeat(interval, -1, callback, plugin));
	}

	public Timer Repeat(float interval, int repeats, Action callback)
	{
		return new Timer(timer.Repeat(interval, repeats, callback, plugin));
	}

	public void Destroy(ref Timer timer)
	{
		timer?.DestroyToPool();
		timer = null;
	}

	public void Clear()
	{
		timer.Clear();
	}
}
