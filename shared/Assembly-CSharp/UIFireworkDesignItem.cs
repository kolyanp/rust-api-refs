using Rust.UI;
using UnityEngine;

public class UIFireworkDesignItem : MonoBehaviour
{
	public static readonly Phrase EmptyPhrase;

	public static readonly Phrase UntitledPhrase;

	public RustText Title;

	public RustButton LoadButton;

	public RustButton SaveButton;

	public RustButton EraseButton;

	public UIFireworkDesigner Designer;

	public int Index;

	static UIFireworkDesignItem()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		EmptyPhrase = new Phrase("firework.pattern.design.empty", "Empty");
		UntitledPhrase = new Phrase("firework.pattern.design.untitled", "Untitled");
	}
}
