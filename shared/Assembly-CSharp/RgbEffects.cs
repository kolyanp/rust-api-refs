using System.ComponentModel;
using UnityEngine;

public class RgbEffects : SingletonComponent<RgbEffects>
{
	[ClientVar(Help = "Enables RGB lighting effects (supports SteelSeries and Razer)", Saved = true)]
	public static bool Enabled;

	[ClientVar(Help = "Controls how RGB values are mapped to LED lights on SteelSeries devices", Saved = true)]
	public static Vector3 ColorCorrection_SteelSeries;

	[ClientVar(Help = "Controls how RGB values are mapped to LED lights on Razer devices", Saved = true)]
	public static Vector3 ColorCorrection_Razer;

	[ClientVar(Help = "Brightness of colors, from 0 to 1 (note: may affect color accuracy)", Saved = true)]
	public static float Brightness;

	public Color defaultColor;

	public Color buildingPrivilegeColor;

	public Color coldColor;

	public Color hotColor;

	public Color hurtColor;

	public Color healedColor;

	public Color irradiatedColor;

	public Color comfortedColor;

	[ClientVar(Name = "static", Help = "(Generated) Sets all RGB lighting devices to a static colour; takes an RGBA colour argument and applies it to the RGB controller instance")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void ConVar_Static(ConsoleSystem.Arg args)
	{
	}

	[ClientVar(Name = "pulse", Help = "(Generated) Pulses all RGB lighting devices to a given colour over the specified duration in seconds; uses the RGB controller pulse animation")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void ConVar_Pulse(ConsoleSystem.Arg args)
	{
	}

	static RgbEffects()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Enabled = true;
		ColorCorrection_SteelSeries = new Vector3(1.5f, 1.5f, 1.5f);
		ColorCorrection_Razer = new Vector3(3f, 3f, 3f);
		Brightness = 1f;
	}
}
