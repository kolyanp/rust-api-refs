using UnityEngine;
using UnityEngine.UI;

public class ArmorInformationPanel : ItemInformationPanel
{
	public ItemTextValue projectileDisplay;

	public ItemTextValue meleeDisplay;

	public ItemTextValue coldDisplay;

	public ItemTextValue explosionDisplay;

	public ItemTextValue radiationDisplay;

	public ItemTextValue biteDisplay;

	public ItemTextValue speedDisplay;

	public ItemTextValue spacer;

	public Text areaProtectionText;

	public Phrase LegText;

	public Phrase ChestText;

	public Phrase HeadText;

	public Phrase ChestLegsText;

	public Phrase WholeBodyText;

	public ItemTextValue eggVision;

	public ItemIcon[] insertIcons;

	public GridLayoutGroup informationGridLayout;

	public RectOffset paddingOnResize;

	public Vector2 cellSizeOnResize;

	public Vector2 spacingOnResize;

	private RectOffset originalPadding;

	private Vector2 originalCellSize;

	private Vector2 originalSpacing;

	private ProtectionProperties protection;
}
