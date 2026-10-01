using System;
using UnityEngine;

public class LivestockGenesDisplay : MonoBehaviour, IClientComponent
{
	[Tooltip("One per gene, in the order they should read. A gene with no widget here is simply not shown.")]
	public LivestockGeneWidget[] GeneWidgets = Array.Empty<LivestockGeneWidget>();

	[Tooltip("Turned off with the row, so the whole genetics section disappears rather than leaving a gap.")]
	public GameObject Root;

	[Tooltip("The two lineage markers, sat at the end of the row so the inbred warning has a cause to point at.")]
	public LivestockLineageWidget LineageWidget;

	[Tooltip("Shown when both of the animal's lineage markers match, which is what being inbred means.")]
	public GameObject InbredWarning;
}
