using UnityEngine;

public class LeadingRope : FakePhysicsRope
{
	[Tooltip("Rope length at the near and the far end of Min Max Distance.")]
	public Vector2 minMaxLength = new Vector2(1f, 6f);

	[Tooltip("The gap between the two ends that the length above is read off.")]
	public Vector2 minMaxDistance = new Vector2(0.5f, 5f);

	[Tooltip("Where the rope sits on the holder you are looking through, from their eyes and turned with them. Just below the bottom of the view, so the rope comes in from the edge of the screen.")]
	public Vector3 firstPersonOffset = new Vector3(0.18f, -0.3f, 0.12f);

	public LeadingRope()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
	}
}
