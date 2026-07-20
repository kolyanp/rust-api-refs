using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class PhysicsTumblingEntity : BaseEntity
{
	[ServerVar(Saved = true, Help = "Minimum force applied on collision to cause tumbling")]
	public static float min_tumbling_force = 600f;

	[ServerVar(Saved = true, Help = "Maximum force applied on collision to cause tumbling")]
	public static float max_tumbling_force = 1000f;

	[ServerVar(Saved = true, Help = "Cone angle in degrees for randomizing tumbling force direction")]
	public static float tumbling_force_cone_angle = 30f;

	[ServerVar(Saved = true, Help = "Override for rigidbody drag. Set to -1 to disable.")]
	public static float drag_override = -1f;

	[ServerVar(Saved = true, Help = "Override for rigidbody angular drag. Set to -1 to disable.")]
	public static float angular_drag_override = -1f;

	[ServerVar(Saved = true, Help = "Multiplier for impulse applied to players when ragdolled by this entity")]
	public static float player_impulse_multiplier = 10f;

	[ServerVar(Saved = true, Help = "Minimum velocity required for an object to get tumbling force applied on collision")]
	public static float velocity_threshold_for_tumbling_force = 1.5f;

	private Rigidbody rb;

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null)
		{
			rb = ((Component)this).GetComponent<Rigidbody>();
		}
		if ((Object)(object)hitEntity == (Object)null)
		{
			hitEntity = GameObjectEx.ToBaseEntity(collision.collider);
		}
		if ((Object)(object)hitEntity == (Object)(object)this || ((Object)(object)hitEntity != (Object)null && hitEntity.isServer != base.isServer) || hitEntity is TimedExplosive)
		{
			return;
		}
		float num = Random.Range(min_tumbling_force, max_tumbling_force);
		Vector3 val = rb.linearVelocity;
		if (!(((Vector3)(ref val)).magnitude < velocity_threshold_for_tumbling_force))
		{
			if ((Object)(object)hitEntity == (Object)null)
			{
				Vector3 point = ((ContactPoint)(ref collision.contacts[0])).point;
				val = collision.impulse;
				RagdollPlayers(point, -((Vector3)(ref val)).normalized * player_impulse_multiplier);
			}
			else
			{
				val = collision.impulse;
				Vector3 val2 = RandomVectorInCone(((Vector3)(ref val)).normalized, tumbling_force_cone_angle);
				val2 *= num;
				rb.AddForce(val2, (ForceMode)1);
				Debug.DrawLine(((ContactPoint)(ref collision.contacts[0])).point, ((ContactPoint)(ref collision.contacts[0])).point + val2, Color.gray, 10f);
			}
		}
	}

	public void FixedUpdate()
	{
		if ((Object)(object)rb == (Object)null)
		{
			rb = ((Component)this).GetComponent<Rigidbody>();
		}
		if (drag_override != -1f && rb.linearDamping != drag_override)
		{
			rb.linearDamping = drag_override;
		}
		if (angular_drag_override != -1f && rb.angularDamping != angular_drag_override)
		{
			rb.angularDamping = angular_drag_override;
		}
	}

	private void RagdollPlayers(Vector3 worldPosition, Vector3 impulse)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Vis.Entities(worldPosition, 0.4f, list, 131072, (QueryTriggerInteraction)2);
		foreach (BasePlayer item in list)
		{
			if (item.isServer && !item.IsDead() && !item.InSafeZone())
			{
				item.Ragdoll(item.GetWorldVelocity() + impulse);
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	public static Vector3 RandomVectorInCone(Vector3 normal, float coneAngle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		normal = ((Vector3)(ref normal)).normalized;
		float num = Random.Range(0f, coneAngle);
		float num2 = Random.Range(0f, 360f);
		Vector3 val = Vector3.Cross(normal, Vector3.up);
		if (((Vector3)(ref val)).sqrMagnitude < 0.001f)
		{
			val = Vector3.Cross(normal, Vector3.right);
		}
		((Vector3)(ref val)).Normalize();
		val = Quaternion.AngleAxis(num2, normal) * val;
		Vector3 val2 = Quaternion.AngleAxis(num, val) * normal;
		return ((Vector3)(ref val2)).normalized;
	}
}
