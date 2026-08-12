using Rust.UI;
using UnityEngine;

public class EggHuntNote : MonoBehaviour, IClientComponent
{
	public Canvas canvas;

	public CanvasGroup mainGroup;

	public CanvasGroup timerGroup;

	public RustText timerText;

	public SeasonalEventType EventType;

	public static readonly Phrase startsInPhrase;

	static EggHuntNote()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		startsInPhrase = new Phrase("egghunt.start", "Starts in: {0}");
	}
}
