using System;
using System.Linq;
using ConVar;
using Facepunch.Rust;
using Oxide.Core;
using UnityEngine;

public class DroppedItem : WorldItem, IContainerSounds, Hopper.IHopperTarget
{
	public enum DropReasonEnum
	{
		Unknown,
		Player,
		Death,
		Loot
	}

	public class DroppedItemUnderwaterQueue : PersistentObjectWorkQueue<DroppedItem>
	{
		protected override void RunJob(DroppedItem entity)
		{
			if ((Object)(object)entity != (Object)null)
			{
				entity.CheckUnderwaterStatus(canSplash: true);
			}
		}
	}

	[Header("DroppedItem")]
	public GameObjectRef itemModel;

	public GameObjectRef splashEffect;

	[ServerVar(Help = "How many milliseconds to spend on updating underwater drag levels")]
	public static float underwater_drag_budget_ms = 0.05f;

	[ServerVar(Help = "Whether Rigidbody components are removed from DroppedItems when sleeping")]
	public static bool remove_rb_on_sleep = false;

	[ServerVar(Help = "Will broadcast debug ddraw information on ALL dropped items to ALL players")]
	public static bool broadcast_debug_ddraw = false;

	private const Flags FLAG_STUCK = Flags.Reserved1;

	private const Flags FLAG_UNDERWATER = Flags.Reserved2;

	public const Flags FLAG_HOPPERANIMATING = Flags.Reserved3;

	private int originalLayer;

	[NonSerialized]
	public DropReasonEnum DropReason;

	[NonSerialized]
	public ulong DroppedBy;

	[NonSerialized]
	public DateTime DroppedTime;

	[NonSerialized]
	public bool NeverCombine;

	private Rigidbody rB;

	private CollisionDetectionMode originalCollisionMode;

	private Vector3 prevLocalPos;

	private const float SLEEP_CHECK_FREQUENCY = 11f;

	private const float ANGULAR_DRAG = 0.1f;

	private const float AIR_DRAG = 0.1f;

	private const float UNDERWATER_DRAG = 7f;

	private bool hasLastPos;

	private bool hadParent;

	private EntityRef parentRef;

	private Vector3 lastGoodColliderCentre;

	private Vector3 lastGoodPos;

	private Quaternion lastGoodRot;

	private Action cachedSleepCheck;

	private float maxBoundsExtent;

	private readonly Vector3 smallVerticalOffset;

	public static DroppedItemUnderwaterQueue underwaterStatusQueue = new DroppedItemUnderwaterQueue();

	private TimeSince lastUnderwaterFlowImpulse;

	public Collider childCollider { get; private set; }

	private bool StuckInSomething => HasFlag(Flags.Reserved1);

	public SoundDefinition OpenSound
	{
		get
		{
			if (item == null)
			{
				return null;
			}
			ItemModContainer component = ((Component)item.info).GetComponent<ItemModContainer>();
			if ((Object)(object)component == (Object)null)
			{
				return null;
			}
			return component.openSound;
		}
	}

	public SoundDefinition CloseSound
	{
		get
		{
			if (item == null)
			{
				return null;
			}
			ItemModContainer component = ((Component)item.info).GetComponent<ItemModContainer>();
			if ((Object)(object)component == (Object)null)
			{
				return null;
			}
			return component.closeSound;
		}
	}

	public Rigidbody Rigidbody => rB;

	public int NumberOfItemsToTransfer => 1 + ((item.contents != null) ? item.contents.itemList.Count() : 0);

	public float EndPositionToleranceMultiplier => 1f;

	public bool IsSleeping
	{
		get
		{
			if ((Object)(object)rB != (Object)null)
			{
				return rB.IsSleeping();
			}
			return false;
		}
	}

	public BaseEntity ToEntity => this;

	protected override bool CanBePickedUp => !HasFlag(Flags.Reserved3);

	public void TransferAllItemsToContainer(ItemContainer itemContainer, Vector3 itemFallbackPosition)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || itemContainer == null)
		{
			return;
		}
		if (item.contents != null && item.IsBackpack())
		{
			int capacity = item.contents.capacity;
			for (int i = 0; i < capacity; i++)
			{
				if (item.contents != null)
				{
					Item slot = item.contents.GetSlot(i);
					if (slot != null && !slot.MoveToContainer(itemContainer))
					{
						slot.DropAndTossUpwards(itemFallbackPosition);
					}
				}
			}
		}
		if (item != null)
		{
			if (item.MoveToContainer(itemContainer))
			{
				RemoveItem();
			}
			else
			{
				CancelHopper();
			}
		}
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
		ReceiveCollisionMessages(b: true);
		prevLocalPos = ((Component)this).transform.localPosition;
		((PersistentObjectWorkQueue<DroppedItem>)underwaterStatusQueue).Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<DroppedItem>)underwaterStatusQueue).Remove(this);
	}

	public virtual float GetDespawnDuration()
	{
		return item?.GetDespawnDuration() ?? Server.itemdespawn;
	}

	public void IdleDestroy()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Interface.CallHook("OnItemDespawn", item);
		Facepunch.Rust.Analytics.Azure.OnItemDespawn(this, item, (int)DropReason, DroppedBy);
		if (item != null)
		{
			BuriedItems.Instance.Register(item, ((Component)this).transform.position);
		}
		DestroyItem();
		Kill();
	}

	private void SetupRigidbody()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		UpdateItemMass();
		rB.drag = 0.1f;
		rB.angularDrag = 0.1f;
		rB.interpolation = (RigidbodyInterpolation)0;
		rB.collisionDetectionMode = (CollisionDetectionMode)3;
		originalCollisionMode = rB.collisionDetectionMode;
		rB.sleepThreshold = Mathf.Max(0.05f, Physics.sleepThreshold);
	}

	private Vector3 GetColliderCentre()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = childCollider.bounds;
		Vector3 val2 = ((Bounds)(ref val)).center + smallVerticalOffset;
		if (HasParent())
		{
			Matrix4x4 worldToLocalMatrix = ((Component)GetParentEntity()).transform.worldToLocalMatrix;
			val2 = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(val2);
		}
		return val2;
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (item != null && item.MaxStackable() > 1)
		{
			DroppedItem droppedItem = hitEntity as DroppedItem;
			if (!((Object)(object)droppedItem == (Object)null) && droppedItem.item != null)
			{
				droppedItem.OnDroppedOn(this);
			}
		}
	}

	public void OnDroppedOn(DroppedItem di)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || Interface.CallHook("CanCombineDroppedItem", this, di) != null || NeverCombine || di.NeverCombine || !item.CanStack(di.item))
		{
			return;
		}
		Interface.CallHook("OnDroppedItemCombined", this);
		int num = di.item.amount + item.amount;
		if (num <= item.MaxStackable() && num != 0)
		{
			if (di.DropReason == DropReasonEnum.Player)
			{
				DropReason = DropReasonEnum.Player;
			}
			di.item.MigrateItemOwnership(item, di.item.amount);
			di.DestroyItem();
			di.Kill();
			int worldModelIndex = item.info.GetWorldModelIndex(item.amount);
			item.amount = num;
			item.MarkDirty();
			if (GetDespawnDuration() < float.PositiveInfinity)
			{
				Invoke(IdleDestroy, GetDespawnDuration());
			}
			Effect.server.Run("assets/bundled/prefabs/fx/notice/stack.world.fx.prefab", this, 0u, Vector3.zero, Vector3.zero);
			int worldModelIndex2 = item.info.GetWorldModelIndex(item.amount);
			if (worldModelIndex != worldModelIndex2)
			{
				item.Drop(((Component)this).transform.position, Vector3.zero, ((Component)this).transform.rotation);
			}
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		hadParent = false;
		if ((Object)(object)newParent != (Object)null && (Object)(object)newParent != (Object)(object)oldParent)
		{
			OnParented();
		}
		else if ((Object)(object)newParent == (Object)null && (Object)(object)oldParent != (Object)null)
		{
			OnUnparented();
		}
	}

	internal override void OnParentRemoved()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rB == (Object)null)
		{
			base.OnParentRemoved();
			return;
		}
		Vector3 val = ((Component)this).transform.position;
		Quaternion rotation = ((Component)this).transform.rotation;
		SetParent(null);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val + Vector3.up * 2f, Vector3.down, ref val2, 2f, 161546240) && val.y < ((RaycastHit)(ref val2)).point.y)
		{
			val += Vector3.up * 1.5f;
		}
		((Component)this).transform.position = val;
		((Component)this).transform.rotation = rotation;
		Unstick();
		if (GetDespawnDuration() < float.PositiveInfinity)
		{
			Invoke(IdleDestroy, GetDespawnDuration());
		}
	}

	public void StickIn()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: true);
	}

	public void Unstick()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
	}

	private void SleepCheck()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!HasParent() || StuckInSomething)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)rB) || rB.isKinematic)
		{
			if (maxBoundsExtent == 0f)
			{
				float num;
				if (!((Object)(object)childCollider != (Object)null))
				{
					num = Vector3Ex.Max(((Bounds)(ref bounds)).extents);
				}
				else
				{
					Bounds val = childCollider.bounds;
					num = Vector3Ex.Max(((Bounds)(ref val)).extents);
				}
				maxBoundsExtent = num;
			}
			Ray ray = default(Ray);
			((Ray)(ref ray))._002Ector(CenterPoint(), Vector3.down);
			if (!GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var _, maxBoundsExtent + 0.1f, -928830719, (QueryTriggerInteraction)1, this))
			{
				BecomeActive();
			}
		}
		else if (Vector3.SqrMagnitude(((Component)this).transform.localPosition - prevLocalPos) < 0.075f)
		{
			BecomeInactive();
		}
		prevLocalPos = ((Component)this).transform.localPosition;
	}

	public void OnPhysicsNeighbourChanged()
	{
		if (!StuckInSomething)
		{
			BecomeActive();
		}
	}

	public override void OnPositionalNetworkUpdate()
	{
		base.OnPositionalNetworkUpdate();
		if (!HasFlag(Flags.Reserved3))
		{
			CheckValidPosition();
		}
	}

	protected override void TransformChanged()
	{
		base.TransformChanged();
		SingletonComponent<NpcFoodManager>.Instance.Move(this);
	}

	protected override bool ShouldUpdateNetworkPosition()
	{
		if (syncPosition && Object.op_Implicit((Object)(object)rB))
		{
			return !rB.isKinematic;
		}
		return false;
	}

	private void CheckValidPosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)rB) || !Object.op_Implicit((Object)(object)childCollider))
		{
			return;
		}
		using (TimeWarning.New("DroppedItemCheck"))
		{
			Vector3 val = GetColliderCentre();
			Vector3 val2 = val;
			Vector3 val3 = lastGoodColliderCentre;
			bool flag = HasParent();
			BaseEntity baseEntity = GetParentEntity();
			if (flag != hadParent || (Object)(object)baseEntity != (Object)(object)parentRef.Get(serverside: true))
			{
				hasLastPos = false;
			}
			hadParent = flag;
			parentRef = parentEntity;
			if (flag)
			{
				Matrix4x4 localToWorldMatrix = ((Component)GetParentEntity()).transform.localToWorldMatrix;
				val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(val);
				val3 = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyPoint3x4(val3);
			}
			Vector3 val4 = val - val3;
			Ray ray = new Ray(val3, ((Vector3)(ref val4)).normalized);
			if (broadcast_debug_ddraw)
			{
				UnityEngine.DDraw.BroadcastSphere(((Component)this).transform.position, 0.1f, childCollider.enabled ? Color.green : Color.red, 30f, distanceFade: true, zTest: false);
				if (hasLastPos)
				{
					UnityEngine.DDraw.BroadcastLine(val3, val3 + val4, Color.yellow, 30f, distanceFade: true, zTest: false);
				}
			}
			if (hasLastPos && GamePhysics.TraceRealm(GamePhysics.Realm.Server, ray, 0f, out var hitInfo, ((Vector3)(ref val4)).magnitude, 1218511105, (QueryTriggerInteraction)1, this))
			{
				if (broadcast_debug_ddraw)
				{
					UnityEngine.DDraw.BroadcastLine(((Component)this).transform.position, lastGoodPos + smallVerticalOffset, Color.magenta, 30f, distanceFade: true, zTest: false);
				}
				((Component)this).transform.localPosition = lastGoodPos + smallVerticalOffset;
				((Component)this).transform.localRotation = lastGoodRot;
				if (rB.isKinematic)
				{
					return;
				}
				if (flag && Object.op_Implicit((Object)(object)((RaycastHit)(ref hitInfo)).rigidbody))
				{
					BaseEntity a = GetParentEntity();
					BaseEntity baseEntity2 = GameObjectEx.ToBaseEntity(((RaycastHit)(ref hitInfo)).collider);
					if (Object.op_Implicit((Object)(object)baseEntity2) && (GamePhysics.CompareEntity(a, baseEntity2) || GamePhysics.CompareEntity(a, baseEntity2.GetRootParentEntity())))
					{
						BecomeInactive();
					}
				}
				else
				{
					rB.linearVelocity = Vector3.zero;
					rB.angularVelocity = Vector3.zero;
				}
			}
			else
			{
				lastGoodColliderCentre = val2;
				lastGoodPos = ((Component)this).transform.localPosition;
				lastGoodRot = ((Component)this).transform.localRotation;
				hasLastPos = true;
			}
		}
	}

	public void PrepareForHopper()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		if ((Object)(object)childCollider != (Object)null)
		{
			childCollider.enabled = false;
		}
	}

	public void HopperCancelled()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		if ((Object)(object)childCollider != (Object)null)
		{
			childCollider.enabled = true;
		}
	}

	public void CancelHopper()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		if ((Object)(object)Rigidbody != (Object)null)
		{
			Rigidbody.useGravity = true;
		}
		if ((Object)(object)childCollider != (Object)null)
		{
			childCollider.enabled = true;
		}
	}

	private void OnUnparented()
	{
		if (cachedSleepCheck != null)
		{
			CancelInvoke(cachedSleepCheck);
		}
		hasLastPos = false;
	}

	private void OnParented()
	{
		if (!((Object)(object)childCollider == (Object)null) && base.isServer && !StuckInSomething)
		{
			hasLastPos = false;
			if (cachedSleepCheck == null)
			{
				cachedSleepCheck = SleepCheck;
			}
			InvokeRandomized(cachedSleepCheck, 5.5f, 11f, Random.Range(-1.1f, 1.1f));
		}
	}

	public override void PostInitShared()
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		base.PostInitShared();
		GameObject val = null;
		if (0 == 0)
		{
			if (item != null && item.GetWorldModel().isValid)
			{
				val = base.gameManager.CreatePrefab(item.GetWorldModel().resourcePath, ((Component)this).transform);
				val.transform.localScale = item.GetWorldModel().Get().transform.localScale;
			}
			else
			{
				val = base.gameManager.CreatePrefab(itemModel.resourcePath, ((Component)this).transform);
			}
			val.transform.localPosition = Vector3.zero;
			val.transform.localRotation = Quaternion.identity;
			TransformEx.SetLayerRecursive(val, ((Component)this).gameObject.layer);
			childCollider = val.GetComponentInChildren<Collider>();
			if (Object.op_Implicit((Object)(object)childCollider))
			{
				if (HasParent())
				{
					OnParented();
				}
				originalLayer = ((Component)childCollider).gameObject.layer;
			}
		}
		if (base.isServer)
		{
			rB = ((Component)this).gameObject.AddComponent<Rigidbody>();
			SetupRigidbody();
			hasLastPos = false;
			CheckValidPosition();
			CheckUnderwaterStatus(canSplash: false);
			UpdateUnderwaterDrag();
		}
		if (item != null)
		{
			PhysicsEffects component = ((Component)this).gameObject.GetComponent<PhysicsEffects>();
			if ((Object)(object)component != (Object)null)
			{
				component.entity = this;
				if ((Object)(object)item.info.physImpactSoundDef != (Object)null)
				{
					component.physImpactSoundDef = item.info.physImpactSoundDef;
				}
			}
			Buoyancy component2 = val.GetComponent<Buoyancy>();
			if ((Object)(object)component2 != (Object)null && base.isServer)
			{
				component2.rigidBody = rB;
			}
		}
		if (0 == 0)
		{
			val.SetActive(true);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if ((old & Flags.Reserved1) != Flags.Reserved1 && (next & Flags.Reserved1) == Flags.Reserved1)
		{
			BecomeInactive();
		}
		else if ((old & Flags.Reserved1) == Flags.Reserved1 && (next & Flags.Reserved1) != Flags.Reserved1)
		{
			BecomeActive();
		}
		if (base.isServer && (old & Flags.Reserved2) == Flags.Reserved2 != ((next & Flags.Reserved2) == Flags.Reserved2))
		{
			UpdateUnderwaterDrag();
		}
	}

	private void BecomeActive()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("DroppedItem.BecomeActive"))
		{
			if (base.isServer)
			{
				if (!Object.op_Implicit((Object)(object)rB))
				{
					rB = ((Component)this).gameObject.AddComponent<Rigidbody>();
					SetupRigidbody();
				}
				rB.isKinematic = false;
				rB.collisionDetectionMode = originalCollisionMode;
				rB.WakeUp();
				if (HasParent())
				{
					Rigidbody component = ((Component)GetParentEntity()).GetComponent<Rigidbody>();
					if ((Object)(object)component != (Object)null)
					{
						rB.linearVelocity = component.linearVelocity;
						rB.angularVelocity = component.angularVelocity;
					}
				}
				prevLocalPos = ((Component)this).transform.localPosition;
				hasLastPos = false;
				CheckUnderwaterStatus(canSplash: false);
				UpdateUnderwaterDrag();
			}
			if ((Object)(object)childCollider != (Object)null)
			{
				((Component)childCollider).gameObject.layer = originalLayer;
			}
		}
	}

	private void BecomeInactive()
	{
		using (TimeWarning.New("DroppedItem.BecomeInactive"))
		{
			if (base.isServer)
			{
				if (remove_rb_on_sleep)
				{
					Object.Destroy((Object)(object)rB);
					rB = null;
					SendNetworkUpdate();
				}
				else
				{
					rB.collisionDetectionMode = (CollisionDetectionMode)0;
					rB.isKinematic = true;
				}
			}
			if ((Object)(object)childCollider != (Object)null)
			{
				((Component)childCollider).gameObject.layer = 19;
			}
		}
	}

	public void UpdateItemMass()
	{
		if ((Object)(object)rB == (Object)null)
		{
			rB = ((Component)this).GetComponent<Rigidbody>();
		}
		if ((Object)(object)rB == (Object)null || item == null || item.contents?.itemList == null)
		{
			return;
		}
		float num = item.info.GetWorldModelMass();
		ItemModContainer component = ((Component)item.info).GetComponent<ItemModContainer>();
		if ((Object)(object)component != (Object)null)
		{
			_ = component.worldWeightScale;
		}
		foreach (Item item in item.contents.itemList)
		{
			num += item.info.GetWorldModelMass() * component.worldWeightScale;
		}
		if ((Object)(object)component != (Object)null && component.maxWeight > 0f)
		{
			num = Mathf.Min(component.maxWeight, num);
		}
		rB.mass = num;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
	}

	public override bool ShouldInheritNetworkGroup()
	{
		return false;
	}

	private void CheckUnderwaterStatus(bool canSplash)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Rigidbody != (Object)null) || !Rigidbody.IsSleeping())
		{
			bool flag = WaterLevel.Test(((Component)this).transform.position, waves: false, volumes: true, this);
			if ((canSplash & flag) && !HasFlag(Flags.Reserved2) && splashEffect.isValid)
			{
				Effect.server.Run(splashEffect.resourcePath, ((Component)this).transform.position, Vector3.zero);
			}
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved2, flag);
			}
			if (flag && (Object)(object)rB != (Object)null && !rB.IsSleeping() && TimeSince.op_Implicit(lastUnderwaterFlowImpulse) > 1f)
			{
				lastUnderwaterFlowImpulse = TimeSince.op_Implicit(0f - Random.Range(0f, 1f));
				rB.AddForceAtPosition(Random.onUnitSphere, ((Component)this).transform.position + Random.onUnitSphere * 3f, (ForceMode)1);
			}
		}
	}

	private void UpdateUnderwaterDrag()
	{
		if ((Object)(object)rB != (Object)null)
		{
			rB.linearDamping = (HasFlag(Flags.Reserved2) ? 7f : 0.1f);
		}
	}

	public DroppedItem()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		originalLayer = -1;
		smallVerticalOffset = new Vector3(0f, 0.05f, 0f);
		base._002Ector();
	}
}
