using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockLeadingRope : LeadingRope
{
	[Tooltip("Where the rope sits on the holder when it is the player you are looking through, whose own hands are not drawn.")]
	public Vector3 firstPersonOffset = new Vector3(0.05f, 1.03f, 0f);

	public LivestockLeadingRope()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
	}
}
