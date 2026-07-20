using UnityEngine;

public class SocketMod_TerrainBoundsCheck : SocketMod
{
	private static Phrase lastError = new Phrase("", "");

	protected override Phrase ErrorPhrase => lastError;

	public static bool IsInTerrainBounds(Vector3 vPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return !TerrainMeta.OutOfBounds(vPoint);
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vPoint = place.position + place.rotation * worldPosition;
		lastError = null;
		if (IsInTerrainBounds(vPoint))
		{
			return true;
		}
		if (lastError == null)
		{
			lastError = ConstructionErrors.NotInTerrain;
		}
		return false;
	}
}
