using Rust.UI;
using UnityEngine;

public class DartsGameDialogUI : UIDialog
{
	[Header("Prefabs")]
	public DartsGameUILeaderboardRow LeaderboardRowPrefab;

	[Header("Hierarchy References")]
	public RustSlider FocusSlider;

	public CanvasGroup focusCanvasGroup;

	public RustText TimerText;

	public Transform Leaderboard;

	public Transform LeaderboardPanel;

	public RustText LeaderboardTimeText;
}
