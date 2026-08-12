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

	public Phrase LocalPhrase;

	public Phrase CardsPhrase;

	public Phrase TeamPhrase;

	public TmProEmojiRedirector EmojiRedirector;

	public Phrase ClanPhrase;

	public ChatEntry()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		LocalPhrase = new Phrase("local", "local");
		CardsPhrase = new Phrase("cards", "cards");
		TeamPhrase = new Phrase("team", "team");
		ClanPhrase = new Phrase("clan", "clan");
		((MonoBehaviour)this)._002Ector();
	}
}
