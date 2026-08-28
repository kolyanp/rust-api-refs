using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class StringLights : IOEntity
{
	public struct PointEntry
	{
		public Vector3 point;

		public Vector3 normal;

		public float slack;
	}

	[Serializable]
	public struct BulbSettings
	{
		public GameObjectRef BulbPrefab;

		public float Weight;
	}

	private const int PLACEMENT_LAYER = 1218510849;

	private const float MIN_SLACK = 0f;

	private const float MAX_SLACK = 2f;

	private BasePlayer usingPlayer;

	[SerializeField]
	private float lengthPerAmount = 0.5f;

	[SerializeField]
	private float maxPlaceDistance = 5f;

	[SerializeField]
	private ItemDefinition itemToConsume;

	[SerializeField]
	[Header("Line Generation Settings")]
	protected BulbSettings[] bulbSettings;

	[SerializeField]
	private GameObjectRef pointLightPrefab;

	[SerializeField]
	private Transform lightsParent;

	[SerializeField]
	private Transform wireOrigin;

	[SerializeField]
	protected float bulbSpacing = 0.25f;

	[SerializeField]
	protected float wireThickness = 0.02f;

	[SerializeField]
	protected float maxDeviation = 0.25f;

	[SerializeField]
	protected float deviationFactor = 1f;

	[SerializeField]
	protected bool bulbFaceNormal;

	[SerializeField]
	[Space]
	protected LineRenderer lineRenderer;

	[SerializeField]
	protected RendererLOD rendererLod;

	protected readonly List<PointEntry> points = new List<PointEntry>();

	protected readonly List<StringLightsBulb> bulbs = new List<StringLightsBulb>();

	protected readonly List<Light> pointLights = new List<Light>();

	private readonly Dictionary<int, GameObject> prefabLookup = new Dictionary<int, GameObject>();

	public bool useBatching;

	protected List<StringLightsBulb> lastBatchedMeshes = new List<StringLightsBulb>();

	private int lengthUsed;

	private const Flags Flag_Used = Flags.Reserved5;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StringLights.OnRpcMessage"))
		{
			if (rpc == 4045900594u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_AddPoint"));
				}
				using (TimeWarning.New("SERVER_AddPoint"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4045900594u, "SERVER_AddPoint", this, player, 3uL))
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
							SERVER_AddPoint(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_AddPoint");
					}
				}
				return true;
			}
			if (rpc == 3733663691u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_RemovePoint"));
				}
				using (TimeWarning.New("SERVER_RemovePoint"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3733663691u, "SERVER_RemovePoint", this, player, 3uL))
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
							SERVER_RemovePoint(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_RemovePoint");
					}
				}
				return true;
			}
			if (rpc == 2400039444u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StartDeploying"));
				}
				using (TimeWarning.New("SERVER_StartDeploying"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2400039444u, "SERVER_StartDeploying", this, player, 3f))
						{
							return true;
						}
					}
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
							SERVER_StartDeploying(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_StartDeploying");
					}
				}
				return true;
			}
			if (rpc == 2702400742u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_StopDeploying"));
				}
				using (TimeWarning.New("SERVER_StopDeploying"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_StopDeploying(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_StopDeploying");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected Item GetOwnerItem()
	{
		BasePlayer basePlayer = null;
		if (base.isServer)
		{
			basePlayer = usingPlayer;
		}
		if ((Object)(object)basePlayer == (Object)null || (Object)(object)basePlayer.inventory == (Object)null)
		{
			return null;
		}
		return basePlayer.inventory.FindItemByItemID(itemToConsume.itemid);
	}

	public override Item GetItem()
	{
		return GetOwnerItem();
	}

	private bool CheckValidPlacement(Vector3 position, float radius, int layerMask)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(position, radius, list, layerMask, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (item is AnimatedBuildingBlock)
			{
				result = false;
				break;
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
		return result;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		if (base.isServer)
		{
			lengthUsed = 1;
			PlayerStartsDeploying(deployedBy);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void SERVER_StartDeploying(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!IsUsed() && player.CanBuild())
		{
			PlayerStartsDeploying(player);
		}
	}

	[RPC_Server]
	public void SERVER_StopDeploying(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)usingPlayer != (Object)(object)player) && player.CanBuild())
		{
			PlayerStopsDeploying(player);
		}
	}

	public void PlayerStartsDeploying(BasePlayer player)
	{
		if (!IsUsed() && !((Object)(object)player == (Object)null))
		{
			usingPlayer = player;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: true);
			}
			if (IsInvoking(ServerWireDeployingTick))
			{
				CancelInvoke(ServerWireDeployingTick);
			}
			InvokeRepeating(ServerWireDeployingTick, 0f, 0f);
			ClientRPC(RpcTarget.Player("CLIENT_StartDeploying", player));
		}
	}

	public void PlayerStopsDeploying(BasePlayer player)
	{
		usingPlayer = null;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		CancelInvoke(ServerWireDeployingTick);
		ClientRPC(RpcTarget.Player("CLIENT_StopDeploying", player));
	}

	public void ServerWireDeployingTick()
	{
		if (!usingPlayer.IsValid() || !usingPlayer.IsConnected)
		{
			PlayerStopsDeploying(usingPlayer);
		}
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	public void SERVER_AddPoint(RPCMessage msg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if ((Object)(object)player != (Object)(object)usingPlayer)
		{
			return;
		}
		Vector3 val = msg.read.Vector3();
		Vector3 val2 = msg.read.Vector3();
		float num = msg.read.Float();
		if (Vector3Ex.IsNaNOrInfinity(val) || Vector3Ex.IsNaNOrInfinity(val2) || FloatEx.IsNaNOrInfinity(num))
		{
			return;
		}
		num = Mathf.Clamp(num, 0f, 2f);
		Item item = GetItem();
		if (item != null && item.amount >= 1 && CanPlayerUse(player) && !(Vector3.Distance(val, player.eyes.position) > maxPlaceDistance) && CheckValidPlacement(val, 0.1f, 1218510849) && Interface.CallHook("OnPoweredLightsPointAdd", this, player, val, val2) == null)
		{
			int num2 = 1;
			float num3 = 0f;
			Vector3 val3 = ((points.Count > 0) ? ((Component)this).transform.TransformPoint(points[points.Count - 1].point) : wireOrigin.position);
			num3 = Vector3.Distance(val, val3);
			num3 = Mathf.Max(num3, lengthPerAmount);
			float num4 = (float)item.amount * lengthPerAmount;
			if (player.IsInCreativeMode && Creative.unlimitedIo)
			{
				num4 = 200f;
			}
			if (num3 > num4)
			{
				num3 = num4;
				val = val3 + Vector3Ex.Direction(val, val3) * num3;
			}
			num3 = Mathf.Min(num4, num3);
			num2 = Mathf.CeilToInt(num3 / lengthPerAmount);
			if (player.IsInCreativeMode && Creative.unlimitedIo)
			{
				num2 = 0;
			}
			AddPoint(((Component)this).transform.InverseTransformPoint(val), ((Component)this).transform.InverseTransformDirection(val2), num);
			UseItemAmount(num2);
			AddLengthUsed(num2);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	public void SERVER_RemovePoint(RPCMessage msg)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (!((Object)(object)player != (Object)(object)usingPlayer) && CanPlayerUse(player) && points.Count != 0)
		{
			Vector3 val = ((Component)this).transform.TransformPoint(points[points.Count - 1].point);
			Vector3 val2 = ((Component)this).transform.position;
			if (points.Count > 1)
			{
				val2 = ((Component)this).transform.TransformPoint(points[points.Count - 2].point);
			}
			int num = Mathf.CeilToInt(Vector3.Distance(val, val2) / lengthPerAmount);
			RemoveLastPoint();
			if (!player.IsInCreativeMode || !Creative.unlimitedIo)
			{
				GiveItemAmount(player, num);
				AddLengthUsed(-num);
			}
			SendNetworkUpdate();
		}
	}

	private void GiveItemAmount(BasePlayer player, int amount)
	{
		if (amount > 0)
		{
			Item ownerItem = GetOwnerItem();
			if (ownerItem == null)
			{
				ownerItem = ItemManager.Create(itemToConsume, amount, 0uL, isServerSide: true, 0uL);
				player.GiveItem(ownerItem, GiveItemReason.PickedUp);
			}
			else
			{
				ownerItem.amount += amount;
				ownerItem.MarkDirty();
				ownerItem.ReduceItemOwnership(amount);
			}
		}
	}

	protected void UseItemAmount(int amount)
	{
		if (amount <= 0)
		{
			return;
		}
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null)
		{
			ownerItem.amount -= amount;
			ownerItem.MarkDirty();
			ownerItem.ReduceItemOwnership(amount);
			if (ownerItem.amount <= 0)
			{
				ownerItem.Remove();
			}
		}
	}

	public bool IsUsed()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void ClearPoints()
	{
		points.Clear();
	}

	public void AddPoint(Vector3 newPoint, Vector3 newNormal, float slackLevel, bool addFirstPoint = true)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (addFirstPoint && base.isServer && points.Count == 0)
		{
			PointEntry item = new PointEntry
			{
				point = ((Component)this).transform.InverseTransformPoint(wireOrigin.position),
				normal = newNormal,
				slack = slackLevel
			};
			points.Add(item);
		}
		slackLevel = Mathf.Clamp(slackLevel, 0f, 2f);
		PointEntry item2 = new PointEntry
		{
			point = newPoint,
			normal = newNormal,
			slack = slackLevel
		};
		points.Add(item2);
	}

	public void RemoveLastPoint()
	{
		points.RemoveAt(points.Count - 1);
		if (points.Count == 1)
		{
			points.Clear();
		}
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	protected override int GetPickupCount()
	{
		return Mathf.Max(lengthUsed, 1);
	}

	public void AddLengthUsed(int addLength)
	{
		lengthUsed += addLength;
	}

	protected bool CanPlayerUse(BasePlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (player.CanBuild())
		{
			return !GamePhysics.CheckSphere(player.eyes.position, 0.1f, 536870912, (QueryTriggerInteraction)2);
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.lightString = Pool.Get<LightString>();
		info.msg.lightString.points = Pool.Get<List<StringPoint>>();
		info.msg.lightString.lengthUsed = lengthUsed;
		foreach (PointEntry point in points)
		{
			StringPoint val = Pool.Get<StringPoint>();
			val.point = point.point;
			val.normal = point.normal;
			val.slack = point.slack;
			info.msg.lightString.points.Add(val);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.lightString == null)
		{
			return;
		}
		ClearPoints();
		foreach (StringPoint point in info.msg.lightString.points)
		{
			AddPoint(point.point, point.normal, point.slack, addFirstPoint: false);
		}
		lengthUsed = info.msg.lightString.lengthUsed;
	}
}
