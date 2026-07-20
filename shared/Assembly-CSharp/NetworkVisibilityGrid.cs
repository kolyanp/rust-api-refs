using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ConVar;
using Network;
using Network.Visibility;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class NetworkVisibilityGrid : MonoBehaviour, Provider
{
	private class Layer
	{
		private readonly NetworkVisibilityGrid _grid;

		private readonly float _gridSize;

		public readonly int LayerIndex;

		public readonly float CellSize;

		public readonly float HalfGridSize;

		public readonly float HalfCellSize;

		public readonly int CellCount;

		public readonly Group[] Groups;

		public Layer(NetworkVisibilityGrid grid, float gridSize, int layerIndex, float cellSize)
		{
			_grid = grid;
			_gridSize = gridSize;
			LayerIndex = layerIndex;
			CellSize = cellSize;
			HalfGridSize = _gridSize / 2f;
			HalfCellSize = CellSize / 2f;
			CellCount = (int)((_gridSize + CellSize - 0.5f) / CellSize);
			Groups = new Group[CellCount * CellCount];
		}

		public int PositionToGrid(float value)
		{
			return Mathf.Clamp((int)((value + HalfGridSize) / CellSize), 0, CellCount - 1);
		}

		public float GridToPosition(int value)
		{
			return (float)value * CellSize - HalfGridSize;
		}

		public void SetupGroup(Group group)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			var (value, value2, _) = _grid.DeconstructGroupId((int)group.ID);
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(GridToPosition(value) - HalfCellSize, 0f, GridToPosition(value2) - HalfCellSize);
			Vector3 max = default(Vector3);
			((Vector3)(ref max))._002Ector(val.x + CellSize, 0f, val.z + CellSize);
			if (LayerIndex >= 10)
			{
				group.restricted = true;
				int num = LayerIndex - 10;
				val.y = _grid.dynamicDungeonsThreshold + (float)num * _grid.dynamicDungeonsInterval + float.Epsilon;
				max.y = val.y + _grid.dynamicDungeonsInterval;
			}
			else if (LayerIndex == 5)
			{
				val.y = ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).min.y;
				max.y = _grid.dynamicDungeonsThreshold;
			}
			else if (LayerIndex >= 0 && LayerIndex <= 4)
			{
				val.y = -10000f;
				max.y = _grid.dynamicDungeonsThreshold - float.Epsilon;
			}
			else
			{
				Debug.LogError((object)$"Cannot get bounds for unknown layer {LayerIndex}!", (Object)(object)_grid);
			}
			Bounds bounds = default(Bounds);
			((Bounds)(ref bounds)).min = val;
			((Bounds)(ref bounds)).max = max;
			group.bounds = bounds;
		}
	}

	public const int overworldSmallLayer = 0;

	public const int overworldMediumLayer = 1;

	public const int overworldLargeLayer = 2;

	public const int cavesLayer = 3;

	public const int tunnelsLayer = 4;

	public const int deepSeaLayer = 5;

	public const int dynamicDungeonsFirstLayer = 10;

	public const int GlobalId = 0;

	public const int LimboId = 1;

	public const int MainIslandId = 2;

	public const int DeepSeaId = 3;

	public const int TutorialNetworkGroupStart = 100;

	public const int TutorialNetworkGroupEnd = 1000;

	public int startID = 1024;

	public int gridSize = 100;

	public int baseCellSize = 32;

	[FormerlySerializedAs("visibilityRadius")]
	public int visibilityRadiusFar = 2;

	public int visibilityRadiusNear = 1;

	public float switchTolerance = 20f;

	public float cavesThreshold = -0.5f;

	public float tunnelsThreshold = -20f;

	public float dynamicDungeonsThreshold = 1000f;

	public float dynamicDungeonsInterval = 100f;

	public int gizmoLayer;

	private Group[] _hardcodedGroups;

	private Layer[] _layers;

	private static List<ListHashSet<Vector2i>> tileOffsetsByRadius = new List<ListHashSet<Vector2i>>(64);

	public float SmallCellSize => (float)baseCellSize * 0.5f;

	public float DefaultCellSize => baseCellSize;

	public float LargeCellSize => (float)baseCellSize * 2f;

	public float DeepSeaCellSize => (float)baseCellSize * 2f;

	public float DynamicDungeonCellSize => dynamicDungeonsInterval;

	public void Awake()
	{
		Debug.Assert(Net.sv != null, "Network.Net.sv is NULL when creating Visibility Grid");
		Debug.Assert(Net.sv.visibility == null, "Network.Net.sv.visibility is being set multiple times");
		Net.sv.visibility = new Manager(this);
		_hardcodedGroups = new Group[startID];
		_layers = new Layer[15];
		_layers[0] = new Layer(this, gridSize, 0, SmallCellSize);
		_layers[1] = new Layer(this, gridSize, 1, DefaultCellSize);
		_layers[2] = new Layer(this, gridSize, 2, LargeCellSize);
		_layers[3] = new Layer(this, gridSize, 3, DefaultCellSize);
		_layers[4] = new Layer(this, gridSize, 4, DefaultCellSize);
		_layers[5] = new Layer(this, gridSize, 5, LargeCellSize);
		for (int i = 10; i < _layers.Length; i++)
		{
			_layers[i] = new Layer(this, gridSize, i, DynamicDungeonCellSize);
		}
		GetTileOffsets(visibilityRadiusNear);
		GetTileOffsets(visibilityRadiusFar);
		if (Net.visibilityRadiusNearOverride > -1)
		{
			GetTileOffsets(Net.visibilityRadiusNearOverride);
		}
		if (Net.visibilityRadiusFarOverride > -1)
		{
			GetTileOffsets(Net.visibilityRadiusFarOverride);
		}
		GetTileOffsets(Net.visibilityRadiusDeepSea);
	}

	private void OnDisable()
	{
		if (Application.isQuitting)
		{
			return;
		}
		if (Net.sv != null && Net.sv.visibility != null)
		{
			Net.sv.visibility.Dispose();
			Net.sv.visibility = null;
		}
		tileOffsetsByRadius = null;
		if (_layers != null)
		{
			Layer[] layers = _layers;
			for (int i = 0; i < layers.Length; i++)
			{
				Cleanup(layers[i].Groups);
			}
			_layers = null;
		}
		if (_hardcodedGroups != null)
		{
			Cleanup(_hardcodedGroups);
			_hardcodedGroups = null;
		}
		static void Cleanup(Group[] groups)
		{
			for (int j = 0; j < groups.Length; j++)
			{
				groups[j]?.Dispose();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (gizmoLayer >= 0 && gizmoLayer < _layers.Length && _layers[gizmoLayer] != null)
		{
			Gizmos.color = Color.blue;
			Layer layer = _layers[gizmoLayer];
			for (int i = 0; i <= layer.CellCount; i++)
			{
				float num = 0f - layer.HalfGridSize + (float)i * layer.CellSize - layer.HalfCellSize;
				Gizmos.DrawLine(new Vector3(layer.HalfGridSize, 0f, num), new Vector3(0f - layer.HalfGridSize, 0f, num));
				Gizmos.DrawLine(new Vector3(num, 0f, layer.HalfGridSize), new Vector3(num, 0f, 0f - layer.HalfGridSize));
			}
		}
	}

	public int PositionToLayer(float x, float y, float z, EntityNetworkRange range)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if (y >= dynamicDungeonsThreshold)
		{
			return Mathf.Min(10 + Mathf.FloorToInt((y - dynamicDungeonsThreshold) / dynamicDungeonsInterval), _layers.Length - 1);
		}
		if (DeepSeaManager.IsInsideDeepSea(new Vector3(x, 0f, z)))
		{
			return 5;
		}
		float normX = Mathf.Clamp01(TerrainMeta.NormalizeX(x));
		float normZ = Mathf.Clamp01(TerrainMeta.NormalizeZ(z));
		float num = y - TerrainMeta.HeightMap.GetHeight(normX, normZ);
		if (num < tunnelsThreshold)
		{
			if (!CaveNetworkGroupLayerOverride.Includes(new Vector3(x, y, z)))
			{
				return 4;
			}
			return 3;
		}
		if (num < cavesThreshold)
		{
			return 3;
		}
		return range switch
		{
			EntityNetworkRange.Small => 0, 
			EntityNetworkRange.Medium => 1, 
			EntityNetworkRange.Large => 2, 
			_ => 1, 
		};
	}

	private uint CoordToID(int x, int y, int layer)
	{
		Assert.IsTrue(layer >= 0 && layer < _layers.Length, "layer >= 0 && layer < _layers.Length");
		Assert.IsNotNull<Layer>(_layers[layer], "_layers[layer] != null");
		Assert.IsTrue(x >= 0 && x < _layers[layer].CellCount, "x >= 0 && x < _layers[layer].CellCount");
		Assert.IsTrue(y >= 0 && y < _layers[layer].CellCount, "y >= 0 && y < _layers[layer].CellCount");
		return CoordToIDUnchecked(x, y, layer);
	}

	private uint CoordToIDUnchecked(int x, int y, int layer)
	{
		Assert.IsTrue(layer >= 0 && layer <= 15, "layer >= 0 && layer <= 0xF");
		Assert.IsTrue(x >= 0 && x <= 16383, "x >= 0 && x <= 0x3FFF");
		Assert.IsTrue(y >= 0 && y <= 16383, "y >= 0 && y <= 0x3FFF");
		int num = ((layer & 0xF) << 28) | ((x & 0x3FFF) << 14) | (y & 0x3FFF);
		return (uint)(startID + num);
	}

	public (int x, int y, int layer) DeconstructGroupId(int groupId)
	{
		groupId -= startID;
		int item = (groupId >> 28) & 0xF;
		int item2 = (groupId >> 14) & 0x3FFF;
		int item3 = groupId & 0x3FFF;
		return (x: item2, y: item3, layer: item);
	}

	public bool IsGroupIdSpecial(uint groupId)
	{
		return groupId < startID;
	}

	public float GetFarDistanceForRange(EntityNetworkRange range)
	{
		int visibilityRadiusFarOverride = Net.visibilityRadiusFarOverride;
		int num = ((visibilityRadiusFarOverride > 0) ? visibilityRadiusFarOverride : visibilityRadiusFar);
		return range switch
		{
			EntityNetworkRange.Small => (float)num * SmallCellSize, 
			EntityNetworkRange.Medium => (float)num * DefaultCellSize, 
			EntityNetworkRange.Large => (float)num * LargeCellSize, 
			_ => 0f, 
		};
	}

	public void ForEach(int layerInd, Action<Group> callback)
	{
		Group[] groups = _layers[layerInd].Groups;
		foreach (Group obj in groups)
		{
			if (obj != null)
			{
				callback(obj);
			}
		}
	}

	public void AddGroups(int layerInd, ListHashSet<Group> groups, bool create)
	{
		Layer layer = _layers[layerInd];
		for (int i = 0; i < layer.Groups.Length; i++)
		{
			Group obj = layer.Groups[i];
			if (obj == null)
			{
				if (!create)
				{
					continue;
				}
				int x = i % layer.CellCount;
				int y = i / layer.CellCount;
				obj = GetOrCreateFromLayer(x, y, layer);
			}
			groups.TryAdd(obj);
		}
	}

	private uint GetID(Vector3 vPos, EntityNetworkRange range)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = PositionToLayer(vPos.x, vPos.y, vPos.z, range);
		Assert.IsNotNull<Layer>(_layers[num], "_layers[layerIdx] != null");
		Layer layer = _layers[num];
		int num2 = layer.PositionToGrid(vPos.x);
		int num3 = layer.PositionToGrid(vPos.z);
		if (TerrainMeta.IsPointWithinTutorialBounds(vPos))
		{
			Enumerator<TutorialIsland.IslandBounds> enumerator = TutorialIsland.BoundsListServer.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TutorialIsland.IslandBounds current = enumerator.Current;
					if (current.Contains(vPos))
					{
						return current.Id;
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
		uint num4 = CoordToID(num2, num3, num);
		if (num4 < startID)
		{
			Debug.LogError((object)string.Format("NetworkVisibilityGrid.GetID - group is below range {0} {1} {2} {3}", new object[4] { num2, num3, layer, num4 }));
		}
		return num4;
	}

	public bool IsInside(Group group, Vector3 vPos, EntityNetworkRange range)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false || group.ID == 0 || ((Bounds)(ref group.bounds)).Contains(vPos);
		int item = DeconstructGroupId((int)group.ID).layer;
		if (PositionToLayer(vPos.x, vPos.y, vPos.z, range) != item)
		{
			return false;
		}
		if (!group.restricted)
		{
			flag = flag || ((Bounds)(ref group.bounds)).SqrDistance(vPos) < switchTolerance;
		}
		return flag;
	}

	public bool IsVisibleFromFar(Group from, Group to)
	{
		int visibilityRadiusFarOverride = Net.visibilityRadiusFarOverride;
		int radius = ((visibilityRadiusFarOverride > 0) ? visibilityRadiusFarOverride : visibilityRadiusFar);
		return IsVisibleFrom(from, to, radius);
	}

	public bool IsVisibleFromNear(Group from, Group to)
	{
		int visibilityRadiusNearOverride = Net.visibilityRadiusNearOverride;
		int radius = ((visibilityRadiusNearOverride > 0) ? visibilityRadiusNearOverride : visibilityRadiusNear);
		return IsVisibleFrom(from, to, radius);
	}

	private bool IsVisibleFrom(Group from, Group to, int radius)
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if (to.isGlobal)
		{
			return true;
		}
		if (from.ID < startID)
		{
			if (from.restricted)
			{
				return from == to;
			}
			return false;
		}
		var (sourceX, sourceY, num) = DeconstructGroupId((int)from.ID);
		Assert.IsNotNull<Layer>(_layers[num], "_layers[fromLayer] != null");
		if (num == 5)
		{
			if (to.ID == 3)
			{
				return true;
			}
		}
		else if (to.ID == 2)
		{
			return true;
		}
		if (from.restricted)
		{
			return from == to;
		}
		if (to.ID < startID)
		{
			return false;
		}
		var (num2, num3, num4) = DeconstructGroupId((int)to.ID);
		Assert.IsNotNull<Layer>(_layers[num4], "_layers[toLayer] != null");
		Vector2i item = ConvertLayerCoords(_layers[num], sourceX, sourceY, num4).Position;
		Vector2i val = default(Vector2i);
		((Vector2i)(ref val))._002Ector(num2 - item.x, num3 - item.y);
		switch (num)
		{
		case 0:
		case 1:
		case 2:
			switch (num4)
			{
			case 0:
			case 1:
			case 2:
				return GetTileOffsets(radius).Contains(val);
			case 3:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 3:
			switch (num4)
			{
			case 3:
				return GetTileOffsets(radius).Contains(val);
			case 0:
			case 1:
			case 2:
			case 4:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 4:
			switch (num4)
			{
			case 4:
				return GetTileOffsets(radius).Contains(val);
			case 3:
				return GetTileOffsets(radius / 2).Contains(val);
			}
			break;
		case 5:
			if (num4 == 5)
			{
				return GetTileOffsets(radius).Contains(val);
			}
			break;
		}
		return false;
	}

	public unsafe Group GetGroup(Vector3 vPos, EntityNetworkRange range)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		uint iD = GetID(vPos, range);
		if (iD == 0)
		{
			return null;
		}
		Group obj = GetGroup(iD);
		if (Net.network_group_debug && !IsInside(obj, vPos, range))
		{
			float num = ((Bounds)(ref obj.bounds)).SqrDistance(vPos);
			Debug.Log((object)("Group is inside is all fucked " + iD + "/" + num + "/" + ((object)(*(Vector3*)(&vPos))/*cast due to constrained. prefix*/).ToString()));
		}
		return obj;
	}

	public Group GetGroup(uint groupId)
	{
		if (groupId < startID)
		{
			return GetOrCreateFromHardcoded(_hardcodedGroups, groupId);
		}
		var (x, y, layerInd) = DeconstructGroupId((int)groupId);
		return GetOrCreateFromLayer(x, y, layerInd);
	}

	private Group GetOrCreateFromHardcoded(Group[] groupLayer, uint groupId)
	{
		Group obj = groupLayer[groupId];
		if (obj != null)
		{
			return obj;
		}
		obj = new Group(Net.sv.visibility, groupId);
		return Interlocked.CompareExchange(ref groupLayer[groupId], obj, null) ?? obj;
	}

	private Group GetOrCreateFromLayer(int x, int y, int layerInd)
	{
		Layer layer = _layers[layerInd];
		return GetOrCreateFromLayer(x, y, layer);
	}

	private Group GetOrCreateFromLayer(int x, int y, Layer layer)
	{
		int num = y * layer.CellCount + x;
		Group obj = layer.Groups[num];
		if (obj != null)
		{
			return obj;
		}
		uint id = CoordToIDUnchecked(x, y, layer.LayerIndex);
		obj = new Group(Net.sv.visibility, id);
		layer.SetupGroup(obj);
		return Interlocked.CompareExchange(ref layer.Groups[num], obj, null) ?? obj;
	}

	public bool TryGetGroup(uint groupId, out Group group)
	{
		Group[] array;
		uint num;
		if (groupId < startID)
		{
			array = _hardcodedGroups;
			num = groupId;
		}
		else
		{
			(int x, int y, int layer) tuple = DeconstructGroupId((int)groupId);
			int item = tuple.x;
			int item2 = tuple.y;
			int item3 = tuple.layer;
			Layer layer = _layers[item3];
			array = layer.Groups;
			num = (uint)(item2 * layer.CellCount + item);
		}
		group = array[num];
		return group != null;
	}

	public void GetVisibleFromDistance(Group group, ListHashSet<Group> groups, float radiusInWorldUnits)
	{
		int radius = Mathf.FloorToInt(Mathf.Min(radiusInWorldUnits / (float)baseCellSize, 1f)) + 1;
		GetVisibleFrom(group, groups, radius);
	}

	public void GetVisibleFromFar(Group group, ListHashSet<Group> groups)
	{
		int num = Net.visibilityRadiusFarOverride;
		if (DeconstructGroupId((int)group.ID).layer == 5 && Net.visibilityRadiusDeepSea > num)
		{
			num = Net.visibilityRadiusDeepSea;
		}
		int radius = ((num > 0) ? num : visibilityRadiusFar);
		GetVisibleFrom(group, groups, radius);
	}

	public void GetVisibleFromNear(Group group, ListHashSet<Group> groups)
	{
		int visibilityRadiusNearOverride = Net.visibilityRadiusNearOverride;
		int radius = ((visibilityRadiusNearOverride > 0) ? visibilityRadiusNearOverride : visibilityRadiusNear);
		GetVisibleFrom(group, groups, radius);
	}

	private void GetGlobalNetworkGroups(Group group, ListHashSet<Group> groups)
	{
		groups.Add(GetGroup(0u));
		if (group.ID >= startID)
		{
			if (DeconstructGroupId((int)group.ID).layer == 5)
			{
				groups.Add(BaseNetworkable.DeepSeaGroup);
			}
			else
			{
				groups.Add(BaseNetworkable.MainIslandGroup);
			}
		}
	}

	public void GetVisibleFrom(Group group, ListHashSet<Group> groups, int radius)
	{
		if (Interface.CallHook("OnNetworkSubscriptionsGather", this, group, groups, radius) != null)
		{
			return;
		}
		ListHashSet<Group> groups2 = groups;
		GetGlobalNetworkGroups(group, groups2);
		if (group.restricted)
		{
			groups2.Add(group);
			return;
		}
		int iD = (int)group.ID;
		if (iD >= startID)
		{
			(int x, int y, int layer) tuple = DeconstructGroupId(iD);
			int item = tuple.x;
			int item2 = tuple.y;
			int item3 = tuple.layer;
			Layer layer = _layers[item3];
			Assert.IsNotNull<Layer>(layer, "layer != null");
			if (item3 == 0 || item3 == 1 || item3 == 2)
			{
				AddLayer(layer, item, item2, 0, radius);
				AddLayer(layer, item, item2, 1, radius);
				AddLayer(layer, item, item2, 2, radius);
				AddLayer(layer, item, item2, 3, radius / 2);
			}
			if (item3 == 3)
			{
				AddLayer(layer, item, item2, 3, radius);
				AddLayer(layer, item, item2, 0, radius / 2);
				AddLayer(layer, item, item2, 1, radius / 2);
				AddLayer(layer, item, item2, 2, radius / 2);
				AddLayer(layer, item, item2, 4, radius / 2);
			}
			if (item3 == 4)
			{
				AddLayer(layer, item, item2, 4, radius);
				AddLayer(layer, item, item2, 3, radius / 2);
			}
			if (item3 == 5)
			{
				AddLayer(layer, item, item2, 5, radius);
			}
			Assert.IsTrue(groups2.Count > 0, "groups.Count > 0");
		}
		void AddLayer(Layer sourceLayer, int sourceX, int sourceY, int targetLayerIdx, int targetLayerRadius)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			(Layer Layer, Vector2i Position) tuple2 = ConvertLayerCoords(sourceLayer, sourceX, sourceY, targetLayerIdx);
			Layer item4 = tuple2.Layer;
			Vector2i item5 = tuple2.Position;
			Enumerator<Vector2i> enumerator = GetTileOffsets(targetLayerRadius).Values.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Vector2i current = enumerator.Current;
					Vector2i val = item5 + current;
					if (val.x >= 0 && val.x < item4.CellCount && val.y >= 0 && val.y < item4.CellCount)
					{
						Group orCreateFromLayer = GetOrCreateFromLayer(val.x, val.y, item4);
						groups2.Add(orCreateFromLayer);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private (Layer Layer, Vector2i Position) ConvertLayerCoords(Layer sourceLayer, int sourceX, int sourceY, int destLayerIdx)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Layer layer;
		Vector2i item = default(Vector2i);
		if (destLayerIdx == sourceLayer.LayerIndex)
		{
			layer = sourceLayer;
			((Vector2i)(ref item))._002Ector(sourceX, sourceY);
		}
		else
		{
			layer = _layers[destLayerIdx];
			if (layer == null)
			{
				throw new InvalidOperationException($"Destination layer {destLayerIdx} is null");
			}
			if (Mathf.Approximately(layer.CellSize, sourceLayer.CellSize))
			{
				((Vector2i)(ref item))._002Ector(sourceX, sourceY);
			}
			else
			{
				Vector2 val = new Vector2((float)sourceX, (float)sourceY) * sourceLayer.CellSize + new Vector2(sourceLayer.HalfCellSize - sourceLayer.HalfGridSize, sourceLayer.HalfCellSize - sourceLayer.HalfGridSize);
				((Vector2i)(ref item))._002Ector(layer.PositionToGrid(val.x), layer.PositionToGrid(val.y));
			}
		}
		return (Layer: layer, Position: item);
	}

	private static ListHashSet<Vector2i> GetTileOffsets(int radius)
	{
		radius = Mathf.Clamp(radius, 0, 64);
		if (radius < tileOffsetsByRadius.Count)
		{
			return tileOffsetsByRadius[radius];
		}
		while (radius >= tileOffsetsByRadius.Count)
		{
			tileOffsetsByRadius.Add(GenerateTileOffsetsUncached(tileOffsetsByRadius.Count));
		}
		return tileOffsetsByRadius[radius];
	}

	private static ListHashSet<Vector2i> GenerateTileOffsetsUncached(int radius)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		ListHashSet<Vector2i> val = new ListHashSet<Vector2i>();
		if (radius <= 1)
		{
			val.Add(Vector2i.zero);
		}
		else
		{
			HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
			int num = radius;
			int num2 = 0;
			int num3 = 1 - (radius << 1);
			int num4 = 0;
			int num5 = 0;
			while (num >= num2)
			{
				for (int i = -num; i <= num; i++)
				{
					hashSet.Add(new Vector2i(i, num2));
					hashSet.Add(new Vector2i(i, -num2));
				}
				for (int j = -num2; j <= num2; j++)
				{
					hashSet.Add(new Vector2i(j, num));
					hashSet.Add(new Vector2i(j, -num));
				}
				num2++;
				num5 += num4;
				num4 += 2;
				if ((num5 << 1) + num3 > 0)
				{
					num--;
					num5 += num3;
					num3 += 2;
				}
			}
			foreach (Vector2i item in hashSet.OrderBy((Vector2i v) => v.x * v.x + v.y * v.y))
			{
				val.Add(item);
			}
		}
		return val;
	}
}
