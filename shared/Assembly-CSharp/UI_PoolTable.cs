using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_PoolTable : UIDialog, IShadowGroupVisibility
{
	private const int BallsPerPlayer = 7;

	private const int EightBallDisplayIndex = 7;

	[SerializeField]
	private RustSlider powerBar;

	[Header("Instructions")]
	[SerializeField]
	private GameObject holdInstruction;

	[SerializeField]
	private GameObject flickInstruction;

	[SerializeField]
	private GameObject cancelInstruction;

	[SerializeField]
	[Header("Players")]
	private RustText localPlayerNameText;

	[SerializeField]
	private RustText opponentNameText;

	[SerializeField]
	private RawImage localPlayerAvatar;

	[SerializeField]
	private RawImage opponentAvatar;

	[Header("Current Turn")]
	[SerializeField]
	private GameObject localTurnBall;

	[SerializeField]
	private GameObject localTurnHighlight;

	[SerializeField]
	private GameObject opponentTurnBall;

	[SerializeField]
	private GameObject opponentTurnHighlight;

	[SerializeField]
	[Header("Ball Display")]
	private Color localBallColour;

	[SerializeField]
	private Color opponentBallColour;

	[SerializeField]
	private Color eightBallColour;

	[Tooltip("Contains the 15 ball displays in left-to-right order.")]
	[SerializeField]
	private Transform ballDisplayRoot;

	public bool ShouldUpdateShadows => true;

	public UI_PoolTable()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		localBallColour = new Color(0.11f, 0.37f, 0.58f, 1f);
		opponentBallColour = new Color(0.8f, 0.25f, 0.17f, 1f);
		eightBallColour = new Color(0.13f, 0.13f, 0.13f, 1f);
		base._002Ector();
	}
}
