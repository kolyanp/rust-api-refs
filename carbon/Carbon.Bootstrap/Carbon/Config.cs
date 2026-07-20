using System;
using System.IO;
using Newtonsoft.Json;
using Utility;

namespace Carbon;

[Serializable]
public class Config
{
	public class AnalyticsConfig
	{
		public bool Enabled { get; set; } = true;
	}

	public static Config Singleton;

	public AnalyticsConfig Analytics { get; set; } = new AnalyticsConfig();

	public static void Init()
	{
		if (Singleton == null)
		{
			if (!File.Exists(Context.CarbonConfig))
			{
				Singleton = new Config();
			}
			else
			{
				Singleton = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Context.CarbonConfig));
			}
		}
	}
}
