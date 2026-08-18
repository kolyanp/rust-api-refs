using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class InstrumentTool : HeldEntity
{
	public InstrumentKeyController KeyController;

	public SoundDefinition DeploySound;

	public Vector2 PitchClamp;

	public bool UseAnimationSlotEvents;

	public Transform MuzzleT;

	public bool UsableByAutoTurrets;

	private NoteBindingCollection.NoteData lastPlayedTurretData;

	public override Transform MuzzleTransform => MuzzleT;

	public override bool IsUsableByTurret => UsableByAutoTurrets;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("InstrumentTool.OnRpcMessage"))
		{
			if (rpc == 1625188589 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_PlayNote"));
				}
				using (TimeWarning.New("Server_PlayNote"))
				{
					using (TimeWarning.New("Conditions"))
					{
						long position = msg.read.Position;
						msg.read.Read<int>();
						msg.read.Read<int>();
						msg.read.Read<int>();
						if (!RPC_Server.InputValidation.Test(msg.read.Read<float>()))
						{
							return true;
						}
						msg.read.Position = position;
						if (!RPC_Server.IsActiveItem.Test(1625188589u, "Server_PlayNote", this, player))
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
							Server_PlayNote(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_PlayNote");
					}
				}
				return true;
			}
			if (rpc == 705843933 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_StopNote"));
				}
				using (TimeWarning.New("Server_StopNote"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(705843933u, "Server_StopNote", this, player))
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
							Server_StopNote(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Server_StopNote");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.InputValidation(new Type[]
	{
		typeof(int),
		typeof(int),
		typeof(int),
		typeof(float)
	})]
	[RPC_Server.IsActiveItem]
	private void Server_PlayNote(RPCMessage msg)
	{
		int arg = msg.read.Int32();
		int arg2 = msg.read.Int32();
		int arg3 = msg.read.Int32();
		float arg4 = msg.read.Float();
		KeyController.ProcessServerPlayedNote(GetOwnerPlayer());
		ClientRPC(RpcTarget.NetworkGroup("Client_PlayNote"), arg, arg2, arg3, arg4);
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_StopNote(RPCMessage msg)
	{
		int arg = msg.read.Int32();
		int arg2 = msg.read.Int32();
		int arg3 = msg.read.Int32();
		ClientRPC(RpcTarget.NetworkGroup("Client_StopNote"), arg, arg2, arg3);
	}

	public override void ServerUse(HeldEntityServerUseParams parameters)
	{
		base.ServerUse(parameters);
		if (!IsInvoking(StopAfterTime))
		{
			lastPlayedTurretData = KeyController.Bindings.BaseBindings[Random.Range(0, KeyController.Bindings.BaseBindings.Length)];
			ClientRPC(RpcTarget.NetworkGroup("Client_PlayNote"), (int)lastPlayedTurretData.Note, (int)lastPlayedTurretData.Type, lastPlayedTurretData.NoteOctave, 1f);
			Invoke(StopAfterTime, 0.2f);
		}
	}

	private void StopAfterTime()
	{
		ClientRPC(RpcTarget.NetworkGroup("Client_StopNote"), (int)lastPlayedTurretData.Note, (int)lastPlayedTurretData.Type, lastPlayedTurretData.NoteOctave);
	}

	public override bool IsInstrument()
	{
		return true;
	}

	public InstrumentTool()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		PitchClamp = new Vector2(-90f, 90f);
		base._002Ector();
	}
}
