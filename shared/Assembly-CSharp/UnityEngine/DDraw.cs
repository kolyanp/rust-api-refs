using System;

namespace UnityEngine;

public class DDraw : MonoBehaviour
{
	public GUISkin skin;

	public static void BroadcastArrow(Vector3 start, Vector3 end, Color color, float duration = 10f, float headSize = 0.5f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.arrow", duration, color, start, end, headSize, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastLine(Vector3 start, Vector3 end, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.line", duration, color, start, end, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastSphere(Vector3 pos, float radius, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.sphere", duration, color, pos, radius, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastText(Vector3 pos, string text, Color color, float duration = 10f, bool distanceFade = true, bool zTest = false)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.text", duration, color, pos, text, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastCapsule(Vector3 pos, Vector3 rot, float radius, float height, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.capsule", duration, color, pos, rot, radius, height, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastBox(Vector3 pos, Vector3 size, Vector3 rot, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.box", duration, color, pos, $"{size.x} {size.y} {size.z}", rot, distanceFade, zTest);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void BroadcastBounds(Bounds bounds, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		BroadcastBox(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size, Vector3.zero, color, duration, distanceFade, zTest);
	}

	public static void BroadcastClear()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.IsConnected)
				{
					current.SendConsoleCommand("ddraw.clear");
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void Arrow(BasePlayer player, Vector3 start, Vector3 end, Color color, float duration = 10f, float headSize = 0.5f, bool distanceFade = true, bool zTest = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.arrow", duration, color, start, end, headSize, distanceFade, zTest);
		}
	}

	public static void Line(BasePlayer player, Vector3 start, Vector3 end, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.line", duration, color, start, end, distanceFade, zTest);
		}
	}

	public static void Sphere(BasePlayer player, Vector3 pos, float radius, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.sphere", duration, color, pos, radius, distanceFade, zTest);
		}
	}

	public static void Text(BasePlayer player, Vector3 pos, string text, Color color, float duration = 10f, bool distanceFade = true, bool zTest = false, float scale = 2f)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.text", duration, color, pos, text, distanceFade, zTest, scale);
		}
	}

	public static void Capsule(BasePlayer player, Vector3 pos, Vector3 rot, float radius, float height, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.capsule", duration, color, pos, rot, radius, height, distanceFade, zTest);
		}
	}

	public static void Bounds(BasePlayer player, Bounds bounds, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Box(player, ((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size, Vector3.zero, color, duration, distanceFade, zTest);
	}

	public static void Box(BasePlayer player, Vector3 pos, Vector3 size, Vector3 rot, Color color, float duration = 10f, bool distanceFade = true, bool zTest = true)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.box", duration, color, pos, $"{size.x} {size.y} {size.z}", rot, distanceFade, zTest);
		}
	}

	public static void Clear(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && player.IsConnected)
		{
			player.SendConsoleCommand("ddraw.clear");
		}
	}
}
