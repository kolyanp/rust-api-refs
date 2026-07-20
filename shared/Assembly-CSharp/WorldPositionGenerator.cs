using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facepunch;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/WorldPositionGenerator")]
public class WorldPositionGenerator : ScriptableObject
{
	private struct InputValuesIdentifierData(Vector3 origin, float minDist, float maxDist) : IEquatable<InputValuesIdentifierData>
	{
		public Vector3 origin = origin;

		public float minDist = minDist;

		public float maxDist = maxDist;

		public bool Equals(InputValuesIdentifierData other)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			if (origin == other.origin && Mathf.Approximately(minDist, other.minDist))
			{
				return Mathf.Approximately(maxDist, other.maxDist);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is InputValuesIdentifierData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return HashCode.Combine<Vector3, float, float>(origin, minDist, maxDist);
		}
	}

	private class PreprocessedData
	{
		public Rect[] preprocessedElementRects;
	}

	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public bool aboveWater;

	public float MaxSlopeRadius;

	public float MaxSlopeDegrees = 90f;

	public float CheckSphereRadius;

	public LayerMask CheckSphereMask;

	private Vector3 _origin;

	private Vector3 _area;

	private ByteQuadtree _quadtree = new ByteQuadtree();

	private Dictionary<InputValuesIdentifierData, PreprocessedData> _processedValuesCache = new Dictionary<InputValuesIdentifierData, PreprocessedData>();

	private bool isInitialized;

	private static WorldPositionGenerator precalculatePositionsInstance;

	private static int res;

	private static byte[] map;

	[ThreadStatic]
	private static float factor;

	private static Action<int, int> _actionSlopeCheck;

	private static bool isPrecalculating;

	private static Action<int, int> actionSlopeCheck => SlopeCheck;

	public bool TrySample(Vector3 origin, float minDist, float maxDist, float minDist_2x, float maxDist_2x, out Vector3 position)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		BufferList<Rect> blockedRects;
		Rect[] potentialElementRects;
		int elementsToCheckCount;
		int endIdx;
		using (TimeWarning.New("WorldPositionGenerator.TrySample"))
		{
			position = Vector3.zero;
			if (!isInitialized)
			{
				PrecalculatePositions(this);
			}
			Rect inclusion = new Rect(origin.x - maxDist, origin.z - maxDist, maxDist_2x, maxDist_2x);
			Rect exclusion = new Rect(origin.x - minDist, origin.z - minDist, minDist_2x, minDist_2x);
			blockedRects = Pool.Get<BufferList<Rect>>();
			Rect val2 = default(Rect);
			foreach (ListHashSet<Vector3> value2 in BaseMission.blockedPoints.Values)
			{
				for (int i = 0; i < value2.Count; i++)
				{
					Vector3 val = value2[i];
					((Rect)(ref val2))._002Ector(val.x - 10f, val.z - 10f, 20f, 20f);
					blockedRects.Add(val2);
				}
			}
			bool result = false;
			List<ByteQuadtree.Element> elementsBuffer;
			if (_processedValuesCache.TryGetValue(new InputValuesIdentifierData(origin, minDist, maxDist), out var value))
			{
				potentialElementRects = value.preprocessedElementRects;
			}
			else
			{
				elementsBuffer = Pool.Get<List<ByteQuadtree.Element>>();
				List<Rect> list = Pool.Get<List<Rect>>();
				elementsBuffer.Add(_quadtree.Root);
				for (int j = 0; j < elementsBuffer.Count; j++)
				{
					ByteQuadtree.Element element = elementsBuffer[j];
					if (element.IsLeaf)
					{
						Rect elementRect = GetElementRect(element);
						list.Add(elementRect);
						continue;
					}
					ListEx.RemoveUnordered<ByteQuadtree.Element>(elementsBuffer, j--);
					EvaluateElement(element.Child1);
					EvaluateElement(element.Child2);
					EvaluateElement(element.Child3);
					EvaluateElement(element.Child4);
				}
				InputValuesIdentifierData key = new InputValuesIdentifierData(origin, minDist, maxDist);
				_processedValuesCache.Add(key, new PreprocessedData
				{
					preprocessedElementRects = list.ToArray()
				});
				potentialElementRects = _processedValuesCache[key].preprocessedElementRects;
				Pool.FreeUnmanaged<ByteQuadtree.Element>(ref elementsBuffer);
				Pool.FreeUnmanaged<Rect>(ref list);
				if (_processedValuesCache.Count > 16)
				{
					Debug.LogWarning((object)(string.Format("{0} {1} added a new preprocessed values cache for input values origin: {2}, minDist: {3}, maxDist: {4} bringing total number of preprocessed value instances to {5}. ", new object[6]
					{
						"WorldPositionGenerator",
						((Object)this).name,
						origin,
						minDist,
						maxDist,
						_processedValuesCache.Count
					}) + "This means that either this server either has many potential origin points for this WorldPositionGenerator, or the origin points are moving."));
				}
			}
			elementsToCheckCount = potentialElementRects.Length;
			endIdx = elementsToCheckCount;
			while (elementsToCheckCount > 0)
			{
				int num = Random.Range(0, endIdx);
				Rect val3 = potentialElementRects[num];
				if (IsCandidateValid(val3, out var foundPosition))
				{
					result = true;
					position = foundPosition;
					break;
				}
				DiscardCandidate_End(val3, num);
			}
			Pool.FreeUnmanaged<Rect>(ref blockedRects);
			return result;
			void EvaluateElement(ByteQuadtree.Element child)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				if (child.Value != 0)
				{
					Rect elementRect2 = GetElementRect(child);
					if (((Rect)(ref elementRect2)).Overlaps(inclusion) && (!((Rect)(ref exclusion)).Contains(((Rect)(ref elementRect2)).min) || !((Rect)(ref exclusion)).Contains(((Rect)(ref elementRect2)).max)))
					{
						elementsBuffer.Add(child);
					}
				}
			}
		}
		void DiscardCandidate_End(Rect candidate, int candidateIdx)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			endIdx--;
			Rect val4 = potentialElementRects[endIdx];
			potentialElementRects[candidateIdx] = val4;
			potentialElementRects[endIdx] = candidate;
			elementsToCheckCount--;
		}
		bool IsCandidateValid(Rect rect, out Vector3 reference)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("WorldPositionGenerator.IsCandidateValid"))
			{
				reference = Vector3.zero;
				if (blockedRects.Count > 0)
				{
					for (int k = 0; k < blockedRects.Count; k++)
					{
						Rect val4 = blockedRects[k];
						if (((Rect)(ref val4)).Contains(((Rect)(ref rect)).min) && ((Rect)(ref val4)).Contains(((Rect)(ref rect)).max))
						{
							return false;
						}
					}
				}
				if (CheckSphereRadius <= float.Epsilon)
				{
					reference = Vector3Ex.XZ3D(((Rect)(ref rect)).min + ((Rect)(ref rect)).size * new Vector2(Random.value, Random.value));
				}
				else
				{
					Vector3 val5 = Vector3Ex.XZ3D(((Rect)(ref rect)).center);
					val5.y = TerrainMeta.HeightMap.GetHeight(val5);
					if (Physics.CheckSphere(val5, CheckSphereRadius, ((LayerMask)(ref CheckSphereMask)).value))
					{
						return false;
					}
					reference = val5;
				}
				reference = Vector3Ex.WithY(reference, aboveWater ? WaterLevel.GetWaterOrTerrainSurface(reference, waves: false, volumes: false) : TerrainMeta.HeightMap.GetHeight(reference));
				return BaseMission.PositionGenerator.TryAlignToGround(reference, out reference);
			}
		}
	}

	private Rect GetElementRect(ByteQuadtree.Element element)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		int num = 1 << element.Depth;
		float num2 = 1f / (float)num;
		Vector2 val = element.Coords * num2;
		return new Rect(_origin.x + val.x * _area.x, _origin.z + val.y * _area.z, _area.x * num2, _area.z * num2);
	}

	private static void SlopeCheck(int slopeX, int slopeZ)
	{
		if (TerrainMeta.HeightMap.GetSlope(slopeX, slopeZ) > precalculatePositionsInstance.MaxSlopeDegrees)
		{
			factor = 0f;
		}
	}

	public static void PrecalculatePositions(WorldPositionGenerator positionGenerator)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (positionGenerator.isInitialized)
		{
			return;
		}
		if (isPrecalculating)
		{
			Debug.LogWarning((object)("Attempted to precalculate positions for " + ((Object)positionGenerator).name + " while already precalculating " + ((Object)precalculatePositionsInstance).name));
			return;
		}
		precalculatePositionsInstance = positionGenerator;
		isPrecalculating = true;
		using (TimeWarning.New("WorldPositionGenerator.PrecalculatePositions"))
		{
			if (map == null)
			{
				res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
				map = new byte[res * res];
			}
			Parallel.For(0, res, delegate(int z)
			{
				for (int i = 0; i < res; i++)
				{
					float normX = ((float)i + 0.5f) / (float)res;
					float normZ = ((float)z + 0.5f) / (float)res;
					factor = precalculatePositionsInstance.Filter.GetFactor(normX, normZ);
					if (factor > 0f && precalculatePositionsInstance.MaxSlopeRadius > 0f)
					{
						TerrainMeta.HeightMap.ForEach(normX, normZ, precalculatePositionsInstance.MaxSlopeRadius / (float)res, actionSlopeCheck);
					}
					map[z * res + i] = (byte)((factor >= precalculatePositionsInstance.FilterCutoff) ? (255f * factor) : 0f);
				}
			});
			precalculatePositionsInstance._origin = TerrainMeta.Position;
			precalculatePositionsInstance._area = TerrainMeta.Size;
			byte[] baseValues = map.ToArray();
			precalculatePositionsInstance._quadtree.UpdateValues(baseValues);
			precalculatePositionsInstance.isInitialized = true;
			isPrecalculating = false;
		}
	}
}
