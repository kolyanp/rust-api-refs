using Rust.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DartsGameUIScoreRow : FacepunchBehaviour
{
	public RustText ScoreText;

	public Color baseColour;

	public Color crossedOutColour;

	public void SetText(int score)
	{
		SetText(score.ToString());
	}

	public void SetText(string text)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ScoreText.SetText(text);
		((Graphic)ScoreText).color = baseColour;
	}

	public void CrossOutText()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		ScoreText.SetText("<s>" + ((TMP_Text)ScoreText).text + "</s>");
		((Graphic)ScoreText).color = crossedOutColour;
	}
}
