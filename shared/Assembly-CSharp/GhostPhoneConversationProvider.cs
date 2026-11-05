using System;
using System.Collections.Generic;
using ProtoBuf;
using Rust;
using UnityEngine;

public class GhostPhoneConversationProvider : PointEntity, IConversationProvider
{
	public Phrase NPCName = new Phrase("", "");

	[ConversationData.DisplayGraphViewButton]
	public ConversationData[] phoneConversations = Array.Empty<ConversationData>();

	private static readonly Phrase Phrase_WeatherClear = new Phrase("weather.type.clear", "clear skies");

	private static readonly Phrase Phrase_WeatherOvercast = new Phrase("weather.type.overcast", "overcast skies");

	private static readonly Phrase Phrase_WeatherStorm = new Phrase("weather.type.storm", "storms");

	private static readonly Phrase Phrase_WeatherDust = new Phrase("weather.type.dust", "a dusty haze");

	private static readonly Phrase Phrase_WeatherFog = new Phrase("weather.type.fog", "fog");

	private static readonly Phrase Phrase_WeatherRain = new Phrase("weather.type.rain", "rain");

	private static readonly Phrase Phrase_Minutes = new Phrase("time.minutes", "minutes");

	private static readonly Phrase Phrase_Hour = new Phrase("time.hour", "hour");

	private static readonly Phrase Phrase_Hours = new Phrase("time.hours", "hours");

	private static readonly Phrase Phrase_GridEmergency = new Phrase("powergrid.service.emergency", "Emergency power rationing is in effect. Most power systems can be expected to be non-operational.");

	private static readonly Phrase Phrase_GridRestricted = new Phrase("powergrid.service.restricted", "Restricted service is in effect. Some power systems can be expected to be non-operational.");

	private static readonly Phrase Phrase_GridReduced = new Phrase("powergrid.service.reduced", "Reduced service is in effect. Most power systems can be expected to be operational.");

	private static readonly Phrase Phrase_GridNormal = new Phrase("powergrid.service.normal", "Regular service has been restored. All power systems are operational. Thank you for your patience.");

	private ConversationHost _conversationHost;

	private ConversationHost ConversationHost => _conversationHost ?? (_conversationHost = new ConversationHost(this));

	public override void ServerInit()
	{
		base.ServerInit();
		limitNetworking = true;
		if (!Application.isLoadingSave)
		{
			ConversationHost.RegisterPhoneNumbers(phoneConversations, 0);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		ConversationHost.RegisterPhoneNumbers(phoneConversations, 0);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			ConversationHost.SavePhoneNumbers(info.msg);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			ConversationHost.LoadPhoneNumbers(info.msg);
		}
	}

	internal override void DoServerDestroy()
	{
		ConversationHost.EndAllConversations();
		ConversationHost.DeregisterPhoneNumbers();
		base.DoServerDestroy();
	}

	public bool ProviderBusy()
	{
		return false;
	}

	public Phrase GetNPCName()
	{
		return NPCName;
	}

	public Phrase GetPhoneDirectoryName()
	{
		return NPCName;
	}

	public ConversationData GetInPersonConversationFor(BasePlayer player)
	{
		return null;
	}

	public ConversationData GetPhoneConversationFor(BasePlayer player)
	{
		return ConversationHost.GetDirectCallConversation(phoneConversations, player, this);
	}

	public bool BeginPhoneConversation(BasePlayer player, PhoneController phone, ConversationData conversation)
	{
		return ConversationHost.BeginPhoneConversation(player, phone, conversation);
	}

	public void HandleResponse(BasePlayer player, int speechNodeIndex, int responseIndex, uint previewingMissionId, uint selectedMissionId, NetworkableId selectedAnimalId)
	{
		ConversationHost.HandleResponse(player, speechNodeIndex, responseIndex);
	}

	public void OnConversationEnded(BasePlayer player)
	{
		ConversationHost.EndConversation(player);
	}

	public void AddFormatTextValues(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		if (speechNode.UsesFormatText)
		{
			AddWeatherValues(speechNode, values);
			AddTravellingVendorValues(speechNode, values);
			AddPowergridValues(speechNode, values);
		}
	}

	public void AddSpeechNodeState(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, ConversationState state)
	{
	}

	public void OnConversationAction(BasePlayer player, string action)
	{
	}

	public void UpdateFlags()
	{
	}

	private static void AddWeatherValues(ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		Climate instance = SingletonComponent<Climate>.Instance;
		if (!((Object)(object)instance == (Object)null) && !((Object)(object)instance.CurrentWeather == (Object)null))
		{
			AddFormatTextValue(values, speechNode, "weather_now", GetWeatherPhrase(instance.CurrentWeather.Type).token, valueIsPhraseToken: true);
			if (instance.TryGetWeatherForecast(out var forecastType, out var chance, out var hoursUntil))
			{
				AddFormatTextValue(values, speechNode, "weather_next", GetWeatherPhrase(forecastType).token, valueIsPhraseToken: true);
				AddFormatTextValue(values, speechNode, "weather_next_chance", chance.ToString());
				AddHoursValues(values, speechNode, "weather_next_in", Mathf.RoundToInt(GameHoursToRealSeconds(hoursUntil) / 3600f));
			}
		}
	}

	private static void AddTravellingVendorValues(ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		TravellingVendor visitingVendor = TravellingVendor.GetVisitingVendor();
		if ((Object)(object)visitingVendor != (Object)null)
		{
			AddTimeLeftValues(values, speechNode, "vendor_leaves_in", visitingVendor.TimeLeftOnMap);
		}
		EventSchedule eventSchedule = TravellingVendorEvent.FindSchedule();
		if ((Object)(object)eventSchedule != (Object)null)
		{
			float num = GameHoursToRealSeconds(Mathf.Max(0f, eventSchedule.HoursRemaining));
			AddHoursValues(values, speechNode, "vendor_returns_in", Mathf.CeilToInt(num / 3600f));
		}
	}

	private static void AddPowergridValues(ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
		PowergridManager serverInstance = PointEntity<PowergridManager>.ServerInstance;
		if (!((Object)(object)serverInstance == (Object)null) && !(PowergridStageConfig.instance == null) && serverInstance.Server_GetFuseSocketsCount() > 0)
		{
			AddFormatTextValue(values, speechNode, "grid_power", Mathf.RoundToInt(100f * (float)serverInstance.Server_GetPowerPlantInsertedFuses() / (float)serverInstance.Server_GetFuseSocketsCount()).ToString());
			AddFormatTextValue(values, speechNode, "grid_service", GetGridServicePhrase(serverInstance.CurrentStage, PowergridStageConfig.instance.GetNumberOfStages()).token, valueIsPhraseToken: true);
		}
	}

	private static Phrase GetGridServicePhrase(int stage, int lastStage)
	{
		if (stage >= lastStage)
		{
			return Phrase_GridNormal;
		}
		return stage switch
		{
			1 => Phrase_GridEmergency, 
			2 => Phrase_GridRestricted, 
			_ => Phrase_GridReduced, 
		};
	}

	private static Phrase GetWeatherPhrase(WeatherPresetType type)
	{
		return type switch
		{
			WeatherPresetType.Overcast => Phrase_WeatherOvercast, 
			WeatherPresetType.Storm => Phrase_WeatherStorm, 
			WeatherPresetType.Dust => Phrase_WeatherDust, 
			WeatherPresetType.Fog => Phrase_WeatherFog, 
			WeatherPresetType.Rain => Phrase_WeatherRain, 
			_ => Phrase_WeatherClear, 
		};
	}

	private static float GameHoursToRealSeconds(float hours)
	{
		if (!((Object)(object)TOD_Sky.Instance != (Object)null))
		{
			return 0f;
		}
		return TOD_Sky.Instance.Components.Time.GameHoursToRealSeconds(hours);
	}

	private static void AddTimeLeftValues(List<FormatTextValue> values, ConversationData.AbstractSpeechNodeData speechNode, string key, float seconds)
	{
		if (seconds >= 3600f)
		{
			AddHoursValues(values, speechNode, key, Mathf.FloorToInt(seconds / 3600f));
			return;
		}
		AddFormatTextValue(values, speechNode, key, Mathf.Max(10, Mathf.FloorToInt(seconds / 600f) * 10).ToString());
		AddFormatTextValue(values, speechNode, key + "_unit", Phrase_Minutes.token, valueIsPhraseToken: true);
	}

	private static void AddHoursValues(List<FormatTextValue> values, ConversationData.AbstractSpeechNodeData speechNode, string key, int hours)
	{
		hours = Mathf.Max(1, hours);
		AddFormatTextValue(values, speechNode, key, hours.ToString());
		AddFormatTextValue(values, speechNode, key + "_unit", ((hours == 1) ? Phrase_Hour : Phrase_Hours).token, valueIsPhraseToken: true);
	}

	private static void AddFormatTextValue(List<FormatTextValue> values, ConversationData.AbstractSpeechNodeData speechNode, string key, string value, bool valueIsPhraseToken = false)
	{
		if (speechNode.UsesFormatTextKey(key))
		{
			ConversationHost.AddFormatTextValue(values, key, value, valueIsPhraseToken);
		}
	}

	public GhostPhoneConversationProvider()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
	}

	static GhostPhoneConversationProvider()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected Obj, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected Obj, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected Obj, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected Obj, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected Obj, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected Obj, but got Unknown
	}
}
