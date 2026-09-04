using System;
using System.IO;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("world")]
public class World : ConsoleSystem
{
	[ClientVar(Help = "(Generated) When enabled, caches world data for faster loading; disabled by default in editor server builds to ensure fresh data during development")]
	[ServerVar]
	public static bool cache = true;

	[ClientVar(Help = "(Generated) When enabled, world assets are streamed in and out based on proximity; disable to force all world data to stay loaded at once")]
	public static bool streaming = true;

	[ServerVar(Help = "(Generated) World generation config string passed directly to the procedural map generator; overrides the config file if set")]
	public static string configString = string.Empty;

	[ServerVar(Help = "(Generated) Path to a world generation config file used by the procedural map generator; used when configString is empty")]
	public static string configFile = string.Empty;

	[ServerVar(Help = "(Generated) Prints a table of all monuments on the current map including type, display name, prefab path, and world position; admin/developer only")]
	[ClientVar(Help = "(Generated) Prints a table of all monuments on the current map including type, display name, prefab path, and world position; admin/developer only")]
	public static void monuments(Arg arg)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)TerrainMeta.Path))
		{
			return;
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("type");
			val.AddColumn("name");
			val.AddColumn("prefab");
			val.AddColumn("pos");
			foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
			{
				val.AddRow(new string[4]
				{
					monument.Type.ToString(),
					monument.displayPhrase.translated,
					((Object)monument).name,
					((object)((Component)monument).transform.position/*cast due to constrained. prefix*/).ToString()
				});
			}
			arg.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Clientside = true, Help = "Renders a high resolution PNG of the current map")]
	public static void rendermap(Arg arg)
	{
		float scale = arg.GetFloat(0, 1f);
		byte[] array = MapImageRenderer.Render(out var _, out var _, out var _, scale, lossy: false);
		if (array == null)
		{
			arg.ReplyWith("Failed to render the map (is a map loaded now?)");
			return;
		}
		string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"map_{(global::World.Size)}_{(global::World.Seed)}.png"));
		File.WriteAllBytes(fullPath, array);
		arg.ReplyWith("Saved map render to: " + fullPath);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's tunnel network")]
	public static void rendertunnels(Arg arg)
	{
		RenderMapLayerToFile(arg, "tunnels", MapLayer.TrainTunnels);
	}

	[ServerVar(Clientside = true, Help = "Renders a PNG of the current map's underwater labs, for a specific floor")]
	public static void renderlabs(Arg arg)
	{
		int underwaterLabFloorCount = MapLayerRenderer.GetOrCreate().GetUnderwaterLabFloorCount();
		int num = arg.GetInt(0);
		if (num < 0 || num >= underwaterLabFloorCount)
		{
			arg.ReplyWith($"Floor number must be between 0 and {underwaterLabFloorCount}");
		}
		else
		{
			RenderMapLayerToFile(arg, $"labs_{num}", (MapLayer)(1 + num));
		}
	}

	private static void RenderMapLayerToFile(Arg arg, string name, MapLayer layer)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MapLayerRenderer orCreate = MapLayerRenderer.GetOrCreate();
			orCreate.Render(layer);
			string fullPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, $"{name}_{(global::World.Size)}_{(global::World.Seed)}.png"));
			RenderTexture targetTexture = orCreate.renderCamera.targetTexture;
			Texture2D val = new Texture2D(((Texture)targetTexture).width, ((Texture)targetTexture).height);
			RenderTexture active = RenderTexture.active;
			try
			{
				RenderTexture.active = targetTexture;
				val.ReadPixels(new Rect(0f, 0f, (float)((Texture)targetTexture).width, (float)((Texture)targetTexture).height), 0, 0);
				val.Apply();
				File.WriteAllBytes(fullPath, ImageConversion.EncodeToPNG(val));
			}
			finally
			{
				RenderTexture.active = active;
				Object.DestroyImmediate((Object)(object)val);
			}
			arg.ReplyWith("Saved " + name + " render to: " + fullPath);
		}
		catch (Exception ex)
		{
			Debug.LogWarning((object)ex);
			arg.ReplyWith("Failed to render " + name);
		}
	}

	[ServerVar(Help = "(Generated) Draws flat wireframe boxes in the world showing world bounds (red), terrain margin (yellow), deep sea bounds (cyan), and portal bounds (green/magenta) for the given duration in seconds")]
	public static void drawbounds(Arg arg)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		float duration = arg.GetFloat(0, 60f);
		float y = 1f;
		if ((Object)(object)SingletonComponent<ValidBounds>.Instance != (Object)null)
		{
			DrawBoundsFlat(basePlayer, SingletonComponent<ValidBounds>.Instance.worldBounds, y, duration, Color.red, "WorldBounds");
		}
		if ((bool)TerrainMeta.TerrainRenderer)
		{
			float num = TerrainMeta.Position.x - TerrainMeta.Size.x;
			float num2 = TerrainMeta.Position.x + TerrainMeta.Size.x + TerrainMeta.Size.x;
			float num3 = TerrainMeta.Position.z - TerrainMeta.Size.z;
			float num4 = TerrainMeta.Position.z + TerrainMeta.Size.z + TerrainMeta.Size.z;
			Bounds bounds = default(Bounds);
			((Bounds)(ref bounds)).SetMinMax(new Vector3(num, 0f, num3), new Vector3(num2, 0f, num4));
			DrawBoundsFlat(basePlayer, bounds, y, duration, Color.yellow, "TerrainMargin");
		}
		DrawBoundsFlat(basePlayer, DeepSeaManager.DeepSeaBounds, y, duration, Color.cyan, "DeepSea");
		if (!DeepSea.enabled || !((Object)(object)DeepSeaManager.Get(server: true) != (Object)null))
		{
			return;
		}
		foreach (DeepSeaPortal serverPortal in DeepSeaManager.ServerPortals)
		{
			bool flag = serverPortal.PortalMode == DeepSeaPortal.PortalModeEnum.Entrance;
			Color val = (flag ? Color.green : Color.magenta);
			val = (serverPortal.IsOpen() ? val : val.WithAlpha(0.1f));
			OBB val2 = serverPortal.WorldSpaceBounds();
			DrawBoundsFlat(basePlayer, ((OBB)(ref val2)).ToBounds(), y, duration, val, flag ? "Entrance Portal" : "Exit Portal");
		}
		static void DrawBoundsFlat(BasePlayer player, Bounds val4, float num5, float duration2, Color color, string label)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(((Bounds)(ref val4)).min.x, num5, ((Bounds)(ref val4)).min.z);
			Vector3 val5 = default(Vector3);
			((Vector3)(ref val5))._002Ector(((Bounds)(ref val4)).max.x, num5, ((Bounds)(ref val4)).min.z);
			Vector3 val6 = default(Vector3);
			((Vector3)(ref val6))._002Ector(((Bounds)(ref val4)).max.x, num5, ((Bounds)(ref val4)).max.z);
			Vector3 val7 = default(Vector3);
			((Vector3)(ref val7))._002Ector(((Bounds)(ref val4)).min.x, num5, ((Bounds)(ref val4)).max.z);
			UnityEngine.DDraw.Line(player, val3, val5, color, duration2, distanceFade: false);
			UnityEngine.DDraw.Line(player, val5, val6, color, duration2, distanceFade: false);
			UnityEngine.DDraw.Line(player, val6, val7, color, duration2, distanceFade: false);
			UnityEngine.DDraw.Line(player, val7, val3, color, duration2, distanceFade: false);
			UnityEngine.DDraw.Text(player, val3, label, color, duration2, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, val5, label, color, duration2, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, val6, label, color, duration2, distanceFade: true, zTest: false, 1f);
			UnityEngine.DDraw.Text(player, val7, label, color, duration2, distanceFade: true, zTest: false, 1f);
		}
	}
}
