using System;
using Network;
using Rust.Demo;
using UnityEngine;

public class ThrownBoomerang : BaseEntity
{
	[Header("References")]
	public ItemDefinition boomerangItem;

	[Header("Settings")]
	public float timeToReturnOnArc;

	public float secondsUntilStartArc;

	public float lerpSpeed;

	private const float CATCH_DISTANCE = 1.5f;

	private const float HOMING_TO_PLAYER_DISTANCE = 6f;

	private Vector3 lastMoveDirection;

	private Vector3 gravityVelocity;

	private bool calculated;

	private float returnTimer;

	private float timeToReturn;

	private Vector3 startLocation;

	private Vector3 midLocation;

	private Vector3 endLocation;

	private Vector3 spawnLocation;

	private ThrownBoomerangServerProjectile projectile;

	private BasePlayer creatorPlayer;

	private Boomerang originEntityItem;

	[NonSerialized]
	public ItemOwnershipShare ItemOwnership;

	[NonSerialized]
	public float Condition;

	public override bool PositionTickFixedTime
	{
		protected get
		{
			return true;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ThrownBoomerang.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private void DoBoomerangMove()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		if (Reader.IsActive && Reader.Active.IsScrubbing)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		float num = 15f;
		Vector3 val;
		if (!calculated)
		{
			returnTimer = 0f;
			startLocation = ((Component)this).transform.position;
			endLocation = spawnLocation;
			endLocation += Vector3.up * 1.2f;
			val = endLocation - startLocation;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			Vector3 val2 = Vector3.Cross(Vector3.up, normalized);
			midLocation = (startLocation + endLocation) / 2f;
			midLocation += val2 * num;
			if (base.isServer)
			{
				projectile.ProjectileHandleMovement(state: false);
			}
			calculated = true;
		}
		BasePlayer basePlayer = null;
		if (base.isServer)
		{
			basePlayer = creatorPlayer;
		}
		if ((Object)(object)basePlayer != (Object)null && Vector3.Distance(((Component)basePlayer).transform.position, spawnLocation) <= 6f && IsValidPlayer(basePlayer))
		{
			endLocation = ((Component)basePlayer).transform.position;
			endLocation += Vector3.up * 1.5f;
			val = endLocation - startLocation;
			Vector3 normalized2 = ((Vector3)(ref val)).normalized;
			Vector3 val3 = Vector3.Cross(Vector3.up, normalized2);
			midLocation = (startLocation + endLocation) / 2f;
			midLocation += val3 * num;
		}
		float num2 = returnTimer / timeToReturnOnArc;
		Vector3 val4 = FakePhysicsRope.GetRationalBezierPoint(startLocation, midLocation, endLocation, Mathf.Clamp01(num2));
		if (num2 >= 1f)
		{
			gravityVelocity += Vector3.down * 9.81f * deltaTime;
			lastMoveDirection += gravityVelocity * deltaTime;
			val4 = ((Component)this).transform.position + lastMoveDirection;
		}
		else if (num2 > 0.95f)
		{
			val4 += Vector3.down * 0.03f;
		}
		Vector3 val5 = val4 - ((Component)this).transform.position;
		if (val5 != Vector3.zero && base.isServer)
		{
			projectile.SetVelocity(val5);
			((Component)this).transform.rotation = Quaternion.Slerp(((Component)this).transform.rotation, Quaternion.LookRotation(((Vector3)(ref val5)).normalized), deltaTime * 2f);
		}
		((Component)this).transform.position = Vector3.MoveTowards(((Component)this).transform.position, val4, deltaTime * lerpSpeed);
		if (num2 <= 1f)
		{
			lastMoveDirection = val5;
		}
		returnTimer += deltaTime;
	}

	private bool IsValidPlayer(BasePlayer ply)
	{
		if ((Object)(object)ply == (Object)null)
		{
			return false;
		}
		if (ply.IsDead())
		{
			return false;
		}
		if (ply.IsSleeping())
		{
			return false;
		}
		return true;
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (base.isClient)
		{
			return;
		}
		spawnLocation = ((Component)this).transform.position;
		projectile = ((Component)this).GetComponent<ThrownBoomerangServerProjectile>();
		if (Object.op_Implicit((Object)(object)projectile))
		{
			projectile.InitializeVelocity(((Component)this).transform.forward * projectile.speed);
			projectile.ProjectileHandleMovement(state: true);
			projectile.SetStartPosition(spawnLocation);
			InvokeRepeating(DoBoomerangMove, secondsUntilStartArc, 0f);
			if (!(creatorEntity is BasePlayer basePlayer))
			{
				return;
			}
			creatorPlayer = basePlayer;
			base.OwnerID = creatorPlayer.userID;
			creatorEntity = creatorPlayer;
			Item activeItem = creatorPlayer.GetActiveItem();
			if (activeItem != null)
			{
				if (activeItem.GetHeldEntity() is Boomerang boomerang)
				{
					originEntityItem = boomerang;
				}
				ItemOwnership = activeItem.TakeOwnershipShare();
			}
			Invoke(LateRPC, 0.1f);
			InvokeRepeating(CheckReturnToHand, secondsUntilStartArc, 0f);
		}
		else
		{
			KillThrownBoomerang();
		}
	}

	private void LateRPC()
	{
		Item activeItem = creatorPlayer.GetActiveItem();
		if (activeItem != null && activeItem.GetHeldEntity() is Boomerang)
		{
			ClientRPC(RpcTarget.Player("SetClientPlayer", creatorPlayer), activeItem.uid.Value);
		}
	}

	public void CreateWorldModel(HitInfo info, Vector3 attackDir)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(projectile.scanRange, projectile.radius);
		num /= 2f;
		Item item = ItemManager.Create(boomerangItem, 1, 0uL, isServerSide: true, 0uL);
		BaseEntity baseEntity = null;
		bool flag = false;
		if ((Object)(object)info.HitEntity == (Object)null || !info.HitEntity.IsValid())
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld + -attackDir * num * 0.3f, Quaternion.LookRotation(-attackDir));
			flag = info.HitMaterial != Projectile.WaterMaterialID();
			if (!info.HitEntity.IsValid())
			{
				flag = false;
			}
		}
		else if (info.HitBone == 0)
		{
			Vector3 hitPositionLocal = info.HitPositionLocal;
			baseEntity = item.CreateWorldObject(hitPositionLocal, Quaternion.LookRotation(((Component)info.HitEntity).transform.InverseTransformDirection(((Vector3)(ref attackDir)).normalized)), info.HitEntity);
			flag = false;
		}
		else
		{
			baseEntity = item.CreateWorldObject(info.HitPositionWorld, Quaternion.LookRotation(((Component)info.HitEntity).transform.InverseTransformDirection(((Vector3)(ref attackDir)).normalized)));
			flag = false;
		}
		if (flag)
		{
			DroppedItem droppedItem = baseEntity as DroppedItem;
			if ((Object)(object)droppedItem != (Object)null)
			{
				droppedItem.StickIn();
			}
			else
			{
				((Component)baseEntity).GetComponent<Rigidbody>().isKinematic = true;
			}
		}
		else
		{
			((Component)baseEntity).GetComponent<Rigidbody>().AddTorque(((Vector3)(ref attackDir)).normalized * Random.Range(5f, 10f), (ForceMode)1);
		}
		item.condition = Condition;
		item.SetItemOwnership(ItemOwnership);
		baseEntity.OwnerID = base.OwnerID;
		baseEntity.creatorEntity = creatorEntity;
	}

	public void OnHit()
	{
		if ((Object)(object)originEntityItem != (Object)null)
		{
			Item item = originEntityItem.GetItem();
			if (item != null)
			{
				float num = item.maxCondition * 0.1f;
				Condition -= num;
				item.UseItem();
				item.SetParent(null);
			}
		}
	}

	private void KillThrownBoomerang()
	{
		CancelInvoke(CheckReturnToHand);
		Kill();
	}

	public void CheckReturnToHand()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)creatorPlayer == (Object)null) && !creatorPlayer.IsDead() && !creatorPlayer.IsSleeping() && Vector3.Distance(((Component)creatorPlayer).transform.position, ((Component)this).transform.position) <= 1.5f)
		{
			Item activeItem = creatorPlayer.GetActiveItem();
			if (activeItem != null && activeItem.GetHeldEntity() is Boomerang { HasThrown: not false } boomerang && !((Object)(object)boomerang != (Object)(object)originEntityItem))
			{
				boomerang.SetHasThrown(thrown: false);
				KillThrownBoomerang();
			}
		}
	}

	public ThrownBoomerang()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		timeToReturnOnArc = 3f;
		secondsUntilStartArc = 0.9f;
		lerpSpeed = 20f;
		gravityVelocity = Vector3.zero;
		spawnLocation = Vector3.zero;
		base._002Ector();
	}
}
