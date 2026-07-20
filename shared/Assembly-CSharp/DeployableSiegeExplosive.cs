using System;
using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class DeployableSiegeExplosive : BaseCombatEntity, IIgniteable, ISplashable
{
	public GameObjectRef ExplosionEffect;

	public GameObjectRef ExplosionImpact;

	public Vector3 EffectOffset;

	public Transform ExplosionSpawnPoint;

	public const Flags Lit = Flags.Reserved1;

	public float MinimumFuseTime = 3f;

	public float MaximumFuseTime = 10f;

	public float NeighbourExplodeRadius = 2f;

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		creatorEntity = deployedBy;
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Sprinkler.SplashableGrid.OnParentChanged(this, oldParent, newParent);
	}

	public override void Hurt(HitInfo info)
	{
		if (!base.isClient && !HasFlag(Flags.Reserved1))
		{
			info.damageTypes.ScaleAll(0f);
			base.Hurt(info);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved1, b: true);
			}
			Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (HasFlag(Flags.Reserved1))
		{
			Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
		}
		Sprinkler.SplashableGrid.RegisterEntity(this);
	}

	private void ActuallyExplode()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (ExplosionEffect.isValid)
		{
			Vector3 val = default(Vector3);
			Quaternion rot = default(Quaternion);
			ExplosionSpawnPoint.GetPositionAndRotation(ref val, ref rot);
			BaseEntity baseEntity = GameManager.server.CreateEntity(ExplosionEffect.resourcePath, val, rot);
			ServerProjectile component = ((Component)baseEntity).GetComponent<ServerProjectile>();
			baseEntity.Spawn();
			TimedExplosive timedExplosive = default(TimedExplosive);
			if (((Component)component).TryGetComponent<TimedExplosive>(ref timedExplosive))
			{
				timedExplosive.creatorEntity = creatorEntity;
				timedExplosive.Explode();
			}
			if (ExplosionImpact.isValid)
			{
				Effect.server.Run(ExplosionImpact.resourcePath, val + EffectOffset);
			}
			PooledList<DeployableSiegeExplosive> val2 = Pool.Get<PooledList<DeployableSiegeExplosive>>();
			try
			{
				Vis.Entities(CenterPoint(), NeighbourExplodeRadius, (List<DeployableSiegeExplosive>)(object)val2, 256, (QueryTriggerInteraction)2);
				foreach (DeployableSiegeExplosive item in (List<DeployableSiegeExplosive>)(object)val2)
				{
					if (item.isServer && !item.HasFlag(Flags.Reserved1) && CanSee(val, item.ExplosionSpawnPoint.position))
					{
						item.Hurt(3f, DamageType.Heat, this);
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		Kill();
	}

	public void Ignite(Vector3 fromPos)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		Invoke(ActuallyExplode, Random.Range(MinimumFuseTime, MaximumFuseTime));
	}

	public bool CanIgnite()
	{
		return !HasFlag(Flags.Reserved1);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return HasFlag(Flags.Reserved1);
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
		CancelInvoke(ActuallyExplode);
		return 0;
	}

	private void OnGroundMissing()
	{
		ActuallyExplode();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		Sprinkler.SplashableGrid.DeregisterEntity(this);
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (!HasFlag(Flags.Reserved1))
		{
			return base.ShouldDisplayPickupOption(player);
		}
		return false;
	}
}
