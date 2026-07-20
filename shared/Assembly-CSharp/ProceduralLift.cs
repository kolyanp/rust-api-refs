using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ProceduralLift : BaseEntity
{
	public float movementSpeed = 1f;

	public float resetDelay = 5f;

	public ProceduralLiftCabin cabin;

	public ProceduralLiftStop[] stops;

	public GameObjectRef triggerPrefab;

	public string triggerBone;

	private int floorIndex = -1;

	public SoundDefinition startSoundDef;

	public SoundDefinition stopSoundDef;

	public SoundDefinition movementLoopSoundDef;

	private Sound movementLoopSound;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ProceduralLift.OnRpcMessage"))
		{
			if (rpc == 2657791441u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UseLift"));
				}
				using (TimeWarning.New("RPC_UseLift"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2657791441u, "RPC_UseLift", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_UseLift(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_UseLift");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Spawn()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.Spawn();
		if (!Application.isLoadingSave)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(triggerPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			baseEntity.Spawn();
			baseEntity.SetParent(this, triggerBone);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_UseLift(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && Interface.CallHook("OnLiftUse", this, rpc.player) == null && !IsBusy())
		{
			MoveToFloor((floorIndex + 1) % stops.Length);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SnapToFloor(0);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lift = Pool.Get<Lift>();
		info.msg.lift.floor = floorIndex;
	}

	public override void Load(LoadInfo info)
	{
		if (info.msg.lift != null)
		{
			if (floorIndex == -1)
			{
				SnapToFloor(info.msg.lift.floor);
			}
			else
			{
				MoveToFloor(info.msg.lift.floor);
			}
		}
		base.Load(info);
	}

	private void ResetLift()
	{
		MoveToFloor(0);
	}

	private void MoveToFloor(int floor)
	{
		floorIndex = Mathf.Clamp(floor, 0, stops.Length - 1);
		if (!base.isServer)
		{
			return;
		}
		SetFlagLocal(Flags.Busy, b: true);
		foreach (BaseEntity child in children)
		{
			using FlagsUpdateScope flagsUpdateScope = child.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		SendNetworkUpdateImmediate();
		CancelInvoke(ResetLift);
	}

	private void SnapToFloor(int floor)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		floorIndex = Mathf.Clamp(floor, 0, stops.Length - 1);
		ProceduralLiftStop proceduralLiftStop = stops[floorIndex];
		((Component)cabin).transform.position = ((Component)proceduralLiftStop).transform.position;
		if (!base.isServer)
		{
			return;
		}
		SetFlagLocal(Flags.Busy, b: false);
		foreach (BaseEntity child in children)
		{
			using FlagsUpdateScope flagsUpdateScope = child.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Busy, b: false);
		}
		SendNetworkUpdateImmediate();
		CancelInvoke(ResetLift);
	}

	private void OnFinishedMoving()
	{
		if (!base.isServer)
		{
			return;
		}
		SetFlagLocal(Flags.Busy, b: false);
		foreach (BaseEntity child in children)
		{
			using FlagsUpdateScope flagsUpdateScope = child.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Busy, b: false);
		}
		SendNetworkUpdateImmediate();
		if (floorIndex != 0)
		{
			Invoke(ResetLift, resetDelay);
		}
	}

	protected void Update()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (floorIndex < 0 || floorIndex > stops.Length - 1)
		{
			return;
		}
		ProceduralLiftStop proceduralLiftStop = stops[floorIndex];
		if (!(((Component)cabin).transform.position == ((Component)proceduralLiftStop).transform.position))
		{
			((Component)cabin).transform.position = Vector3.MoveTowards(((Component)cabin).transform.position, ((Component)proceduralLiftStop).transform.position, movementSpeed * Time.deltaTime);
			if (((Component)cabin).transform.position == ((Component)proceduralLiftStop).transform.position)
			{
				OnFinishedMoving();
			}
		}
	}

	[UnityEvent]
	public void StartMovementSounds()
	{
	}

	[UnityEvent]
	public void StopMovementSounds()
	{
	}
}
