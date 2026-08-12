using UnityEngine;

public class ChippyArcadeGame : BaseArcadeGame
{
	public ChippyMainCharacter mainChar;

	public SpriteArcadeEntity mainCharAim;

	public ChippyBoss currentBoss;

	public ChippyBoss[] bossPrefabs;

	public SpriteArcadeEntity mainMenuLogo;

	public Transform respawnPoint;

	public Vector2 mouseAim;

	public TextArcadeEntity levelIndicator;

	public TextArcadeEntity gameOverIndicator;

	public TextArcadeEntity playGameButton;

	public TextArcadeEntity highScoresButton;

	public bool OnMainMenu;

	public bool GameActive;

	public int level;

	public TextArcadeEntity[] scoreDisplays;

	public MenuButtonArcadeEntity[] mainMenuButtons;

	public int selectedButtonIndex;

	public bool OnHighScores;

	public ChippyArcadeGame()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		mouseAim = new Vector2(0f, 1f);
		base._002Ector();
	}
}
