using System.Collections.Generic;
using ProtoBuf;

public interface IConversationProvider
{
	BaseEntity GetEntity();

	bool ProviderBusy();

	Phrase GetNPCName();

	Phrase GetPhoneDirectoryName();

	ConversationData GetInPersonConversationFor(BasePlayer player);

	ConversationData GetPhoneConversationFor(BasePlayer player);

	bool BeginPhoneConversation(BasePlayer player, PhoneController phone, ConversationData conversation);

	void HandleResponse(BasePlayer player, int speechNodeIndex, int responseIndex, uint previewingMissionId, uint selectedMissionId, NetworkableId selectedAnimalId);

	void OnConversationEnded(BasePlayer player);

	void AddFormatTextValues(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, List<FormatTextValue> values);

	void AddSpeechNodeState(BasePlayer player, ConversationData.AbstractSpeechNodeData speechNode, ConversationState state);

	void OnConversationAction(BasePlayer player, string action);

	void UpdateFlags();
}
