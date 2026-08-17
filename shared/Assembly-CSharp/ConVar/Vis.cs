namespace ConVar;

[Factory("vis")]
public class Vis : ConsoleSystem
{
	[ClientVar]
	[Help("Turns on debug display of lerp")]
	public static bool lerp;

	[ServerVar]
	[Help("Turns on debug display of damages")]
	public static bool damage;

	[ClientVar]
	[Help("Turns on debug display of attacks")]
	[ServerVar]
	public static bool attack;

	[ClientVar]
	[Help("Turns on debug display of protection")]
	[ServerVar]
	public static bool protection;

	[ServerVar]
	[Help("Turns on debug display of weakspots")]
	public static bool weakspots;

	[ServerVar]
	[Help("Show trigger entries")]
	public static bool triggers;

	[Help("Turns on debug display of hitboxes")]
	[ServerVar]
	public static bool hitboxes;

	[Help("Turns on debug display of line of sight checks")]
	[ServerVar]
	public static bool lineofsight;

	[Help("Turns on debug display of senses, which are received by Ai")]
	[ServerVar]
	public static bool sense;
}
