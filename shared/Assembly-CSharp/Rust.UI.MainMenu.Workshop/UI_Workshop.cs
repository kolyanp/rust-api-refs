using UnityEngine;

namespace Rust.UI.MainMenu.Workshop;

public class UI_Workshop : UI_Page
{
	public static UI_Workshop Instance;

	[SerializeField]
	private UI_WorkshopItemList itemList;

	[SerializeField]
	private RustButton initialTabButton;

	public static Phrase loading_workshop = (Phrase)(object)new TokenisedPhrase("loading.workshop", "Loading Workshop");

	public static Phrase loading_workshop_setup = (Phrase)(object)new TokenisedPhrase("loading.workshop.initializing", "Setting Up Scene");

	public static Phrase loading_workshop_skinnables = (Phrase)(object)new TokenisedPhrase("loading.workshop.skinnables", "Getting Skinnables");

	public static Phrase loading_workshop_item = (Phrase)(object)new TokenisedPhrase("loading.workshop.item", "Loading Item Data");

	private readonly Phrase createNewSkinPhrase = new Phrase("workshop.createskin.title", "Create skin");

	private readonly Phrase createNewSkinBodyPhrase = new Phrase("workshop.createskin.body", "Do you want to create a new skin? This will load the workshop scene.");

	private readonly Phrase yesPhrase = new Phrase("workshop.continue", "Continue");

	private readonly Phrase cancelPhrase = new Phrase("workshop.cancel", "Cancel");

	public UI_Workshop()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected Obj, but got Unknown
	}
}
