using ConVar;
using UnityEngine;

public class OverfishedArea : DepletedArea
{
	protected override float Radius => Fishing.overfishedAreaRadius;

	protected override float DurationMinutes => Fishing.overfishedAreaDurationMinutes;

	protected override bool DebugEnabled => Fishing.debugOverfishing;

	public static OverfishedArea GetOverfishedAreaAtPosition(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return DepletedArea.GetAtPosition<OverfishedArea>(position, Fishing.overfishedAreaRadius);
	}
}
