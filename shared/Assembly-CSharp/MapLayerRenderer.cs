using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MapLayerRenderer : SingletonComponent<MapLayerRenderer>
{
	private NetworkableId? _currentlyRenderedDungeon;

	private int? _underwaterLabFloorCount;

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations on the map for underwater lab entrance locations and floor boundaries; admin-only")]
	public static bool DebugLabs;

	public Camera renderCamera;

	public Material renderMaterial;

	private MapLayer? _currentlyRenderedLayer;

	private void RenderDungeonsLayer()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		ProceduralDynamicDungeon proceduralDynamicDungeon = FindDungeon(MainCamera.isValid ? MainCamera.position : Vector3.zero);
		if (_currentlyRenderedLayer == MapLayer.Dungeons)
		{
			NetworkableId? currentlyRenderedDungeon = _currentlyRenderedDungeon;
			NetworkableId? val = proceduralDynamicDungeon?.net?.ID;
			if (currentlyRenderedDungeon.HasValue == val.HasValue && (!currentlyRenderedDungeon.HasValue || currentlyRenderedDungeon.GetValueOrDefault() == val.GetValueOrDefault()))
			{
				return;
			}
		}
		_currentlyRenderedLayer = MapLayer.Dungeons;
		_currentlyRenderedDungeon = proceduralDynamicDungeon?.net?.ID;
		CommandBuffer val2 = BuildCommandBufferDungeons(proceduralDynamicDungeon);
		try
		{
			Graphics.ExecuteCommandBuffer(val2);
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private CommandBuffer BuildCommandBufferDungeons(ProceduralDynamicDungeon closest)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer val = CreateCommandBuffer("DungeonsLayer Render");
		if ((Object)(object)closest != (Object)null && closest.spawnedCells != null)
		{
			Matrix4x4 val2 = Matrix4x4.Translate(closest.mapOffset);
			foreach (ProceduralDungeonCell spawnedCell in closest.spawnedCells)
			{
				if ((Object)(object)spawnedCell == (Object)null || spawnedCell.mapRendererLods == null || spawnedCell.mapRendererLods.Length == 0)
				{
					continue;
				}
				RendererLOD[] mapRendererLods = spawnedCell.mapRendererLods;
				foreach (RendererLOD rendererLOD in mapRendererLods)
				{
					if ((Object)(object)rendererLOD == (Object)null)
					{
						continue;
					}
					Mesh finalLodMesh = rendererLOD.GetFinalLodMesh(out var localToWorldMatrix);
					if (!((Object)(object)finalLodMesh == (Object)null))
					{
						int subMeshCount = finalLodMesh.subMeshCount;
						Matrix4x4 val3 = val2 * localToWorldMatrix;
						for (int j = 0; j < subMeshCount; j++)
						{
							val.DrawMesh(finalLodMesh, val3, renderMaterial, j);
						}
					}
				}
			}
		}
		return val;
	}

	public static ProceduralDynamicDungeon FindDungeon(Vector3 position, float maxDist = 200f)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		ProceduralDynamicDungeon result = null;
		float num = 100000f;
		foreach (ProceduralDynamicDungeon dungeon in ProceduralDynamicDungeon.dungeons)
		{
			if (!((Object)(object)dungeon == (Object)null) && dungeon.isClient)
			{
				float num2 = Vector3.Distance(position, ((Component)dungeon).transform.position);
				if (!(num2 > maxDist) && !(num2 > num))
				{
					result = dungeon;
					num = num2;
				}
			}
		}
		return result;
	}

	private void RenderTrainLayer()
	{
		CommandBuffer val = BuildCommandBufferTrainTunnels();
		try
		{
			Graphics.ExecuteCommandBuffer(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private CommandBuffer BuildCommandBufferTrainTunnels()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer val = CreateCommandBuffer("TrainLayer Render");
		foreach (DungeonGridCell dungeonGridCell in TerrainMeta.Path.DungeonGridCells)
		{
			if (dungeonGridCell.MapRendererLods == null || dungeonGridCell.MapRendererLods.Length == 0)
			{
				continue;
			}
			RendererLOD[] mapRendererLods = dungeonGridCell.MapRendererLods;
			for (int i = 0; i < mapRendererLods.Length; i++)
			{
				Mesh finalLodMesh = mapRendererLods[i].GetFinalLodMesh(out var localToWorldMatrix);
				if (!((Object)(object)finalLodMesh == (Object)null))
				{
					int subMeshCount = finalLodMesh.subMeshCount;
					for (int j = 0; j < subMeshCount; j++)
					{
						val.DrawMesh(finalLodMesh, localToWorldMatrix, renderMaterial, j);
					}
				}
			}
		}
		return val;
	}

	private void RenderUnderwaterLabs(int floor)
	{
		CommandBuffer val = BuildCommandBufferUnderwaterLabs(floor);
		try
		{
			Graphics.ExecuteCommandBuffer(val);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public int GetUnderwaterLabFloorCount()
	{
		if (_underwaterLabFloorCount.HasValue)
		{
			return _underwaterLabFloorCount.Value;
		}
		List<DungeonBaseInfo> dungeonBaseEntrances = TerrainMeta.Path.DungeonBaseEntrances;
		_underwaterLabFloorCount = ((dungeonBaseEntrances != null && dungeonBaseEntrances.Count > 0) ? dungeonBaseEntrances.Max((DungeonBaseInfo l) => l.Floors.Count) : 0);
		if (DebugLabs && dungeonBaseEntrances != null)
		{
			Debug.Log((object)$"Setup underwater lab: count: {dungeonBaseEntrances.Count} floors: {_underwaterLabFloorCount.Value}");
		}
		return _underwaterLabFloorCount.Value;
	}

	private CommandBuffer BuildCommandBufferUnderwaterLabs(int floor)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer val = CreateCommandBuffer("UnderwaterLabLayer Render");
		foreach (DungeonBaseInfo dungeonBaseEntrance in TerrainMeta.Path.DungeonBaseEntrances)
		{
			if (dungeonBaseEntrance.Floors.Count <= floor)
			{
				continue;
			}
			foreach (DungeonBaseLink link in dungeonBaseEntrance.Floors[floor].Links)
			{
				if (link.MapRendererLods == null || link.MapRendererLods.Length == 0)
				{
					if (DebugLabs)
					{
						Debug.Log((object)$"{link} has no renderers");
					}
					continue;
				}
				RendererLOD[] mapRendererLods = link.MapRendererLods;
				foreach (RendererLOD rendererLOD in mapRendererLods)
				{
					if ((Object)(object)rendererLOD == (Object)null)
					{
						if (DebugLabs)
						{
							Debug.Log((object)$"{link} has a null renderer");
						}
						continue;
					}
					Mesh finalLodMesh = rendererLOD.GetFinalLodMesh(out var localToWorldMatrix);
					if (!((Object)(object)finalLodMesh == (Object)null))
					{
						int subMeshCount = finalLodMesh.subMeshCount;
						for (int j = 0; j < subMeshCount; j++)
						{
							val.DrawMesh(finalLodMesh, localToWorldMatrix, renderMaterial, j);
						}
					}
				}
			}
		}
		return val;
	}

	public void Render(MapLayer layer)
	{
		if (layer < MapLayer.TrainTunnels)
		{
			return;
		}
		if (layer == MapLayer.Dungeons)
		{
			RenderDungeonsLayer();
		}
		else if (layer != _currentlyRenderedLayer)
		{
			_currentlyRenderedLayer = layer;
			switch (layer)
			{
			case MapLayer.TrainTunnels:
				RenderTrainLayer();
				break;
			case MapLayer.Underwater1:
			case MapLayer.Underwater2:
			case MapLayer.Underwater3:
			case MapLayer.Underwater4:
			case MapLayer.Underwater5:
			case MapLayer.Underwater6:
			case MapLayer.Underwater7:
			case MapLayer.Underwater8:
				RenderUnderwaterLabs((int)(layer - 1));
				break;
			}
		}
	}

	private CommandBuffer CreateCommandBuffer(string name)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected Obj, but got Unknown
		double num = (double)World.Size * 1.5;
		renderCamera.orthographicSize = (float)num / 2f;
		CommandBuffer val = new CommandBuffer
		{
			name = name
		};
		val.SetRenderTarget(RenderTargetIdentifier.op_Implicit((Texture)(object)renderCamera.targetTexture));
		val.ClearRenderTarget(true, true, renderCamera.backgroundColor);
		val.SetViewProjectionMatrices(renderCamera.worldToCameraMatrix, renderCamera.projectionMatrix);
		val.SetViewport(renderCamera.pixelRect);
		return val;
	}

	public static MapLayerRenderer GetOrCreate()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)SingletonComponent<MapLayerRenderer>.Instance != (Object)null)
		{
			return SingletonComponent<MapLayerRenderer>.Instance;
		}
		return GameManager.server.CreatePrefab("assets/prefabs/engine/maplayerrenderer.prefab", Vector3.zero, Quaternion.identity).GetComponent<MapLayerRenderer>();
	}
}
