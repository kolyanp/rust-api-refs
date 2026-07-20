using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SpinnerWheel : Signage
{
	public Transform wheel;

	public float velocity;

	public Quaternion targetRotation = Quaternion.identity;

	[Header("Sound")]
	public SoundDefinition spinLoopSoundDef;

	public SoundDefinition spinStartSoundDef;

	public SoundDefinition spinAccentSoundDef;

	public SoundDefinition spinStopSoundDef;

	public float minTimeBetweenSpinAccentSounds = 0.3f;

	public float spinAccentAngleDelta = 180f;

	private Sound spinSound;

	private SoundModulation.Modulator spinSoundGain;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SpinnerWheel.OnRpcMessage"))
		{
			if (rpc == 3019675107u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_AnyoneSpin"));
				}
				using (TimeWarning.New("RPC_AnyoneSpin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3019675107u, "RPC_AnyoneSpin", this, player, 3f))
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
							RPC_AnyoneSpin(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_AnyoneSpin");
					}
				}
				return true;
			}
			if (rpc == 1455840454 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_Spin"));
				}
				using (TimeWarning.New("RPC_Spin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455840454u, "RPC_Spin", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_Spin(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in RPC_Spin");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool AllowPlayerSpins()
	{
		return true;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.spinnerWheel = Pool.Get<SpinnerWheel>();
		SpinnerWheel spinnerWheel = info.msg.spinnerWheel;
		Quaternion localRotation = wheel.localRotation;
		spinnerWheel.spin = ((Quaternion)(ref localRotation)).eulerAngles;
	}

	public override void Load(LoadInfo info)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.spinnerWheel != null)
		{
			Quaternion localRotation = Quaternion.Euler(info.msg.spinnerWheel.spin);
			if (base.isServer && info.fromDisk)
			{
				localRotation = Quaternion.identity;
			}
			if (base.isServer)
			{
				((Component)wheel).transform.localRotation = localRotation;
			}
		}
	}

	public virtual float GetMaxSpinSpeed()
	{
		return 720f;
	}

	public virtual void Update_Server()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (velocity > 0f)
		{
			float num = Mathf.Clamp(GetMaxSpinSpeed() * velocity, 0f, GetMaxSpinSpeed());
			velocity -= Time.deltaTime * Mathf.Clamp(velocity / 2f, 0.1f, 1f);
			if (velocity < 0f)
			{
				velocity = 0f;
				ToggleChildEntityColliders(state: true);
			}
			wheel.Rotate(Vector3.up, num * Time.deltaTime, (Space)1);
			SendNetworkUpdate();
		}
	}

	public void Update_Client()
	{
	}

	public void Update()
	{
		if (base.isClient)
		{
			Update_Client();
		}
		if (base.isServer)
		{
			Update_Server();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_Spin(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && AllowPlayerSpins() && (AnyoneSpin() || rpc.player.CanBuild()) && Interface.CallHook("OnSpinWheel", rpc.player, this) == null && !(velocity > 15f))
		{
			velocity += Random.Range(4f, 7f);
			ToggleChildEntityColliders(state: false);
		}
	}

	private void ToggleChildEntityColliders(bool state)
	{
		foreach (BaseEntity child in children)
		{
			if (child is DroppedItem droppedItem && (Object)(object)droppedItem.childCollider != (Object)null)
			{
				droppedItem.childCollider.enabled = state;
			}
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RPC_AnyoneSpin(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract())
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, rpc.read.Bit());
	}

	public bool AnyoneSpin()
	{
		return HasFlag(Flags.Reserved3);
	}
}
