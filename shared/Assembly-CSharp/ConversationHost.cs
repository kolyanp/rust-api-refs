using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ConversationHost
{
	private struct PhoneCall
	{
		public PhoneController Phone;

		public ConversationData Conversation;
	}

	private readonly IConversationProvider provider;

	private readonly List<BasePlayer> conversingPlayers = new List<BasePlayer>();

	private readonly Dictionary<BasePlayer, PhoneCall> phoneCalls = new Dictionary<BasePlayer, PhoneCall>();

	private readonly List<NPCPhoneNumber> phoneNumbers = new List<NPCPhoneNumber>();

	private readonly Dictionary<string, int> phoneNumbersByName = new Dictionary<string, int>();

	public IReadOnlyList<BasePlayer> ConversingPlayers => conversingPlayers;

	public ConversationHost(IConversationProvider provider)
	{
		this.provider = provider;
	}

	public bool IsConversingWith(BasePlayer player)
	{
		return conversingPlayers.Contains(player);
	}

	public bool IsInPhoneConversationWith(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return phoneCalls.ContainsKey(player);
		}
		return false;
	}

	public bool CanDriveConversation(BasePlayer player)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!IsInPhoneConversationWith(player))
		{
			return provider.GetEntity().SqrDistance(player.eyes.position) <= 9f;
		}
		return true;
	}

	public ConversationData GetActiveConversationFor(BasePlayer player)
	{
		if (!((Object)(object)player != (Object)null) || !phoneCalls.TryGetValue(player, out var value))
		{
			return provider.GetInPersonConversationFor(player);
		}
		return value.Conversation;
	}

	public static ConversationData GetDirectCallConversation(ConversationData[] phoneConversations, BasePlayer player, IConversationProvider provider)
	{
		foreach (ConversationData conversationData in phoneConversations)
		{
			if ((Object)(object)conversationData != (Object)null && !conversationData.HasPhoneNumber && conversationData.IsAvailableByPhone(player, provider))
			{
				return conversationData;
			}
		}
		return null;
	}

	public void StartConversation(BasePlayer player)
	{
		conversingPlayers.Add(player);
		player.SetActiveTalkingToNpc(provider);
		provider.UpdateFlags();
	}

	public bool BeginPhoneConversation(BasePlayer player, PhoneController phone, ConversationData conversation)
	{
		if ((Object)(object)player == (Object)null || (Object)(object)phone == (Object)null || (Object)(object)conversation == (Object)null)
		{
			return false;
		}
		Cleanup();
		if (IsConversingWith(player))
		{
			provider.OnConversationEnded(player);
		}
		phoneCalls[player] = new PhoneCall
		{
			Phone = phone,
			Conversation = conversation
		};
		StartConversation(player);
		conversation.GetFirstSpeechNodeFrom(player, provider, conversation.entryPoint.resultingNode, out var nodeIndex);
		if (nodeIndex < 0)
		{
			Debug.LogError((object)("Failed to start phone conversation " + ((Object)conversation).name + ", entry node leads to no speech node"), (Object)(object)conversation);
			provider.OnConversationEnded(player);
			return false;
		}
		if (provider is IMissionProvider missionProvider)
		{
			player.Server_SendCanAcceptMissionsFromProvider(missionProvider);
		}
		ConversationState val = BuildStartState(player, conversation, nodeIndex);
		try
		{
			val.isPhoneConversation = true;
			if (val.conversationIndex == -1)
			{
				Debug.LogError((object)("Conversation " + ((Object)conversation).name + " is missing from the conversation manifest, the client cannot find it"), (Object)(object)conversation);
				provider.OnConversationEnded(player);
				return false;
			}
			player.SendConversationStart(val);
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public ConversationState BuildStartState(BasePlayer player, ConversationData conversation, int speechNodeIndex)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		ConversationState val = BuildState(player, conversation, speechNodeIndex);
		Phrase val2 = ((conversation.HasPhoneNumber && IsInPhoneConversationWith(player)) ? conversation.phoneNumberName : provider.GetNPCName());
		val.conversationIndex = ConversationManifest.GetConversationIndex(conversation);
		val.npcNameToken = ((val2 != null) ? val2.token : string.Empty);
		val.npcNameEnglish = ((val2 != null) ? val2.english : string.Empty);
		val.npcEntityId = provider.GetEntity().net.ID;
		return val;
	}

	private ConversationState BuildState(BasePlayer player, ConversationData conversation, int speechNodeIndex)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		ConversationState val = Pool.Get<ConversationState>();
		ConversationData.AbstractSpeechNodeData speechNodeData = conversation.GetSpeechNodeData(speechNodeIndex);
		val.speechNodeIndex = speechNodeIndex;
		val.responseStates = Pool.Get<List<bool>>();
		if (speechNodeData != null)
		{
			for (int i = 0; i < speechNodeData.responses.Length; i++)
			{
				val.responseStates.Add(speechNodeData.responses[i].PassesConditions(player, provider));
			}
		}
		if (speechNodeData != null)
		{
			val.formatTextValues = Pool.Get<List<FormatTextValue>>();
			provider.AddFormatTextValues(player, speechNodeData, val.formatTextValues);
		}
		if (speechNodeData is ConversationData.MissionListSpeechNodeData && provider is IMissionProvider missionProvider)
		{
			val.missionList = Pool.Get<MissionListState>();
			val.missionList.providerId = missionProvider.ProviderID();
			val.missionList.missionIds = Pool.Get<List<uint>>();
			Enumerator<BaseMission> enumerator = missionProvider.GetAllMissions().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					BaseMission current = enumerator.Current;
					val.missionList.missionIds.Add(current.id);
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		provider.AddSpeechNodeState(player, speechNodeData, val);
		return val;
	}

	public static void AddFormatTextValue(List<FormatTextValue> values, string key, string value, bool valueIsPhraseToken = false)
	{
		FormatTextValue val = Pool.Get<FormatTextValue>();
		val.key = key;
		val.value = value;
		val.valueIsPhraseToken = valueIsPhraseToken;
		values.Add(val);
	}

	public void ForceSpeechNode(BasePlayer player, ConversationData conversation, int speechNodeIndex)
	{
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		ConversationState val = BuildState(player, conversation, speechNodeIndex);
		try
		{
			player.SendConversationSpeechNode(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool TryGetShownSpeechNode(BasePlayer player, int speechNodeIndex, out ConversationData conversation, out ConversationData.AbstractSpeechNodeData speechNode)
	{
		conversation = null;
		speechNode = null;
		if (!CanDriveConversation(player))
		{
			return false;
		}
		if (speechNodeIndex != player.ConversationSpeechNodeIndex)
		{
			return false;
		}
		conversation = GetActiveConversationFor(player);
		if ((Object)(object)conversation == (Object)null)
		{
			return false;
		}
		if (speechNodeIndex < 0 || speechNodeIndex >= conversation.speechNodes.Length)
		{
			return false;
		}
		speechNode = conversation.speechNodes[speechNodeIndex];
		return true;
	}

	public bool TryGetResponseResult(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, int responseIndex, out string resultingNodeGuid)
	{
		resultingNodeGuid = string.Empty;
		if (responseIndex >= speechNode.responses.Length)
		{
			return false;
		}
		ConversationData.ResponseNode responseNode = speechNode.responses[responseIndex];
		if (responseNode != null)
		{
			if (responseNode.conditions.Length != 0)
			{
				provider.UpdateFlags();
			}
			bool flag = responseNode.PassesConditions(player, provider);
			if (flag)
			{
				string actionString = responseNode.GetActionString();
				if (!string.IsNullOrEmpty(actionString))
				{
					provider.OnConversationAction(player, actionString);
				}
			}
			resultingNodeGuid = (flag ? responseNode.resultingSpeechNode : responseNode.GetFailedSpeechNode(player, provider));
		}
		return true;
	}

	public void ShowNextSpeechNode(BasePlayer player, ConversationData conversation, string resultingNodeGuid)
	{
		conversation.GetFirstSpeechNodeFrom(player, provider, resultingNodeGuid, out var nodeIndex);
		if (nodeIndex == -1)
		{
			provider.OnConversationEnded(player);
		}
		else
		{
			ForceSpeechNode(player, conversation, nodeIndex);
		}
	}

	public void HandleResponse(BasePlayer player, int speechNodeIndex, int responseIndex)
	{
		if (TryGetShownSpeechNode(player, speechNodeIndex, out var conversation, out var speechNode) && player.CanSeeConversationResponse(responseIndex) && TryGetResponseResult(player, speechNode, responseIndex, out var resultingNodeGuid))
		{
			ShowNextSpeechNode(player, conversation, resultingNodeGuid);
		}
	}

	public void EndConversation(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && conversingPlayers.Contains(player))
		{
			player.ClearActiveTalkingToNpc(provider);
			conversingPlayers.Remove(player);
			player.SendConversationEnd();
			if (phoneCalls.Remove(player, out var value) && (Object)(object)value.Phone != (Object)null)
			{
				value.Phone.OnNPCConversationEnded();
			}
		}
	}

	public void Cleanup()
	{
		for (int num = conversingPlayers.Count - 1; num >= 0; num--)
		{
			BasePlayer basePlayer = conversingPlayers[num];
			if ((Object)(object)basePlayer == (Object)null)
			{
				conversingPlayers.RemoveAt(num);
				phoneCalls.Remove(basePlayer);
			}
			else if (!basePlayer.IsAlive() || basePlayer.IsSleeping() || !basePlayer.IsConnected)
			{
				provider.OnConversationEnded(basePlayer);
			}
		}
	}

	public void EndAllConversations()
	{
		for (int num = conversingPlayers.Count - 1; num >= 0; num--)
		{
			provider.OnConversationEnded(conversingPlayers[num]);
		}
	}

	public int RegisterPhoneNumbers(ConversationData[] phoneConversations, int ownNumber)
	{
		DeregisterPhoneNumbers();
		bool flag = false;
		foreach (ConversationData conversationData in phoneConversations)
		{
			if ((Object)(object)conversationData == (Object)null)
			{
				continue;
			}
			if (!conversationData.HasPhoneNumber)
			{
				flag = true;
				continue;
			}
			string name = conversationData.phoneNumberName.token;
			if (phoneNumbers.Exists((NPCPhoneNumber x) => (Object)(object)x.Conversation != (Object)null && x.Conversation.phoneNumberName.token == name))
			{
				Debug.LogError((object)$"{provider.GetEntity()} has more than one phone conversation with the phone number name {name}, only the first gets a number", (Object)(object)conversationData);
				continue;
			}
			NPCPhoneNumber nPCPhoneNumber = new NPCPhoneNumber(provider, conversationData, phoneNumbersByName.GetValueOrDefault(name));
			TelephoneManager.RegisterNPCPhone(nPCPhoneNumber);
			phoneNumbersByName[name] = nPCPhoneNumber.Number;
			phoneNumbers.Add(nPCPhoneNumber);
		}
		if (!flag)
		{
			return ownNumber;
		}
		NPCPhoneNumber nPCPhoneNumber2 = new NPCPhoneNumber(provider, null, ownNumber);
		TelephoneManager.RegisterNPCPhone(nPCPhoneNumber2);
		phoneNumbers.Add(nPCPhoneNumber2);
		return nPCPhoneNumber2.Number;
	}

	public void DeregisterPhoneNumbers()
	{
		foreach (NPCPhoneNumber phoneNumber in phoneNumbers)
		{
			TelephoneManager.DeregisterNPCPhone(phoneNumber);
		}
		phoneNumbers.Clear();
	}

	public void SavePhoneNumbers(Entity entity)
	{
		if (phoneNumbersByName.Count == 0)
		{
			return;
		}
		entity.npcPhoneNumbers = Pool.Get<NPCPhoneNumbers>();
		entity.npcPhoneNumbers.phoneNumbers = Pool.Get<List<PhoneNumber>>();
		foreach (KeyValuePair<string, int> item in phoneNumbersByName)
		{
			PhoneNumber val = Pool.Get<PhoneNumber>();
			val.name = item.Key;
			val.number = item.Value;
			entity.npcPhoneNumbers.phoneNumbers.Add(val);
		}
	}

	public void LoadPhoneNumbers(Entity entity)
	{
		if (entity.npcPhoneNumbers?.phoneNumbers == null)
		{
			return;
		}
		foreach (PhoneNumber phoneNumber in entity.npcPhoneNumbers.phoneNumbers)
		{
			phoneNumbersByName[phoneNumber.name] = phoneNumber.number;
		}
	}
}
