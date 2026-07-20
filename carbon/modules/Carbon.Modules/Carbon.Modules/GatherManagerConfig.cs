using System.Collections.Generic;

namespace Carbon.Modules;

public class GatherManagerConfig
{
	public Dictionary<string, float> Quarry = new Dictionary<string, float> { ["*"] = 1f };

	public Dictionary<string, float> Excavator = new Dictionary<string, float> { ["*"] = 1f };

	public Dictionary<string, float> Pickup = new Dictionary<string, float>
	{
		["*"] = 1f,
		["seed.black.berry"] = 1f,
		["seed.blue.berry"] = 1f,
		["seed.corn"] = 1f,
		["seed.green.berry"] = 1f,
		["seed.hemp"] = 1f,
		["seed.potato"] = 1f,
		["seed.pumpkin"] = 1f,
		["seed.red.berry"] = 1f,
		["seed.white.berry"] = 1f,
		["seed.yellow.berry"] = 1f
	};

	public Dictionary<string, float> Gather = new Dictionary<string, float>
	{
		["*"] = 1f,
		["skull.wolf"] = 1f,
		["skull.human"] = 1f
	};
}
