using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceMonumentsOffshore : ProceduralComponent
{
	private struct SpawnInfo
	{
		public Prefab prefab;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	public string ResourceFolder = string.Empty;

	public int TargetCount;

	public int MinDistanceFromTerrain = 100;

	public int MaxDistanceFromTerrain = 500;

	public int DistanceBetweenMonuments = 500;

	[FormerlySerializedAs("MinSize")]
	public int MinWorldSize;

	private const int Candidates = 10;

	private const int Attempts = 10000;

	public override void Process(uint seed)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		string[] array = (from folder in ResourceFolder.Split(',')
			select "assets/bundled/prefabs/autospawn/" + folder + "/").ToArray();
		if (World.Networked)
		{
			World.Spawn("Monument", array);
		}
		else
		{
			if (World.Size < MinWorldSize)
			{
				return;
			}
			TerrainHeightMap heightMap = TerrainMeta.HeightMap;
			List<Prefab<MonumentInfo>> list = new List<Prefab<MonumentInfo>>();
			string[] array2 = array;
			for (int num = 0; num < array2.Length; num++)
			{
				Prefab<MonumentInfo>[] array3 = Prefab.Load<MonumentInfo>(array2[num], (GameManager)null, (PrefabAttribute.Library)null, true, true);
				ArrayEx.Shuffle(array3, ref seed);
				list.AddRange(array3);
			}
			Prefab<MonumentInfo>[] array4 = list.ToArray();
			if (array4 == null || array4.Length == 0)
			{
				return;
			}
			ArrayEx.BubbleSort(array4);
			Vector3 position = TerrainMeta.Position;
			Vector3 size = TerrainMeta.Size;
			float num2 = position.x - (float)MaxDistanceFromTerrain;
			float num3 = position.x - (float)MinDistanceFromTerrain;
			float num4 = position.x + size.x + (float)MinDistanceFromTerrain;
			float num5 = position.x + size.x + (float)MaxDistanceFromTerrain;
			float num6 = position.z - (float)MaxDistanceFromTerrain;
			float num7 = position.z - (float)MinDistanceFromTerrain;
			float num8 = position.z + size.z + (float)MinDistanceFromTerrain;
			float num9 = position.z + size.z + (float)MaxDistanceFromTerrain;
			int num10 = 0;
			List<SpawnInfo> list2 = new List<SpawnInfo>();
			int num11 = 0;
			List<SpawnInfo> list3 = new List<SpawnInfo>();
			Vector3 pos = default(Vector3);
			for (int num12 = 0; num12 < 10; num12++)
			{
				num10 = 0;
				list2.Clear();
				Prefab<MonumentInfo>[] array5 = array4;
				foreach (Prefab<MonumentInfo> prefab in array5)
				{
					int num13 = (int)((!Object.op_Implicit((Object)(object)prefab.Parameters)) ? PrefabPriority.Low : (prefab.Parameters.Priority + 1));
					int num14 = num13 * num13 * num13 * num13;
					for (int num15 = 0; num15 < 10000; num15++)
					{
						float num16 = 0f;
						float num17 = 0f;
						switch (seed % 4)
						{
						case 0u:
							num16 = SeedRandom.Range(ref seed, num2, num3);
							num17 = SeedRandom.Range(ref seed, num6, num9);
							break;
						case 1u:
							num16 = SeedRandom.Range(ref seed, num4, num5);
							num17 = SeedRandom.Range(ref seed, num6, num9);
							break;
						case 2u:
							num16 = SeedRandom.Range(ref seed, num2, num5);
							num17 = SeedRandom.Range(ref seed, num6, num7);
							break;
						case 3u:
							num16 = SeedRandom.Range(ref seed, num2, num5);
							num17 = SeedRandom.Range(ref seed, num8, num9);
							break;
						}
						float normX = TerrainMeta.NormalizeX(num16);
						float normZ = TerrainMeta.NormalizeZ(num17);
						float height = heightMap.GetHeight(normX, normZ);
						((Vector3)(ref pos))._002Ector(num16, height, num17);
						Quaternion rot = prefab.Object.transform.localRotation;
						Vector3 scale = prefab.Object.transform.localScale;
						if (!CheckRadius(list2, pos, DistanceBetweenMonuments))
						{
							prefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
							if ((!Object.op_Implicit((Object)(object)prefab.Component) || prefab.Component.CheckPlacement(pos, rot, scale)) && prefab.ApplyEnvironmentVolumeChecks(pos, rot, scale) && !prefab.CheckEnvironmentVolumes(pos, rot, scale, EnvironmentType.Underground | EnvironmentType.TrainTunnels))
							{
								SpawnInfo item = new SpawnInfo
								{
									prefab = prefab,
									position = pos,
									rotation = rot,
									scale = scale
								};
								list2.Add(item);
								num10 += num14;
								break;
							}
						}
					}
					if (TargetCount > 0 && list2.Count >= TargetCount)
					{
						break;
					}
				}
				if (num10 > num11)
				{
					num11 = num10;
					GenericsUtil.Swap<List<SpawnInfo>>(ref list2, ref list3);
				}
			}
			foreach (SpawnInfo item2 in list3)
			{
				World.AddPrefab("Monument", item2.prefab, item2.position, item2.rotation, item2.scale);
			}
		}
	}

	public bool CheckRadius(List<SpawnInfo> spawns, Vector3 pos, float radius)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * radius;
		foreach (SpawnInfo spawn in spawns)
		{
			Vector3 val = spawn.position - pos;
			if (((Vector3)(ref val)).sqrMagnitude < num)
			{
				return true;
			}
		}
		return false;
	}
}
