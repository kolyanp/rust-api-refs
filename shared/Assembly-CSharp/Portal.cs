using Facepunch;
using UnityEngine;

public class Portal : IPooled
{
	public Room RoomA;

	public Room RoomB;

	public BlockFace FaceA;

	public BlockFace FaceB;

	public bool PermanentlyOpen;

	public BaseEntity Filler;

	public bool FillerBlocksVision;

	public bool? OverrideOpen;

	public const float SpanDepth = 0.1f;

	public const float SpanReach = 1.5f;

	public bool IsOpen()
	{
		if (OverrideOpen.HasValue)
		{
			return OverrideOpen.Value;
		}
		return ComputeOpen(PermanentlyOpen, (Object)(object)Filler != (Object)null, FillerBlocksVision, (Object)(object)Filler != (Object)null && Filler.IsOpen());
	}

	public static bool ComputeOpen(bool permanentlyOpen, bool hasFiller, bool fillerBlocksVision, bool fillerIsOpen)
	{
		if (permanentlyOpen)
		{
			return true;
		}
		if (!hasFiller)
		{
			return true;
		}
		if (!fillerBlocksVision)
		{
			return true;
		}
		return fillerIsOpen;
	}

	public bool Spans(OBB bounds)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (FaceA == null)
		{
			return false;
		}
		return SpansPlane(bounds, FaceA.WorldCenter, FaceA.WorldNormal);
	}

	public static bool SpansPlane(OBB bounds, Vector3 planePoint, Vector3 planeNormal)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (bounds.SqrDistance(planePoint) > 2.25f)
		{
			return false;
		}
		Vector3 normalized = planeNormal.normalized;
		float num = Mathf.Abs(Vector3.Dot(bounds.extents.x * bounds.right, normalized)) + Mathf.Abs(Vector3.Dot(bounds.extents.y * bounds.up, normalized)) + Mathf.Abs(Vector3.Dot(bounds.extents.z * bounds.forward, normalized));
		float num2 = Vector3.Dot(bounds.position - planePoint, normalized);
		return num - Mathf.Abs(num2) >= 0.1f;
	}

	public Room OtherRoom(Room room)
	{
		if (room == RoomA)
		{
			return RoomB;
		}
		if (room == RoomB)
		{
			return RoomA;
		}
		return null;
	}

	void IPooled.EnterPool()
	{
		RoomA = null;
		RoomB = null;
		FaceA = null;
		FaceB = null;
		PermanentlyOpen = false;
		Filler = null;
		FillerBlocksVision = false;
		OverrideOpen = null;
	}

	void IPooled.LeavePool()
	{
	}
}
