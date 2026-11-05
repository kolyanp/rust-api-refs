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
		public GameObject Root;

		public Image Disc;

		public RustText LetterText;

		[Tooltip("The vertical line behind this disc and the one above it, joining the two. Drawn in this disc's colour.")]
		public Image Link;
	}

	public const string UnknownText = "?";

	public LivestockGene Gene;

	public AlleleDisc Expressed;

	[Tooltip("The copy hidden behind it. A grey question mark unless the server has livestock.panelAlleles on.")]
	public AlleleDisc Hidden;

	[Header("Colours")]
	public Color BadColour = new Color(0.6745f, 0.2745f, 0.1843f, 1f);

	public Color OkColour = new Color(0.5f, 0.5f, 0.5f, 1f);

	public Color GoodColour = new Color(0.5412f, 0.6902f, 0.2745f, 1f);

	[Tooltip("The hidden copy and its link while livestock.panelAlleles is off.")]
	public Color UnknownColour = new Color(0.25f, 0.25f, 0.25f, 1f);

	public Color LetterColour = new Color(0.898f, 0.886f, 0.874f, 1f);

	public static string LetterFor(LivestockGene gene)
	{
		return gene switch
		{
			LivestockGene.Dung => "D", 
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
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
	}
}
