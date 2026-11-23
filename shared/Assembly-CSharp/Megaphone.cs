using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Megaphone : HeldEntity
{
	[Header("Megaphone")]
	public VoiceProcessor voiceProcessor;

	public float VoiceDamageMinFrequency = 2f;

	public float VoiceDamageAmount = 1f;

	public AudioSource VoiceSource;

	public SoundDefinition StartBroadcastingSfx;

	public SoundDefinition StopBroadcastingSfx;

	[ReplicatedVar(Default = "100")]
	public static float MegaphoneVoiceRange { get; set; } = 100f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Megaphone.OnRpcMessage"))
		{
			if (rpc == 4196056309u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_ToggleBroadcasting"));
				}
				using (TimeWarning.New("Server_ToggleBroadcasting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(4196056309u, "Server_ToggleBroadcasting", this, player))
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
							Server_ToggleBroadcasting(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_ToggleBroadcasting");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void UpdateItemCondition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && ownerItem.hasCondition)
		{
			ownerItem.LoseCondition(VoiceDamageAmount);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void Server_ToggleBroadcasting(RPCMessage msg)
	{
		bool flag = msg.read.Int8() == 1;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, flag);
		}
		if (flag)
		{
			if (!IsInvoking(UpdateItemCondition))
			{
				InvokeRepeating(UpdateItemCondition, 0f, VoiceDamageMinFrequency);
			}
		}
		else if (IsInvoking(UpdateItemCondition))
		{
			CancelInvoke(UpdateItemCondition);
		}
	}
}
