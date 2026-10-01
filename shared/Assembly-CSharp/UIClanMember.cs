using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UIClanMember : BaseMonoBehaviour, IUIPlayerRefreshCallback
{
	public static Phrase OnlinePhrase = new Phrase("clan.member.online", "Online");

	public Image Highlight;

	public Color HighlightColor;

	public Color SelectedColor;

	public RawImage Avatar;

	public RustText Name;

	public RustText Rank;

	public RustText LastSeen;

	static UIClanMember()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
