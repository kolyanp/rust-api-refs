using UnityEngine;

public class DDrawCommand
{
	public static string Sphere(Vector3 position, float duration, Color color, float radius, bool distanceFade = true, bool zTest = true)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ConsoleSystem.BuildCommand("ddraw.sphere", duration, color, position, radius, distanceFade, zTest);
	}

	public unsafe static string Box(Vector3 position, float duration, Color color, Vector3 size, Quaternion rotation = default(Quaternion), bool distanceFade = true, bool zTest = true)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		object[] obj = new object[7]
		{
			duration,
			color,
			position,
			((object)(*(Vector3*)(&size))/*cast due to constrained. prefix*/).ToString(),
			null,
			null,
			null
		};
		Quaternion val = default(Quaternion);
		val = ((rotation != val) ? rotation : Quaternion.identity);
		obj[4] = ((Quaternion)(ref val)).eulerAngles;
		obj[5] = distanceFade;
		obj[6] = zTest;
		return ConsoleSystem.BuildCommand("ddraw.box", obj);
	}

	public static string Text(Vector3 position, float duration, Color color, string text, float scale = 2f, bool distanceFade = true, bool zTest = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return ConsoleSystem.BuildCommand("ddraw.text", duration, color, position, text, distanceFade, zTest, scale);
	}

	public static string Line(Vector3 pos1, Vector3 pos2, float duration, Color color, bool distanceFade = true, bool zTest = false)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return ConsoleSystem.BuildCommand("ddraw.line", duration, color, pos1, pos2, distanceFade, zTest);
	}

	public static string Bounds(Bounds bounds, float duration, Color color, bool distanceFade = true, bool zTest = true)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Box(((Bounds)(ref bounds)).center, duration, color, ((Bounds)(ref bounds)).size, Quaternion.identity, distanceFade, zTest);
	}
}
