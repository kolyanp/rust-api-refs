using System.Collections.Generic;
using UnityEngine;

public class PlaceWaterTreatmentPipes : PlaceDecorUniform
{
	private static bool doesWaterTreatmentExist = false;

	private static bool haveMonumentsBeenCached = false;

	private static List<OBB> monumentOBBs = new List<OBB>();

	public override void Process(uint seed)
	{
		if (!haveMonumentsBeenCached)
		{
			CheckMonuments();
		}
		if (doesWaterTreatmentExist)
		{
			base.Process(seed);
		}
	}

	protected override bool IsValidLocation(Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!haveMonumentsBeenCached)
		{
			CheckMonuments();
		}
		foreach (OBB monumentOBB in monumentOBBs)
		{
			OBB current = monumentOBB;
			if (((OBB)(ref current)).Contains(pos))
			{
				return false;
			}
		}
		return true;
	}

	private void CheckMonuments()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		haveMonumentsBeenCached = true;
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
			if (((Bounds)(ref monument.Bounds)).size != Vector3.zero)
			{
				monumentOBBs.Add(new OBB(((Component)monument).transform.position, ((Component)monument).transform.rotation, new Bounds(((Bounds)(ref monument.Bounds)).center, ((Bounds)(ref monument.Bounds)).size * 2f)));
			}
		}
	}
}
