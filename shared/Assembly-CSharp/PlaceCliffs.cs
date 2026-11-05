using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PlaceCliffs : ProceduralComponent
{
	private struct ChildScan
	{
		public bool found;

		public Vector3 pos;

		public Quaternion rot;

		public Vector3 scale;

		public int angle;
	}

	private class CliffPlacement
	{
		public int count;

		public int score;

		public Prefab prefab;

		public Vector3 pos = Vector3.zero;

		public Quaternion rot = Quaternion.identity;

		public Vector3 scale = Vector3.one;

		public CliffPlacement next;

		public CliffPlacement()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public SpawnFilter Filter;

	public string ResourceFolder = string.Empty;

	public int RetryMultiplier = 1;

	[FormerlySerializedAs("CutoffSlope")]
	public int CutoffSlopeInitial = 10;

	public int CutoffSlopeRepeat = 10;

	[FormerlySerializedAs("MinHeight")]
	public int MinTerrainHeight;

	[FormerlySerializedAs("MaxHeight")]
	public int MaxTerrainHeight = 500;

	public int MinCliffHeight;

	public int MaxCliffHeight = 500;

	[FormerlySerializedAs("MinScale")]
	public float MinCliffScale = 1f;

	[FormerlySerializedAs("MaxScale")]
	public float MaxCliffScale = 2f;

	public int TargetCount = 8;

	public int TargetLength;

	public TerrainAnchorMode AnchorModeInitial = TerrainAnchorMode.MaximizeHeight;

	public TerrainAnchorMode AnchorModeRepeat = TerrainAnchorMode.MinimizeMovement;

	[InspectorFlags]
	public SpawnFilterMode FilterModeInitial = SpawnFilterMode.PivotPoint;

	[InspectorFlags]
	public SpawnFilterMode FilterModeRepeat = SpawnFilterMode.PivotPoint;

	private static float min_scale_delta = 0.1f;

	private static int max_scale_attempts = 10;

	private static int min_rotation = 0;

	private static int max_rotation = 60;

	private static int rotation_delta = 10;

	private static float offset_c = 0f;

	private static float offset_l = -0.75f;

	private static float offset_r = 0.75f;

	private static Vector3[] offsets = new Vector3[5]
	{
		new Vector3(offset_c, offset_c, offset_c),
		new Vector3(offset_l, offset_c, offset_c),
		new Vector3(offset_r, offset_c, offset_c),
		new Vector3(offset_c, offset_c, offset_l),
		new Vector3(offset_c, offset_c, offset_r)
	};

	private readonly Dictionary<(PrefabAttribute, Vector3, Quaternion), ChildScan> childScans = new Dictionary<(PrefabAttribute, Vector3, Quaternion), ChildScan>();

	public override void Process(uint seed)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		if (World.Networked)
		{
			World.Spawn("Decor", "assets/bundled/prefabs/autospawn/" + ResourceFolder + "/");
			return;
		}
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		Prefab[] array = Prefab.Load("assets/bundled/prefabs/autospawn/" + ResourceFolder);
		if (array == null || array.Length == 0)
		{
			return;
		}
		Prefab[] array2 = array.Where((Prefab prefab) => (bool)prefab.Attribute.Find<DecorSocketMale>(prefab.ID) && (bool)prefab.Attribute.Find<DecorSocketFemale>(prefab.ID)).ToArray();
		if (array2 == null || array2.Length == 0)
		{
			return;
		}
		Prefab[] array3 = array.Where((Prefab prefab) => prefab.Attribute.Find<DecorSocketMale>(prefab.ID)).ToArray();
		if (array3 == null || array3.Length == 0)
		{
			return;
		}
		Prefab[] array4 = array.Where((Prefab prefab) => prefab.Attribute.Find<DecorSocketFemale>(prefab.ID)).ToArray();
		if (array4 == null || array4.Length == 0)
		{
			return;
		}
		Vector3 position = TerrainMeta.Position;
		Vector3 size = TerrainMeta.Size;
		float x = position.x;
		float z = position.z;
		float num = position.x + size.x;
		float num2 = position.z + size.z;
		int num3 = Mathf.RoundToInt(size.x * size.z * 0.001f * (float)RetryMultiplier);
		for (int num4 = 0; num4 < num3; num4++)
		{
			float num5 = SeedRandom.Range(ref seed, x, num);
			float num6 = SeedRandom.Range(ref seed, z, num2);
			float normX = TerrainMeta.NormalizeX(num5);
			float normZ = TerrainMeta.NormalizeZ(num6);
			float num7 = SeedRandom.Value(ref seed);
			Prefab random = ArrayEx.GetRandom(array2, ref seed);
			PlaceCliffParameters placeCliffParameters = random.Attribute.Find<PlaceCliffParameters>(random.ID);
			int num8 = (placeCliffParameters ? placeCliffParameters.CutoffSlopeInitial : CutoffSlopeInitial);
			int num9 = (placeCliffParameters ? placeCliffParameters.MinTerrainHeight : MinTerrainHeight);
			int num10 = (placeCliffParameters ? placeCliffParameters.MaxTerrainHeight : MaxTerrainHeight);
			int num11 = (placeCliffParameters ? placeCliffParameters.MinCliffHeight : MinCliffHeight);
			int num12 = (placeCliffParameters ? placeCliffParameters.MaxCliffHeight : MaxCliffHeight);
			float num13 = (placeCliffParameters ? placeCliffParameters.MinCliffScale : MinCliffScale);
			float num14 = (placeCliffParameters ? placeCliffParameters.MaxCliffScale : MaxCliffScale);
			if ((FilterModeInitial & SpawnFilterMode.PivotPoint) != 0)
			{
				float factor = Filter.GetFactor(normX, normZ);
				if (factor * factor < num7)
				{
					continue;
				}
			}
			float height = heightMap.GetHeight(normX, normZ);
			if (height < (float)num9 || height > (float)num10)
			{
				continue;
			}
			Vector3 normal = heightMap.GetNormal(normX, normZ);
			if (Vector3.Angle(Vector3.up, normal) < (float)num8)
			{
				continue;
			}
			Vector3 val = new Vector3(num5, height, num6);
			Quaternion val2 = QuaternionEx.LookRotationForcedUp(normal, Vector3.up);
			float num15 = Mathf.Max((num14 - num13) / (float)max_scale_attempts, min_scale_delta);
			for (float num16 = num14; num16 >= num13; num16 -= num15)
			{
				Vector3 pos = val;
				Quaternion rot = val2 * random.Object.transform.localRotation;
				Vector3 scale = num16 * random.Object.transform.localScale;
				random.ApplyDecorComponents(ref pos, ref rot, ref scale);
				if (random.ApplyTerrainFilters(pos, rot, scale) && random.ApplyTerrainAnchors(ref pos, rot, scale, AnchorModeInitial, ((FilterModeInitial & SpawnFilterMode.TerrainAnchorPoints) != 0) ? Filter : null) && !(pos.y < (float)num11) && !(pos.y > (float)num12) && random.ApplyTerrainChecks(pos, rot, scale, ((FilterModeInitial & SpawnFilterMode.TerrainCheckPoints) != 0) ? Filter : null) && random.ApplyWaterChecks(pos, rot, scale) && random.ApplyEnvironmentVolumeChecks(pos, rot, scale) && random.CheckTerrainFootprint(pos, rot, scale))
				{
					childScans.Clear();
					CliffPlacement cliffPlacement = PlaceMale(array3, ref seed, random, pos, rot, scale);
					CliffPlacement cliffPlacement2 = PlaceFemale(array4, ref seed, random, pos, rot, scale);
					random.FillTerrainFootprint(pos, rot, scale);
					World.AddPrefab("Decor", random, pos, rot, scale);
					while (cliffPlacement != null && cliffPlacement.prefab != null)
					{
						cliffPlacement.prefab.FillTerrainFootprint(cliffPlacement.pos, cliffPlacement.rot, cliffPlacement.scale);
						World.AddPrefab("Decor", cliffPlacement.prefab, cliffPlacement.pos, cliffPlacement.rot, cliffPlacement.scale);
						cliffPlacement = cliffPlacement.next;
						num4++;
					}
					while (cliffPlacement2 != null && cliffPlacement2.prefab != null)
					{
						cliffPlacement2.prefab.FillTerrainFootprint(cliffPlacement2.pos, cliffPlacement2.rot, cliffPlacement2.scale);
						World.AddPrefab("Decor", cliffPlacement2.prefab, cliffPlacement2.pos, cliffPlacement2.rot, cliffPlacement2.scale);
						cliffPlacement2 = cliffPlacement2.next;
						num4++;
					}
					break;
				}
			}
		}
		childScans.Clear();
	}

	private CliffPlacement PlaceMale(Prefab[] prefabs, ref uint seed, Prefab parentPrefab, Vector3 parentPos, Quaternion parentRot, Vector3 parentScale)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Place<DecorSocketFemale, DecorSocketMale>(prefabs, ref seed, parentPrefab, parentPos, parentRot, parentScale);
	}

	private CliffPlacement PlaceFemale(Prefab[] prefabs, ref uint seed, Prefab parentPrefab, Vector3 parentPos, Quaternion parentRot, Vector3 parentScale)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Place<DecorSocketMale, DecorSocketFemale>(prefabs, ref seed, parentPrefab, parentPos, parentRot, parentScale);
	}

	private CliffPlacement Place<ParentSocketType, ChildSocketType>(Prefab[] prefabs, ref uint seed, Prefab parentPrefab, Vector3 parentPos, Quaternion parentRot, Vector3 parentScale, int parentAngle = 0, int parentCount = 0, int parentScore = 0) where ParentSocketType : PrefabAttribute where ChildSocketType : PrefabAttribute
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		CliffPlacement cliffPlacement = null;
		if (parentAngle > 160 || parentAngle < -160)
		{
			return cliffPlacement;
		}
		int num = SeedRandom.Range(ref seed, 0, prefabs.Length);
		ParentSocketType val = parentPrefab.Attribute.Find<ParentSocketType>(parentPrefab.ID);
		Vector3 connectionPos = parentPos + parentRot * Vector3.Scale(val.worldPosition, parentScale);
		for (int i = 0; i < prefabs.Length; i++)
		{
			Prefab prefab = prefabs[(num + i) % prefabs.Length];
			if (prefab == parentPrefab)
			{
				continue;
			}
			ParentSocketType val2 = prefab.Attribute.Find<ParentSocketType>(prefab.ID);
			ChildSocketType childConnection = prefab.Attribute.Find<ChildSocketType>(prefab.ID);
			bool flag = val2 != null;
			if (((cliffPlacement != null && cliffPlacement.count > TargetCount && cliffPlacement.score > TargetLength) & flag) || !TryScanChild(prefab, childConnection, connectionPos, parentRot, out var scan))
			{
				continue;
			}
			int parentAngle2 = parentAngle + scan.angle;
			int num2 = parentCount + 1;
			int num3 = parentScore + Mathf.CeilToInt(Vector3Ex.Distance2D(parentPos, scan.pos));
			CliffPlacement cliffPlacement2 = null;
			if (flag)
			{
				cliffPlacement2 = Place<ParentSocketType, ChildSocketType>(prefabs, ref seed, prefab, scan.pos, scan.rot, scan.scale, parentAngle2, num2, num3);
				if (cliffPlacement2 != null)
				{
					num2 = cliffPlacement2.count;
					num3 = cliffPlacement2.score;
				}
			}
			else
			{
				num3 *= 2;
			}
			if (cliffPlacement == null)
			{
				cliffPlacement = new CliffPlacement();
			}
			if (cliffPlacement.score < num3)
			{
				cliffPlacement.next = cliffPlacement2;
				cliffPlacement.count = num2;
				cliffPlacement.score = num3;
				cliffPlacement.prefab = prefab;
				cliffPlacement.pos = scan.pos;
				cliffPlacement.rot = scan.rot;
				cliffPlacement.scale = scan.scale;
			}
		}
		return cliffPlacement;
	}

	private bool TryScanChild(Prefab childPrefab, PrefabAttribute childConnection, Vector3 connectionPos, Quaternion parentRot, out ChildScan scan)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		(PrefabAttribute, Vector3, Quaternion) key = (childConnection, connectionPos, parentRot);
		if (!childScans.TryGetValue(key, out scan))
		{
			scan = ScanChild(childPrefab, childConnection, connectionPos, parentRot);
			childScans.Add(key, scan);
		}
		return scan.found;
	}

	private ChildScan ScanChild(Prefab childPrefab, PrefabAttribute childConnection, Vector3 connectionPos, Quaternion parentRot)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		TerrainHeightMap heightMap = TerrainMeta.HeightMap;
		PlaceCliffParameters placeCliffParameters = childPrefab.Attribute.Find<PlaceCliffParameters>(childPrefab.ID);
		int num = (placeCliffParameters ? placeCliffParameters.CutoffSlopeRepeat : CutoffSlopeRepeat);
		int num2 = (placeCliffParameters ? placeCliffParameters.MinTerrainHeight : MinTerrainHeight);
		int num3 = (placeCliffParameters ? placeCliffParameters.MaxTerrainHeight : MaxTerrainHeight);
		int num4 = (placeCliffParameters ? placeCliffParameters.MinCliffHeight : MinCliffHeight);
		int num5 = (placeCliffParameters ? placeCliffParameters.MaxCliffHeight : MaxCliffHeight);
		float num6 = (placeCliffParameters ? placeCliffParameters.MinCliffScale : MinCliffScale);
		float num7 = (placeCliffParameters ? placeCliffParameters.MaxCliffScale : MaxCliffScale);
		float num8 = Mathf.Max((num7 - num6) / (float)max_scale_attempts, min_scale_delta);
		for (float num9 = num7; num9 >= num6; num9 -= num8)
		{
			for (int i = min_rotation; i <= max_rotation; i += rotation_delta)
			{
				for (int j = -1; j <= 1; j += 2)
				{
					if (i == 0 && j > 0)
					{
						continue;
					}
					Vector3[] array = offsets;
					foreach (Vector3 val in array)
					{
						Vector3 scale = childPrefab.Object.transform.localScale * num9;
						Quaternion rot = Quaternion.Euler(0f, (float)(j * i), 0f) * parentRot;
						Vector3 pos = connectionPos - rot * (Vector3.Scale(childConnection.worldPosition, scale) + val);
						float normX = TerrainMeta.NormalizeX(pos.x);
						float normZ = TerrainMeta.NormalizeZ(pos.z);
						if ((FilterModeRepeat & SpawnFilterMode.PivotPoint) != 0)
						{
							float factor = Filter.GetFactor(normX, normZ);
							if (factor * factor < 0.5f)
							{
								continue;
							}
						}
						float height = heightMap.GetHeight(normX, normZ);
						if (height < (float)num2 || height > (float)num3)
						{
							continue;
						}
						Vector3 normal = heightMap.GetNormal(normX, normZ);
						if (!(Vector3.Angle(Vector3.up, normal) < (float)num))
						{
							childPrefab.ApplyDecorComponents(ref pos, ref rot, ref scale);
							if (childPrefab.ApplyTerrainAnchors(ref pos, rot, scale, AnchorModeRepeat, ((FilterModeRepeat & SpawnFilterMode.TerrainAnchorPoints) != 0) ? Filter : null) && !(pos.y < (float)num4) && !(pos.y > (float)num5) && childPrefab.ApplyTerrainChecks(pos, rot, scale, ((FilterModeRepeat & SpawnFilterMode.TerrainCheckPoints) != 0) ? Filter : null) && childPrefab.ApplyTerrainFilters(pos, rot, scale) && childPrefab.ApplyWaterChecks(pos, rot, scale) && childPrefab.ApplyEnvironmentVolumeChecks(pos, rot, scale))
							{
								return new ChildScan
								{
									found = true,
									pos = pos,
									rot = rot,
									scale = scale,
									angle = i
								};
							}
						}
					}
				}
			}
		}
		return default;
	}

	static PlaceCliffs()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
	}
}
