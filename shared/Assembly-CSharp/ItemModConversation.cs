using UnityEngine;

public class ItemModConversation : ItemMod
{
	public static readonly Phrase SquakTitle;

	public static readonly Phrase SquakDesc;

	public ConversationData conversationData;

	public GameObjectRef conversationEntity;

	public GameObjectRef squakEffect;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (command == "squak")
		{
			if (squakEffect.isValid)
			{
				Effect.server.Run(squakEffect.resourcePath, player.eyes.position);
			}
			NPCMissionProvider obj = GameManager.server.CreateEntity(conversationEntity.resourcePath, ((Component)player).transform.position + Vector3.up * -2f) as NPCMissionProvider;
			obj.conversations[0] = conversationData;
			obj.Spawn();
			obj.DelayedKill(600f);
		}
	}

	static ItemModConversation()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		SquakTitle = new Phrase("squak", "MISSING SQUAK PHRASE");
		SquakDesc = new Phrase("squak_desc", "MISSING SQUAK DESC PHRASE");
	}
}
