using Rust.UI;
using UnityEngine;

public class LookAtBoatBuildingBlock : FacepunchBehaviour
{
	public Canvas Canvas;

	public CanvasGroup Group;

	public RustText TextBlockCount;

	public RustText TextDeployableCount;

	public RustText TextMass;

	public RustText TextPower;

	public RustText TextPowerToMass;

	public RustText TextHealth;

	public RustText TextHelp;

	public Transform BlockCountParent;

	public Transform DeployableCountParent;

	public Transform RequiredItemsParent;

	public Transform PrefabRequiredItemIcon;

	public Transform HelpParent;

	private static readonly Phrase missingPrefixPhrase;

	static LookAtBoatBuildingBlock()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		missingPrefixPhrase = new Phrase("boatbuilding.missing_prefix", "Missing: {0}");
	}
}
