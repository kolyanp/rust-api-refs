using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Game.Rust.Cui;

public static class CuiHelper
{
	private class JsonWriterResources
	{
		public readonly StringBuilder StringBuilder = new StringBuilder(65536);

		public readonly StringWriter StringWriter;

		public readonly JsonTextWriter JsonWriter;

		public readonly JsonSerializer Serializer;

		public JsonWriterResources()
		{
			StringWriter = new StringWriter(StringBuilder, CultureInfo.InvariantCulture);
			JsonWriter = new JsonTextWriter(StringWriter)
			{
				ArrayPool = JsonArrayPool<char>.Shared,
				CloseOutput = false
			};
			Serializer = JsonSerializer.Create(Settings);
		}

		public void Reset(bool format = false)
		{
			StringBuilder.Clear();
			JsonWriter.Formatting = (format ? Formatting.Indented : Formatting.None);
		}
	}

	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		DefaultValueHandling = DefaultValueHandling.Ignore,
		NullValueHandling = NullValueHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		FloatFormatHandling = FloatFormatHandling.Symbol,
		StringEscapeHandling = StringEscapeHandling.Default
	};

	private static readonly ThreadLocal<JsonWriterResources> SharedWriterResources = new ThreadLocal<JsonWriterResources>(() => new JsonWriterResources());

	public static string ToJson(IReadOnlyList<CuiElement> elements, bool format = false)
	{
		JsonWriterResources value = SharedWriterResources.Value;
		value.Reset(format);
		value.Serializer.Serialize(value.JsonWriter, elements);
		value.JsonWriter.Flush();
		return value.StringBuilder.ToString().Replace("\\n", "\n");
	}

	public static List<CuiElement> FromJson(string json)
	{
		return JsonConvert.DeserializeObject<List<CuiElement>>(json);
	}

	public static string GetGuid()
	{
		return Guid.NewGuid().ToString("N");
	}

	public static bool AddUi(BasePlayer player, List<CuiElement> elements)
	{
		if (player?.net == null)
		{
			return false;
		}
		return AddUi(player, ToJson(elements));
	}

	public static bool AddUi(BasePlayer player, string json)
	{
		if (player?.net != null && Interface.CallHook("CanUseUI", player, json) == null)
		{
			CommunityEntity.ServerInstance.ClientRPC(RpcTarget.Player("AddUI", player.net.connection), json);
			return true;
		}
		return false;
	}

	public static bool DestroyUi(BasePlayer player, string elem)
	{
		if (player?.net != null)
		{
			Interface.CallHook("OnDestroyUI", player, elem);
			CommunityEntity.ServerInstance.ClientRPC(RpcTarget.Player("DestroyUI", player.net.connection), elem);
			return true;
		}
		return false;
	}

	public static bool DestroyUi(BasePlayer player, List<string> elems)
	{
		if (player?.net == null || elems == null || elems.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < elems.Count; i++)
		{
			Interface.CallHook("OnDestroyUI", player, elems[i]);
		}
		CommunityEntity.ServerInstance.SendDestroyUIs(player, elems);
		return true;
	}

	public static bool DestroyUi(BasePlayer player, string[] elems)
	{
		if (player?.net == null || elems == null || elems.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < elems.Length; i++)
		{
			Interface.CallHook("OnDestroyUI", player, elems[i]);
		}
		CommunityEntity.ServerInstance.SendDestroyUIs(player, elems);
		return true;
	}

	public static void SetColor(this ICuiColor elem, Color color)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		elem.Color = string.Format("{0} {1} {2} {3}", new object[4] { color.r, color.g, color.b, color.a });
	}

	public static Color GetColor(this ICuiColor elem)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ColorEx.Parse(elem.Color);
	}
}
