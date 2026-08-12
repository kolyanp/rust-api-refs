using System.IO;
using UnityEngine;

namespace ConVar;

[Factory("data")]
public class Data : ConsoleSystem
{
	[ServerVar(Help = "(Generated) Exports a named terrain map layer (splatmap, heightmap, biomemap, topologymap, alphamap, watermap) to a .raw file in the persistent data path")]
	[ClientVar(Help = "(Generated) Exports a named terrain map layer (splatmap, heightmap, biomemap, topologymap, alphamap, watermap) to a .raw file in the persistent data path")]
	public static void export(Arg args)
	{
		string text = args.GetString(0, "none");
		string text2 = Path.Combine(Application.persistentDataPath, text + ".raw");
		switch (text)
		{
		case "splatmap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.SplatMap))
			{
				RawWriter.Write(TerrainMeta.SplatMap.ToEnumerable(), text2);
			}
			break;
		case "heightmap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.HeightMap))
			{
				RawWriter.Write(TerrainMeta.HeightMap.ToEnumerable(), text2);
			}
			break;
		case "biomemap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.BiomeMap))
			{
				RawWriter.Write(TerrainMeta.BiomeMap.ToEnumerable(), text2);
			}
			break;
		case "topologymap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap))
			{
				RawWriter.Write(TerrainMeta.TopologyMap.ToEnumerable(), text2);
			}
			break;
		case "alphamap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.AlphaMap))
			{
				RawWriter.Write(TerrainMeta.AlphaMap.ToEnumerable(), text2);
			}
			break;
		case "watermap":
			if (Object.op_Implicit((Object)(object)TerrainMeta.WaterMap))
			{
				RawWriter.Write(TerrainMeta.WaterMap.ToEnumerable(), text2);
			}
			break;
		default:
			args.ReplyWith("Unknown export source: " + text);
			return;
		}
		args.ReplyWith("Export written to " + text2);
	}
}
