using System;
using System.Collections.Generic;
using Rust.Rendering.IndirectInstancing;
using UnityEngine;

public class StagedRendererLOD : InstancedLODComponent, IHLODMeshSource
{
	public struct LodVisual
	{
		public Mesh Mesh;

		public Material[] Materials;

		public bool IsValid
		{
			get
			{
				if ((Object)(object)Mesh != (Object)null && Materials != null)
				{
					return Materials.Length != 0;
				}
				return false;
			}
		}
	}

	[Serializable]
	public class State
	{
		[Range(0f, 1000f)]
		public float distance;

		public bool disableShadows;
	}

	[Tooltip("The single renderer everything is drawn through. Its mesh and materials are swapped at runtime.")]
	[Header("Target Renderer")]
	public MeshRenderer TargetRenderer;

	public MeshFilter TargetFilter;

	[Tooltip("Distance at which each LOD level becomes active. LOD levels the active stage has no mesh for are culled.")]
	[Header("LOD Levels")]
	public State[] States = Array.Empty<State>();

	public StagedDestructionEntityInfo FindStageInfo()
	{
		Transform root = ((Component)this).transform.root;
		if (!((Object)(object)root != (Object)null))
		{
			return null;
		}
		return ((Component)root).GetComponentInChildren<StagedDestructionEntityInfo>(true);
	}

	public IEnumerable<Mesh> GetAllStageMeshes()
	{
		StagedDestructionEntityInfo stagedDestructionEntityInfo = FindStageInfo();
		if (stagedDestructionEntityInfo == null || stagedDestructionEntityInfo.Stages == null)
		{
			yield break;
		}
		StagedDestructionEntityInfo.ResourceStage[] stages = stagedDestructionEntityInfo.Stages;
		foreach (StagedDestructionEntityInfo.ResourceStage resourceStage in stages)
		{
			if (resourceStage == null || resourceStage.VisualLods == null)
			{
				continue;
			}
			StagedDestructionEntityInfo.LodVisual[] visualLods = resourceStage.VisualLods;
			foreach (StagedDestructionEntityInfo.LodVisual lodVisual in visualLods)
			{
				if (lodVisual != null && (Object)(object)lodVisual.Mesh != (Object)null)
				{
					yield return lodVisual.Mesh;
				}
			}
		}
	}

	public void EditorPreviewStage(int stageIndex, int lodLevel = 0)
	{
		if ((Object)(object)TargetFilter == (Object)null)
		{
			TargetFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if ((Object)(object)TargetRenderer == (Object)null)
		{
			TargetRenderer = ((Component)this).GetComponent<MeshRenderer>();
		}
	}
}
