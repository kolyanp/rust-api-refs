using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class WheelSwitch : IOEntity
{
	public Transform wheelObj;

	public float rotateSpeed = 90f;

	public Flags BeingRotated = Flags.Reserved1;

	public Flags RotatingLeft = Flags.Reserved2;

	public Flags RotatingRight = Flags.Reserved3;

	public float rotateProgress;

	public Animator animator;

	public float kineticEnergyPerSec = 1f;

	public bool requiresPowerToTurn;

	private BasePlayer rotatorPlayer;

	private float progressTickRate = 0.1f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WheelSwitch.OnRpcMessage"))
		{
			if (rpc == 2223603322u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BeginRotate"));
				}
				using (TimeWarning.New("BeginRotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2223603322u, "BeginRotate", this, player, 3f))
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
							BeginRotate(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BeginRotate");
					}
				}
				return true;
			}
			if (rpc == 434251040 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - CancelRotate"));
				}
				using (TimeWarning.New("CancelRotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(434251040u, "CancelRotate", this, player, 3f))
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
							CancelRotate(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in CancelRotate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetIOState()
	{
		CancelPlayerRotation();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void BeginRotate(RPCMessage msg)
	{
		if (!IsBeingRotated())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(BeingRotated, b: true);
			}
			rotatorPlayer = msg.player;
			InvokeRepeating(RotateProgress, 0f, progressTickRate);
		}
	}

	public void CancelPlayerRotation()
	{
		CancelInvoke(RotateProgress);
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(BeingRotated, b: false);
		}
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null)
			{
				iOSlot.connectedTo.Get().IOInput(this, ioType, 0f, iOSlot.connectedToSlot);
			}
		}
		rotatorPlayer = null;
	}

	public void RotateProgress()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)rotatorPlayer) || rotatorPlayer.IsDead() || rotatorPlayer.IsSleeping() || Vector3Ex.Distance2D(((Component)rotatorPlayer).transform.position, ((Component)this).transform.position) > 2f)
		{
			CancelPlayerRotation();
			return;
		}
		float num = kineticEnergyPerSec * progressTickRate;
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null)
			{
				num = iOSlot.connectedTo.Get().IOInput(this, ioType, num, iOSlot.connectedToSlot);
			}
		}
		if (num == 0f)
		{
			SetRotateProgress(rotateProgress + 0.1f);
		}
		SendNetworkUpdate();
	}

	public void SetRotateProgress(float newValue)
	{
		float num = rotateProgress;
		rotateProgress = newValue;
		SetFlagLocal(Flags.Reserved4, num != newValue);
		SendNetworkUpdate();
		CancelInvoke(StoppedRotatingCheck);
		Invoke(StoppedRotatingCheck, 0.25f);
	}

	public void StoppedRotatingCheck()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, b: false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void CancelRotate(RPCMessage msg)
	{
		CancelPlayerRotation();
	}

	public void Powered()
	{
		float inputAmount = kineticEnergyPerSec * progressTickRate;
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			if ((Object)(object)iOSlot.connectedTo.Get() != (Object)null)
			{
				inputAmount = iOSlot.connectedTo.Get().IOInput(this, ioType, inputAmount, iOSlot.connectedToSlot);
			}
		}
		SetRotateProgress(rotateProgress + 0.1f);
	}

	public override float IOInput(IOEntity from, IOType inputType, float inputAmount, int slot = 0)
	{
		if (inputAmount < 0f)
		{
			SetRotateProgress(rotateProgress + inputAmount);
			SendNetworkUpdate();
		}
		if (inputType == IOType.Electric && slot == 1)
		{
			if (inputAmount == 0f)
			{
				CancelInvoke(Powered);
			}
			else
			{
				InvokeRepeating(Powered, 0f, progressTickRate);
			}
		}
		return Mathf.Clamp(inputAmount - 1f, 0f, inputAmount);
	}

	public bool IsBeingRotated()
	{
		return HasFlag(BeingRotated);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.sphereEntity != null)
		{
			rotateProgress = info.msg.sphereEntity.radius;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.sphereEntity = Pool.Get<SphereEntity>();
		info.msg.sphereEntity.radius = rotateProgress;
	}
}
