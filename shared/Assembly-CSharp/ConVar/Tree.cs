namespace ConVar;

[Factory("tree")]
public class Tree : ConsoleSystem
{
	[Help("extra aim forgiveness radius in meters, applied when a melee swing narrowly misses a thin tree, 0 to disable")]
	[ReplicatedVar(Default = "0.12")]
	public static float hit_forgiveness = 0.12f;

	[Help("only trunks with a radius in meters at or below this are given melee aim forgiveness, sized to cover tiny and small trees only")]
	[ReplicatedVar(Default = "0.15")]
	public static float hit_forgiveness_max_trunk = 0.15f;

	[ServerVar(Help = "(Generated) When enabled, tree harvest events are broadcast to all connected clients, not just nearby players; useful for testing tree sync across the network")]
	public static bool global_broadcast = false;

	[ServerVar(Help = "(Generated) When enabled, trees use a simplified capsule collider instead of the full mesh collider, reducing physics CPU cost at the expense of collision accuracy")]
	public static bool simplified_collider = false;
}
