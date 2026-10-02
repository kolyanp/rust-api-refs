namespace ConVar;

[Factory("workbench")]
public class Workbench : ConsoleSystem
{
	[Help("Skip server-side upgrade clearance zone checks")]
	[ServerVar]
	public static bool skipclearancechecks = false;

	[Help("Whether the range upgrade scales the comfort trigger radius")]
	[ServerVar]
	public static bool scalecomfortradius = true;

	[Help("Multiplier applied to the comfort trigger radius after range upgrade scaling")]
	[ServerVar]
	public static float comfortradiusscale = 0.95f;
}
