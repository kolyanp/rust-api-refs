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

	public NetworkableId RoomId { get; private set; }
}
