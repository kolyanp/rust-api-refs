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

	public List<Color> positionColours = new List<Color>();

	public void SetLeaderboardRowStats(DartsGameLeaderboardEntry leaderboardEntry, int position)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		PlayerName.SetText(leaderboardEntry.playerName);
		DartsThrown.SetText(leaderboardEntry.dartsThrown.ToString());
		TimeTaken.SetText(TimeSpan.FromSeconds(leaderboardEntry.timeTaken).ToString("m\\:ss"));
		if (position < positionColours.Count)
		{
			Color color = positionColours[position];
			((Graphic)PlayerName).color = color;
			((Graphic)DartsThrown).color = color;
			((Graphic)TimeTaken).color = color;
		}
	}
}
