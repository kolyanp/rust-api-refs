using System;
using System.Collections.Generic;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class PhoneController : EntityComponent<BaseEntity>, IServerFileReceiver
{
	private PhoneController activeOtherPhoneController;

	private NPCPhoneNumber activeNPCNumber;

	private BasePlayer playerCallingNpc;

	public int PhoneNumber;

	public string PhoneName;

	public bool CanModifyPhoneName = true;

	public bool CanSaveNumbers = true;

	public bool RequirePower = true;

	public bool RequireParent;

	public const float CallWaitingTime = 24f;

	public bool AppendGridToName;

	public bool IsMobile;

	public bool CanSaveVoicemail;

	public GameObjectRef PhoneDialog;

	public VoiceProcessor VProcessor;

	public PreloadedCassetteContent PreloadedContent;

	public SoundDefinition DialToneSfx;

	public SoundDefinition RingingSfx;

	public SoundDefinition ErrorSfx;

	public SoundDefinition CallIncomingWhileBusySfx;

	public SoundDefinition PickupHandsetSfx;

	public SoundDefinition PutDownHandsetSfx;

	public SoundDefinition FailedWrongNumber;

	public SoundDefinition FailedNoAnswer;

	public SoundDefinition FailedNetworkBusy;

	public SoundDefinition FailedEngaged;

	public SoundDefinition FailedRemoteHangUp;

	public SoundDefinition FailedSelfHangUp;

	public Light RingingLight;

	public float RingingLightFrequency = 0.4f;

	public AudioSource answeringMachineSound;

	public EntityRef currentPlayerRef;

	public List<VoicemailEntry> savedVoicemail;

	public Telephone.CallState serverState { get; set; }

	private int ConnectedCallNumber
	{
		get
		{
			if (!((Object)(object)activeOtherPhoneController != (Object)null))
			{
				if (activeNPCNumber == null)
				{
					return 0;
				}
				return activeNPCNumber.Number;
			}
			return activeOtherPhoneController.PhoneNumber;
		}
	}

	public uint AnsweringMessageId
	{
		get
		{
			if (!(baseEntity is Telephone telephone))
			{
				return 0u;
			}
			return telephone.AnsweringMessageId;
		}
	}

	public int MaxVoicemailSlots
	{
		get
		{
			if (!((Object)(object)cachedCassette != (Object)null))
			{
				return 0;
			}
			return cachedCassette.MaximumVoicemailSlots;
		}
	}

	public BasePlayer currentPlayer
	{
		get
		{
			if (currentPlayerRef.IsValid(isServer))
			{
				return currentPlayerRef.Get(isServer).ToPlayer();
			}
			return null;
		}
		set
		{
			currentPlayerRef.Set(value);
		}
	}

	private bool isServer
	{
		get
		{
			if ((Object)(object)baseEntity != (Object)null)
			{
				return baseEntity.isServer;
			}
			return false;
		}
	}

	public int lastDialedNumber { get; set; }

	public PhoneDirectory savedNumbers { get; set; }

	public BaseEntity ParentEntity => baseEntity;

	private Cassette cachedCassette
	{
		get
		{
			if (!((Object)(object)baseEntity != (Object)null) || !(baseEntity is Telephone telephone))
			{
				return null;
			}
			return telephone.cachedCassette;
		}
	}

	public void ServerInit()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (PhoneNumber == 0 && !Application.isLoadingSave)
		{
			PhoneNumber = TelephoneManager.GetUnusedTelephoneNumber();
			if (AppendGridToName & !string.IsNullOrEmpty(PhoneName))
			{
				PhoneName = PhoneName + " [" + MapHelper.PositionToString(((Component)this).transform.position) + "]";
			}
			TelephoneManager.RegisterTelephone(this);
		}
	}

	public void PostServerLoad()
	{
		currentPlayer = null;
		baseEntity.SetFlagLocal(BaseEntity.Flags.Busy, b: false);
		TelephoneManager.RegisterTelephone(this);
	}

	public void DoServerDestroy()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TelephoneManager.DeregisterTelephone(this);
		FileStorage.server.RemoveAll(baseEntity.net.ID);
	}

	public void ClearCurrentUser(BaseEntity.RPCMessage msg)
	{
		ClearCurrentUser();
	}

	public void ClearCurrentUser()
	{
		if ((Object)(object)currentPlayer != (Object)null)
		{
			currentPlayer.SetActiveTelephone(null);
			currentPlayer = null;
		}
		using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(BaseEntity.Flags.Busy, b: false);
	}

	public void SetCurrentUser(BaseEntity.RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)currentPlayer == (Object)(object)player))
		{
			UpdateServerPlayer(player);
			if (serverState == Telephone.CallState.Dialing || serverState == Telephone.CallState.Ringing || serverState == Telephone.CallState.InProcess)
			{
				ServerHangUp(default);
			}
		}
	}

	private void UpdateServerPlayer(BasePlayer newPlayer)
	{
		if (!((Object)(object)currentPlayer == (Object)(object)newPlayer))
		{
			if ((Object)(object)currentPlayer != (Object)null)
			{
				currentPlayer.SetActiveTelephone(null);
			}
			currentPlayer = newPlayer;
			using (BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(BaseEntity.Flags.Busy, (Object)(object)currentPlayer != (Object)null);
			}
			if ((Object)(object)currentPlayer != (Object)null)
			{
				currentPlayer.SetActiveTelephone(this);
			}
		}
	}

	public void InitiateCall(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)currentPlayer))
		{
			int number = msg.read.Int32();
			CallPhone(number);
		}
	}

	public void CallPhone(int number)
	{
		if (number == PhoneNumber)
		{
			OnDialFailed(Telephone.DialFailReason.CallSelf);
			return;
		}
		if (TelephoneManager.GetCurrentActiveCalls() + 1 > TelephoneManager.MaxConcurrentCalls)
		{
			OnDialFailed(Telephone.DialFailReason.NetworkBusy);
			return;
		}
		PhoneController telephone = TelephoneManager.GetTelephone(number);
		if ((Object)(object)telephone != (Object)null)
		{
			if (Interface.CallHook("OnPhoneDial", this, telephone, currentPlayer) == null)
			{
				if (telephone.serverState == Telephone.CallState.Idle && telephone.CanReceiveCall())
				{
					SetPhoneState(Telephone.CallState.Dialing);
					lastDialedNumber = number;
					activeOtherPhoneController = telephone;
					activeOtherPhoneController.ReceiveCallFrom(this);
				}
				else
				{
					OnDialFailed(Telephone.DialFailReason.Engaged);
					telephone.OnIncomingCallWhileBusy();
				}
			}
		}
		else
		{
			NPCPhoneNumber nPCPhone = TelephoneManager.GetNPCPhone(number);
			ConversationData conversationData = nPCPhone?.GetConversationFor(currentPlayer);
			if ((Object)(object)conversationData != (Object)null)
			{
				CallNPC(nPCPhone, conversationData);
			}
			else
			{
				OnDialFailed(Telephone.DialFailReason.WrongNumber);
			}
		}
	}

	private void CallNPC(NPCPhoneNumber npcNumber, ConversationData conversation)
	{
		if (activeNPCNumber == null && !((Object)(object)activeOtherPhoneController != (Object)null))
		{
			BasePlayer basePlayer = currentPlayer;
			if ((Object)(object)basePlayer == (Object)null)
			{
				OnDialFailed(Telephone.DialFailReason.TimedOut);
				return;
			}
			if (!npcNumber.Provider.BeginPhoneConversation(basePlayer, this, conversation))
			{
				OnDialFailed(Telephone.DialFailReason.TimedOut);
				return;
			}
			lastDialedNumber = npcNumber.Number;
			activeNPCNumber = npcNumber;
			playerCallingNpc = basePlayer;
			SetPhoneState(Telephone.CallState.InProcess);
		}
	}

	public void OnNPCConversationEnded()
	{
		if (activeNPCNumber != null)
		{
			activeNPCNumber = null;
			playerCallingNpc = null;
			ServerHangUp();
		}
	}

	private void EndNPCCall()
	{
		if (activeNPCNumber != null)
		{
			NPCPhoneNumber nPCPhoneNumber = activeNPCNumber;
			BasePlayer player = playerCallingNpc;
			activeNPCNumber = null;
			playerCallingNpc = null;
			nPCPhoneNumber.Provider.OnConversationEnded(player);
		}
	}

	private bool CanReceiveCall()
	{
		object obj = Interface.CallHook("CanReceiveCall", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (RequirePower && !IsPowered())
		{
			return false;
		}
		if (RequireParent && !baseEntity.HasParent())
		{
			return false;
		}
		return true;
	}

	public void AnswerPhone(BaseEntity.RPCMessage msg)
	{
		if (IsInvoking(TimeOutDialing))
		{
			CancelInvoke(TimeOutDialing);
		}
		if (!((Object)(object)activeOtherPhoneController == (Object)null))
		{
			BasePlayer player = msg.player;
			if (Interface.CallHook("OnPhoneAnswer", this, activeOtherPhoneController) == null)
			{
				UpdateServerPlayer(player);
				BeginCall();
				activeOtherPhoneController.BeginCall();
				Interface.CallHook("OnPhoneAnswered", this, activeOtherPhoneController);
			}
		}
	}

	public void ReceiveCallFrom(PhoneController t)
	{
		activeOtherPhoneController = t;
		SetPhoneState(Telephone.CallState.Ringing);
		Invoke(TimeOutDialing, 24f);
	}

	private void TimeOutDialing()
	{
		if (Interface.CallHook("OnPhoneDialTimeout", activeOtherPhoneController, this, activeOtherPhoneController.currentPlayer) == null)
		{
			if ((Object)(object)activeOtherPhoneController != (Object)null)
			{
				activeOtherPhoneController.ServerPlayAnsweringMessage(this);
			}
			SetPhoneState(Telephone.CallState.Idle);
			Interface.CallHook("OnPhoneDialTimedOut", activeOtherPhoneController, this, activeOtherPhoneController.currentPlayer);
		}
	}

	public void OnDialFailed(Telephone.DialFailReason reason)
	{
		if (Interface.CallHook("OnPhoneDialFail", this, reason, currentPlayer) == null)
		{
			SetPhoneState(Telephone.CallState.Idle);
			baseEntity.ClientRPC(RpcTarget.NetworkGroup("ClientOnDialFailed"), (int)reason);
			activeOtherPhoneController = null;
			EndNPCCall();
			if (IsInvoking(TimeOutCall))
			{
				CancelInvoke(TimeOutCall);
			}
			if (IsInvoking(TriggerTimeOut))
			{
				CancelInvoke(TriggerTimeOut);
			}
			if (IsInvoking(TimeOutDialing))
			{
				CancelInvoke(TimeOutDialing);
			}
			Interface.CallHook("OnPhoneDialFailed", this, reason, currentPlayer);
		}
	}

	public void ServerPlayAnsweringMessage(PhoneController fromPhone)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId arg = default;
		uint num = 0u;
		uint arg2 = 0u;
		if ((Object)(object)activeOtherPhoneController != (Object)null && (Object)(object)activeOtherPhoneController.cachedCassette != (Object)null)
		{
			arg = activeOtherPhoneController.cachedCassette.net.ID;
			num = activeOtherPhoneController.cachedCassette.AudioId;
			if (num == 0)
			{
				arg2 = activeOtherPhoneController.cachedCassette.PreloadContent.GetSoundContentId(activeOtherPhoneController.cachedCassette.PreloadedAudio);
			}
		}
		if (arg.IsValid)
		{
			baseEntity.ClientRPC(RpcTarget.NetworkGroup("ClientPlayAnsweringMessage"), arg, num, arg2, fromPhone.HasVoicemailSlot() ? 1 : 0, activeOtherPhoneController.PhoneNumber);
			Invoke(TriggerTimeOut, activeOtherPhoneController.cachedCassette.MaxCassetteLength);
		}
		else
		{
			OnDialFailed(Telephone.DialFailReason.TimedOut);
		}
	}

	private void TriggerTimeOut()
	{
		OnDialFailed(Telephone.DialFailReason.TimedOut);
	}

	public void SetPhoneStateWithPlayer(Telephone.CallState state)
	{
		serverState = state;
		baseEntity.ClientRPC(RpcTarget.NetworkGroup("SetClientState"), (int)serverState, ConnectedCallNumber);
		if (baseEntity is MobilePhone mobilePhone)
		{
			mobilePhone.ToggleRinging(state == Telephone.CallState.Ringing);
		}
	}

	private void SetPhoneState(Telephone.CallState state)
	{
		if (state == Telephone.CallState.Idle && (Object)(object)currentPlayer == (Object)null)
		{
			using BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(BaseEntity.Flags.Busy, b: false);
		}
		serverState = state;
		baseEntity.ClientRPC(RpcTarget.NetworkGroup("SetClientState"), (int)serverState, ConnectedCallNumber);
		if (baseEntity is Telephone telephone)
		{
			telephone.MarkDirtyForceUpdateOutputs();
		}
		if (baseEntity is MobilePhone mobilePhone)
		{
			mobilePhone.ToggleRinging(state == Telephone.CallState.Ringing);
		}
	}

	public void BeginCall()
	{
		if (Interface.CallHook("OnPhoneCallStart", this, activeOtherPhoneController, currentPlayer) == null)
		{
			if (IsMobile && (Object)(object)activeOtherPhoneController != (Object)null && !activeOtherPhoneController.RequirePower)
			{
				_ = (Object)(object)currentPlayer != (Object)null;
			}
			SetPhoneStateWithPlayer(Telephone.CallState.InProcess);
			Invoke(TimeOutCall, TelephoneManager.MaxCallLength);
			Interface.CallHook("OnPhoneCallStarted", this, activeOtherPhoneController, currentPlayer);
		}
	}

	public void ServerHangUp(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)currentPlayer))
		{
			ServerHangUp();
		}
	}

	public void ServerHangUp()
	{
		if ((Object)(object)activeOtherPhoneController != (Object)null)
		{
			activeOtherPhoneController.RemoteHangUp();
		}
		SelfHangUp();
	}

	private void SelfHangUp()
	{
		OnDialFailed(Telephone.DialFailReason.SelfHangUp);
	}

	private void RemoteHangUp()
	{
		OnDialFailed(Telephone.DialFailReason.RemoteHangUp);
	}

	private void TimeOutCall()
	{
		OnDialFailed(Telephone.DialFailReason.TimeOutDuringCall);
	}

	public void OnReceivedVoiceFromUser(ReadOnlySpan<byte> data)
	{
		if ((Object)(object)activeOtherPhoneController != (Object)null)
		{
			activeOtherPhoneController.OnReceivedDataFromConnectedPhone(data);
		}
	}

	public void OnReceivedDataFromConnectedPhone(ReadOnlySpan<byte> data)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		baseEntity.ClientRPC(RpcTarget.SendInfo("OnReceivedVoice", new SendInfo(BaseNetworkable.GetConnectionsWithin(((Component)this).transform.position, 15f))
		{
			priority = Priority.Immediate
		}), data.Length, data);
	}

	public void OnIncomingCallWhileBusy()
	{
		baseEntity.ClientRPC(RpcTarget.NetworkGroup("OnIncomingCallDuringCall"));
	}

	private void DestroyServer()
	{
		if (isServer && serverState != Telephone.CallState.Idle && (Object)(object)activeOtherPhoneController != (Object)null)
		{
			activeOtherPhoneController.RemoteHangUp();
		}
		EndNPCCall();
	}

	public void RPC_UpdatePhoneName(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)currentPlayer))
		{
			string newName = msg.read.String();
			UpdatePhoneName(newName);
		}
	}

	public void UpdatePhoneName(string newName)
	{
		if (newName.Length > 30)
		{
			newName = newName.Substring(0, 30);
		}
		if (Interface.CallHook("OnPhoneNameUpdate", this, newName, currentPlayer) == null)
		{
			PhoneName = newName;
			baseEntity.SendNetworkUpdate();
			Interface.CallHook("OnPhoneNameUpdated", this, PhoneName, currentPlayer);
		}
	}

	private void InitSavedNumbers()
	{
		if (savedNumbers == null)
		{
			savedNumbers = Pool.Get<PhoneDirectory>();
		}
		if (savedNumbers.entries == null)
		{
			savedNumbers.entries = Pool.Get<List<DirectoryEntry>>();
		}
	}

	public void Server_RequestPhoneDirectory(BaseEntity.RPCMessage msg)
	{
		if ((Object)(object)msg.player != (Object)(object)currentPlayer)
		{
			return;
		}
		int page = msg.read.Int32();
		PhoneDirectory val = Pool.Get<PhoneDirectory>();
		try
		{
			TelephoneManager.GetPhoneDirectory(msg.player, PhoneNumber, page, 12, val);
			baseEntity.ClientRPC(RpcTarget.Player("ReceivePhoneDirectory", msg.player), val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void Server_AddSavedNumber(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)currentPlayer))
		{
			int number = msg.read.Int32();
			string phoneName = msg.read.String();
			InitSavedNumbers();
			if (savedNumbers.entries.Count < 10)
			{
				AddSavedNumber(number, phoneName);
			}
		}
	}

	public void AddSavedNumber(int number, string phoneName)
	{
		InitSavedNumbers();
		if (IsSavedContactValid(phoneName, number))
		{
			DirectoryEntry val = Pool.Get<DirectoryEntry>();
			val.phoneName = phoneName;
			val.phoneNumber = number;
			val.ShouldPool = false;
			savedNumbers.ShouldPool = false;
			savedNumbers.entries.Add(val);
			baseEntity.SendNetworkUpdate();
		}
	}

	public void Server_RemoveSavedNumber(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player != (Object)(object)currentPlayer))
		{
			uint number = msg.read.UInt32();
			if (savedNumbers.entries.RemoveAll((DirectoryEntry p) => p.phoneNumber == number) > 0)
			{
				baseEntity.SendNetworkUpdate();
			}
		}
	}

	public string GetDirectoryName()
	{
		return PhoneName;
	}

	public void WatchForDisconnects()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if ((Object)(object)currentPlayer != (Object)null)
		{
			if (currentPlayer.IsSleeping())
			{
				flag = true;
			}
			if (currentPlayer.IsDead())
			{
				flag = true;
			}
			if (Vector3.SqrMagnitude(((Component)this).transform.position - ((Component)currentPlayer).transform.position) > 25f)
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			ServerHangUp();
			ClearCurrentUser();
		}
	}

	public void OnParentChanged(BaseEntity newParent)
	{
		if ((Object)(object)newParent != (Object)null && newParent is BasePlayer)
		{
			TelephoneManager.RegisterTelephone(this, checkPhoneNumber: true);
		}
		else
		{
			TelephoneManager.DeregisterTelephone(this);
		}
	}

	private bool HasVoicemailSlot()
	{
		return MaxVoicemailSlots > 0;
	}

	public void ServerSendVoicemail(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)msg.player == (Object)null))
		{
			byte[] array = msg.read.BytesWithSize();
			PhoneController telephone = TelephoneManager.GetTelephone(msg.read.Int32());
			if (!((Object)(object)telephone == (Object)null) && !((Object)(object)telephone.cachedCassette == (Object)null) && Cassette.IsOggValid(array, telephone.cachedCassette))
			{
				telephone.SaveVoicemail(array, msg.player.displayName);
				Facepunch.Rust.Analytics.Azure.OnUGCCreated(msg.player, telephone.baseEntity, "voicemail", array.Length);
			}
		}
	}

	public void SaveVoicemail(byte[] data, string playerName)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		uint audioId = FileStorage.server.StoreByCrc(baseEntity.net.ID, FileStorage.Type.ogg, data);
		if (savedVoicemail == null)
		{
			savedVoicemail = Pool.Get<List<VoicemailEntry>>();
		}
		VoicemailEntry val = Pool.Get<VoicemailEntry>();
		val.audioId = audioId;
		val.timestamp = DateTime.Now.ToBinary();
		val.userName = playerName;
		val.ShouldPool = false;
		savedVoicemail.Add(val);
		while (savedVoicemail.Count > MaxVoicemailSlots)
		{
			uint audioId2 = savedVoicemail[0].audioId;
			savedVoicemail.RemoveAt(0);
			RemoveVoicemailFile(audioId2);
		}
		baseEntity.SendNetworkUpdate();
	}

	public void ServerPlayVoicemail(BaseEntity.RPCMessage msg)
	{
		if (!((Object)(object)cachedCassette == (Object)null))
		{
			baseEntity.ClientRPC(RpcTarget.NetworkGroup("ClientToggleVoicemail"), 1, msg.read.UInt32());
		}
	}

	public void ServerStopVoicemail(BaseEntity.RPCMessage msg)
	{
		baseEntity.ClientRPC(RpcTarget.NetworkGroup("ClientToggleVoicemail"), 0, 0);
	}

	public void ServerDeleteVoicemail(BaseEntity.RPCMessage msg)
	{
		uint num = msg.read.UInt32();
		for (int i = 0; i < savedVoicemail.Count; i++)
		{
			if (savedVoicemail[i].audioId == num)
			{
				VoicemailEntry val = savedVoicemail[i];
				val.ShouldPool = true;
				val.Dispose();
				savedVoicemail.RemoveAt(i);
				RemoveVoicemailFile(num);
				baseEntity.SendNetworkUpdate();
				break;
			}
		}
	}

	public void DeleteAllVoicemail()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (savedVoicemail == null)
		{
			return;
		}
		foreach (VoicemailEntry item in savedVoicemail)
		{
			item.ShouldPool = true;
			FileStorage.server.Remove(baseEntity.net.ID, item.audioId);
		}
		Pool.Free<VoicemailEntry>(ref savedVoicemail, false);
	}

	private void RemoveVoicemailFile(uint audioId)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		foreach (VoicemailEntry item in savedVoicemail)
		{
			if (item.audioId == audioId)
			{
				return;
			}
		}
		FileStorage.server.Remove(baseEntity.net.ID, audioId);
	}

	public override void InitShared()
	{
		base.InitShared();
		UpdateBaseEntity();
	}

	public override void DestroyShared()
	{
		DestroyServer();
	}

	private bool IsPowered()
	{
		if ((Object)(object)baseEntity != (Object)null && baseEntity is IOEntity iOEntity)
		{
			return iOEntity.IsPowered();
		}
		return false;
	}

	public bool IsSavedContactValid(string contactName, int contactNumber)
	{
		if (contactName.Length <= 0 || contactName.Length > 30)
		{
			return false;
		}
		if (contactNumber < 10000000 || contactNumber >= 100000000)
		{
			return false;
		}
		return true;
	}

	public void OnFlagsChanged(BaseEntity.Flags old, BaseEntity.Flags next)
	{
	}
}
