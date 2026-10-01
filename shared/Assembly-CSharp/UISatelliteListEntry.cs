using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISatelliteListEntry : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Labels")]
	public TextMeshProUGUI textName;

	public TextMeshProUGUI textPayload;

	public TextMeshProUGUI textMass;

	public TextMeshProUGUI textFuel;

	public TextMeshProUGUI textSize;

	[Header("Interaction")]
	public Button button;

	[Header("Hover")]
	public Color panelColor = new Color(218f / 255f, 50f / 255f, 14f / 255f);

	public Color textColor = new Color(55f / 255f, 22f / 255f, 1f / 17f);

	public Image rowFill;

	public Image selectFrame;

	public void Init(SatelliteData sat, int index, SatelliteMenuUI menu)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	public UISatelliteListEntry()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
	}
}
