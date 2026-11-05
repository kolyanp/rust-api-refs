using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LivestockAnimalStatusWidget : MonoBehaviour, IClientComponent
{
	public RustText NameText;

	public GameObject SpecialStar;

	public Color SpecialNameColor = new Color(0.937f, 0.765f, 0.341f);

	public TMP_InputField NameInput;

	[Tooltip("Takes Escape while renaming, so it cancels the rename rather than opening the pause menu.")]
	public UIEscapeCapture NameEscape;

	[Tooltip("The portrait beside the name, taken from the same PrefabInformation the death screen shows, so an animal looks the same wherever the game pictures it.")]
	public Image SpeciesIcon;

	[Tooltip("The sex symbol at the end of the header. Both are gated behind livestock.panelGender.")]
	public GameObject MaleIcon;

	public GameObject FemaleIcon;

	public GameObject OwnerRow;

	public ItemOwnershipPanel OwnerShare;

	[Tooltip("The owner row's background on a wild animal.")]
	public Color WildOwnerColor = new Color(80f / 255f, 77f / 255f, 72f / 255f, 0.6f);

	public GameObject NoOwnerAlert;

	[Header("Facts")]
	[Tooltip("The age row, text with no bar. Gated behind livestock.panelAge, and hidden on an animal nobody owns, which does not age.")]
	public GameObject AgeRow;

	public RustText AgeText;

	public InfoBar PregnancyBar;

	[Tooltip("Gated behind livestock.panelTrust, along with its row.")]
	public InfoBar TrustBar;

	[Tooltip("How content the animal was raised, which its sale price is multiplied by. Gated behind livestock.panelPotential, along with its row.")]
	public InfoBar PotentialBar;

	[Tooltip("Shown while the animal's potential is rising, which is while it is young and happier than its potential.")]
	public GameObject PotentialIncreasing;

	[Tooltip("Shown while the animal's potential is falling, which is while it is young and less happy than its potential.")]
	public GameObject PotentialDecreasing;

	[Tooltip("Shown once the animal is grown up, when its potential can no longer change.")]
	public GameObject PotentialLocked;

	[Tooltip("The band edges on the trust row, moved to the server's trust thresholds. Spare ones are hidden.")]
	public RectTransform[] TrustPips;

	[Tooltip("The box the rows above sit in. Hidden outright when a server has turned every one of them off, so the panel closes up rather than leaving an empty plate.")]
	public GameObject FactsSection;

	[Tooltip("When the animal's milk or wool is next ready. Hidden on a bull, on the young and on an animal nobody owns. livestock.panelYield adds what it produces and how often.")]
	public InfoBar YieldBar;

	[Tooltip("How often the animal dungs. Gated behind livestock.panelDung, and hidden anyway on a stage that does not dung.")]
	public InfoBar DungBar;

	[Header("Conditions")]
	public InfoBar FullnessBar;

	public InfoBar HydrationBar;

	[Tooltip("The grass need as it is, which can read 0 while Overall holds at the shelter floor.")]
	public InfoBar GrassBar;

	[Tooltip("The sun need as it is, which can read 0 while Overall holds at the shelter floor.")]
	public InfoBar SunBar;

	public InfoBar PersonalSpaceBar;

	public InfoBar OverallBar;

	[Tooltip("The row of gene discs. Gated behind livestock.panelGenes unless the animal's species always shows it.")]
	public LivestockGenesDisplay GenesDisplay;

	private const int LocalOwnerColorIndex = 0;

	private const int OtherOwnerColorIndex = 1;

	public LivestockAnimalStatusWidget()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
	}
}
