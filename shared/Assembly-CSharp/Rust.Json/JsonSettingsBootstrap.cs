using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Rust.Json;

[Preserve]
public static class JsonSettingsBootstrap
{
	private static bool registered;

	[Preserve]
	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void RegisterUnityConverters()
	{
		if (registered)
		{
			return;
		}
		registered = true;
		Func<JsonSerializerSettings> previous = JsonConvert.DefaultSettings;
		JsonConvert.DefaultSettings = () =>
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			JsonSerializerSettings val = previous?.Invoke() ?? new JsonSerializerSettings();
			if (val.Converters is List<JsonConverter> list)
			{
				list.AddRange(UnityJsonConverters.All);
			}
			else
			{
				JsonConverter[] all = UnityJsonConverters.All;
				foreach (JsonConverter item in all)
				{
					val.Converters.Add(item);
				}
			}
			val.ReferenceLoopHandling = (ReferenceLoopHandling)1;
			return val;
		};
	}
}
