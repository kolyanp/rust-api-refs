using Oxide.Core;
using UnityEngine;

public class FlameExplosive : TimedExplosive
{
	public GameObjectRef createOnExplode;

	public bool blockCreateUnderwater;

	public float numToCreate;

	public float minVelocity;

	public float maxVelocity;

	public float spreadAngle;

	public bool forceUpForExplosion;

	public AnimationCurve velocityCurve;

	public AnimationCurve spreadCurve;

	public override void Explode()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		FlameExplode(forceUpForExplosion ? Vector3.up : (-((Component)this).transform.forward));
	}

	public void FlameExplode(Vector3 surfaceNormal)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		if (!base.isServer)
		{
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		if (blockCreateUnderwater && WaterLevel.Test(position, waves: true, volumes: false))
		{
			base.Explode();
			return;
		}
		Collider component = ((Component)this).GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.enabled = false;
		}
		for (int i = 0; (float)i < numToCreate; i++)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(createOnExplode.resourcePath, position);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				float num = (float)i / numToCreate;
				Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(spreadAngle * spreadCurve.Evaluate(num), surfaceNormal);
				float num2 = Random.Range(0f, 360f);
				Quaternion val = Quaternion.Euler(0f, num2, 0f);
				((Component)baseEntity).transform.SetPositionAndRotation(position, val);
				baseEntity.creatorEntity = (((Object)(object)creatorEntity == (Object)null) ? baseEntity : creatorEntity);
				baseEntity.Spawn();
				Interface.CallHook("OnFlameExplosion", this, component);
				Vector3 val2 = ((Vector3)(ref modifiedAimConeDirection)).normalized * Random.Range(minVelocity, maxVelocity) * velocityCurve.Evaluate(num * Random.Range(1f, 1.1f));
				FireBall component2 = ((Component)baseEntity).GetComponent<FireBall>();
				if ((Object)(object)component2 != (Object)null)
				{
					component2.SetDelayedVelocity(val2);
				}
				else
				{
					baseEntity.SetVelocity(val2);
				}
			}
		}
		base.Explode();
	}

	public override void ProjectileImpact(RaycastHit info, Vector3 rayOrigin)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		hitNormal = ((RaycastHit)(ref info)).normal;
		FlameExplode(((RaycastHit)(ref info)).normal);
	}

	public FlameExplosive()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		numToCreate = 10f;
		minVelocity = 2f;
		maxVelocity = 5f;
		spreadAngle = 90f;
		velocityCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 1f)
		});
		spreadCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 1f)
		});
		base._002Ector();
	}
}
