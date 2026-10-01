using System;
using Rust.Ai.Gen2;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class LivestockGeneWidget : MonoBehaviour, IClientComponent
{
	[Serializable]
	public class AlleleDisc
	{
		[Tooltip("Turned on and off as a whole, so the hidden copy can be hidden without leaving a gap.")]
		public GameObject Root;

		public Image Disc;

		public RustText LetterText;
	}

	[Tooltip("Which gene this widget shows. Set per widget on the prefab.")]
	public LivestockGene Gene;

	[Tooltip("The copy the animal expresses. Always shown.")]
	public AlleleDisc Expressed;

	[Tooltip("The copy hidden behind it. Only shown when the server has livestock.panelAlleles on.")]
	public AlleleDisc Hidden;

	[Header("Colours")]
	public Color BadColour = new Color(0.6745f, 0.2745f, 0.1843f, 1f);

	public Color OkColour = new Color(0.5f, 0.5f, 0.5f, 1f);

	public Color GoodColour = new Color(0.5412f, 0.6902f, 0.2745f, 1f);

	public Color LetterColour = new Color(0.898f, 0.886f, 0.874f, 1f);

	public static string LetterFor(LivestockGene gene)
	{
		return gene switch
		{
			LivestockGene.Dung => "D", 
			LivestockGene.Longevity => "L", 
			LivestockGene.Yield => "Y", 
			LivestockGene.Fertility => "F", 
			LivestockGene.Hardiness => "H", 
			_ => "?", 
		};
	}

	public LivestockGeneWidget()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
	}
}
