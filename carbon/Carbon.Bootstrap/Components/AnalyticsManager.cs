using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using API.Abstracts;
using API.Analytics;
using Carbon;
using Carbon.Components;
using ConVar;
using Facepunch;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using Utility;

namespace Components;

internal sealed class AnalyticsManager : CarbonBehaviour, IAnalyticsManager
{
	private int _sessions;

	private float _lastUpdate;

	private float _lastEngagement;

	private static string _location;

	private string MeasurementQuery;

	private const string MeasurementEntrypoint = "https://www.google-analytics.com/mp/collect";

	private const string MeasurementID = "G-M7ZBRYS3X7";

	private const string MeasurementSecret = "edBQH3_wRCWxZSzx5Y2IWA";

	private static readonly Lazy<string> _branch = new Lazy<string>(delegate
	{
		string value = _infoVersion.Value;
		if (value != null)
		{
			if (value.Contains("Debug"))
			{
				return "debug";
			}
			string text = value;
			if (text.Contains("Release"))
			{
				return "release";
			}
			string text2 = value;
			if (text2.Contains("Minimal"))
			{
				return "minimal";
			}
		}
		return "Unknown";
	});

	private static readonly Lazy<string> _infoVersion = new Lazy<string>(() => AccessTools.TypeByName("Carbon.Community").Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);

	private static readonly Lazy<string> _platform = new Lazy<string>(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : "linux");

	private static readonly Lazy<string> _protocol = new Lazy<string>(() => AccessTools.TypeByName("Carbon.Community").Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version);

	private static readonly Lazy<string> _userAgent = new Lazy<string>(() => "carbon/" + _version.Value + " (" + _platform.Value + "; x64; " + _branch.Value + "; +https://github.com/CarbonCommunity/Carbon)");

	private static readonly Lazy<string> _version = new Lazy<string>(() => AccessTools.TypeByName("Carbon.Community").Assembly.GetName().Version.ToString());

	private static readonly Lazy<Identity> _serverInfo = new Lazy<Identity>(delegate
	{
		Identity identity = default(Identity);
		try
		{
			_location = Path.Combine(Context.Game, "server", Server.identity, "carbon.id");
			if (File.Exists(_location))
			{
				string text = File.ReadAllText(_location);
				identity = JsonConvert.DeserializeObject<Identity>(text);
				if (!_serverInfo.Equals(default(Identity)))
				{
					return identity;
				}
			}
			Bootstrap.Analytics.HasNewIdentifier = true;
			identity = new Identity
			{
				UID = $"{Guid.NewGuid()}"
			};
			Utility.Logger.Warn("A new server identity was generated.");
			File.WriteAllText(_location, JsonConvert.SerializeObject((object)identity, (Formatting)1));
		}
		catch (Exception ex)
		{
			Utility.Logger.Error("Unable to process server identity", ex);
		}
		return identity;
	});

	public bool Enabled => ((Behaviour)this).enabled;

	public bool HasNewIdentifier { get; private set; }

	public string Branch => _branch.Value;

	public string InformationalVersion => _infoVersion.Value;

	public string Platform => _platform.Value;

	public bool IsMinimalBuild => false;

	public string Protocol => _protocol.Value;

	public string UserAgent => _userAgent.Value;

	public string Version => _version.Value;

	public string ClientID => _serverInfo.Value.UID;

	public string SessionID { get; private set; }

	public string SystemID => SystemInfo.deviceUniqueIdentifier;

	public WebClient Client { get; set; }

	public Dictionary<string, object> Segments { get; set; }

	public void Awake()
	{
		HasNewIdentifier = false;
		_lastUpdate = 0f;
		_lastEngagement = float.MinValue;
		SessionID = Util.GetRandomNumber(10);
		Config.Init();
		if (!Config.Singleton.Analytics.Enabled)
		{
			Utility.Logger.Warn("You have opted out from analytics data collection");
			((Behaviour)this).enabled = false;
		}
		else
		{
			Utility.Logger.Warn("We use Google Analytics to collect basic data about Carbon such as Carbon version, platform, branch and plug-in count.");
			Utility.Logger.Warn("We have no access to any personal identifiable data such as steamids, server name, ip:port, title or description.");
			Utility.Logger.Warn("If you'd like to opt-out, disable it in the 'config.json' file.");
		}
		Segments = new Dictionary<string, object>
		{
			{ "branch", Branch },
			{ "platform", Platform }
		};
		MeasurementQuery = "https://www.google-analytics.com/mp/collect?api_secret=edBQH3_wRCWxZSzx5Y2IWA&measurement_id=G-M7ZBRYS3X7";
	}

	private void Update()
	{
		_lastUpdate += Time.deltaTime;
		if (!(_lastUpdate < 300f))
		{
			_lastUpdate = 0f;
			LogEvent("user_engagement");
		}
	}

	public void SessionStart()
	{
		LogEvent(HasNewIdentifier ? "first_visit" : "user_engagement");
	}

	public void LogEvent(string eventName)
	{
		SendEvent(eventName);
	}

	public void LogEvents(string eventName)
	{
		SendMPEvent(eventName);
	}

	public void SendEvent(string eventName)
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		float num = Math.Min(Math.Max(Time.realtimeSinceStartup - _lastEngagement, 0f), float.MaxValue);
		_lastEngagement = Time.realtimeSinceStartup;
		string text = "v=2&tid=G-M7ZBRYS3X7&cid=" + ClientID + "&en=" + eventName;
		if (num >= 1800f)
		{
			if (num == float.MaxValue)
			{
				num = 0f;
			}
			SessionID = Util.GetRandomNumber(10);
			text += "&_ss=1";
			_sessions++;
		}
		text += $"&seg=1&_et={Math.Round(num * 1000f)}&sid={SessionID}&sct={_sessions}";
		SendRequest("https://www.google-analytics.com/g/collect?" + text + "&_z=" + Util.GetRandomNumber(10));
	}

	private void SendMPEvent(string eventName)
	{
		if (!((Behaviour)this).enabled)
		{
			return;
		}
		float num = Math.Min(Math.Max(Time.realtimeSinceStartup - _lastEngagement, 0f), 1800f);
		_lastEngagement = Time.realtimeSinceStartup;
		List<Dictionary<string, object>> list = Pool.Get<List<Dictionary<string, object>>>();
		Dictionary<string, object> dictionary = Pool.Get<Dictionary<string, object>>();
		Dictionary<string, object> dictionary2 = Pool.Get<Dictionary<string, object>>();
		Dictionary<string, object> dictionary3 = Pool.Get<Dictionary<string, object>>();
		List<Dictionary<string, object>> list2 = Pool.Get<List<Dictionary<string, object>>>();
		Dictionary<string, object> dictionary4 = Pool.Get<Dictionary<string, object>>();
		dictionary2["session_id"] = SessionID;
		dictionary2["engagement_time_msec"] = Math.Round(num * 1000f);
		dictionary3["client_id"] = ClientID;
		dictionary3["non_personalized_ads"] = true;
		if (Analytics.Metrics != null)
		{
			foreach (KeyValuePair<string, object> metric in Analytics.Metrics)
			{
				dictionary2.Add(metric.Key, metric.Value);
			}
		}
		dictionary4["name"] = eventName;
		dictionary4["params"] = dictionary2;
		list2.Add(dictionary4);
		dictionary3.Add("events", list2);
		if (Segments != null)
		{
			foreach (KeyValuePair<string, object> segment in Segments)
			{
				Dictionary<string, object> dictionary5 = Pool.Get<Dictionary<string, object>>();
				dictionary5["value"] = segment.Value;
				dictionary[segment.Key] = dictionary5;
				list.Add(dictionary5);
			}
			dictionary3.Add("user_properties", dictionary);
		}
		SendRequest(MeasurementQuery, JsonConvert.SerializeObject((object)dictionary3));
		foreach (Dictionary<string, object> item in list)
		{
			Dictionary<string, object> dictionary6 = item;
			Pool.FreeUnmanaged<string, object>(ref dictionary6);
		}
		Pool.FreeUnmanaged<Dictionary<string, object>>(ref list2);
		Pool.FreeUnmanaged<Dictionary<string, object>>(ref list);
		Pool.FreeUnmanaged<string, object>(ref dictionary4);
		Pool.FreeUnmanaged<string, object>(ref dictionary);
		Pool.FreeUnmanaged<string, object>(ref dictionary2);
		Pool.FreeUnmanaged<string, object>(ref dictionary3);
	}

	private void SendRequest(string url, string body = null)
	{
		try
		{
			if (body == null)
			{
				body = string.Empty;
			}
			if (Client == null)
			{
				Client = new WebClient();
				Client.Headers.Add(HttpRequestHeader.UserAgent, UserAgent);
				Client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			}
			Client.UploadStringAsync(new Uri(url), "POST", body, url);
		}
		catch (Exception)
		{
		}
	}
}
