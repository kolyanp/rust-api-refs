using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon;
using Facepunch;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Game.Rust.Cui;

public static class CuiHelper
{
	private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
	{
		DefaultValueHandling = (DefaultValueHandling)1,
		NullValueHandling = (NullValueHandling)1,
		DateParseHandling = (DateParseHandling)0,
		FloatFormatHandling = (FloatFormatHandling)1,
		StringEscapeHandling = (StringEscapeHandling)0
	};

	private static readonly JsonSerializer _serializer = JsonSerializer.Create(_settings);

	private static readonly ThreadLocal<JsonContext> _jsonContext = new ThreadLocal<JsonContext>(() => new JsonContext());

	private static readonly ThreadLocal<StringBuilder> _colorSb = new ThreadLocal<StringBuilder>(() => new StringBuilder(32));

	public static Dictionary<BasePlayer, HashSet<string>> ActivePanels { get; } = new Dictionary<BasePlayer, HashSet<string>>();

	public static HashSet<string> GetActivePanelList(BasePlayer player)
	{
		if (!ActivePanels.TryGetValue(player, out var value))
		{
			value = (ActivePanels[player] = new HashSet<string>());
		}
		return value;
	}

	public static int DestroyActivePanelList(BasePlayer player, string[] except = null)
	{
		List<string> list = Pool.Get<List<string>>();
		list.AddRange(GetActivePanelList(player));
		int num = 0;
		foreach (string item in list.Where((string x) => except == null || except.Length == 0 || !except.Any((string y) => x.StartsWith(y))))
		{
			DestroyUi(player, item);
			num++;
		}
		Pool.FreeUnmanaged<string>(ref list);
		return num;
	}

	public static string ToJson(IReadOnlyList<CuiElement> elements, bool format = false)
	{
		JsonContext value = _jsonContext.Value;
		value.sb.Clear();
		JsonTextWriter val = (format ? value.jwFormatted : value.jw);
		_serializer.Serialize((JsonWriter)(object)val, (object)elements);
		((JsonWriter)val).Flush();
		return value.sb.Replace("\\n", "\n").ToString();
	}

	public static string ToJson(CuiElement element, bool format = false)
	{
		return JsonConvert.SerializeObject((object)element, (Formatting)(format ? 1 : 0), _settings).Replace("\\n", "\n");
	}

	public static List<CuiElement> FromJson(string json)
	{
		return JsonConvert.DeserializeObject<List<CuiElement>>(json);
	}

	public static string GetGuid()
	{
		return Guid.NewGuid().ToString("N");
	}

	public static bool AddUi(BasePlayer player, string json)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || ((BaseNetworkable)player).net == null)
		{
			return false;
		}
		if (HookCaller.CallStaticHook(1307002116u, player, json) != null)
		{
			return false;
		}
		((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("AddUI", player), json);
		return true;
	}

	public static bool AddUi(BasePlayer player, List<CuiElement> elements)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || ((BaseNetworkable)player).net == null)
		{
			return false;
		}
		string text = ToJson(elements);
		if (HookCaller.CallStaticHook(1307002116u, player, text) != null)
		{
			return false;
		}
		if (elements != null && elements.Count > 0)
		{
			string name = elements[0].Name;
			HashSet<string> activePanelList = GetActivePanelList(player);
			if (!activePanelList.Contains(name))
			{
				activePanelList.Add(name);
			}
		}
		((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("AddUI", player), text);
		return true;
	}

	public static bool DestroyUi(BasePlayer player, string name)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseNetworkable)(player?)).net != null)
		{
			HashSet<string> activePanelList = GetActivePanelList(player);
			if (activePanelList.Contains(name))
			{
				activePanelList.Remove(name);
			}
			HookCaller.CallStaticHook(503981600u, player, name);
			((BaseEntity)CommunityEntity.ServerInstance).ClientRPC(RpcTarget.Player("DestroyUI", player), name);
			return true;
		}
		return false;
	}

	public static void SetColor(this ICuiColor elem, Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		StringBuilder value = _colorSb.Value;
		value.Clear();
		value.Append(color.r).Append(' ').Append(color.g)
			.Append(' ')
			.Append(color.b)
			.Append(' ')
			.Append(color.a);
		elem.Color = value.ToString();
	}

	public static Color GetColor(this ICuiColor elem)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ColorEx.Parse(elem.Color);
	}
}
