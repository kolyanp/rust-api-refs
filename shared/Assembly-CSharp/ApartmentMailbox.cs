using System;
using Facepunch;
using ProtoBuf;
using TMPro;
using UnityEngine;

public class ApartmentMailbox : Mailbox, LootPanel.IHasLootPanel
{
	[Header("Apartment Mailbox")]
	public string RoomNumber;

	public TextMeshPro TextMesh;

	[NonSerialized]
	public ApartmentRoom Room;

	private static Phrase mailboxPanelTitlePhrase;

	Phrase LootPanel.IHasLootPanel.LootPanelTitle => Phrase.op_Implicit(string.Format(mailboxPanelTitlePhrase.translated, RoomNumber));

	public override bool PlayerIsOwner(BasePlayer player)
	{
		if ((Object)(object)Room != (Object)null)
		{
			return Room.IsAuthed(player.userID);
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.apartmentMailbox = Pool.Get<ApartmentMailbox>();
		info.msg.apartmentMailbox.roomNumber = RoomNumber;
		info.msg.apartmentMailbox.roomId = (NetworkableId)(((Object)(object)Room != (Object)null) ? Room.net.ID : default(NetworkableId));
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.apartmentMailbox != null)
		{
			RoomNumber = info.msg.apartmentMailbox.roomNumber;
		}
	}

	static ApartmentMailbox()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		mailboxPanelTitlePhrase = new Phrase("apartment.mailbox.lootpanel.title", "Mailbox of room {0}");
	}
}
