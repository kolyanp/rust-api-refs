using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceWaterTreatmentPipes : PlaceDecorRoadside
{
	private struct BlockedVolume
	{
		public OBB Obb;

		public float Radius;
	}

	[Tooltip("Clearance from a prevent building volume. Only the pivot is tested.")]
	public float PreventBuildingPadding = 2f;

	private bool doesWaterTreatmentExist;

	private List<BlockedVolume> preventBuildingVolumes = new List<BlockedVolume>();

	protected override bool ShouldPlace()
	{
		CheckMonuments();
		return doesWaterTreatmentExist;
	}

	protected override bool IsValidLocation(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		foreach (BlockedVolume preventBuildingVolume in preventBuildingVolumes)
		{
			float num = PreventBuildingPadding + preventBuildingVolume.Radius;
			Vector3 val = pos - preventBuildingVolume.Obb.position;
			if (!(((Vector3)(ref val)).sqrMagnitude > num * num))
			{
				OBB obb = preventBuildingVolume.Obb;
				if (((OBB)(ref obb)).Distance(pos) < PreventBuildingPadding)
				{
					return false;
				}
			}
		}
		return true;
	}

	private void CheckMonuments()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		doesWaterTreatmentExist = false;
		preventBuildingVolumes.Clear();
		if ((Object)(object)TerrainMeta.Path == (Object)null || TerrainMeta.Path.Monuments == null)
		{
			Debug.LogError((object)"[PlaceWaterTreatmentPipes] PROCESSING: TerrainMeta.Path.Monuments is null, cannot check for water-treatment monument, skipping placement.");
			return;
		}
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument.IsWaterTreatmentPlant())
			{
				doesWaterTreatmentExist = true;
			}
		}
		Enumerator<PreventBuildingMonumentTag> enumerator2 = PreventBuildingMonumentTag.All.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				if (enumerator2.Current.TryGetVolume(out var result))
				{
					preventBuildingVolumes.Add(new BlockedVolume
					{
						Obb = result,
						Radius = ((Vector3)(ref result.extents)).magnitude
					});
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
