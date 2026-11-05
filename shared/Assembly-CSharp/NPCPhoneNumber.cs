using UnityEngine;

public class NPCPhoneNumber
{
	public readonly IConversationProvider Provider;

	public readonly ConversationData Conversation;

	public int Number;

	public Phrase Name
	{
		get
		{
			if (!((Object)(object)Conversation != (Object)null))
			{
				return Provider.GetPhoneDirectoryName();
			}
			return Conversation.phoneNumberName;
		}
	}

	public NPCPhoneNumber(IConversationProvider provider, ConversationData conversation, int number)
	{
		Provider = provider;
		Conversation = conversation;
		Number = number;
	}

	public ConversationData GetConversationFor(BasePlayer player)
	{
		if ((Object)(object)Conversation == (Object)null)
		{
			return Provider.GetPhoneConversationFor(player);
		}
		if (!Conversation.IsAvailableByPhone(player, Provider))
		{
			return null;
		}
		return Conversation;
	}
}
