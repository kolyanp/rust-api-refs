using Newtonsoft.Json;
using UnityEngine;

namespace Carbon.Modules;

public class VanishConfig
{
	public class EffectConfig
	{
		public string Vanishing = "assets/prefabs/npc/patrol helicopter/effects/rocket_fire.prefab";

		public string Unvanishing = "assets/bundled/prefabs/fx/player/gutshot_scream.prefab";
	}

	public string VanishPermission;

	public string VanishUnlockWhileVanishedPermission;

	public string PermanentVanishPermission;

	public string VanishCommand;

	public bool ToggleNoclipOnVanish;

	public bool ToggleNoclipOnUnvanish;

	public string InvisibleText;

	public int InvisibleTextSize;

	public string InvisibleTextColor;

	[JsonProperty("InvisibleTextAnchor [Anchor]")]
	public TextAnchor InvisibleTextAnchor;

	public float[] InvisibleTextAnchorX;

	public float[] InvisibleTextAnchorY;

	public string InvisibleIconUrl;

	public string InvisibleIconColor;

	public float[] InvisibleIconMinAnchor;

	public float[] InvisibleIconMaxAnchor;

	public float[] InvisibleIconMinOffset;

	public float[] InvisibleIconMaxOffset;

	public EffectConfig Effect;

	public bool BroadcastVanishSounds;

	public bool WhooshSoundOnVanish;

	public bool GutshotScreamOnUnvanish;

	public bool EnableLogs;

	public bool TeleportBackOnUnvanish;

	public bool CanDamageWhenVanished;

	[JsonProperty("[Anchor] Legend")]
	public string AnchorLegend => "(0=UpperLeft, 1=UpperCenter, 2=UpperRight, 3=MiddleLeft, 4=MiddleCenter, 5=MiddleRight, 6=LowerLeft, 7=LowerCenter, 8=LowerRight)";

	public VanishConfig()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		VanishPermission = "vanish.allow";
		VanishUnlockWhileVanishedPermission = "vanish.unlock";
		PermanentVanishPermission = "vanish.permanent";
		VanishCommand = "vanish";
		ToggleNoclipOnVanish = true;
		InvisibleText = "You are currently invisible.";
		InvisibleTextSize = 10;
		InvisibleTextColor = "#8bba49";
		InvisibleTextAnchor = (TextAnchor)7;
		InvisibleTextAnchorX = new float[2] { 0f, 1f };
		InvisibleTextAnchorY = new float[2] { 0f, 0.025f };
		InvisibleIconUrl = "";
		InvisibleIconColor = "1 1 1 0.3";
		InvisibleIconMinAnchor = new float[2] { 0.5f, 0f };
		InvisibleIconMaxAnchor = new float[2] { 0.5f, 0f };
		InvisibleIconMinOffset = new float[2] { -350f, 15f };
		InvisibleIconMaxOffset = new float[2] { -250f, 125f };
		Effect = new EffectConfig();
		WhooshSoundOnVanish = true;
		GutshotScreamOnUnvanish = true;
		EnableLogs = true;
		CanDamageWhenVanished = true;
		base._002Ector();
	}
}
