using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/ConversationManifest")]
public class ConversationManifest : ScriptableObject
{
	public const string ResourceName = "ConversationManifest";

	public string[] conversationPaths = Array.Empty<string>();

	private static ConversationManifest instance;

	private static readonly Dictionary<int, ConversationData> loadedConversations = new Dictionary<int, ConversationData>();

	private static Dictionary<ConversationData, int> conversationIndices;

	public static ConversationManifest Get()
	{
		using (TimeWarning.New("ConversationManifest.Get"))
		{
			if ((Object)(object)instance != (Object)null)
			{
				return instance;
			}
			instance = Resources.Load<ConversationManifest>("ConversationManifest");
			if ((Object)(object)instance == (Object)null)
			{
				Debug.LogError((object)"Failed to load ConversationManifest");
			}
			return instance;
		}
	}

	public static ConversationData GetConversation(int index)
	{
		if (index < 0)
		{
			return null;
		}
		if (loadedConversations.TryGetValue(index, out var value))
		{
			return value;
		}
		ConversationManifest conversationManifest = Get();
		if ((Object)(object)conversationManifest == (Object)null || index >= conversationManifest.conversationPaths.Length)
		{
			return null;
		}
		string text = conversationManifest.conversationPaths[index];
		ConversationData conversationData = FileSystem.Load<ConversationData>(text, true);
		if ((Object)(object)conversationData == (Object)null)
		{
			Debug.LogError((object)("Failed to load conversation at path " + text));
		}
		loadedConversations[index] = conversationData;
		return conversationData;
	}

	public static int GetConversationIndex(ConversationData conversation)
	{
		if ((Object)(object)conversation == (Object)null)
		{
			return -1;
		}
		ConversationManifest conversationManifest = Get();
		if ((Object)(object)conversationManifest == (Object)null)
		{
			return -1;
		}
		if (conversationIndices == null)
		{
			conversationIndices = new Dictionary<ConversationData, int>();
			for (int i = 0; i < conversationManifest.conversationPaths.Length; i++)
			{
				ConversationData conversation2 = GetConversation(i);
				if ((Object)(object)conversation2 != (Object)null)
				{
					conversationIndices[conversation2] = i;
				}
			}
		}
		if (!conversationIndices.TryGetValue(conversation, out var value))
		{
			return -1;
		}
		return value;
	}
}
