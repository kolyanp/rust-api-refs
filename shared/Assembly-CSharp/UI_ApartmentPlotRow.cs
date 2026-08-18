using System.Runtime.CompilerServices;
using Rust.UI;
using UnityEngine;

public class UI_ApartmentPlotRow : BaseMonoBehaviour
{
	[SerializeField]
	private RustText indexText;

	[SerializeField]
	private RustText nameText;

	[SerializeField]
	private RustText storageText;

	[SerializeField]
	private RustText roomsText;

	[SerializeField]
	private RustText valueText;

	[Space]
	[SerializeField]
	private GameObject[] occupiedVisuals;

	[SerializeField]
	private GameObject[] unoccupiedVisuals;

	[CompilerGenerated]
	private NetworkableId _003CRoomId_003Ek__BackingField;

	private static readonly Phrase lowPhrase;

	private static readonly Phrase mediumPhrase;

	private static readonly Phrase highPhrase;

	private static readonly Phrase nullPhrase;

	private static readonly Phrase slotsPhrase;

	private static readonly Phrase shopPhrase;

	public NetworkableId RoomId
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CRoomId_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CRoomId_003Ek__BackingField = value;
		}
	}

	static UI_ApartmentPlotRow()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		lowPhrase = new Phrase("apartment.value.low", "Low");
		mediumPhrase = new Phrase("apartment.value.medium", "Medium");
		highPhrase = new Phrase("apartment.value.high", "High");
		nullPhrase = new Phrase("apartment.value.null", "Null");
		slotsPhrase = new Phrase("apartment.slots", "{0} Slots");
		shopPhrase = new Phrase("apartment.shop", "Shop");
	}
}
