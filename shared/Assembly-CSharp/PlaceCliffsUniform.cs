using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlaceCliffsUniform : ProceduralComponent
{
	private struct Candidate
	{
		public float worldX;

		public float worldZ;

		public float normX;

		public float normZ;

		public float filtercut;
	}

	private class PrefabData : IComparable<PrefabData>
	{
		public readonly Prefab prefab;

		public readonly Transform transform;

		public readonly DecorComponent[] decor;

		public readonly TerrainFilter[] filters;

		public readonly TerrainAnchor[] anchors;

		public readonly TerrainCheck[] checks;

		public readonly TerrainModifier[] modifiers;

		public readonly TerrainFootprint footprint;

		public readonly bool hasPlacement;

		public Vector3 decorPosition;

		public Quaternion decorRotation;

		public Vector3 decorScale;

		public readonly bool[] orientationValid;

		public readonly Vector3[] orientationPositions;

		public PrefabData(Prefab prefab, int orientationCount)
		{
			this.prefab = prefab;
			transform = prefab.Object.transform;
			decor = prefab.Attribute.FindAll<DecorComponent>(prefab.ID);
			filters = prefab.Attribute.FindAll<TerrainFilter>(prefab.ID);
			anchors = prefab.Attribute.FindAll<TerrainAnchor>(prefab.ID);
			checks = prefab.Attribute.FindAll<TerrainCheck>(prefab.ID);
			modifiers = prefab.Attribute.FindAll<TerrainModifier>(prefab.ID);
			footprint = prefab.Attribute.Find<TerrainFootprint>(prefab.ID);
			hasPlacement = prefab.Attribute.FindAll<TerrainPlacement>(prefab.ID).Length != 0;
			orientationValid = new bool[orientationCount];
			orientationPositions = new Vector3[orientationCount];
		}

		public void ApplyDecorComponents(Vector3 basePos)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			decorPosition = basePos;
			decorRotation = transform.localRotation;
			decorScale = transform.localScale;
			transform.ApplyDecorComponents(decor, ref decorPosition, ref decorRotation, ref decorScale);
		}

		public float GetWriteReach(Vector3 scale)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			if (hasPlacement)
			{
				return float.PositiveInfinity;
			}
			float num = Mathf.Max(new float[3]
			{
				Mathf.Abs(scale.x),
				Mathf.Abs(scale.y),
				Mathf.Abs(scale.z)
			});
			float num2 = 0f;
			TerrainModifier[] array = modifiers;
			foreach (TerrainModifier terrainModifier in array)
			{
				num2 = Mathf.Max(num2, terrainModifier.worldPosition.magnitude * num + Mathf.Abs(scale.y) * terrainModifier.Radius);
			}
			if ((bool)footprint)
			{
				float num3 = (footprint.RimWidth + footprint.Feather) * Mathf.Max(1f, num);
				Vector3[] ring = footprint.Ring;
				for (int i = 0; i < ring.Length; i++)
				{
					Vector3 val = ring[i];
					num2 = Mathf.Max(num2, val.magnitude * num + num3);
				}
			}
			return num2 + 10f;
		}

		public int CompareTo(PrefabData other)
		{
			return prefab.CompareTo(other.prefab);
		}
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public float ObjectDistance = 10f;

	public float ObjectDithering = 5f;

	public TerrainAnchorMode AnchorMode;

	[InspectorFlags]
	public SpawnFilterMode FilterMode = (SpawnFilterMode)(-1);

	private static int min_rotation_z = 0;

	private static int max_rotation_z = 12;

	private static int rotation_delta_z = 4;

	private static int min_rotation_x = 0;

	private static int max_rotation_x = 12;

	private static int rotation_delta_x = 2;

	private const int BatchSize = 16384;

	private const int PivotChunkSize = 256;

	private const float ReachMargin = 10f;

	private const float DirtyCellSize = 16f;

	private Quaternion[] orientations;

	private List<Candidate> batch;

	private bool[] batchPasses;

	private PrefabData[] prefabData;

	private PrefabData[] sortedPrefabData;

	private WorldSpaceGrid<bool> dirtyCells;

	private SpawnFilter AnchorFilter
	{
		get
		{
			if ((FilterMode & SpawnFilterMode.TerrainAnchorPoints) == 0)
			{
				return null;
			}
			return Filter;
		}
	}

	private SpawnFilter CheckFilter
	{
		get
		{
			if ((FilterMode & SpawnFilterMode.TerrainCheckPoints) == 0)
			{
				return null;
			}
			return Filter;
		}
	}

	public override void Process(uint seed)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			World.Spawn("Decor", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
			return;
		}
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + ResourceFolder);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		if (orientations == null)
		{
			orientations = CreateOrientations();
		}
		prefabData = new PrefabData[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			prefabData[i] = new PrefabData(array[i], orientations.Length);
		}
		sortedPrefabData = (PrefabData[])prefabData.Clone();
		ArrayEx.BubbleSort(sortedPrefabData);
		batch = new List<Candidate>(16384);
		batchPasses = new bool[16384];
		dirtyCells = new WorldSpaceGrid<bool>(size.x, 16f, (RoundingMode)0);
		float x = position.x;
		float z = position.z;
		float num = position.x + size.x;
		float num2 = position.z + size.z;
		for (int num3 = 128; num3 > 0; num3 /= 2)
		{
			float num4 = ObjectDistance * (float)num3;
			for (float num5 = z; num5 < num2; num5 += num4)
			{
				for (float num6 = x; num6 < num; num6 += num4)
				{
					float num7 = num6 + SeedRandom.Range(ref seed, 0f - ObjectDithering, ObjectDithering);
					float num8 = num5 + SeedRandom.Range(ref seed, 0f - ObjectDithering, ObjectDithering);
					float normX = TerrainMeta.NormalizeX(num7);
					float normZ = TerrainMeta.NormalizeZ(num8);
					float filtercut = SeedRandom.Value(ref seed);
					batch.Add(new Candidate
					{
						worldX = num7,
						worldZ = num8,
						normX = normX,
						normZ = normZ,
						filtercut = filtercut
					});
					if (batch.Count == 16384)
					{
						ProcessBatch();
					}
				}
			}
		}
		ProcessBatch();
		batch = null;
		batchPasses = null;
		prefabData = null;
		sortedPrefabData = null;
		dirtyCells = null;
	}

	private void ProcessBatch()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		int count = batch.Count;
		int toExclusive = (count + 256 - 1) / 256;
		Parallel.For(0, toExclusive, (int chunk) =>
		{
			int num2 = Mathf.Min(count, (chunk + 1) * 256);
			for (int i = chunk * 256; i < num2; i++)
			{
				batchPasses[i] = PassesPivotFilter(batch[i]);
			}
		});
		for (int num = 0; num < count; num++)
		{
			Candidate candidate = batch[num];
			if (dirtyCells[new Vector3(candidate.worldX, 0f, candidate.worldZ)] ? PassesPivotFilter(candidate) : batchPasses[num])
			{
				TryPlaceCandidate(candidate);
			}
		}
		Array.Clear(dirtyCells.Cells, 0, dirtyCells.Cells.Length);
		batch.Clear();
	}

	private void MarkDirtyCells(Vector3 position, float radius)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		if (float.IsInfinity(radius))
		{
			Array.Fill(dirtyCells.Cells, value: true);
			return;
		}
		Vector2i val = dirtyCells.WorldToGridCoords(new Vector3(position.x - radius, 0f, position.z - radius));
		Vector2i val2 = dirtyCells.WorldToGridCoords(new Vector3(position.x + radius, 0f, position.z + radius));
		for (int i = val.y; i <= val2.y; i++)
		{
			for (int j = val.x; j <= val2.x; j++)
			{
				dirtyCells[j, i] = true;
			}
		}
	}

	private bool PassesPivotFilter(Candidate candidate)
	{
		if ((FilterMode & SpawnFilterMode.PivotPoint) == 0)
		{
			return true;
		}
		float factor = Filter.GetFactor(candidate.normX, candidate.normZ);
		return !(factor * factor < candidate.filtercut);
	}

	private void TryPlaceCandidate(Candidate candidate)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		float height = TerrainMeta.HeightMap.GetHeight(candidate.normX, candidate.normZ);
		Vector3 basePos = new Vector3(candidate.worldX, height, candidate.worldZ);
		EvaluateOrientations(basePos);
		PrefabData[] array = sortedPrefabData;
		foreach (PrefabData data in array)
		{
			if (SelectBest(data, out var bestPos, out var bestRot, out var bestScale) && TrySpawn(data, bestPos, bestRot, bestScale))
			{
				ArrayEx.BubbleSort(sortedPrefabData);
				break;
			}
		}
	}

	private void EvaluateOrientations(Vector3 basePos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		PrefabData[] array = prefabData;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].ApplyDecorComponents(basePos);
		}
		int orientationCount = orientations.Length;
		Parallel.For(0, prefabData.Length * orientationCount, (int num) =>
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			PrefabData prefabData = this.prefabData[num / orientationCount];
			int num2 = num % orientationCount;
			prefabData.orientationValid[num2] = IsValidOrientation(prefabData, orientations[num2], out prefabData.orientationPositions[num2]);
		});
	}

	private bool IsValidOrientation(PrefabData data, Quaternion orientation, out Vector3 pos)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		pos = data.decorPosition;
		Quaternion rot = data.decorRotation * orientation;
		Vector3 decorScale = data.decorScale;
		if (!data.transform.ApplyTerrainFilters(data.filters, pos, rot, decorScale))
		{
			return false;
		}
		if (!data.transform.ApplyTerrainAnchors(data.anchors, ref pos, rot, decorScale, AnchorMode, AnchorFilter))
		{
			return false;
		}
		return data.transform.ApplyTerrainChecks(data.checks, pos, rot, decorScale, CheckFilter);
	}

	private bool SelectBest(PrefabData data, out Vector3 bestPos, out Quaternion bestRot, out Vector3 bestScale)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Prefab prefab = data.prefab;
		bool flag = false;
		bestPos = Vector3.zero;
		bestRot = Quaternion.identity;
		bestScale = Vector3.zero;
		for (int i = 0; i < orientations.Length; i++)
		{
			if (!data.orientationValid[i])
			{
				continue;
			}
			Vector3 val = data.orientationPositions[i];
			if (!flag || !(val.y <= bestPos.y))
			{
				Quaternion val2 = data.decorRotation * orientations[i];
				Vector3 decorScale = data.decorScale;
				if (prefab.ApplyWaterChecks(val, val2, decorScale) && prefab.ApplyEnvironmentVolumeChecks(val, val2, decorScale))
				{
					flag = true;
					bestPos = val;
					bestRot = val2;
					bestScale = decorScale;
				}
			}
		}
		return flag;
	}

	private bool TrySpawn(PrefabData data, Vector3 bestPos, Quaternion bestRot, Vector3 bestScale)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Prefab prefab = data.prefab;
		Quaternion rot = Quaternion.Euler(bestRot.eulerAngles);
		if (!prefab.CheckTerrainFootprint(bestPos, rot, bestScale))
		{
			return false;
		}
		prefab.FillTerrainFootprint(bestPos, rot, bestScale);
		prefab.SpawnedCount++;
		World.AddPrefab("Decor", prefab, bestPos, bestRot, bestScale);
		MarkDirtyCells(bestPos, data.GetWriteReach(bestScale) + 10f);
		return true;
	}

	private static Quaternion[] CreateOrientations()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		List<Quaternion> list = new List<Quaternion>();
		for (int i = min_rotation_z; i <= max_rotation_z; i += rotation_delta_z)
		{
			for (int j = min_rotation_x; j <= max_rotation_x; j += rotation_delta_x)
			{
				for (int k = -1; k <= 1; k += 2)
				{
					if (i == 0 && k > 0)
					{
						continue;
					}
					for (int l = -1; l <= 1; l += 2)
					{
						if (j != 0 || l <= 0)
						{
							list.Add(Quaternion.Euler((float)(l * j), 0f, (float)(k * i)));
						}
					}
				}
			}
		}
		return list.ToArray();
	}
}
