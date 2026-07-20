using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class GenerateSplat : ProceduralComponent
{
	[DllImport("RustNative", EntryPoint = "generate_splat")]
	public unsafe static extern void Native_GenerateSplat(byte* map, int res, Vector3 pos, Vector3 size, uint seed, float lootAngle, float biomeAngle, short* heightmap, int heightres, byte* biomemap, int biomeres, int* topologymap, int topologyres);

	public unsafe override void Process(uint seed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		byte* unsafePtr = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr<byte>(TerrainMeta.SplatMap.dst);
		int res = TerrainMeta.SplatMap.res;
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float lootAxisAngle = TerrainMeta.LootAxisAngle;
		float biomeAxisAngle = TerrainMeta.BiomeAxisAngle;
		short* unsafePtr2 = (short*)NativeArrayUnsafeUtility.GetUnsafePtr<short>(TerrainMeta.HeightMap.src);
		int res2 = TerrainMeta.HeightMap.res;
		byte* unsafePtr3 = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr<byte>(TerrainMeta.BiomeMap.src);
		int res3 = TerrainMeta.BiomeMap.res;
		int* unsafePtr4 = (int*)NativeArrayUnsafeUtility.GetUnsafePtr<int>(TerrainMeta.TopologyMap.src);
		int res4 = TerrainMeta.TopologyMap.res;
		Native_GenerateSplat(unsafePtr, res, position, size, seed, lootAxisAngle, biomeAxisAngle, unsafePtr2, res2, unsafePtr3, res3, unsafePtr4, res4);
	}
}
