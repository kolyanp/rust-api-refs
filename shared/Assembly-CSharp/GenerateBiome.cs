using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateBiome : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_biome")]
	public unsafe static extern void Native_GenerateBiome(byte* nativeArrayPtr, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float lootTier0, float lootTier1, float lootTier2, float biomeAngle, float biomeArid, float biomeTemperate, float biomeTundra, float biomeArctic, float biomeJungle, short* heightmap, int heightres);

	public unsafe override void Process(uint seed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		byte* unsafePtr = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr<byte>(TerrainMeta.BiomeMap.dst);
		int res = TerrainMeta.BiomeMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		short* unsafePtr2 = (short*)NativeArrayUnsafeUtility.GetUnsafePtr<short>(TerrainMeta.HeightMap.src);
		int res2 = TerrainMeta.HeightMap.res;
		Native_GenerateBiome(unsafePtr, res, position, size, seed, lootAxisAngle, World.Config.PercentageTier0, World.Config.PercentageTier1, World.Config.PercentageTier2, biomeAxisAngle, World.Config.PercentageBiomeArid, World.Config.PercentageBiomeTemperate, World.Config.PercentageBiomeTundra, World.Config.PercentageBiomeArctic, World.Config.PercentageBiomeJungle, unsafePtr2, res2);
	}
}
