using UnityEngine;

public class SocketMod_Inside : SocketMod
{
	public bool wantsInside = true;

	public bool customDirections;

	public Vector3[] customRayDirections;

	private static readonly Vector3[] outsideLookupDirs;

	protected override Phrase ErrorPhrase
	{
		get
		{
			if (!wantsInside)
			{
				return ConstructionErrors.WantsOutside;
			}
			return ConstructionErrors.WantsInside;
		}
	}

	private Vector3[] directions
	{
		get
		{
			if (!customDirections)
			{
				return outsideLookupDirs;
			}
			return customRayDirections;
		}
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = place.position + place.rotation * baseSocket.localPosition;
		Quaternion val2 = place.rotation * baseSocket.localRotation;
		Vector3 pos = val + val2 * localPosition;
		Quaternion rotation = val2 * localRotation;
		bool flag = IsOutside(pos, rotation, directions);
		return !wantsInside == flag;
	}

	public static bool IsOutside(Vector3 pos, Transform tr, int layerMask = 136380416)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return IsOutside(pos, tr.rotation, outsideLookupDirs, layerMask);
	}

	public static bool IsOutside(Vector3 pos, Transform tr, Vector3[] dirs, int layerMask = 136380416)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return IsOutside(pos, tr.rotation, dirs, layerMask);
	}

	public static bool IsOutside(Vector3 pos, Quaternion rotation, Vector3[] dirs, int layerMask = 136380416)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SocketMod_Inside.IsOutside"))
		{
			float num = 20f;
			int num2 = 0;
			bool flag = true;
			RaycastHit val2 = default(RaycastHit);
			for (int i = 0; i < dirs.Length; i++)
			{
				Vector3 val = rotation * dirs[i];
				if (Physics.Raycast(new Ray(pos, val), ref val2, num - 0.5f, layerMask))
				{
					if (GameObjectEx.IsOnLayers(((Component)((RaycastHit)(ref val2)).collider).gameObject, layerMask))
					{
						num2++;
					}
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				return num2 < 2;
			}
			return true;
		}
	}

	static SocketMod_Inside()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = new Vector3[8];
		Vector3 val = new Vector3(1f, 1f, 0f);
		array[0] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, -1f, 0f);
		array[1] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 1f, 1f);
		array[2] = ((Vector3)(ref val)).normalized;
		val = new Vector3(-1f, 1f, 0f);
		array[3] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 0f, 1f);
		array[4] = ((Vector3)(ref val)).normalized;
		val = new Vector3(0f, 1f, 0f);
		array[5] = ((Vector3)(ref val)).normalized;
		val = new Vector3(1f, 0f, 0.5f);
		array[6] = ((Vector3)(ref val)).normalized;
		val = new Vector3(-1f, 0f, 0.5f);
		array[7] = ((Vector3)(ref val)).normalized;
		outsideLookupDirs = (Vector3[])(object)array;
	}
}
