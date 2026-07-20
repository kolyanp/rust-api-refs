using System;
using API.Events;
using UnityEngine;

namespace API.Hooks;

public class Patch
{
	internal static readonly Lazy<IEventManager> _events = new Lazy<IEventManager>(delegate
	{
		GameObject val = GameObject.Find("Carbon");
		if (!((Object)(object)val == (Object)null))
		{
			return val.GetComponent<IEventManager>();
		}
		throw new Exception("Carbon GameObject not found");
	});

	protected static IEventManager Events => _events.Value;
}
