using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class LivestockAnimalStatusWidget : MonoBehaviour, IClientComponent
{
	[Tooltip("The animal's name, in the header.")]
	public RustText NameText;

	[Tooltip("The portrait beside the name, taken from the same PrefabInformation the death screen shows, so an animal looks the same wherever the game pictures it.")]
	public Image SpeciesIcon;

	[Tooltip("The sex symbol at the end of the header. Both are gated behind livestock.panelGender.")]
	public GameObject MaleIcon;

	public GameObject FemaleIcon;

	public GameObject PregnantIcon;

	[Header("Facts")]
	[Tooltip("Gated behind livestock.panelAge, along with its row.")]
	public InfoBar AgeBar;

	[Tooltip("Gated behind livestock.panelTrust, along with its row.")]
	public InfoBar TrustBar;

	[Tooltip("The box the rows above sit in. Hidden outright when a server has turned every one of them off, so the panel closes up rather than leaving an empty plate.")]
	public GameObject FactsSection;

	[Tooltip("What the animal produces and how often. Gated behind livestock.panelYield, and hidden anyway on a stage that produces nothing.")]
	public InfoBar YieldBar;

	[Tooltip("How often the animal dungs. Gated behind livestock.panelDung, and hidden anyway on a stage that does not dung.")]
	public InfoBar DungBar;

	[Header("Conditions")]
	public InfoBar FullnessBar;

	public InfoBar PersonalSpaceBar;

	public InfoBar HydrationBar;

	[Tooltip("Closes the conditions table, as the growable panel's OVERALL closes its own.")]
	public InfoBar OverallBar;

	[Tooltip("The row of gene discs. Gated behind livestock.panelGenes unless the animal's species always shows it.")]
	public LivestockGenesDisplay GenesDisplay;

	[Tooltip("Shown while the animal is standing in grass the herd has eaten bare, so the player knows fullness won't come back up until they move it or put food in a trough.")]
	public GameObject OvergrazedWarning;

	private static readonly string[] TrustTierTokens = new string[5] { "livestock.trust.stranger", "livestock.trust.known", "livestock.trust.tolerated", "livestock.trust.bonded", "livestock.trust.herd" };

	public static readonly int TrustBands = TrustTierTokens.Length - 1;
}
