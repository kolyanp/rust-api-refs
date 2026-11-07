using UnityEngine;

public class LivestockLineageWidget : MonoBehaviour, IClientComponent
{
	public LivestockGeneWidget.AlleleDisc First;

	[Tooltip("The marker from the other side. A grey question mark unless livestock.panelAlleles is on, like a carried allele.")]
	public LivestockGeneWidget.AlleleDisc Second;

	[Tooltip("A box drawn round this column while the animal is inbred and the second marker is showing. Stretched to the column, so it surrounds both discs.")]
	public GameObject InbredHighlight;

	[Tooltip("How saturated a marker's colour is. Kept well clear of the gene discs' own strength so the two columns do not read as the same kind of thing.")]
	[Range(0f, 1f)]
	[Header("Colours")]
	public float MarkerSaturation = 0.5f;

	[Tooltip("How bright a marker's colour is.")]
	[Range(0f, 1f)]
	public float MarkerValue = 0.72f;

	public Color TextColour = new Color(0.898f, 0.886f, 0.874f, 1f);

	[Tooltip("The second marker and its link while livestock.panelAlleles is off.")]
	public Color UnknownColour = new Color(0.25f, 0.25f, 0.25f, 1f);

	public LivestockLineageWidget()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
	}
}
