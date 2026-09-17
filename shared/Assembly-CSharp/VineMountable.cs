using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;

public class VineMountable : BaseMountable
{
	public struct VinePoint
	{
		public EntityRef<VineSwingingTree> TreeEntity;

		public int PointIndex;

		public VineLaunchPoint Get(bool isServer)
		{
			VineSwingingTree vineSwingingTree = TreeEntity.Get(isServer);
			if ((Object)(object)vineSwingingTree != (Object)null)
			{
				return vineSwingingTree.LaunchPoints[PointIndex];
			}
			return null;
		}

		public void Set(VineLaunchPoint launchPoint)
		{
			TreeEntity.Set(launchPoint.ParentTree);
			PointIndex = launchPoint.Index();
		}

		public VineDestination Save()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			VineDestination obj = Pool.Get<VineDestination>();
			obj.index = PointIndex;
			obj.targetTree = TreeEntity.uid;
			return obj;
		}

		public void Load(VineDestination destination)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			PointIndex = destination.index;
			TreeEntity.uid = destination.targetTree;
		}
	}

	public float moveSpeed;

	[Header("Rotation Settings")]
	public float rotationSpeed;

	public float descendSpeed;

	public Vector3 WorldSpaceAnchorPoint;

	private List<VinePoint> destinations;

	private VinePoint origin;

	public VinePoint currentLocation;

	public const Flags Away = Flags.Reserved1;

	public const Flags Descending = Flags.Reserved2;

	public const Flags Finished = Flags.Reserved3;

	public ViewModel VineViewModel;

	public float DismountViewmodelHoldTime;

	public GameObjectRef VineWorldModel;

	public Transform[] VineDirectionArrows;

	public CapsuleCollider ThisCollider;

	[ServerVar]
	public static bool allowChaining = true;

	private static readonly int DescendHash = Animator.StringToHash("descend");

	private static readonly int VineDescendingHash = Animator.StringToHash("vineDescending");

	private VineLaunchPoint activeOriginPoint;

	private VineLaunchPoint activeDestinationPoint;

	private float currentTime;

	private Vector3 lastPosition;

	private bool isDescending;

	private bool wantsToSyncPos;

	private VineMountable chainTarget;

	private Vector3 lastValidLocation;

	private TimeSince lastValidLocationTime;

	private Action processMovementAction;

	private Action stopReplicatingPosCallback;

	private Action syncVineAtEndAction;

	public int DestinationCount => destinations.Count;

	public static Grid<VineMountable> pointGrid { get; private set; } = new Grid<VineMountable>(32, 8096f);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VineMountable.OnRpcMessage"))
		{
			if (rpc == 2800581258u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Descend"));
				}
				using (TimeWarning.New("SV_Descend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2800581258u, "SV_Descend", this, player, 3f))
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
							SV_Descend(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SV_Descend");
					}
				}
				return true;
			}
			if (rpc == 2867502127u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_Swing"));
				}
				using (TimeWarning.New("SV_Swing"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2867502127u, "SV_Swing", this, player, 3f))
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
							SV_Swing(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SV_Swing");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static void NotifyVinesLaunchSiteRemoved(VineLaunchPoint point)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		PooledList<VineMountable> val = Pool.Get<PooledList<VineMountable>>();
		try
		{
			Vector3 position = ((Component)point).transform.position;
			pointGrid.Query(position.x, position.z, 100f, (List<VineMountable>)(object)val);
			foreach (VineMountable item in (List<VineMountable>)(object)val)
			{
				if ((Object)(object)item.origin.Get(isServer: true) == (Object)(object)point)
				{
					item.Kill();
					continue;
				}
				for (int i = 0; i < item.destinations.Count; i++)
				{
					if ((Object)(object)item.destinations[i].Get(isServer: true) == (Object)(object)point)
					{
						item.destinations.RemoveAt(i);
						if (item.HasFlag(Flags.Reserved1) && item.destinations.Count > 0)
						{
							item.Swing(null, shouldMount: false);
						}
						i--;
					}
				}
				if (item.destinations.Count == 0)
				{
					item.Kill();
				}
				else
				{
					item.SendNetworkUpdate();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void ServerInit()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		pointGrid.Add(this, ((Component)this).transform.position.x, ((Component)this).transform.position.z);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		VineLaunchPoint vineLaunchPoint = origin.Get(isServer: true);
		if ((Object)(object)vineLaunchPoint != (Object)null)
		{
			vineLaunchPoint.OnVineKilled();
		}
		pointGrid.Remove(this);
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerServerInput(inputState, player);
		chainTarget = null;
		if (!allowChaining || isDescending || !(currentTime > 0.5f) || !((Object)(object)activeDestinationPoint != (Object)null) || !inputState.IsDown(BUTTON.USE))
		{
			return;
		}
		PooledList<VineMountable> val = Pool.Get<PooledList<VineMountable>>();
		try
		{
			Vector3 position = ((Component)activeDestinationPoint).transform.position;
			pointGrid.Query(position.x, position.z, 5f, (List<VineMountable>)(object)val);
			foreach (VineMountable item in (List<VineMountable>)(object)val)
			{
				if (item.isServer && (Object)(object)item != (Object)(object)this && item.Distance(position) < 5f && (Object)(object)item.GetTargetDestination(((Component)this).transform.position, ((Component)this).transform.forward, out var foundAngle) != (Object)null && foundAngle < 90f)
				{
					chainTarget = item;
					break;
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool AttackedByPlayer(BasePlayer bp)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!ConVar.Server.allowVineSwinging)
		{
			return false;
		}
		float num = 2f;
		if ((Object)(object)bp != (Object)null)
		{
			if (bp.Distance((BaseEntity)this) < 2f)
			{
				return false;
			}
			if (HasFlag(Flags.Reserved1))
			{
				VineLaunchPoint vineLaunchPoint = origin.Get(base.isServer);
				if ((Object)(object)vineLaunchPoint != (Object)null && bp.Distance(((Component)vineLaunchPoint).transform.position) < num)
				{
					Swing(null, shouldMount: false);
					return true;
				}
			}
			else
			{
				foreach (VinePoint destination in destinations)
				{
					VineLaunchPoint vineLaunchPoint2 = destination.Get(isServer: true);
					if ((Object)(object)vineLaunchPoint2 != (Object)null && bp.Distance(((Component)vineLaunchPoint2).transform.position) < num)
					{
						Swing(null, shouldMount: false, vineLaunchPoint2);
						return true;
					}
				}
			}
		}
		return false;
	}

	private void ProcessMovement()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		lastPosition = ((Component)this).transform.position;
		VineLaunchPoint vineLaunchPoint = activeDestinationPoint;
		VineLaunchPoint vineLaunchPoint2 = activeOriginPoint;
		Flags flags = base.flags;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		if (HasFlag(Flags.Reserved3))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		currentTime = Mathf.MoveTowards(currentTime, 1f, moveSpeed * Time.deltaTime);
		float time = Mathf.SmoothStep(0f, 1f, currentTime);
		if (isDescending && (Object)(object)vineLaunchPoint2 != (Object)null)
		{
			if (GamePhysics.Trace(new Ray(((Component)this).transform.position, -Vector3.up), 0.2f, out var hitInfo, 50f, 1218519297, (QueryTriggerInteraction)0, this) && ((RaycastHit)(ref hitInfo)).distance < 1.5f && !(RaycastHitEx.GetEntity(hitInfo) is VineMountable))
			{
				if (Vector3.Distance(((Component)this).transform.position, ((Component)vineLaunchPoint2).transform.position) < 2f)
				{
					((Component)this).transform.position = ((Component)vineLaunchPoint2).transform.position;
				}
				isDescending = false;
				flagsUpdateScope.Set(Flags.Reserved2, b: false);
				flagsUpdateScope.Set(Flags.Reserved1, b: false);
				OnArrived(null);
				if (!HasFlag(Flags.Reserved3))
				{
					flagsUpdateScope.Set(Flags.Reserved3, b: true);
				}
				if (flags == base.flags)
				{
					SendNetworkUpdate();
				}
				((Component)this).transform.position = ((Component)vineLaunchPoint2).transform.position;
			}
			else
			{
				((Component)this).transform.Translate(-Vector3.up * descendSpeed * Time.deltaTime);
			}
			return;
		}
		if ((Object)(object)vineLaunchPoint == (Object)null || (Object)(object)vineLaunchPoint2 == (Object)null)
		{
			DismountAllPlayers();
			VineLaunchPoint vineLaunchPoint3 = origin.Get(isServer: true);
			if ((Object)(object)vineLaunchPoint3 != (Object)null)
			{
				OnArrived(vineLaunchPoint3);
			}
			return;
		}
		if (TimeSince.op_Implicit(lastValidLocationTime) > 0.1f)
		{
			lastValidLocation = ((Component)this).transform.position;
			lastValidLocationTime = TimeSince.op_Implicit(0f);
		}
		Vector3 swingPointAtTime = vineLaunchPoint2.GetSwingPointAtTime(time, vineLaunchPoint);
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = swingPointAtTime - position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		PooledList<RaycastHit> val2 = Pool.Get<PooledList<RaycastHit>>();
		try
		{
			float num = ThisCollider.height * 0.5f;
			Vector3 position2 = Vector3Ex.WithY(swingPointAtTime, swingPointAtTime.y - num);
			Vector3 position3 = Vector3Ex.WithY(swingPointAtTime, swingPointAtTime.y + num);
			GamePhysics.CapsuleSweep(position2, position3, ThisCollider.radius, normalized, Vector3.Distance(((Component)this).transform.position, swingPointAtTime) * 2f, (List<RaycastHit>)(object)val2, 2097152, (QueryTriggerInteraction)1);
			if (((List<RaycastHit>)(object)val2).Count > 0)
			{
				if (Vector3.Distance(lastValidLocation, ((Component)vineLaunchPoint2).transform.position) < 2f)
				{
					lastValidLocation = ((Component)vineLaunchPoint2).transform.position;
				}
				((Component)this).transform.position = lastValidLocation;
				DismountAllPlayers();
				return;
			}
			((Component)this).transform.position = swingPointAtTime;
			Vector3 val3 = swingPointAtTime - lastPosition;
			val = ((Vector3)(ref val3)).normalized;
			Quaternion val4 = ((!(((Vector3)(ref val)).sqrMagnitude > Mathf.Epsilon)) ? ((Component)this).transform.rotation : Quaternion.LookRotation(((Vector3)(ref val3)).normalized, Vector3.up));
			float num2 = Mathf.Abs((((Component)this).transform.position.y - lastPosition.y) / Time.deltaTime);
			float num3 = Mathf.Clamp01(Mathf.InverseLerp(0f, 6f, num2));
			Quaternion rotation = ((Component)this).transform.rotation;
			Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
			float num4 = Mathf.Clamp(((Quaternion)(ref val4)).eulerAngles.y, 0f - num3, num3);
			Quaternion val5 = Quaternion.Euler(eulerAngles.x, num4, eulerAngles.z);
			Quaternion rotation2 = Quaternion.Slerp(((Component)this).transform.rotation, val5 * val4, Time.deltaTime * rotationSpeed);
			((Component)this).transform.rotation = rotation2;
			if (currentTime >= 1f)
			{
				((Component)this).transform.position = vineLaunchPoint2.GetSwingPointAtTime(1f, vineLaunchPoint);
				OnArrived(vineLaunchPoint);
				if (!HasFlag(Flags.Reserved3))
				{
					flagsUpdateScope.Set(Flags.Reserved3, b: true);
				}
				if (flags == base.flags)
				{
					SendNetworkUpdate();
				}
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public void Initialise(VineLaunchPoint originPoint, List<VineLaunchPoint> destinationPoints, Vector3 anchor)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		origin.Set(originPoint);
		currentLocation.Set(originPoint);
		destinations.Clear();
		foreach (VineLaunchPoint destinationPoint in destinationPoints)
		{
			VinePoint item = default(VinePoint);
			item.Set(destinationPoint);
			destinations.Add(item);
		}
		WorldSpaceAnchorPoint = anchor;
		Vector3 val = ((Component)destinationPoints[0]).transform.position - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		((Component)this).transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		((Component)this).transform.localEulerAngles = Vector3Ex.WithX(((Component)this).transform.localEulerAngles, 0f);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.vineMountable = Pool.Get<VineMountable>();
		info.msg.vineMountable.anchorPoint = WorldSpaceAnchorPoint;
		info.msg.vineMountable.originPoint = origin.Save();
		info.msg.vineMountable.currentLocation = currentLocation.Save();
		info.msg.vineMountable.destinations = Pool.Get<List<VineDestination>>();
		foreach (VinePoint destination in destinations)
		{
			info.msg.vineMountable.destinations.Add(destination.Save());
		}
	}

	public override float AntiHackVelocity()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (ObjectEx.IsUnityNull(activeOriginPoint) || ObjectEx.IsUnityNull(activeDestinationPoint))
		{
			return 1f;
		}
		float num = Vector3.Distance(((Component)activeOriginPoint).transform.position, ((Component)activeDestinationPoint).transform.position);
		float num2 = ((moveSpeed > 0f) ? (1f / moveSpeed) : 1f);
		return Mathf.Clamp(num / num2, 1f, 50f);
	}

	private unsafe float GetMaxVineDistance(Vector3 origin)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		foreach (VinePoint destination in destinations)
		{
			VineLaunchPoint vineLaunchPoint = destination.Get(base.isServer);
			if ((Object)(object)vineLaunchPoint != (Object)null)
			{
				num = Mathf.Max(Vector3.Distance(((Component)vineLaunchPoint).transform.position, origin), num);
			}
		}
		if (num == 0f)
		{
			Debug.Log((object)(" there are " + destinations.Count + " destinations"));
			foreach (VinePoint destination2 in destinations)
			{
				VineLaunchPoint vineLaunchPoint2 = destination2.Get(isServer: false);
				if ((Object)(object)vineLaunchPoint2 != (Object)null)
				{
					float num2 = Vector3.Distance(((Component)vineLaunchPoint2).transform.position, origin);
					Debug.LogWarning((object)("Detected broken distance between " + ((object)((Component)vineLaunchPoint2).transform.position/*cast due to constrained. prefix*/).ToString() + " and origin " + ((object)(*(Vector3*)(&origin))/*cast due to constrained. prefix*/).ToString()));
					Debug.LogWarning((object)("home " + ((object)((Component)this).transform.position/*cast due to constrained. prefix*/).ToString()));
					Debug.LogWarning((object)("dist is  " + num2));
				}
			}
			return 5f;
		}
		return num;
	}

	public override void PostServerLoad()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.PostServerLoad();
		VineLaunchPoint vineLaunchPoint = origin.Get(base.isServer);
		if ((Object)(object)vineLaunchPoint != (Object)null)
		{
			((Component)this).transform.position = ((Component)vineLaunchPoint).transform.position;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
		}
	}

	private void Descend(BasePlayer forPlayer)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)forPlayer == (Object)null || forPlayer.isMounted)
		{
			return;
		}
		StartReplicatingPos();
		isDescending = true;
		activeOriginPoint = origin.Get(isServer: true);
		((Component)this).transform.forward = Vector3Ex.WithY(forPlayer.eyes.BodyForward(), 0f);
		currentTime = 0f;
		MountPlayer(forPlayer);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b: true);
		flagsUpdateScope.Set(Flags.Reserved2, b: true);
	}

	private void Swing(BasePlayer forPlayer, bool shouldMount, VineLaunchPoint overridePoint = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = (((Object)(object)forPlayer != (Object)null) ? forPlayer.eyes.BodyForward() : Vector3.forward);
		Vector3 playerPos = (((Object)(object)forPlayer != (Object)null) ? ((Component)forPlayer).transform.position : ((Component)this).transform.position);
		VineLaunchPoint vineLaunchPoint = null;
		vineLaunchPoint = (HasFlag(Flags.Reserved1) ? origin.Get(base.isServer) : ((!((Object)(object)overridePoint != (Object)null)) ? GetTargetDestination(playerPos, forward, out var _) : overridePoint));
		if ((Object)(object)vineLaunchPoint == (Object)null)
		{
			Debug.Log((object)"Could not find valid vine launch destination, should not happen");
			return;
		}
		Vector3 val = ((Component)vineLaunchPoint).transform.position - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		((Component)this).transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		activeOriginPoint = currentLocation.Get(base.isServer);
		activeDestinationPoint = vineLaunchPoint;
		if ((Object)(object)forPlayer != (Object)null)
		{
			lastPosition = ((Component)forPlayer).transform.position;
			if (shouldMount)
			{
				MountPlayer(forPlayer);
			}
		}
		lastValidLocation = ((Component)this).transform.position;
		lastValidLocationTime = TimeSince.op_Implicit(0f);
		currentTime = 0f;
		currentLocation.Set(vineLaunchPoint);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved1, !HasFlag(Flags.Reserved1));
			flagsUpdateScope.Set(Flags.On, b: true);
			flagsUpdateScope.Set(Flags.Reserved2, b: false);
		}
		StartReplicatingPos();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SV_Swing(RPCMessage msg)
	{
		if (!IsMounted() && ConVar.Server.allowVineSwinging)
		{
			BasePlayer player = msg.player;
			bool flag = msg.read.Bool();
			if (!flag || !((Object)(object)player != (Object)null) || !player.isMounted)
			{
				Swing(player, flag);
			}
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void SV_Descend(RPCMessage msg)
	{
		if (!IsMounted() && ConVar.Server.allowVineSwinging)
		{
			BasePlayer player = msg.player;
			Descend(player);
		}
	}

	private void OnArrived(VineLaunchPoint point)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.forward = -((Component)this).transform.forward;
		((Component)this).transform.localEulerAngles = Vector3Ex.WithX(((Component)this).transform.localEulerAngles, 0f);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		if ((Object)(object)point != (Object)null && point.FindVacantArrivalPoint(this, out var worldPos))
		{
			((Component)this).transform.position = worldPos;
		}
		DeferredStopReplicatingPos();
		BasePlayer mounted = GetMounted();
		DismountAllPlayers();
		if ((Object)(object)chainTarget != (Object)null)
		{
			chainTarget.Swing(mounted, shouldMount: true);
		}
	}

	private void StartReplicatingPos()
	{
		wantsToSyncPos = true;
		if (stopReplicatingPosCallback == null)
		{
			stopReplicatingPosCallback = StopReplcatingPos;
			ToggleNetworkPositionTick(isEnabled: true);
		}
		else if (!IsInvoking(stopReplicatingPosCallback))
		{
			ToggleNetworkPositionTick(isEnabled: true);
		}
	}

	private void StopReplcatingPos()
	{
		if (!wantsToSyncPos)
		{
			ToggleNetworkPositionTick(isEnabled: false);
		}
	}

	private void DeferredStopReplicatingPos()
	{
		wantsToSyncPos = false;
		Invoke(stopReplicatingPosCallback, 0.5f);
	}

	public void Highlight(BasePlayer forPlayer)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)origin.Get(base.isServer)).transform.position;
		foreach (VinePoint destination in destinations)
		{
			Vector3 position2 = ((Component)destination.Get(base.isServer)).transform.position;
			forPlayer.SendConsoleCommand("ddraw.arrow", "60", Color.red, position, position2, 25, 0, 0);
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.vineMountable == null)
		{
			return;
		}
		WorldSpaceAnchorPoint = info.msg.vineMountable.anchorPoint;
		origin.Load(info.msg.vineMountable.originPoint);
		currentLocation.Load(info.msg.vineMountable.currentLocation);
		destinations.Clear();
		foreach (VineDestination destination in info.msg.vineMountable.destinations)
		{
			VinePoint item = default(VinePoint);
			item.Load(destination);
			destinations.Add(item);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!base.isServer)
		{
			return;
		}
		if (processMovementAction == null)
		{
			processMovementAction = ProcessMovement;
		}
		bool flag = IsOn();
		bool flag2 = IsInvoking(processMovementAction);
		if (flag != flag2)
		{
			if (flag)
			{
				InvokeRepeating(processMovementAction, 0f, 0f);
			}
			else
			{
				CancelInvoke(processMovementAction);
			}
		}
	}

	private VineLaunchPoint GetTargetDestination(Vector3 playerPos, Vector3 forward, out float foundAngle)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		VineLaunchPoint result = null;
		forward.y = 0f;
		foreach (VinePoint destination in destinations)
		{
			VineLaunchPoint vineLaunchPoint = destination.Get(base.isServer);
			if ((Object)(object)vineLaunchPoint != (Object)null)
			{
				Vector3 val = forward;
				Vector3 val2 = ((Component)vineLaunchPoint).transform.position - playerPos;
				float num2 = Vector3.Angle(val, Vector3Ex.WithY(((Vector3)(ref val2)).normalized, 0f));
				if (num2 < num)
				{
					result = vineLaunchPoint;
					num = num2;
				}
			}
		}
		foundAngle = num;
		return result;
	}

	public VineMountable()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		rotationSpeed = 0.5f;
		descendSpeed = 5f;
		destinations = new List<VinePoint>();
		DismountViewmodelHoldTime = 0.2f;
		lastValidLocation = Vector3.zero;
		base._002Ector();
	}
}
