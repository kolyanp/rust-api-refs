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

	[ServerVar(Saved = true, Help = "Extra distance in metres around the object's colliders that still counts as hitting a player")]
	public static float player_hit_padding = 0.1f;

	private Rigidbody rb;

	private float unscaledMass;

	private Vector3 appliedScale;

	private readonly List<Collider> solidColliders = new List<Collider>();

	public override void ServerInit()
	{
		base.ServerInit();
		rb = ((Component)this).GetComponent<Rigidbody>();
		unscaledMass = rb.mass;
		((Component)this).GetComponentsInChildren<Collider>(solidColliders);
		solidColliders.RemoveAll((Collider c) => c.isTrigger || ((Component)c).gameObject.layer != 15);
		ApplyScale();
	}

	private void ApplyScale()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		appliedScale = ((Component)this).transform.localScale;
		if (appliedScale != Vector3.one)
		{
			networkEntityScale = true;
		}
		rb.mass = unscaledMass * appliedScale.x * appliedScale.y * appliedScale.z;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rb == (Object)null)
		{
			return;
		}
		if ((Object)(object)hitEntity == (Object)null)
		{
			hitEntity = GameObjectEx.ToBaseEntity(collision.collider);
		}
		if (!((Object)(object)hitEntity == (Object)null) && !((Object)(object)hitEntity == (Object)(object)this) && hitEntity.isServer == isServer && !(hitEntity is TimedExplosive))
		{
			Vector3 val = rb.linearVelocity;
			if (!(val.magnitude < velocity_threshold_for_tumbling_force))
			{
				float num = Random.Range(min_tumbling_force, max_tumbling_force) * rb.mass / unscaledMass;
				val = collision.impulse;
				Vector3 val2 = RandomVectorInCone(val.normalized, tumbling_force_cone_angle);
				val2 *= num;
				rb.AddForce(val2, (ForceMode)1);
				Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + val2, Color.gray, 10f);
			}
		}
	}

	public void FixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)rb == (Object)null))
		{
			if (((Component)this).transform.localScale != appliedScale)
			{
				ApplyScale();
				SendNetworkUpdate();
			}
			if (drag_override != -1f && rb.linearDamping != drag_override)
			{
				rb.linearDamping = drag_override;
			}
			if (angular_drag_override != -1f && rb.angularDamping != angular_drag_override)
			{
				rb.angularDamping = angular_drag_override;
			}
			HitPlayers();
		}
	}

	private void HitPlayers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Vector3 linearVelocity = rb.linearVelocity;
		if (linearVelocity.magnitude < velocity_threshold_for_tumbling_force)
		{
			return;
		}
		OBB val = WorldSpaceBounds();
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		Vis.Entities(val.position, val.extents.magnitude + BasePlayer.GetRadius() + player_hit_padding, list, 131072, (QueryTriggerInteraction)2);
		foreach (BasePlayer item in list)
		{
			if (item.isServer && !item.IsDead() && !item.IsRagdolling() && !item.InSafeZone() && IsTouching(item))
			{
				item.Ragdoll(item.GetWorldVelocity() + linearVelocity.normalized * player_impulse_multiplier);
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	private bool IsTouching(BasePlayer player)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float radius = BasePlayer.GetRadius();
		float num = radius + player_hit_padding;
		Vector3 position = ((Component)player).transform.position;
		float height = player.GetHeight();
		foreach (Collider solidCollider in solidColliders)
		{
			for (int i = 0; i < 3; i++)
			{
				Vector3 val = position + Vector3.up * Mathf.Lerp(radius, height - radius, (float)i * 0.5f);
				Vector3 val2 = solidCollider.ClosestPoint(val) - val;
				if (val2.sqrMagnitude <= num * num)
				{
					return true;
				}
			}
		}
		return false;
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
		normal = normal.normalized;
		float num = Random.Range(0f, coneAngle);
		float num2 = Random.Range(0f, 360f);
		Vector3 val = Vector3.Cross(normal, Vector3.up);
		if (val.sqrMagnitude < 0.001f)
		{
			val = Vector3.Cross(normal, Vector3.right);
		}
		val.Normalize();
		val = Quaternion.AngleAxis(num2, normal) * val;
		Vector3 val2 = Quaternion.AngleAxis(num, val) * normal;
		return val2.normalized;
	}
}
