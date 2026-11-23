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

	private static readonly Phrase lowPhrase = new Phrase("apartment.value.low", "Low");

	private static readonly Phrase mediumPhrase = new Phrase("apartment.value.medium", "Medium");

	private static readonly Phrase highPhrase = new Phrase("apartment.value.high", "High");

	private static readonly Phrase nullPhrase = new Phrase("apartment.value.null", "Null");

	private static readonly Phrase slotsPhrase = new Phrase("apartment.slots", "{0} Slots");

	private static readonly Phrase shopPhrase = new Phrase("apartment.shop", "Shop");

	public NetworkableId RoomId
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	static UI_ApartmentPlotRow()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected Obj, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected Obj, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected Obj, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected Obj, but got Unknown
	}
}
