using UnityEngine;

public class LivestockLineageWidget : MonoBehaviour, IClientComponent
{
	[Tooltip("The marker from one side of the family. Always shown, like an expressed allele.")]
	public LivestockGeneWidget.AlleleDisc First;

	[Tooltip("The marker from the other side. Shown with livestock.panelAlleles, like a carried allele.")]
	public LivestockGeneWidget.AlleleDisc Second;

	[Tooltip("A box drawn round this column while the animal is inbred. Stretched to the column, so it surrounds one disc or two depending on whether the second marker is showing.")]
	public GameObject InbredHighlight;

	[Range(0f, 1f)]
	[Header("Colours")]
	[Tooltip("How saturated a marker's colour is. Kept well clear of the gene discs' own strength so the two columns do not read as the same kind of thing.")]
	public float MarkerSaturation = 0.5f;

	[Tooltip("How bright a marker's colour is.")]
	[Range(0f, 1f)]
	public float MarkerValue = 0.72f;

	public Color TextColour = new Color(0.898f, 0.886f, 0.874f, 1f);

	public LivestockLineageWidget()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
	}
}
