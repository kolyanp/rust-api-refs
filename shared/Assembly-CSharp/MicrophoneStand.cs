using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;

public class MicrophoneStand : BaseMountable
{
	public enum SpeechMode
	{
		Normal,
		HighPitch,
		LowPitch
	}

	public VoiceProcessor VoiceProcessor;

	public AudioSource VoiceSource;

	private SpeechMode currentSpeechMode;

	public AudioMixerGroup NormalMix;

	public AudioMixerGroup HighPitchMix;

	public AudioMixerGroup LowPitchMix;

	public Phrase NormalPhrase;

	public Phrase NormalDescPhrase;

	public Phrase HighPitchPhrase;

	public Phrase HighPitchDescPhrase;

	public Phrase LowPitchPhrase;

	public Phrase LowPitchDescPhrase;

	public GameObjectRef IOSubEntity;

	public Transform IOSubEntitySpawnPos;

	public Transform LeftHandIKTarget;

	public Transform RightHandIKTarget;

	public bool IsStatic;

	public EntityRef<IOEntity> ioEntity;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MicrophoneStand.OnRpcMessage"))
		{
			if (rpc == 1420522459 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetMode"));
				}
				using (TimeWarning.New("SetMode"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage mode = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetMode(mode);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SetMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	public void SetMode(RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)GetMounted()))
		{
			SpeechMode speechMode = (SpeechMode)msg.read.Int32();
			if (speechMode != currentSpeechMode)
			{
				currentSpeechMode = speechMode;
				SendNetworkUpdate();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.microphoneStand == null)
		{
			info.msg.microphoneStand = Pool.Get<MicrophoneStand>();
		}
		info.msg.microphoneStand.microphoneMode = (int)currentSpeechMode;
		info.msg.microphoneStand.IORef = ioEntity.uid;
	}

	public void SpawnChildEntity()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		MicrophoneStandIOEntity microphoneStandIOEntity = GameManager.server.CreateEntity(IOSubEntity.resourcePath, IOSubEntitySpawnPos.localPosition, IOSubEntitySpawnPos.localRotation) as MicrophoneStandIOEntity;
		microphoneStandIOEntity.enableSaving = enableSaving;
		microphoneStandIOEntity.SetParent(this);
		microphoneStandIOEntity.Spawn();
		ioEntity.Set(microphoneStandIOEntity);
		SendNetworkUpdate();
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		SpawnChildEntity();
	}

	public override void PostMapEntitySpawn()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.PostMapEntitySpawn();
		if (!IsStatic)
		{
			return;
		}
		SpawnChildEntity();
		int num = 128;
		List<ConnectedSpeaker> list = Pool.Get<List<ConnectedSpeaker>>();
		GamePhysics.OverlapSphere<ConnectedSpeaker>(((Component)this).transform.position, (float)num, list, 256, (QueryTriggerInteraction)1);
		IOEntity iOEntity = ioEntity.Get(serverside: true);
		List<MicrophoneStand> list2 = Pool.Get<List<MicrophoneStand>>();
		int num2 = 0;
		foreach (ConnectedSpeaker item in list)
		{
			bool flag = true;
			list2.Clear();
			GamePhysics.OverlapSphere<MicrophoneStand>(((Component)item).transform.position, (float)num, list2, 256, (QueryTriggerInteraction)1);
			if (list2.Count > 1)
			{
				float num3 = Distance((BaseEntity)item);
				foreach (MicrophoneStand item2 in list2)
				{
					if (!item2.isClient && item2.Distance((BaseEntity)item) < num3)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				iOEntity.outputs[0].connectedTo.Set(item);
				item.inputs[0].connectedTo.Set(iOEntity);
				iOEntity = item;
				num2++;
			}
		}
		Pool.FreeUnmanaged<ConnectedSpeaker>(ref list);
		Pool.FreeUnmanaged<MicrophoneStand>(ref list2);
	}

	public override void Load(LoadInfo info)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.microphoneStand != null)
		{
			currentSpeechMode = (SpeechMode)info.msg.microphoneStand.microphoneMode;
			ioEntity.uid = info.msg.microphoneStand.IORef;
		}
	}

	public MicrophoneStand()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		NormalPhrase = new Phrase("microphone_normal", "Normal");
		NormalDescPhrase = new Phrase("microphone_normal_desc", "No voice effect");
		HighPitchPhrase = new Phrase("microphone_high", "High Pitch");
		HighPitchDescPhrase = new Phrase("microphone_high_desc", "High pitch voice");
		LowPitchPhrase = new Phrase("microphone_low", "Low");
		LowPitchDescPhrase = new Phrase("microphone_low_desc", "Low pitch voice");
		base._002Ector();
	}
}
