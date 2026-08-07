using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ThrownWeapon : AttackEntity
{
	[Header("Throw Weapon")]
	public GameObjectRef prefabToThrow;

	public float maxThrowVelocity = 10f;

	public float tumbleVelocity;

	public Vector3 overrideAngle = Vector3.zero;

	public bool canStick = true;

	public bool canThrowUnderwater = true;

	public bool canThrowFromHelicopter = true;

	public GameObject throwObjectRoot;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ThrownWeapon.OnRpcMessage"))
		{
			if (rpc == 1513023343 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoDrop"));
				}
				using (TimeWarning.New("DoDrop"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1513023343u, "DoDrop", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							DoDrop(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in DoDrop");
					}
				}
				return true;
			}
			if (rpc == 1974840882 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - DoThrow"));
				}
				using (TimeWarning.New("DoThrow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(1974840882u, "DoThrow", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							DoThrow(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in DoThrow");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return player.GetInheritedThrowVelocity(direction);
	}

	public void ServerThrow(Vector3 targetPosition)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient || !HasItemAmount() || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null || (!canThrowUnderwater && ownerPlayer.IsHeadUnderwater()) || (!canThrowFromHelicopter && ownerPlayer.IsInAHelicopter()))
		{
			return;
		}
		Vector3 position = ownerPlayer.eyes.position;
		Vector3 val = ownerPlayer.eyes.BodyForward();
		float num = 1f;
		SignalBroadcast(Signal.Throw, string.Empty);
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabToThrow.resourcePath, position, Quaternion.LookRotation((overrideAngle == Vector3.zero) ? (-val) : overrideAngle));
		if ((Object)(object)baseEntity == (Object)null)
		{
			return;
		}
		baseEntity.SetCreatorEntity(ownerPlayer);
		Vector3 val2 = val + Quaternion.AngleAxis(10f, Vector3.right) * Vector3.up;
		float num2 = GetThrowVelocity(position, targetPosition, val2);
		if (float.IsNaN(num2))
		{
			val2 = val + Quaternion.AngleAxis(20f, Vector3.right) * Vector3.up;
			num2 = GetThrowVelocity(position, targetPosition, val2);
			if (float.IsNaN(num2))
			{
				num2 = 5f;
			}
		}
		baseEntity.SetVelocity(val2 * num2 * num);
		if (tumbleVelocity > 0f)
		{
			baseEntity.SetAngularVelocity(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * tumbleVelocity);
		}
		baseEntity.Spawn();
		StartAttackCooldown(repeatDelay);
		UseItemAmount(1);
		if ((Object)(object)(baseEntity as TimedExplosive) != (Object)null)
		{
			Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(ownerPlayer, baseEntity);
		}
	}

	public static float GetThrowVelocity(Vector3 throwPos, Vector3 targetPos, Vector3 aimDir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = targetPos - throwPos;
		Vector2 val2 = new Vector2(val.x, val.z);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		float y = val.y;
		val2 = new Vector2(aimDir.x, aimDir.z);
		float magnitude2 = ((Vector2)(ref val2)).magnitude;
		float y2 = aimDir.y;
		float y3 = Physics.gravity.y;
		return Mathf.Sqrt(0.5f * y3 * magnitude * magnitude / (magnitude2 * (magnitude2 * y - y2 * magnitude)));
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void DoThrow(RPCMessage msg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (HasItemAmount() && !HasAttackCooldown())
		{
			Vector3 eyePos = msg.read.Vector3();
			Vector3 throwVelocityOverride = msg.read.Vector3();
			Vector3 normalized = ((Vector3)(ref throwVelocityOverride)).normalized;
			float throwScale = Mathf.Clamp01(msg.read.Float());
			if (DoValidationChecks(ref eyePos, normalized, msg.player, throwScale))
			{
				Vector3 eyePos2 = eyePos;
				BasePlayer player = msg.player;
				throwVelocityOverride = default(Vector3);
				DoThrowImpl(eyePos2, normalized, player, out var thrownEntity, throwScale, throwVelocityOverride);
				Interface.CallHook("OnExplosiveThrown", msg.player, thrownEntity, this);
				UseItemAmount(1, reduceItemOwnership: false);
			}
		}
	}

	public bool DoValidationChecks(ref Vector3 eyePos, Vector3 eyeDir, BasePlayer owningPlayer, float throwScale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(eyeDir))
		{
			return false;
		}
		if (FloatEx.IsNaNOrInfinity(throwScale))
		{
			return false;
		}
		if (owningPlayer.isMounted || owningPlayer.HasParent())
		{
			eyePos = owningPlayer.eyes.position;
		}
		else if (!ValidateEyePos(owningPlayer, eyePos))
		{
			return false;
		}
		if (!canThrowUnderwater && owningPlayer.IsHeadUnderwater())
		{
			return false;
		}
		if (!canThrowFromHelicopter && owningPlayer.IsInAHelicopter())
		{
			return false;
		}
		return true;
	}

	public void DoThrowImpl(Vector3 eyePos, Vector3 eyeDir, BasePlayer owningPlayer, out BaseEntity thrownEntity, float throwScale = 1f, Vector3 throwVelocityOverride = default(Vector3), Item ownerItemOverride = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		thrownEntity = null;
		BaseEntity baseEntity = (thrownEntity = GameManager.server.CreateEntity(prefabToThrow.resourcePath, eyePos, Quaternion.LookRotation((overrideAngle == Vector3.zero) ? (-eyeDir) : overrideAngle)));
		if (!((Object)(object)baseEntity == (Object)null))
		{
			Item item = ownerItemOverride ?? GetOwnerItem();
			if (item != null && item.instanceData != null && item.HasFlag(Item.Flag.IsOn))
			{
				((Component)baseEntity).gameObject.SendMessage("SetFrequency", (object)item.instanceData.dataInt, (SendMessageOptions)1);
			}
			baseEntity.SetCreatorEntity(owningPlayer);
			baseEntity.skinID = skinID;
			if (throwVelocityOverride != Vector3.zero)
			{
				baseEntity.SetVelocity(throwVelocityOverride);
			}
			else
			{
				baseEntity.SetVelocity(GetInheritedVelocity(owningPlayer, eyeDir) + eyeDir * maxThrowVelocity * throwScale + owningPlayer.estimatedVelocity * 0.5f);
			}
			if (tumbleVelocity > 0f)
			{
				baseEntity.SetAngularVelocity(new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * tumbleVelocity);
			}
			baseEntity.Spawn();
			if (baseEntity is TimedExplosive timedExplosive)
			{
				ItemOwnershipShare itemOwnership = item.TakeOwnershipShare();
				timedExplosive.ItemOwnership = itemOwnership;
				timedExplosive.SetCreator(owningPlayer);
			}
			SetUpThrownWeapon(baseEntity, item);
			StartAttackCooldown(repeatDelay);
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	private void DoDrop(RPCMessage msg)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (!HasItemAmount() || HasAttackCooldown() || (!canThrowUnderwater && msg.player.IsHeadUnderwater()) || (!canThrowFromHelicopter && msg.player.IsInAHelicopter()))
		{
			return;
		}
		Vector3 val = msg.read.Vector3();
		Vector3 val2 = msg.read.Vector3();
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		if (msg.player.isMounted || msg.player.HasParent())
		{
			val = msg.player.eyes.position;
		}
		else if (!ValidateEyePos(msg.player, val))
		{
			return;
		}
		BaseEntity baseEntity = GameManager.server.CreateEntity(prefabToThrow.resourcePath, val, Quaternion.LookRotation(Vector3.up));
		if ((Object)(object)baseEntity == (Object)null)
		{
			return;
		}
		RaycastHit hit = default(RaycastHit);
		if (canStick && Physics.SphereCast(new Ray(val, normalized), 0.05f, ref hit, 1.5f, 1237003025))
		{
			Vector3 point = ((RaycastHit)(ref hit)).point;
			Vector3 normal = ((RaycastHit)(ref hit)).normal;
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			Collider collider = ((RaycastHit)(ref hit)).collider;
			if (Object.op_Implicit((Object)(object)entity) && entity is StabilityEntity && baseEntity is TimedExplosive)
			{
				entity = entity.ToServer<BaseEntity>();
				TimedExplosive timedExplosive = baseEntity as TimedExplosive;
				timedExplosive.onlyDamageParent = true;
				timedExplosive.DoStick(point, normal, entity, collider);
				Facepunch.Rust.Analytics.Azure.OnExplosiveLaunched(msg.player, timedExplosive);
			}
			else
			{
				baseEntity.SetVelocity(normalized);
			}
		}
		else
		{
			baseEntity.SetVelocity(normalized);
		}
		baseEntity.creatorEntity = msg.player;
		baseEntity.skinID = skinID;
		baseEntity.Spawn();
		SetUpThrownWeapon(baseEntity, GetOwnerItem());
		StartAttackCooldown(repeatDelay);
		Interface.CallHook("OnExplosiveDropped", msg.player, baseEntity, this);
		UseItemAmount(1);
	}

	protected virtual void SetUpThrownWeapon(BaseEntity ent, Item ownerItem)
	{
	}
}
