using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;

namespace Rust.Json;

[Preserve]
public static class JsonSettingsBootstrap
{
	private static bool registered;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	[Preserve]
	private static void RegisterUnityConverters()
	{
		if (registered)
		{
			return;
		}
		registered = true;
		Func<JsonSerializerSettings> previous = JsonConvert.DefaultSettings;
		JsonConvert.DefaultSettings = delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			JsonSerializerSettings val = (JsonSerializerSettings)(((object)previous?.Invoke()) ?? ((object)new JsonSerializerSettings()));
			JsonConverter[] array = UnityJsonConverters.CreateAll();
			foreach (JsonConverter item in array)
			{
				val.Converters.Add(item);
			}
			val.ReferenceLoopHandling = (ReferenceLoopHandling)1;
			return val;
		};
	}
}
