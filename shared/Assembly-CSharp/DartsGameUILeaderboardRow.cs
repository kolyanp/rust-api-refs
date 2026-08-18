using System;
using System.Collections.Generic;
using ProtoBuf;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class DartsGameUILeaderboardRow : FacepunchBehaviour
{
	public RustText PlayerName;

	public RustText DartsThrown;

	public RustText TimeTaken;

	[Tooltip("The image tinted with the colours below - light, dark, or current winner.")]
	[Header("Row Styles")]
	public Image rowImage;

	public Color lightColour;

	public Color darkColour;

	public Color currentWinnerColour;

	[Header("Text Colours")]
	[Tooltip("Every text in here gets the normal colour, or the winner colour on the current winner row.")]
	public List<RustText> rowTexts;

	public Color normalTextColour;

	public Color winnerTextColour;

	[Tooltip("Turned on for the current winner row only, off on every other row.")]
	public GameObject currentWinnerIcon;

	public void SetLeaderboardRowStats(DartsGameLeaderboardEntry leaderboardEntry, int position)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		PlayerName.SetText(leaderboardEntry.playerName);
		DartsThrown.SetText(leaderboardEntry.dartsThrown.ToString());
		TimeTaken.SetText(TimeSpan.FromSeconds(leaderboardEntry.timeTaken).ToString("m\\:ss"));
		bool flag = position == 0;
		Color color = (flag ? currentWinnerColour : ((position % 2 == 1) ? lightColour : darkColour));
		if ((Object)(object)rowImage != (Object)null)
		{
			((Graphic)rowImage).color = color;
		}
		Color color2 = (flag ? winnerTextColour : normalTextColour);
		foreach (RustText rowText in rowTexts)
		{
			if ((Object)(object)rowText != (Object)null)
			{
				((Graphic)rowText).color = color2;
			}
		}
		if ((Object)(object)currentWinnerIcon != (Object)null)
		{
			currentWinnerIcon.SetActive(flag);
		}
	}

	public DartsGameUILeaderboardRow()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		lightColour = Color.white;
		darkColour = Color.grey;
		currentWinnerColour = Color.yellow;
		rowTexts = new List<RustText>();
		normalTextColour = Color.white;
		winnerTextColour = Color.black;
		base._002Ector();
	}
}
