using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootPanelHorse : LootPanel
{
	public RustText breedTitle;

	public RustText healthText;

	public RustText maxSpeedText;

	public RustText staminaText;

	public GameObject[] storageSlots;

	public GameObject buffs;

	public Image horseAvatar;

	public RustText nameText;

	public TMP_InputField nameInput;

	public GameObject nameEditRoot;

	public RustButton nameEditButton;

	public static readonly Phrase unnamedHorse = new Phrase("unnamed.horse", "Unnamed Horse");

	static LootPanelHorse()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
