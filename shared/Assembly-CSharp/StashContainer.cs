using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

[ConsoleSystem.Factory("stash")]
public class StashContainer : StorageContainer
{
	public static class StashContainerFlags
	{
		public const Flags Hidden = Flags.Reserved5;
	}

	public Transform visuals;

	public float burriedOffset;

	public float raisedOffset;

	public GameObjectRef buryEffect;

	public float uncoverRange = 3f;

	public float uncoverTime = 2f;

	[ServerVar(Name = "reveal_tick_rate", Help = "(Generated) Interval in seconds between player-detection ticks for the stash container reveal mechanic; registered as stash.reveal_tick_rate")]
	public static float PlayerDetectionTickRate = 0.5f;

	private float lastToggleTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StashContainer.OnRpcMessage"))
		{
			if (rpc == 4130263076u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_HideStash"));
				}
				using (TimeWarning.New("RPC_HideStash"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4130263076u, "RPC_HideStash", this, player, 3f))
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
							RPC_HideStash(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_HideStash");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsHidden()
	{
		return HasFlag(Flags.Reserved5);
	}

	public bool PlayerInRange(BasePlayer ply)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)this).transform.position, ((Component)ply).transform.position) <= uncoverRange)
		{
			Vector3 val = ((Component)this).transform.position - ply.eyes.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			if (Vector3.Dot(ply.eyes.BodyForward(), normalized) > 0.95f)
			{
				return true;
			}
		}
		return false;
	}

	public override void InitShared()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		((Component)visuals).transform.localPosition = Vector3Ex.WithY(((Component)visuals).transform.localPosition, raisedOffset);
	}

	public void DoOccludedCheck()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.SphereCast(new Ray(((Component)this).transform.position + Vector3.up * 5f, Vector3.down), 0.25f, 5f, 2097152) && Interface.CallHook("OnStashOcclude", this) == null)
		{
			DropItems();
			Kill();
		}
	}

	public void OnPhysicsNeighbourChanged()
	{
		if (!IsInvoking(DoOccludedCheck))
		{
			Invoke(DoOccludedCheck, Random.Range(5f, 10f));
		}
	}

	private void RemoveFromNetworkRange()
	{
		base.limitNetworking = true;
	}

	private void ReturnToNetworkRange()
	{
		if (base.limitNetworking)
		{
			base.limitNetworking = false;
			SendNetworkUpdateImmediate();
		}
		CancelInvoke(RemoveFromNetworkRange);
	}

	public void SetHidden(bool isHidden)
	{
		if (Time.realtimeSinceStartup - lastToggleTime < 3f || isHidden == HasFlag(Flags.Reserved5))
		{
			return;
		}
		if (isHidden)
		{
			Invoke(RemoveFromNetworkRange, 3f);
		}
		else
		{
			ReturnToNetworkRange();
		}
		lastToggleTime = Time.realtimeSinceStartup;
		Invoke(Decay, 259200f);
		if (!base.isServer)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, isHidden);
	}

	public void DisableNetworking()
	{
		base.limitNetworking = true;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Disabled, b: true);
	}

	public void Decay()
	{
		Kill();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetHidden(isHidden: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (IsHidden())
		{
			RemoveFromNetworkRange();
		}
	}

	public void ToggleHidden()
	{
		SetHidden(!IsHidden());
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_HideStash(RPCMessage rpc)
	{
		if (Interface.CallHook("CanHideStash", rpc.player, this) == null)
		{
			Facepunch.Rust.Analytics.Azure.OnStashHidden(rpc.player, this);
			SetHidden(isHidden: true);
			Interface.CallHook("OnStashHidden", this, rpc.player);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool num = (old & Flags.Reserved5) == Flags.Reserved5;
		bool flag = (next & Flags.Reserved5) == Flags.Reserved5;
		if (num != flag)
		{
			float to = (flag ? burriedOffset : raisedOffset);
			LeanTween.cancel(((Component)visuals).gameObject);
			LeanTween.moveLocalY(((Component)visuals).gameObject, to, 1f);
		}
	}
}
