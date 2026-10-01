using System.Collections.Generic;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai;

public class CoverPointVolume : MonoBehaviour, IServerComponent
{
	internal enum CoverType
	{
		None,
		Partial,
		Full
	}

	public float DefaultCoverPointScore = 1f;

	public float CoverPointRayLength = 1f;

	public LayerMask CoverLayerMask;

	public Transform BlockerGroup;

	public Transform ManualCoverPointGroup;

	[ServerVar(Help = "cover_point_sample_step_size defines the size of the steps we do horizontally for the cover point volume's cover point generation (smaller steps gives more accurate cover points, but at a higher processing cost). (default: 6.0)")]
	public static float cover_point_sample_step_size = 6f;

	[ServerVar(Help = "cover_point_sample_step_height defines the height of the steps we do vertically for the cover point volume's cover point generation (smaller steps gives more accurate cover points, but at a higher processing cost). (default: 2.0)")]
	public static float cover_point_sample_step_height = 2f;

	public readonly List<CoverPoint> CoverPoints = new List<CoverPoint>();

	private readonly List<CoverPointBlockerVolume> _coverPointBlockers = new List<CoverPointBlockerVolume>();

	private float _dynNavMeshBuildCompletionTime = -1f;

	private int _genAttempts;

	private Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

	public bool repeat => true;

	public float? ExecuteUpdate(float deltaTime, float nextInterval)
	{
		if (CoverPoints.Count == 0)
		{
			if (_dynNavMeshBuildCompletionTime < 0f)
			{
				if ((Object)(object)SingletonComponent<DynamicNavMesh>.Instance == (Object)null || !((Behaviour)SingletonComponent<DynamicNavMesh>.Instance).enabled || !SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
				{
					_dynNavMeshBuildCompletionTime = Time.realtimeSinceStartup;
				}
			}
			else if (_genAttempts < 4 && Time.realtimeSinceStartup - _dynNavMeshBuildCompletionTime > 0.25f)
			{
				GenerateCoverPoints(null);
				if (CoverPoints.Count != 0)
				{
					return null;
				}
				_dynNavMeshBuildCompletionTime = Time.realtimeSinceStartup;
				_genAttempts++;
				if (_genAttempts >= 4)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
					return null;
				}
			}
		}
		return 1f + Random.value * 2f;
	}

	[ContextMenu("Clear Cover Points")]
	private void ClearCoverPoints()
	{
		CoverPoints.Clear();
		_coverPointBlockers.Clear();
	}

	public Bounds GetBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = bounds.center;
		if (Mathf.Approximately(center.sqrMagnitude, 0f))
		{
			bounds = new Bounds(((Component)this).transform.position, ((Component)this).transform.localScale);
		}
		return bounds;
	}

	[ContextMenu("Pre-Generate Cover Points")]
	public void PreGenerateCoverPoints()
	{
		GenerateCoverPoints(null);
	}

	[ContextMenu("Convert to Manual Cover Points")]
	public void ConvertToManualCoverPoints()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		foreach (CoverPoint coverPoint in CoverPoints)
		{
			ManualCoverPoint manualCoverPoint = new GameObject("MCP").AddComponent<ManualCoverPoint>();
			((Component)manualCoverPoint).transform.localPosition = Vector3.zero;
			((Component)manualCoverPoint).transform.position = coverPoint.Position;
			manualCoverPoint.Normal = coverPoint.Normal;
			manualCoverPoint.NormalCoverType = coverPoint.NormalCoverType;
			manualCoverPoint.Volume = this;
		}
	}

	public void GenerateCoverPoints(Transform coverPointGroup)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		_ = Time.realtimeSinceStartup;
		ClearCoverPoints();
		if ((Object)(object)ManualCoverPointGroup == (Object)null)
		{
			ManualCoverPointGroup = coverPointGroup;
		}
		if ((Object)(object)ManualCoverPointGroup == (Object)null)
		{
			ManualCoverPointGroup = ((Component)this).transform;
		}
		if (ManualCoverPointGroup.childCount > 0)
		{
			ManualCoverPoint[] componentsInChildren = ((Component)ManualCoverPointGroup).GetComponentsInChildren<ManualCoverPoint>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				CoverPoint item = componentsInChildren[i].ToCoverPoint(this);
				CoverPoints.Add(item);
			}
		}
		if (_coverPointBlockers.Count == 0 && (Object)(object)BlockerGroup != (Object)null)
		{
			CoverPointBlockerVolume[] componentsInChildren2 = ((Component)BlockerGroup).GetComponentsInChildren<CoverPointBlockerVolume>();
			if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
			{
				_coverPointBlockers.AddRange(componentsInChildren2);
			}
		}
		if (CoverPoints.Count != 0 || !RustNavMeshHelpers.SamplePosition(((Component)this).transform.position, out var hitWS, ((Component)this).transform.localScale.y * cover_point_sample_step_height, -1))
		{
			return;
		}
		Vector3 position = ((Component)this).transform.position;
		Vector3 val = ((Component)this).transform.lossyScale * 0.5f;
		NavMeshHit info = default;
		for (float num = position.x - val.x + 1f; num < position.x + val.x - 1f; num += cover_point_sample_step_size)
		{
			for (float num2 = position.z - val.z + 1f; num2 < position.z + val.z - 1f; num2 += cover_point_sample_step_size)
			{
				for (float num3 = position.y - val.y; num3 < position.y + val.y; num3 += cover_point_sample_step_height)
				{
					if (!NavMesh.FindClosestEdge(new Vector3(num, num3, num2), ref info, hitWS.mask))
					{
						continue;
					}
					info.position = new Vector3(info.position.x, info.position.y + 0.5f, info.position.z);
					bool flag = true;
					foreach (CoverPoint coverPoint2 in CoverPoints)
					{
						Vector3 val2 = coverPoint2.Position - info.position;
						if (val2.sqrMagnitude < cover_point_sample_step_size * cover_point_sample_step_size)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						CoverPoint coverPoint = CalculateCoverPoint(info);
						if (coverPoint != null)
						{
							CoverPoints.Add(coverPoint);
						}
					}
				}
			}
		}
	}

	private CoverPoint CalculateCoverPoint(NavMeshHit info)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		CoverType coverType = ProvidesCoverInDir(new Ray(info.position, -info.normal), CoverPointRayLength, out var _);
		if (coverType == CoverType.None)
		{
			return null;
		}
		CoverPoint coverPoint = new CoverPoint(this, DefaultCoverPointScore)
		{
			Position = info.position,
			Normal = -info.normal
		};
		switch (coverType)
		{
		case CoverType.Full:
			coverPoint.NormalCoverType = CoverPoint.CoverType.Full;
			break;
		case CoverType.Partial:
			coverPoint.NormalCoverType = CoverPoint.CoverType.Partial;
			break;
		}
		return coverPoint;
	}

	internal CoverType ProvidesCoverInDir(Ray ray, float maxDistance, out RaycastHit rayHit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		rayHit = default;
		if (Vector3Ex.IsNaNOrInfinity(ray.origin))
		{
			return CoverType.None;
		}
		if (Vector3Ex.IsNaNOrInfinity(ray.direction))
		{
			return CoverType.None;
		}
		if (ray.direction == Vector3.zero)
		{
			return CoverType.None;
		}
		ray.origin += PlayerEyes.EyeOffset;
		if (Physics.Raycast(ray.origin, ray.direction, ref rayHit, maxDistance, LayerMask.op_Implicit(CoverLayerMask)))
		{
			return CoverType.Full;
		}
		ray.origin += PlayerEyes.DuckOffset;
		if (Physics.Raycast(ray.origin, ray.direction, ref rayHit, maxDistance, LayerMask.op_Implicit(CoverLayerMask)))
		{
			return CoverType.Partial;
		}
		return CoverType.None;
	}

	public bool Contains(Vector3 point)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = new Bounds(((Component)this).transform.position, ((Component)this).transform.localScale);
		return val.Contains(point);
	}

	public CoverPointVolume()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
	}
}
