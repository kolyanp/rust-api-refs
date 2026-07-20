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

	public string VanishPermission = "vanish.allow";

	public string VanishUnlockWhileVanishedPermission = "vanish.unlock";

	public string PermanentVanishPermission = "vanish.permanent";

	public string VanishCommand = "vanish";

	public bool ToggleNoclipOnVanish = true;

	public bool ToggleNoclipOnUnvanish;

	public string InvisibleText = "You are currently invisible.";

	public int InvisibleTextSize = 10;

	public string InvisibleTextColor = "#8bba49";

	[JsonProperty("InvisibleTextAnchor [Anchor]")]
	public TextAnchor InvisibleTextAnchor = (TextAnchor)7;

	public float[] InvisibleTextAnchorX = new float[2] { 0f, 1f };

	public float[] InvisibleTextAnchorY = new float[2] { 0f, 0.025f };

	public string InvisibleIconUrl = "";

	public string InvisibleIconColor = "1 1 1 0.3";

	public float[] InvisibleIconMinAnchor = new float[2] { 0.5f, 0f };

	public float[] InvisibleIconMaxAnchor = new float[2] { 0.5f, 0f };

	public float[] InvisibleIconMinOffset = new float[2] { -350f, 15f };

	public float[] InvisibleIconMaxOffset = new float[2] { -250f, 125f };

	public EffectConfig Effect = new EffectConfig();

	public bool BroadcastVanishSounds;

	public bool WhooshSoundOnVanish = true;

	public bool GutshotScreamOnUnvanish = true;

	public bool EnableLogs = true;

	public bool TeleportBackOnUnvanish;

	public bool CanDamageWhenVanished = true;

	[JsonProperty("[Anchor] Legend")]
	public string AnchorLegend => "(0=UpperLeft, 1=UpperCenter, 2=UpperRight, 3=MiddleLeft, 4=MiddleCenter, 5=MiddleRight, 6=LowerLeft, 7=LowerCenter, 8=LowerRight)";
}
