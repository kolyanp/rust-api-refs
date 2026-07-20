using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rust.Rendering.IndirectInstancing;

internal struct TerrainRef
{
	public ReadOnly<short> data;

	public ReadOnly<byte> alpha;

	public Vector3 pos;

	public Vector3 size;

	public Vector3 one_over_size;

	public int res;

	public int alpha_res;

	public static Rust.Rendering.IndirectInstancing.TerrainRef FromCurrent()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsNotNull<TerrainHeightMap>(TerrainMeta.HeightMap, "Cannot create TerrainRef because there is no terrain!");
		return new Rust.Rendering.IndirectInstancing.TerrainRef
		{
			data = TerrainMeta.HeightMap.src.AsReadOnly(),
			alpha = TerrainMeta.AlphaMap.src.AsReadOnly(),
			pos = TerrainMeta.Position,
			size = TerrainMeta.Size,
			one_over_size = TerrainMeta.OneOverSize,
			res = TerrainMeta.HeightMap.res,
			alpha_res = TerrainMeta.AlphaMap.res
		};
	}
}
