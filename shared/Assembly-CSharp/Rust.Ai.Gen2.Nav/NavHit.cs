using UnityEngine.AI;

namespace Rust.Ai.Gen2.Nav;

public struct NavHit
{
	public NavVector3 position;

	public NavVector3 normal;

	public float distance;

	public int mask;

	public bool hit;

	public static NavHit FromUnity(in NavMeshHit unityHitNS)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		NavHit result = default;
		NavMeshHit val = unityHitNS;
		result.position = new NavVector3(val.position);
		val = unityHitNS;
		result.normal = new NavVector3(val.normal);
		val = unityHitNS;
		result.distance = val.distance;
		val = unityHitNS;
		result.mask = val.mask;
		val = unityHitNS;
		result.hit = val.hit;
		return result;
	}

	public NavMeshHit ToUnity()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		NavMeshHit result = default;
		result.position = position.Value;
		result.normal = normal.Value;
		result.distance = distance;
		result.mask = mask;
		result.hit = hit;
		return result;
	}
}
