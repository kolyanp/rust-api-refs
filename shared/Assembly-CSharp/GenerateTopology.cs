using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateTopology : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_topology")]
	public unsafe static extern void Native_GenerateTopology(int* map, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float lootTier0, float lootTier1, float lootTier2, float biomeAngle, float biomeArid, float biomeTemperate, float biomeTundra, float biomeArctic, short* heightmap, int heightres, byte* biomemap, int biomeres);

	public unsafe override void Process(uint seed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		int* unsafePtr = (int*)NativeArrayUnsafeUtility.GetUnsafePtr<int>(TerrainMeta.TopologyMap.dst);
		int res = TerrainMeta.TopologyMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		short* unsafePtr2 = (short*)NativeArrayUnsafeUtility.GetUnsafePtr<short>(TerrainMeta.HeightMap.src);
		int res2 = TerrainMeta.HeightMap.res;
		byte* unsafePtr3 = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr<byte>(TerrainMeta.BiomeMap.src);
		int res3 = TerrainMeta.BiomeMap.res;
		Native_GenerateTopology(unsafePtr, res, position, size, seed, lootAxisAngle, World.Config.PercentageTier0, World.Config.PercentageTier1, World.Config.PercentageTier2, biomeAxisAngle, World.Config.PercentageBiomeArid, World.Config.PercentageBiomeTemperate, World.Config.PercentageBiomeTundra, World.Config.PercentageBiomeArctic, unsafePtr2, res2, unsafePtr3, res3);
	}
}
