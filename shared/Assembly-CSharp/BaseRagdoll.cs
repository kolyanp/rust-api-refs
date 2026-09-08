using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class BaseRagdoll : BaseMountable
{
	[SerializeField]
	[Header("Ragdolling")]
	private Ragdoll Ragdoll;

	[SerializeField]
	private PlayerBonePosData BonePosData;

	[SerializeField]
	private List<DamageTypeEntry> impactDamage;

	[SerializeField]
	private List<Rigidbody> flailBodies;

	private EntityRef<BasePlayer> parentPlayer;

	private BaseEntity initiator;

	private bool dieOnImpact;

	private float lastMovingTime;

	private float largestNegYVelocityOnCollision;

	private bool inTheAir;

	private bool flailInAir;

	private float spinDampening;

	private Vector3 ragdollSpinDirection;

	private bool matchPlayerGravity;

	private int clippedFrameCount;

	private Vector3 lastTransformPos;

	private Vector3 lastEyePos;

	private Vector3 lastPelvisPoint;

	private List<(Vector3, Quaternion)> lastRagdollRbPosRot;

	public GameObjectRef fleshImpact;

	[ClientVar(Help = "(Generated) When enabled, draws debug visualisations for ragdoll physics state including bone positions and velocities")]
	public static bool debug_vis;

	protected override bool BypassClothingMountBlocks => true;

	public override bool DirectlyMountable()
	{
		return false;
	}

	public override void Save(SaveInfo info)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.temporaryRagdoll = Pool.Get<TemporaryRagdoll>();
		if (parentPlayer.IsValid(base.isServer))
		{
			info.msg.temporaryRagdoll.parentID = parentPlayer.uid;
			info.msg.temporaryRagdoll.mountPose = (int)mountPose;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.temporaryRagdoll != null)
		{
			Load(info.msg.temporaryRagdoll);
		}
	}

	private void Load(TemporaryRagdoll tempRagdoll)
	{
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.OnPlayerDismounted(player);
		player.SetPlayerFlag(BasePlayer.PlayerFlags.Ragdolling, b: false);
		PlayerEyes eyes = player.eyes;
		Quaternion rotation = player.eyes.rotation;
		eyes.rotation = Quaternion.Euler(Vector3Ex.WithX(((Quaternion)(ref rotation)).eulerAngles, 0f));
		if (dieOnImpact)
		{
			KillPlayerImpact(player, doRadiusDamage: true);
		}
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
			val.mass = 10f;
			val.linearDamping = 0f;
			val.angularDamping = 0f;
		}
		val.useGravity = true;
		val.collisionDetectionMode = (CollisionDetectionMode)3;
		val.sleepThreshold = Mathf.Max(0.05f, Physics.sleepThreshold);
		lastMovingTime = Time.time;
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		GameObjectExtensions.SetIgnoreCollisions(((Component)this).gameObject, ((Component)GetMounted()).gameObject, true);
		Invoke(StopRagdolling, 10f);
	}

	public override void VehicleFixedUpdate()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseRagdoll.VehicleFixedUpdate"))
		{
			base.VehicleFixedUpdate();
			BasePlayer mounted = GetMounted();
			if ((Object)(object)mounted == (Object)null)
			{
				Kill();
				return;
			}
			AdjustForClipping();
			Vector3 val = rigidBody.linearVelocity;
			if (!(((Vector3)(ref val)).magnitude > 2f))
			{
				val = rigidBody.angularVelocity;
				if (!(((Vector3)(ref val)).magnitude > 2f))
				{
					goto IL_0074;
				}
			}
			lastMovingTime = Time.time;
			goto IL_0074;
			IL_0074:
			if (matchPlayerGravity)
			{
				Vector3 val2 = 2.5f * Physics.gravity - Physics.gravity;
				foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
				{
					rigidbody.AddForce(val2, (ForceMode)5);
				}
			}
			if (inTheAir && flailInAir)
			{
				foreach (Rigidbody flailBody in flailBodies)
				{
					Vector3 val3 = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) * (Random.Range(5f, 10f) * spinDampening);
					flailBody.AddForce(val3 * 15f, (ForceMode)5);
				}
				rigidBody.AddTorque(ragdollSpinDirection * spinDampening, (ForceMode)5);
				spinDampening *= 0.98f;
			}
			if (largestNegYVelocityOnCollision < 0f)
			{
				if (Object.op_Implicit((Object)(object)mounted))
				{
					mounted.ApplyFallDamageFromVelocity(largestNegYVelocityOnCollision);
				}
				largestNegYVelocityOnCollision = 0f;
			}
			if (Time.time > lastMovingTime + 1.25f)
			{
				CancelInvoke(StopRagdolling);
				StopRagdolling();
			}
		}
	}

	private void AdjustForClipping()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("AdjustForClipping"))
		{
			Vector3 position = lastTransformPos;
			lastTransformPos = ((Component)this).transform.position;
			Vector3 start = lastEyePos;
			Vector3 end = (lastEyePos = GetMounted().eyes.position);
			Vector3 start2 = lastPelvisPoint;
			Vector3 end2 = (lastPelvisPoint = Ragdoll.primaryBody.position);
			BasePlayer basePlayer = parentPlayer.Get(serverside: true);
			Vector3 linearVelocity = rigidBody.linearVelocity;
			bool flag = ((Vector3)(ref linearVelocity)).sqrMagnitude > 3f;
			bool flag2 = false;
			bool flag3 = false;
			List<RaycastHit> hits = Pool.Get<List<RaycastHit>>();
			bool flag4 = flag && ClippedOnPath(start, end, 0.3f, in hits, basePlayer);
			bool flag5 = ClippedOnPath(start, end, 0f, in hits, basePlayer);
			flag2 |= flag4 | flag5;
			flag3 |= flag5;
			bool flag6 = flag && ClippedOnPath(start2, end2, 0.3f, in hits, basePlayer);
			bool flag7 = ClippedOnPath(start2, end2, 0f, in hits, basePlayer);
			flag2 |= flag6 | flag7;
			flag3 |= flag7;
			Pool.FreeUnmanaged<RaycastHit>(ref hits);
			if (!flag2)
			{
				for (int i = 0; i < Ragdoll.rigidbodies.Count; i++)
				{
					Rigidbody val = Ragdoll.rigidbodies[i];
					lastRagdollRbPosRot[i] = (val.position, val.rotation);
				}
				return;
			}
			if (flag3 && ++clippedFrameCount >= 3)
			{
				basePlayer.Hurt(new HitInfo(initiator, basePlayer, DamageType.Blunt, 1000f));
				StopRagdolling();
				return;
			}
			for (int j = 0; j < Ragdoll.rigidbodies.Count; j++)
			{
				Rigidbody val2 = Ragdoll.rigidbodies[j];
				if (!((Object)(object)val2 == (Object)null))
				{
					var (position2, rotation) = lastRagdollRbPosRot[j];
					val2.position = position2;
					val2.rotation = rotation;
					val2.linearVelocity = Vector3.zero;
					val2.angularVelocity = Vector3.zero;
				}
			}
			((Component)this).transform.position = position;
			lastTransformPos = position;
			lastEyePos = start;
			lastPelvisPoint = start2;
		}
	}

	private bool ClippedOnPath(Vector3 start, Vector3 end, float radius, in List<RaycastHit> hits, BasePlayer ignorePlayer)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		Vector3 val = end - start;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return false;
		}
		val /= magnitude;
		Ray ray = new Ray(start, val);
		hits.Clear();
		GamePhysics.TraceAllUnordered(ray, radius, hits, magnitude, -910884607, (QueryTriggerInteraction)1);
		foreach (RaycastHit hit in hits)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (!GamePhysics.CompareEntity(entity, this) && !GamePhysics.CompareEntity(entity, ignorePlayer))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void InitFromPlayer(BasePlayer bp, Vector3 velocityOverride = default(Vector3), bool matchPlayerGravity = true, bool flailInAir = false, bool dieOnImpact = false, BaseEntity initiator = null)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		parentPlayer.Set(bp);
		lastEyePos = bp.eyes.position;
		if (bp.isMounted)
		{
			mountPose = bp.GetMounted().mountPose;
		}
		PlayerBonePosData.BonePosData bonePositionData = BonePosData.GetBonePositionData(bp.playerFlags, bp.modelState);
		if (bonePositionData != null)
		{
			model.skeleton.CopyFrom(bonePositionData.bonePositions, bonePositionData.boneRotations, true);
			Transform transform = model.skeleton.Bones[0].transform;
			transform.localEulerAngles += bonePositionData.rootRotationOffset;
		}
		float x = ((Component)bp).transform.eulerAngles.x;
		Quaternion bodyRotation = bp.eyes.bodyRotation;
		Quaternion val = Quaternion.Euler(x, ((Quaternion)(ref bodyRotation)).eulerAngles.y, ((Component)bp).transform.eulerAngles.z);
		((Component)this).transform.SetPositionAndRotation(((Component)bp).transform.position, val);
		lastTransformPos = ((Component)this).transform.position;
		Ragdoll.ServerInit();
		rigidBody.linearDamping = 0f;
		rigidBody.angularDamping = 0f;
		inTheAir = true;
		Vector3 val2 = ((velocityOverride != Vector3.zero) ? velocityOverride : (bp.isMounted ? bp.GetMountVelocity() : bp.estimatedVelocity));
		rigidBody.AddForce(val2, (ForceMode)1);
		lastRagdollRbPosRot = new List<(Vector3, Quaternion)>(Ragdoll.rigidbodies.Count);
		foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
		{
			rigidbody.linearDamping = 0f;
			rigidbody.angularDamping = 0f;
			rigidbody.AddForceAtPosition(val2 * rigidbody.mass, ((Component)rigidbody).transform.position, (ForceMode)1);
			rigidbody.collisionDetectionMode = (CollisionDetectionMode)3;
			lastRagdollRbPosRot.Add((rigidbody.position, rigidbody.rotation));
		}
		lastPelvisPoint = Ragdoll.primaryBody.position;
		this.flailInAir = flailInAir;
		if (flailInAir)
		{
			spinDampening = 1f;
			Vector3 zero = Vector3.zero;
			((Vector3)(ref zero))[Random.Range(0, 3)] = 1f;
			ragdollSpinDirection = zero * 0.8f;
		}
		if (Object.op_Implicit((Object)(object)initiator))
		{
			GameObjectExtensions.SetIgnoreCollisions(((Component)this).gameObject, ((Component)initiator).gameObject, true);
		}
		this.matchPlayerGravity = matchPlayerGravity;
		this.initiator = initiator;
		this.dieOnImpact = dieOnImpact;
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		List<Collider> list = Pool.Get<List<Collider>>();
		((Component)this).GetComponentsInChildren<Collider>(list);
		foreach (Collider item in list)
		{
			item.enabled = false;
		}
		bool dismountPosition = base.GetDismountPosition(player, out res, silent);
		foreach (Collider item2 in list)
		{
			item2.enabled = true;
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return dismountPosition;
	}

	private void StopRagdolling()
	{
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted != (Object)null)
		{
			mounted.SetPlayerFlag(BasePlayer.PlayerFlags.Ragdolling, b: false);
		}
		DismountAllPlayers();
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override bool AllowPlayerInstigatedDismount(BasePlayer player)
	{
		object obj = Interface.CallHook("CanRagdollDismount", this, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	protected void ProcessCollision(Collision collision, BaseEntity hitEntity, Rigidbody ourRigidbody)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient || collision == null || (Object)(object)collision.gameObject == (Object)null || (Object)(object)collision.gameObject == (Object)null)
		{
			return;
		}
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null)
		{
			return;
		}
		if (dieOnImpact)
		{
			KillPlayerImpact(mounted, doRadiusDamage: true);
		}
		else
		{
			largestNegYVelocityOnCollision = Mathf.Min(largestNegYVelocityOnCollision, 0f - collision.relativeVelocity.y);
		}
		if (!inTheAir)
		{
			return;
		}
		inTheAir = false;
		if (!flailInAir)
		{
			return;
		}
		rigidBody.linearDamping = 1f;
		rigidBody.angularDamping = 1f;
		foreach (Rigidbody rigidbody in Ragdoll.rigidbodies)
		{
			rigidbody.linearDamping = 1f;
			rigidbody.angularDamping = 1f;
		}
	}

	private void KillPlayerImpact(BasePlayer mounted, bool doRadiusDamage)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Effect.server.Run(mounted.fallDamageEffect.resourcePath, ((Component)this).transform.position, Vector3.zero);
		Effect.server.Run(fleshImpact.resourcePath, ((Component)this).transform.position, Vector3.zero);
		if (doRadiusDamage)
		{
			DamageUtil.RadiusDamage(mounted, initiator, ((Component)mounted).transform.position, 1f, 3.5f, impactDamage, 133120, useLineOfSight: true, ignoreAI: false, ignoreAttackingPlayer: false, extendedLineOfSight: false, null, removeWallpaper: false, includeBoatBuildingPieces: true, mounted);
		}
		Invoke(delegate
		{
			StopRagdolling();
			mounted.Hurt(new HitInfo(initiator, mounted, DamageType.Blunt, 1000f));
		}, 1f);
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (base.isServer)
		{
			ProcessCollision(collision, hitEntity, rigidBody);
		}
	}
}
