using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class NPCTalking : NPCShopKeeper, IConversationProvider
{
	[Serializable]
	public class NPCConversationResultAction
	{
		public string action;

		public int scrapCost;

		public string broadcastMessage;

		public float broadcastRange;
	}

	public Phrase NPCName = new Phrase("", "");

	[ConversationData.DisplayGraphViewButton]
	public ConversationData[] conversations = Array.Empty<ConversationData>();

	[ConversationData.DisplayGraphViewButton]
	public ConversationData[] phoneConversations = Array.Empty<ConversationData>();

	public static ListHashSet<IMissionProvider> serverMissionProviders = new ListHashSet<IMissionProvider>();

	public NPCConversationResultAction[] conversationResultActions;

	private ConversationHost _conversationHost;

	public BasePlayer lastActionPlayer;

	[NonSerialized]
	public float maxConversationDistance = 5f;

	private int __sync_PhoneNumber;

	[Sync(Autosave = true)]
	public int PhoneNumber
	{
		[CompilerGenerated]
		get
		{
			return __sync_PhoneNumber;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_PhoneNumber, value))
			{
				__sync_PhoneNumber = value;
				byte nameID = __GetWeaverID("PhoneNumber");
				QueueSyncVar(nameID);
			}
		}
	}

	private ConversationHost ConversationHost => _conversationHost ?? (_conversationHost = new ConversationHost(this));

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NPCTalking.OnRpcMessage"))
		{
			if (rpc == 2112414875 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_BeginTalking"));
				}
				using (TimeWarning.New("Server_BeginTalking"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2112414875u, "Server_BeginTalking", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2112414875u, "Server_BeginTalking", this, player, 3f))
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
							Server_BeginTalking(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_BeginTalking");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public Phrase GetNPCName()
	{
		return NPCName;
	}

	public bool ProviderBusy()
	{
		return HasFlag(Flags.Reserved1);
	}

	public Phrase GetPhoneDirectoryName()
	{
		if (NPCName == null || !NPCName.IsValid())
		{
			return Phrase.op_Implicit(ShortPrefabName);
		}
		return NPCName;
	}

	public BasePlayer GetActionPlayer()
	{
		return lastActionPlayer;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Application.isLoadingSave)
		{
			RegisterPhoneNumbers();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		RegisterPhoneNumbers();
	}

	private void RegisterPhoneNumbers()
	{
		PhoneNumber = ConversationHost.RegisterPhoneNumbers(phoneConversations, PhoneNumber);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			ConversationHost.SavePhoneNumbers(info.msg);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			ConversationHost.LoadPhoneNumbers(info.msg);
		}
	}

	internal override void DoServerDestroy()
	{
		ConversationHost.EndAllConversations();
		ConversationHost.DeregisterPhoneNumbers();
		base.DoServerDestroy();
	}

	public virtual ConversationData GetInPersonConversationFor(BasePlayer player)
	{
		if (conversations.Length == 0)
		{
			return null;
		}
		return conversations[0];
	}

	public virtual ConversationData GetPhoneConversationFor(BasePlayer player)
	{
		return ConversationHost.GetDirectCallConversation(phoneConversations, player, this);
	}

	public bool IsInPhoneConversationWith(BasePlayer player)
	{
		return ConversationHost.IsInPhoneConversationWith(player);
	}

	public ConversationData GetActiveConversationFor(BasePlayer player)
	{
		return ConversationHost.GetActiveConversationFor(player);
	}

	public bool BeginPhoneConversation(BasePlayer ply, PhoneController phone, ConversationData conversation)
	{
		return ConversationHost.BeginPhoneConversation(ply, phone, conversation);
	}

	protected virtual void AddSpeechNodeState(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, ConversationState state)
	{
	}

	void IConversationProvider.AddSpeechNodeState(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, ConversationState state)
	{
		AddSpeechNodeState(player, speechNode, state);
	}

	public virtual void AddFormatTextValues(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values)
	{
	}

	protected virtual bool NeedsFormatTextValue(ConversationData.AbstractSpeechNodeData speechNode, string key)
	{
		return speechNode.UsesFormatTextKey(key);
	}

	protected void AddFormatTextValue(List<FormatTextValue> values, ConversationData.AbstractSpeechNodeData speechNode, string key, int value)
	{
		if (NeedsFormatTextValue(speechNode, key))
		{
			ConversationHost.AddFormatTextValue(values, key, value.ToString());
		}
	}

	protected void AddFormatTextValue(List<FormatTextValue> values, ConversationData.AbstractSpeechNodeData speechNode, string key, string value, bool valueIsPhraseToken = false)
	{
		if (NeedsFormatTextValue(speechNode, key))
		{
			ConversationHost.AddFormatTextValue(values, key, value, valueIsPhraseToken);
		}
	}

	public void ForceEndConversation(BasePlayer player)
	{
		OnConversationEnded(player);
	}

	public void ForceSpeechNode(BasePlayer player, ConversationData data, int speechNodeIndex)
	{
		ConversationHost.ForceSpeechNode(player, data, speechNodeIndex);
	}

	public virtual void OnConversationEnded(BasePlayer player)
	{
		Interface.CallHook("OnNpcConversationEnded", this, player);
		ConversationHost.EndConversation(player);
	}

	public void CleanupConversingPlayers()
	{
		ConversationHost.Cleanup();
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void Server_BeginTalking(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		Server_BeginTalking(player);
	}

	protected virtual bool CanTalkTo(BasePlayer bp)
	{
		if ((Object)(object)bp == (Object)null)
		{
			return false;
		}
		if (!bp.IsOnGround() && !bp.IsFlying)
		{
			return false;
		}
		return true;
	}

	public void Server_BeginTalking(BasePlayer ply)
	{
		if (!CanTalkTo(ply))
		{
			return;
		}
		CleanupConversingPlayers();
		BaseMission.MissionStatus num = ply.GetActiveMissionInstance()?.status ?? BaseMission.MissionStatus.Undefined;
		uint num2 = ply.GetActiveMissionInstance()?.missionID ?? 0;
		Server_OnConversationStarted(ply);
		bool flag = num == BaseMission.MissionStatus.Active && ply.HasCompletedMission(num2);
		ConversationData inPersonConversationFor = GetInPersonConversationFor(ply);
		if ((Object)(object)inPersonConversationFor != (Object)null)
		{
			if (ConversationHost.IsConversingWith(ply))
			{
				OnConversationEnded(ply);
			}
			if (Interface.CallHook("OnNpcConversationStart", this, ply, inPersonConversationFor) != null)
			{
				return;
			}
			ConversationHost.StartConversation(ply);
			string text = inPersonConversationFor.entryPoint.resultingNode;
			if (flag && !ply.IsInTutorial)
			{
				ply.conversationRewardMissionId = num2;
				text = string.Empty;
				ConversationData.AbstractConversationNodeData[] controlNodes = inPersonConversationFor.controlNodes;
				for (int i = 0; i < controlNodes.Length; i++)
				{
					if (controlNodes[i] is ConversationData.ActionEventMissionCompletedNodeData actionEventMissionCompletedNodeData)
					{
						text = actionEventMissionCompletedNodeData.GetResultingNodeForMission(num2);
						break;
					}
				}
			}
			if (!string.IsNullOrWhiteSpace(text) || (!ply.IsInTutorial && flag))
			{
				inPersonConversationFor.GetFirstSpeechNodeFrom(ply, this, text, out var nodeIndex);
				if (nodeIndex < 0 && (ply.IsInTutorial || !flag))
				{
					Debug.LogError((object)("Failed to find a valid speech node starting from GUID: " + text + ", player: " + ((Object)ply).name + ", provider: " + ((Object)this).name), (Object)(object)this);
				}
				if (this is IMissionProvider missionProvider && !flag)
				{
					ply.Server_SendCanAcceptMissionsFromProvider(missionProvider);
				}
				ConversationState val = ConversationHost.BuildStartState(ply, inPersonConversationFor, nodeIndex);
				try
				{
					val.didCompleteMission = flag;
					if (val.conversationIndex == -1)
					{
						Debug.LogError((object)("Conversation " + ((Object)inPersonConversationFor).name + " is missing from the conversation manifest, the client cannot find it"), (Object)(object)inPersonConversationFor);
						OnConversationEnded(ply);
					}
					else
					{
						ply.SendConversationStart(val);
					}
					return;
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			Debug.LogError((object)("Failed to start conversation " + ((Object)inPersonConversationFor).name + ", entry node has no output"), (Object)(object)inPersonConversationFor);
			OnConversationEnded(ply);
		}
		else
		{
			OnConversationEnded(ply);
		}
	}

	public virtual void Server_OnConversationStarted(BasePlayer speakingTo)
	{
	}

	public virtual void UpdateFlags()
	{
	}

	public bool ValidConversationPlayer(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)player).transform.position, ((Component)this).transform.position) > maxConversationDistance)
		{
			return false;
		}
		if (ConversationHost.IsConversingWith(player))
		{
			return false;
		}
		return true;
	}

	public void HandleResponse(BasePlayer ply, int speechNodeIndex, int responseIndex, uint previewingMissionId, uint selectedMissionId, NetworkableId selectedAnimalId)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		if (!ConversationHost.TryGetShownSpeechNode(ply, speechNodeIndex, out var conversation, out var speechNode))
		{
			return;
		}
		string resultingNodeGuid = string.Empty;
		IMissionProvider missionProvider = this as IMissionProvider;
		if (speechNode is ConversationData.MissionListSpeechNodeData missionListSpeechNodeData)
		{
			if (!(this is NPCSimpleMissionProvider) || missionProvider == null)
			{
				Debug.LogError((object)string.Format("Response {0} pressed on mission list speech node on {1} but is not a {2}", responseIndex, ((Object)this).name, "NPCSimpleMissionProvider"));
				return;
			}
			if (missionProvider.TryGetMission(selectedMissionId, out var mission) && ply.Server_CanAcceptMission(missionProvider, mission))
			{
				ply.conversationPreviewMissionId = mission.id;
				bool flag = false;
				ConversationData.MissionListNodeOptionData[] resultingNodeOptions = missionListSpeechNodeData.resultingNodeOptions;
				foreach (ConversationData.MissionListNodeOptionData missionListNodeOptionData in resultingNodeOptions)
				{
					if (!(missionListNodeOptionData.selectedMission != mission))
					{
						resultingNodeGuid = missionListNodeOptionData.resultingNode;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					resultingNodeGuid = missionListSpeechNodeData.defaultResultingNode;
				}
			}
		}
		else if (speechNode is ConversationData.SellAnimalsSpeechNodeData sellAnimalsSpeechNodeData && selectedAnimalId.IsValid)
		{
			if (!(this is LivestockVendor livestockVendor))
			{
				Debug.LogError((object)string.Format("Animal {0} selected on sell animals speech node on {1} but is not a {2}", selectedAnimalId, ((Object)this).name, "LivestockVendor"), (Object)(object)this);
				return;
			}
			if (livestockVendor.Server_SelectAnimal(ply, selectedAnimalId))
			{
				resultingNodeGuid = sellAnimalsSpeechNodeData.animalSelectedResultingNode;
			}
		}
		else
		{
			if (!ply.CanSeeConversationResponse(responseIndex))
			{
				return;
			}
			if (speechNode is ConversationData.MissionPreviewSpeechNodeData && this is NPCSimpleMissionProvider && responseIndex == 0 && missionProvider != null && previewingMissionId == ply.conversationPreviewMissionId && missionProvider.TryGetMission(previewingMissionId, out var mission2) && ply.Server_CanAcceptMission(missionProvider, mission2))
			{
				TryAssignMissionToPlayer(mission2, ply);
			}
			if (Interface.CallHook("OnNpcConversationRespond", this, ply, conversation, speechNode, responseIndex) != null || !ConversationHost.TryGetResponseResult(ply, speechNode, responseIndex, out resultingNodeGuid))
			{
				return;
			}
		}
		ConversationHost.ShowNextSpeechNode(ply, conversation, resultingNodeGuid);
		Interface.CallHook("OnNpcConversationResponded", this, ply, conversation, speechNode, responseIndex);
	}

	public void Server_HandleRewardChoice(BasePlayer player, uint missionId, int rewardChoice)
	{
		if (!ConversationHost.CanDriveConversation(player) || player.conversationRewardMissionId == 0 || missionId != player.conversationRewardMissionId)
		{
			return;
		}
		if (!MissionManifest.TryGetFromID(missionId, out var mission))
		{
			Debug.LogError((object)$"{((Object)this).name} failed to retrieve a mission from ID {missionId}", (Object)(object)this);
			return;
		}
		BaseMission.MissionInstance missionInstance = null;
		for (int i = 0; i < player.acceptedMissions.Count; i++)
		{
			BaseMission.MissionInstance missionInstance2 = player.acceptedMissions[i];
			if (missionInstance2 != null && missionInstance2.missionID == missionId)
			{
				BaseMission.MissionStatus status = missionInstance2.status;
				if ((status == BaseMission.MissionStatus.Accomplished || status == BaseMission.MissionStatus.Completed) && !missionInstance2.hasDispensedRewards)
				{
					missionInstance = missionInstance2;
					break;
				}
			}
		}
		if (missionInstance != null)
		{
			mission.DispenseRewards(missionInstance, player, rewardChoice);
		}
	}

	public virtual void OnConversationAction(BasePlayer player, string action)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (action == "openvending")
		{
			InvisibleVendingMachine vendingMachine = GetVendingMachine();
			if ((Object)(object)vendingMachine != (Object)null && Vector3.Distance(((Component)player).transform.position, ((Component)this).transform.position) < 5f)
			{
				ForceEndConversation(player);
				if (Interface.CallHook("OnVendingShopOpen", vendingMachine, player) == null)
				{
					vendingMachine.PlayerOpenLoot(player, "vendingmachine.customer", doPositionChecks: false);
					Interface.CallHook("OnVendingShopOpened", vendingMachine, player);
				}
				return;
			}
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition("scrap");
		NPCConversationResultAction[] array = conversationResultActions;
		foreach (NPCConversationResultAction nPCConversationResultAction in array)
		{
			if (!(nPCConversationResultAction.action == action))
			{
				continue;
			}
			CleanupConversingPlayers();
			foreach (BasePlayer conversingPlayer in ConversationHost.ConversingPlayers)
			{
				if ((Object)(object)conversingPlayer == (Object)(object)player || (Object)(object)conversingPlayer == (Object)null)
				{
					continue;
				}
				int speechNodeIndex = -1;
				ConversationData activeConversationFor = GetActiveConversationFor(conversingPlayer);
				if ((Object)(object)activeConversationFor == (Object)null)
				{
					continue;
				}
				ConversationData.AbstractConversationNodeData[] controlNodes = activeConversationFor.controlNodes;
				for (int j = 0; j < controlNodes.Length; j++)
				{
					if (controlNodes[j] is ConversationData.ActionEventOtherPlayerInvokedData actionEventOtherPlayerInvokedData)
					{
						speechNodeIndex = activeConversationFor.GetSpeechNodeIndex(actionEventOtherPlayerInvokedData.resultingNode);
						break;
					}
				}
				ForceSpeechNode(conversingPlayer, activeConversationFor, speechNodeIndex);
			}
			int num = nPCConversationResultAction.scrapCost;
			PooledList<Item> val = Pool.Get<PooledList<Item>>();
			try
			{
				player.inventory.FindItemsByItemID((List<Item>)(object)val, itemDefinition.itemid);
				foreach (Item item in (List<Item>)(object)val)
				{
					num -= item.amount;
				}
				if (num > 0)
				{
					int speechNodeIndex2 = -1;
					ConversationData activeConversationFor2 = GetActiveConversationFor(player);
					if ((Object)(object)activeConversationFor2 == (Object)null)
					{
						break;
					}
					ConversationData.AbstractConversationNodeData[] controlNodes = activeConversationFor2.controlNodes;
					for (int j = 0; j < controlNodes.Length; j++)
					{
						if (controlNodes[j] is ConversationData.ActionEventPlayerTooPoorNodeData actionEventPlayerTooPoorNodeData)
						{
							speechNodeIndex2 = activeConversationFor2.GetSpeechNodeIndex(actionEventPlayerTooPoorNodeData.resultingNode);
							break;
						}
					}
					ForceSpeechNode(player, activeConversationFor2, speechNodeIndex2);
					break;
				}
				Facepunch.Rust.Analytics.Azure.OnNPCVendor(player, this, nPCConversationResultAction.scrapCost, nPCConversationResultAction.action);
				num = nPCConversationResultAction.scrapCost;
				foreach (Item item2 in (List<Item>)(object)val)
				{
					int num2 = Mathf.Min(num, item2.amount);
					item2.UseItem(num2);
					num -= num2;
					if (num <= 0)
					{
						break;
					}
				}
				lastActionPlayer = player;
				BroadcastEntityMessage(nPCConversationResultAction.broadcastMessage, nPCConversationResultAction.broadcastRange);
				lastActionPlayer = null;
				break;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	protected virtual void TryAssignMissionToPlayer(BaseMission mission, BasePlayer player)
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: PhoneNumber for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_PhoneNumber);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_PhoneNumber;
				int _sync_PhoneNumber = reader.Int32();
				__sync_PhoneNumber = _sync_PhoneNumber;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "PhoneNumber")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_PhoneNumber = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public NPCTalking()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
	}
}
