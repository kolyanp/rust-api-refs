using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class TinCanAlarm : StorageContainer, IDetector
{
	[Serializable]
	private struct Ammo
	{
		[ItemSelector]
		public ItemDefinition item;

		public GameObject go;
	}

	[Space]
	public float maxWireLength = 10f;

	private const int WIRE_PLACEMENT_LAYER = 1084293377;

	private const float WIRE_PLACEMENT_DISTANCE = 3f;

	[Space]
	public LineRenderer lineRenderer;

	public Transform wireOrigin;

	public Transform wireOriginClient;

	public PlayerDetectionTrigger trigger;

	public Transform wireEndCollider;

	public GroundWatch groundWatch;

	public GroundWatch wireGroundWatch;

	public Animator animator;

	[Space]
	public SoundDefinition alarmSoundDef;

	public SoundDefinition armSoundDef;

	[SerializeField]
	private Ammo[] ammoPrefabs;

	public GameObject ammoParent;

	public Transform throwPoint;

	private ItemDefinition loadedAmmoDef;

	public Vector3 endPoint;

	private const Flags Flag_Used = Flags.Reserved5;

	public BaseEntity lastTriggerEntity;

	public float lastTriggerTime;

	private BasePlayer usingPlayer;

	private Item loadedAmmoItem;

	public Transform WireOrigin
	{
		get
		{
			_ = base.isServer;
			return wireOrigin;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TinCanAlarm.OnRpcMessage"))
		{
			if (rpc == 3384266798u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_SetEndPoint"));
				}
				using (TimeWarning.New("RPC_SetEndPoint"))
				{
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
							RPC_SetEndPoint(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_SetEndPoint");
					}
				}
				return true;
			}
			if (rpc == 3516830045u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StartArming"));
				}
				using (TimeWarning.New("SERVER_StartArming"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3516830045u, "SERVER_StartArming", this, player, 3f))
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
							SERVER_StartArming(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_StartArming");
					}
				}
				return true;
			}
			if (rpc == 3508772935u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StopArming"));
				}
				using (TimeWarning.New("SERVER_StopArming"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_StopArming(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_StopArming");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	public void RPC_SetEndPoint(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		Vector3 val = msg.read.Vector3();
		if (!(Vector3.Distance(player.eyes.position, val) > 3f) && !(Vector3.Distance(wireOrigin.position, val) > maxWireLength) && player.IsVisibleAndCanSee(val) && !IsGoingThroughWalls(val) && IsInValidVolume(val) && IsOnValidEntities(val) && player.CanBuild())
		{
			endPoint = val;
			UpdateTrigger();
			UpdateWireTip();
			SendNetworkUpdate();
			PlayerStopsArming(player);
		}
	}

	private bool IsGoingThroughWalls(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float maxDistance = Vector3.Distance(wireOrigin.position, position);
		Vector3 val = position - wireOrigin.position;
		bool flag = GamePhysics.Trace(new Ray(wireOrigin.position, val), 0f, out var _, maxDistance, 1218519297, (QueryTriggerInteraction)1, this);
		if (!flag)
		{
			flag = GamePhysics.Trace(new Ray(position, -val), 0f, out var _, maxDistance, 1218519297, (QueryTriggerInteraction)1, this);
		}
		return flag;
	}

	private bool IsInValidVolume(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(position, 0.1f, list, 536870912, (QueryTriggerInteraction)2);
		bool result = true;
		foreach (Collider item in list)
		{
			if (((Component)item).gameObject.HasCustomTag(GameObjectTag.BlockPlacement))
			{
				result = false;
				break;
			}
			if (!((Object)(object)ColliderEx.GetMonument(item) != (Object)null))
			{
				ColliderInfo component = ((Component)item).GetComponent<ColliderInfo>();
				if (!((Object)(object)component != (Object)null) || !component.HasFlag(ColliderInfo.Flags.Tunnels))
				{
					result = false;
				}
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return result;
	}

	private bool IsOnValidEntities(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, 0.1f, list, 1084293377, (QueryTriggerInteraction)2);
		bool result = true;
		foreach (BaseEntity item in list)
		{
			if (item is AnimatedBuildingBlock || item is ElevatorLift || item is Elevator)
			{
				result = false;
				break;
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		return result;
	}

	public bool IsUsed()
	{
		return HasFlag(Flags.Reserved5);
	}

	private bool IsArmed()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return endPoint != Vector3.zero;
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player))
		{
			return (Object)(object)player.GetBuildingPrivilege() != (Object)null;
		}
		return false;
	}

	public bool ShouldTrigger()
	{
		return IsArmed();
	}

	public void OnObjects()
	{
	}

	public void OnObjectAdded(GameObject obj, Collider col)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return;
		}
		if (baseEntity is BuildingBlock && IsGoingThroughWalls(endPoint))
		{
			CutWire();
			return;
		}
		if (baseEntity is BasePlayer { isMounted: not false } basePlayer)
		{
			baseEntity = basePlayer.GetMounted();
		}
		else
		{
			BaseEntity baseEntity2 = baseEntity.GetParentEntity();
			if ((Object)(object)baseEntity2 != (Object)null)
			{
				baseEntity = baseEntity2;
			}
		}
		if ((!(Time.realtimeSinceStartup - lastTriggerTime < 1f) || !((Object)(object)baseEntity == (Object)(object)lastTriggerEntity)) && (baseEntity is BasePlayer || baseEntity is Door || baseEntity is BaseNpc || baseEntity is BaseVehicle || baseEntity is Elevator || baseEntity is Lift))
		{
			lastTriggerTime = Time.realtimeSinceStartup;
			lastTriggerEntity = baseEntity;
			TriggerAlarm();
		}
	}

	public void OnEmpty()
	{
	}

	public void TriggerAlarm()
	{
		ClientRPC(RpcTarget.NetworkGroup("RPC_TriggerAlarm"));
		ThrowLoadedItem();
	}

	private void ThrowLoadedItem()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (loadedAmmoItem != null)
		{
			ThrownWeapon thrownWeapon = TryGetHeldEntity(loadedAmmoItem);
			if (!((Object)(object)thrownWeapon == (Object)null) && loadedAmmoItem.amount > 0 && !thrownWeapon.HasAttackCooldown())
			{
				BasePlayer owningPlayer = BasePlayer.FindByID(base.OwnerID);
				thrownWeapon.DoThrowImpl(throwPoint.position, throwPoint.forward, owningPlayer, out var _, 1f, throwPoint.forward * 1f, loadedAmmoItem);
				loadedAmmoItem.UseItem();
				loadedAmmoItem = null;
				loadedAmmoDef = null;
				SendNetworkUpdateImmediate();
			}
		}
	}

	private ThrownWeapon TryGetHeldEntity(Item item)
	{
		if (item == null)
		{
			return null;
		}
		BaseEntity heldEntity = item.GetHeldEntity();
		if ((Object)(object)heldEntity == (Object)null)
		{
			return null;
		}
		ThrownWeapon thrownWeapon = heldEntity as ThrownWeapon;
		if ((Object)(object)thrownWeapon != (Object)null)
		{
			return thrownWeapon;
		}
		return null;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if ((Object)(object)TryGetHeldEntity(item) == (Object)null)
		{
			return false;
		}
		return base.ItemFilter(item, targetSlot);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		loadedAmmoItem = (added ? item : null);
		loadedAmmoDef = (added ? item.info : null);
		SendNetworkUpdateImmediate();
	}

	public void ServerOnWireDeploying()
	{
		if (!usingPlayer.IsValid() || !usingPlayer.IsConnected)
		{
			PlayerStopsArming(usingPlayer);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SERVER_StartArming(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!IsUsed() && player.CanBuild())
		{
			PlayerStartsArming(player);
		}
	}

	[RPC_Server]
	public void SERVER_StopArming(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)msg.player != (Object)(object)usingPlayer) && player.CanBuild())
		{
			PlayerStopsArming(player);
		}
	}

	public void PlayerStartsArming(BasePlayer player)
	{
		if (!IsUsed() && !((Object)(object)player == (Object)null))
		{
			usingPlayer = player;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: true);
			}
			if (IsInvoking(ServerOnWireDeploying))
			{
				CancelInvoke(ServerOnWireDeploying);
			}
			InvokeRepeating(ServerOnWireDeploying, 0f, 0f);
			ClientRPC(RpcTarget.Player("CLIENT_StartArming", player));
		}
	}

	public void PlayerStopsArming(BasePlayer player)
	{
		usingPlayer = null;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		CancelInvoke(ServerOnWireDeploying);
		ClientRPC(RpcTarget.Player("CLIENT_StopArming", player));
	}

	public void CutWire()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TriggerAlarm();
		endPoint = Vector3.zero;
		UpdateTrigger();
		UpdateWireTip();
		SendNetworkUpdate();
	}

	private void UpdateWireTip()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			if (!IsArmed())
			{
				ComponentExtensions.SetActive<Transform>(wireEndCollider, false);
				return;
			}
			wireEndCollider.position = endPoint;
			ComponentExtensions.SetActive<Transform>(wireEndCollider, true);
		}
	}

	private void OnGroundMissing()
	{
		if (!base.IsDestroyed && !base.isClient)
		{
			if (!groundWatch.OnGround())
			{
				Kill(DestroyMode.Gib);
			}
			else if (!wireGroundWatch.OnGround())
			{
				CutWire();
			}
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (base.isServer)
		{
			PlayerStartsArming(deployedBy);
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (info.hasDamage && !info.damageTypes.Has(DamageType.Heat))
		{
			TriggerAlarm();
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		ThrowLoadedItem();
	}

	private void UpdateTrigger()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (!IsArmed())
		{
			ComponentExtensions.SetActive<PlayerDetectionTrigger>(trigger, false);
			return;
		}
		ComponentExtensions.SetActive<PlayerDetectionTrigger>(trigger, true);
		Vector3 position = wireOrigin.position;
		Vector3 val = endPoint;
		Vector3 position2 = (position + val) / 2f;
		Vector3 val2 = val - position;
		float magnitude = ((Vector3)(ref val2)).magnitude;
		((Component)trigger).transform.position = position2;
		Vector3 localScale = ((Component)trigger).transform.localScale;
		localScale.z = magnitude;
		((Component)trigger).transform.rotation = Quaternion.LookRotation(val2);
		((Component)trigger).transform.localScale = new Vector3(0.15f, 0.15f, localScale.z);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.tinCanAlarm = Pool.Get<TinCanAlarm>();
		info.msg.tinCanAlarm.endPoint = endPoint;
		info.msg.tinCanAlarm.loadedAmmoItemDefId = ((loadedAmmoItem != null) ? loadedAmmoDef.itemid : 0);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.tinCanAlarm != null)
		{
			endPoint = info.msg.tinCanAlarm.endPoint;
			UpdateTrigger();
			_ = loadedAmmoDef;
			loadedAmmoDef = ItemManager.FindItemDefinition(info.msg.tinCanAlarm.loadedAmmoItemDefId);
			if (info.fromDisk && !usingPlayer.IsValid())
			{
				PlayerStopsArming(usingPlayer);
			}
		}
	}
}
