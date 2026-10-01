using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatEntry : MonoBehaviour
{
	public TextMeshProUGUI text;

	public RawImage avatar;

	public HttpImage httpAvatar;

	public CanvasGroup canvasGroup;

	public Phrase LocalPhrase = new Phrase("local", "local");

	public Phrase CardsPhrase = new Phrase("cards", "cards");

	public Phrase TeamPhrase = new Phrase("team", "team");

	public TmProEmojiRedirector EmojiRedirector;

	public Phrase ClanPhrase = new Phrase("clan", "clan");

	public ChatEntry()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected Obj, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected Obj, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected Obj, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected Obj, but got Unknown
	}
}
