using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WaterVisibilityGrid : ICoarseQueryGridProvider, IDisposable
{
	private CoarseQueryGrid _queryGrid;

	private const int CellSize = 8;

	private readonly ListHashSet<WaterVisibilityTrigger> _dynamicListSet;

	public WaterVisibilityGrid()
	{
		_queryGrid = new CoarseQueryGrid(8, (int)(World.Size + 1000), -5f);
		_dynamicListSet = new ListHashSet<WaterVisibilityTrigger>();
	}

	public CoarseQueryGrid GetQueryGrid()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		_queryGrid.PrepareForDynamicPopulate(_dynamicListSet.Count);
		Enumerator<WaterVisibilityTrigger> enumerator = _dynamicListSet.Values.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				WaterVisibilityTrigger current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current.volume == (Object)null) && !((Object)(object)current.volume.trigger == (Object)null))
				{
					_queryGrid.AddDynamic(current.volume.trigger.bounds);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return _queryGrid;
	}

	public void AddTrigger(WaterVisibilityTrigger trigger)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (trigger.IsDynamic)
		{
			_dynamicListSet.Add(trigger);
		}
		else
		{
			_queryGrid.AddStatic(trigger.volume.trigger.bounds);
		}
	}

	public void RemoveTrigger(WaterVisibilityTrigger trigger)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (trigger.IsDynamic)
		{
			_dynamicListSet.Remove(trigger);
		}
		else
		{
			_queryGrid.RemoveStatic(trigger.volume.trigger.bounds);
		}
	}

	public bool Check(Bounds bounds)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJob(bounds);
	}

	public bool Check(Vector3 worldPosition, float radius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJob(worldPosition, radius);
	}

	public JobHandle Check(ReadOnly<Vector3> positions, ReadOnly<float> radii, NativeList<int> results)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJob(positions, radii, results);
	}

	public JobHandle CheckIndirect(ReadOnly<Vector3> positions, ReadOnly<float> radii, ReadOnly<int> indices, NativeList<int> results)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJobIndirect(positions, radii, indices, results);
	}

	public bool Check(Vector3 start, Vector3 end, float radius)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJob(start, end, radius);
	}

	public JobHandle CheckIndirect(ReadOnly<Vector3> starts, ReadOnly<Vector3> ends, ReadOnly<float> radii, ReadOnly<int> indices, NativeList<int> results)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return GetQueryGrid().CheckJobIndirect(starts, ends, radii, indices, results);
	}

	public void Dispose()
	{
		_queryGrid.Dispose();
	}
}
