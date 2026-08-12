using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class Socket_Free_Snappable : Socket_Free
{
	private struct SnapResult
	{
		public bool Valid;

		public float Score;

		public Construction.Placement Placement;

		public string Label;

		public static SnapResult Invalid => new SnapResult
		{
			Valid = false,
			Score = float.MaxValue
		};
	}

	private struct BuildingBlockPadding
	{
		public enum PaddingType
		{
			WeaksideOnly,
			StrongsideOnly,
			Both
		}

		public float YPadding;

		public float NormalPadding;

		public PaddingType PaddingMode;
	}

	[ClientVar(Saved = true, Help = "The current snapping mode for deployables.")]
	public static int SnappingMode = 2;

	[ClientVar(Help = "(Generated) When enabled, draws debug visualisations for deployable snapping calculations showing candidate snap points and distances")]
	public static bool DebugSnapping = false;

	[Range(-1f, 1f)]
	[Header("Snapping - General")]
	[SerializeField]
	private float generalPadding;

	[Header("Snapping - Walls")]
	[SerializeField]
	[Range(-1f, 1f)]
	private float snappingPadding;

	[SerializeField]
	[Header("Snapping - Corners")]
	private bool allowSnappingToCorners = true;

	[Range(-1f, 1f)]
	[SerializeField]
	private float cornerPadding = -0.01f;

	[SerializeField]
	[Header("Snapping - Same Deployable")]
	private bool allowSnappingToSameDeployable = true;

	[Range(-1f, 1f)]
	[SerializeField]
	private float sameDeployablePadding;

	private BaseEntity staticEntity;

	private Construction staticConstruction;

	private static List<SnapResult> results = new List<SnapResult>();

	private const int SNAP_MASK = 136314880;

	private static readonly Dictionary<string, BuildingBlockPadding> _buildingBlockPaddingDatabase = new Dictionary<string, BuildingBlockPadding>
	{
		{
			"assets/prefabs/building core/foundation/foundation.container.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0.02f,
				NormalPadding = 0f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		},
		{
			"assets/prefabs/building core/wall/wall.wood.full.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0f,
				NormalPadding = 0.1f,
				PaddingMode = BuildingBlockPadding.PaddingType.StrongsideOnly
			}
		},
		{
			"assets/prefabs/building core/wall/wall.twig.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0f,
				NormalPadding = 0.1f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		},
		{
			"assets/prefabs/building boat/wall/wall.wood.full.prefab",
			new BuildingBlockPadding
			{
				YPadding = 0.02f,
				NormalPadding = 0.02f,
				PaddingMode = BuildingBlockPadding.PaddingType.Both
			}
		}
	};

	private void AddDirections(Construction.Target target, PooledList<Vector3> directions, bool rayAligned)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		((List<Vector3>)(object)directions).Clear();
		Vector3 up = GetUp(target);
		Vector3 direction;
		Vector3 val;
		if (rayAligned)
		{
			direction = ((Ray)(ref target.ray)).direction;
			direction -= up * Vector3.Dot(direction, up);
			val = -Vector3.Cross(direction, up);
		}
		else
		{
			direction = ((Component)target.entity).transform.forward;
			val = ((Component)target.entity).transform.right;
		}
		((List<Vector3>)(object)directions).Add(direction);
		((List<Vector3>)(object)directions).Add(-direction);
		((List<Vector3>)(object)directions).Add(val);
		((List<Vector3>)(object)directions).Add(-val);
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.AttributeSetup(rootObj, name, serverside, clientside, bundling);
		staticEntity = rootObj.GetComponent<BaseEntity>();
		staticConstruction = rootObj.GetComponent<Construction>();
	}

	public override Construction.Placement DoPlacement(Construction.Target target)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		if (target.snappingMode == 0)
		{
			return base.DoPlacement(target);
		}
		if (!target.isHoldingShift || (Object)(object)target.entity == (Object)null)
		{
			return base.DoPlacement(target);
		}
		if (target.buildingBlocked)
		{
			return base.DoPlacement(target);
		}
		Vector3 up = GetUp(target);
		Vector3 val = -up;
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement"))
		{
			Vector3 val2 = target.position + target.normal * 0.15f;
			Ray ray = new Ray(val2, val);
			PooledList<RaycastHit> val3 = Pool.Get<PooledList<RaycastHit>>();
			try
			{
				GamePhysics.TraceAll(ray, 0f, (List<RaycastHit>)(object)val3, 2f, 136314880, (QueryTriggerInteraction)0);
				if (((List<RaycastHit>)(object)val3).Count > 0)
				{
					foreach (RaycastHit item in (List<RaycastHit>)(object)val3)
					{
						RaycastHit current = item;
						if (GamePhysics.LineOfSight(((Ray)(ref target.ray)).origin, ((RaycastHit)(ref current)).point + up * 0.1f, 136314880) && !(Vector3Ex.Distance2D(((Ray)(ref target.ray)).origin, ((RaycastHit)(ref current)).point) > staticConstruction.maxplaceDistance))
						{
							float num = ((Bounds)(ref staticEntity.bounds)).extents.y - ((Bounds)(ref staticEntity.bounds)).center.y;
							Vector3 val4 = up * num;
							target.position = ((RaycastHit)(ref current)).point + val4;
							float buildingBlockPadding = GetBuildingBlockPadding(((Object)((RaycastHit)(ref current)).collider).name, yPadding: true, ((Component)((RaycastHit)(ref current)).collider).transform, ((RaycastHit)(ref current)).normal);
							ref Vector3 reference = ref target.position;
							reference += up * buildingBlockPadding;
						}
					}
					results.Clear();
					if (target.snappingMode == 2)
					{
						results.Add(TryCornerSnap(target));
						results.Add(TryMatchingDeployableSnap(target));
					}
					results.Add(TryWallSnap(target));
					SnapResult snapResult = SnapResult.Invalid;
					foreach (SnapResult result in results)
					{
						if (DebugSnapping && result.Valid)
						{
							Debug.Log((object)$"[Snapping] Placement:{result.Label} (score: {result.Score:F3} (valid: {result.Valid})");
						}
						if (result.Valid && result.Score < snapResult.Score && ContainerCorpse.IsValidPointForEntity(staticEntity.prefabID, result.Placement.position, result.Placement.rotation))
						{
							if (DebugSnapping)
							{
								Debug.Log((object)$"Selected best: {result.Label}, Score: {result.Score}");
							}
							snapResult = result;
						}
					}
					if (DebugSnapping)
					{
						Debug.Log((object)$"Final Best Valid: {snapResult.Valid}, Score: {snapResult.Score}, Label: {snapResult.Label}");
					}
					if (snapResult.Valid)
					{
						if (DebugSnapping)
						{
							Debug.Log((object)$"[Snapping] Best placement: {snapResult.Label} (score: {snapResult.Score:F3})");
						}
						return snapResult.Placement;
					}
					target.valid = false;
					return base.DoPlacement(target);
				}
				target.valid = false;
				return base.DoPlacement(target);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
	}

	private float GetMaxDistance()
	{
		return 2.5f;
	}

	private float GetBuildingBlockPadding(string name, bool yPadding, Transform buildingBlockTransform, Vector3 rayNormal)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (_buildingBlockPaddingDatabase.TryGetValue(name, out var value))
		{
			if (yPadding)
			{
				return value.YPadding;
			}
			if (ShouldAddPadding(buildingBlockTransform, rayNormal, value.PaddingMode))
			{
				return value.NormalPadding;
			}
		}
		return 0f;
	}

	private bool ShouldAddPadding(Transform buildingBlockTransform, Vector3 rayNormal, BuildingBlockPadding.PaddingType type)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (type == BuildingBlockPadding.PaddingType.Both)
		{
			return true;
		}
		Matrix4x4 worldToLocalMatrix = buildingBlockTransform.worldToLocalMatrix;
		Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyVector(-rayNormal);
		float num = Vector3Ex.DotDegrees(worldForward, val);
		return type switch
		{
			BuildingBlockPadding.PaddingType.WeaksideOnly => num > 90f, 
			BuildingBlockPadding.PaddingType.StrongsideOnly => num < 90f, 
			_ => false, 
		};
	}

	private SnapResult TryWallSnap(Construction.Target target)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryWallSnap"))
		{
			if (TryFindBestSnappingHit(target, out var bestHit))
			{
				Quaternion targetRotation = ComputeSnappedRotation(target, bestHit);
				Vector3 val = ComputeSnappedPosition(target, bestHit, targetRotation);
				float score = Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref bestHit)).point);
				return new SnapResult
				{
					Valid = true,
					Score = score,
					Placement = new Construction.Placement(target)
					{
						position = val,
						rotation = targetRotation
					},
					Label = "Wall"
				};
			}
			return SnapResult.Invalid;
		}
	}

	private bool TryFindBestSnappingHit(Construction.Target target, out RaycastHit bestHit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		bestHit = default(RaycastHit);
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			AddDirections(target, val, rayAligned: false);
			Vector3 up = GetUp(target);
			Vector3 val2 = target.position + up * 0.15f;
			PooledList<RaycastHit> val3 = Pool.Get<PooledList<RaycastHit>>();
			try
			{
				Ray ray = default(Ray);
				foreach (Vector3 item in (List<Vector3>)(object)val)
				{
					((Ray)(ref ray))._002Ector(val2, item);
					PooledList<RaycastHit> val4 = Pool.Get<PooledList<RaycastHit>>();
					try
					{
						GamePhysics.TraceAll(ray, 0f, (List<RaycastHit>)(object)val4, GetMaxDistance(), 136314880, (QueryTriggerInteraction)0);
						if (((List<RaycastHit>)(object)val4).Count <= 0)
						{
							continue;
						}
						foreach (RaycastHit item2 in (List<RaycastHit>)(object)val4)
						{
							((List<RaycastHit>)(object)val3).Add(item2);
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				float num = float.MaxValue;
				foreach (RaycastHit item3 in (List<RaycastHit>)(object)val3)
				{
					RaycastHit current3 = item3;
					if (!(Vector3.Distance(((RaycastHit)(ref current3)).point, ((Ray)(ref target.ray)).origin) > staticConstruction.maxplaceDistance))
					{
						float num2 = Vector3Ex.Distance2D(val2, ((RaycastHit)(ref current3)).point);
						if (num2 < num)
						{
							num = num2;
							bestHit = current3;
						}
					}
				}
				return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Quaternion ComputeSnappedRotation(Construction.Target target, RaycastHit bestHit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = GetUp(target);
		Vector3 normal = ((RaycastHit)(ref bestHit)).normal;
		Quaternion val = ((normal == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(normal, up));
		Vector3 val2 = ((RaycastHit)(ref bestHit)).point - ((Ray)(ref target.ray)).origin;
		Vector3 val3 = -((Vector3)(ref val2)).normalized;
		val3 -= up * Vector3.Dot(val3, up);
		Quaternion val4 = Quaternion.LookRotation(val3, up) * Quaternion.Euler(target.rotation);
		Quaternion val5 = val * Quaternion.Euler(target.rotation);
		Vector3 val6 = val5 * ((RaycastHit)(ref bestHit)).normal;
		Vector3 val7 = val4 * ((RaycastHit)(ref bestHit)).normal;
		if (Mathf.Abs(Vector3.Dot(val7, val6)) < 0.5f)
		{
			Quaternion val8 = Quaternion.AngleAxis(Mathf.Round(Vector3.SignedAngle(val6, val7, up) / 90f) * 90f, up);
			val5 *= val8;
		}
		return val5;
	}

	private Vector3 ComputeSnappedPosition(Construction.Target target, RaycastHit bestHit, Quaternion targetRotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = GetUp(target);
		Vector3 normal = ((RaycastHit)(ref bestHit)).normal;
		Matrix4x4 val = Matrix4x4.TRS(target.position, targetRotation, ((Component)staticEntity).transform.lossyScale);
		Matrix4x4 inverse = ((Matrix4x4)(ref val)).inverse;
		Vector3 val2 = Vector3.Scale(((Matrix4x4)(ref inverse)).MultiplyVector(normal), ((Bounds)(ref staticEntity.bounds)).extents);
		Vector3 val3 = ((Matrix4x4)(ref val)).MultiplyVector(val2);
		Vector3 val4 = targetRotation * ((Bounds)(ref staticEntity.bounds)).center;
		val4 -= up * Vector3.Dot(val4, up);
		float num = Vector3.Dot(val4, normal);
		Vector3 val5 = ((RaycastHit)(ref bestHit)).point + normal * snappingPadding + val3 - normal * num;
		val5 += normal * generalPadding;
		if ((Object)(object)((RaycastHit)(ref bestHit)).collider != (Object)null)
		{
			float buildingBlockPadding = GetBuildingBlockPadding(((Object)((RaycastHit)(ref bestHit)).collider).name, yPadding: false, ((Component)((RaycastHit)(ref bestHit)).collider).transform, ((RaycastHit)(ref bestHit)).normal);
			val5 += normal * buildingBlockPadding;
		}
		float num2 = Vector3.Dot(target.position, up);
		float num3 = Vector3.Dot(val5, up);
		return val5 + up * (num2 - num3);
	}

	private SnapResult TryCornerSnap(Construction.Target target)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (!allowSnappingToCorners)
		{
			return SnapResult.Invalid;
		}
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryCornerSnap"))
		{
			if (TryFindCornerHits(target, out var hitA, out var hitB))
			{
				RaycastHit bestHit = ((Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref hitA)).point) < Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref hitB)).point)) ? hitA : hitB);
				Quaternion targetRotation = ComputeSnappedRotation(target, bestHit);
				Vector3 val = ComputeCornerSnappedPosition(target, hitA, hitB, targetRotation);
				float num = Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref hitA)).point);
				float num2 = Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref hitB)).point);
				float num3 = Mathf.Min(num, num2);
				num3 *= 0.7f;
				return new SnapResult
				{
					Valid = true,
					Score = num3,
					Placement = new Construction.Placement(target)
					{
						position = val,
						rotation = targetRotation
					},
					Label = "Corner"
				};
			}
			return SnapResult.Invalid;
		}
	}

	private bool TryFindCornerHits(Construction.Target target, out RaycastHit hitA, out RaycastHit hitB)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		hitA = default(RaycastHit);
		hitB = default(RaycastHit);
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			AddDirections(target, val, rayAligned: false);
			Vector3 up = GetUp(target);
			Vector3 val2 = target.position + up * 0.5f;
			float num = float.MaxValue;
			PooledList<RaycastHit> val3 = Pool.Get<PooledList<RaycastHit>>();
			try
			{
				Ray ray = default(Ray);
				foreach (Vector3 item in (List<Vector3>)(object)val)
				{
					((Ray)(ref ray))._002Ector(val2, item);
					PooledList<RaycastHit> val4 = Pool.Get<PooledList<RaycastHit>>();
					try
					{
						GamePhysics.TraceAll(ray, 0f, (List<RaycastHit>)(object)val4, GetMaxDistance(), 136315136, (QueryTriggerInteraction)0);
						if (((List<RaycastHit>)(object)val4).Count <= 0)
						{
							continue;
						}
						foreach (RaycastHit item2 in (List<RaycastHit>)(object)val4)
						{
							((List<RaycastHit>)(object)val3).Add(item2);
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				for (int i = 0; i < ((List<RaycastHit>)(object)val3).Count; i++)
				{
					BaseEntity entity = RaycastHitEx.GetEntity(((List<RaycastHit>)(object)val3)[i]);
					if ((Object)(object)entity == (Object)null || entity.net == null)
					{
						continue;
					}
					for (int j = i + 1; j < ((List<RaycastHit>)(object)val3).Count; j++)
					{
						BaseEntity entity2 = RaycastHitEx.GetEntity(((List<RaycastHit>)(object)val3)[j]);
						if (!((Object)(object)entity2 == (Object)null) && entity2.net != null && !(entity.net.ID == entity2.net.ID))
						{
							RaycastHit val5 = ((List<RaycastHit>)(object)val3)[i];
							Vector3 normal = ((RaycastHit)(ref val5)).normal;
							val5 = ((List<RaycastHit>)(object)val3)[j];
							Vector3 normal2 = ((RaycastHit)(ref val5)).normal;
							float num2 = Mathf.Abs(Vector3.Dot(((Vector3)(ref normal)).normalized, ((Vector3)(ref normal2)).normalized));
							val5 = ((List<RaycastHit>)(object)val3)[i];
							float num3 = Vector3Ex.Distance2D(val2, ((RaycastHit)(ref val5)).point);
							float num4 = num3;
							val5 = ((List<RaycastHit>)(object)val3)[j];
							num3 = num4 + Vector3Ex.Distance2D(val2, ((RaycastHit)(ref val5)).point);
							if (num2 < 0.3f && num3 < num)
							{
								hitA = ((List<RaycastHit>)(object)val3)[i];
								hitB = ((List<RaycastHit>)(object)val3)[j];
								num = num3;
							}
						}
					}
				}
				return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Vector3 GetPlaneIntersectionPoint(Vector3 normal1, Vector3 point1, Vector3 normal2, Vector3 point2)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(normal1, normal2);
		if (((Vector3)(ref val)).sqrMagnitude < Mathf.Epsilon)
		{
			return Vector3.zero;
		}
		float num = Vector3.Dot(normal1, point1);
		float num2 = Vector3.Dot(normal2, point2);
		Vector3 val2 = Vector3.Cross(normal2, val) * num + Vector3.Cross(val, normal1) * num2;
		float num3 = ((Vector3)(ref val)).magnitude * ((Vector3)(ref val)).magnitude;
		return val2 / num3;
	}

	private Vector3 ComputeCornerSnappedPosition(Construction.Target target, RaycastHit hitA, RaycastHit hitB, Quaternion targetRotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normal = ((RaycastHit)(ref hitA)).normal;
		Vector3 normal2 = ((RaycastHit)(ref hitB)).normal;
		Vector3 up = GetUp(target);
		normal -= up * Vector3.Dot(normal, up);
		normal2 -= up * Vector3.Dot(normal2, up);
		Vector3 planeIntersectionPoint = GetPlaneIntersectionPoint(normal, ((RaycastHit)(ref hitA)).point, normal2, ((RaycastHit)(ref hitB)).point);
		float num = Vector3.Dot(target.position, up);
		float num2 = Vector3.Dot(planeIntersectionPoint, up);
		Vector3 val = planeIntersectionPoint + up * (num - num2);
		Vector3 val2 = normal + normal2;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		Vector3 val3 = targetRotation * ((Bounds)(ref staticEntity.bounds)).center;
		Matrix4x4 val4 = Matrix4x4.TRS(val + val3, targetRotation, ((Component)staticEntity).transform.lossyScale);
		Matrix4x4 inverse = ((Matrix4x4)(ref val4)).inverse;
		Vector3 val5 = Vector3.Scale(((Matrix4x4)(ref inverse)).MultiplyVector(normal), ((Bounds)(ref staticEntity.bounds)).extents);
		inverse = ((Matrix4x4)(ref val4)).inverse;
		Vector3 val6 = Vector3.Scale(((Matrix4x4)(ref inverse)).MultiplyVector(normal2), ((Bounds)(ref staticEntity.bounds)).extents);
		Vector3 val7 = ((Matrix4x4)(ref val4)).MultiplyVector(val5 + val6);
		Vector3 val8 = val + val7 + normalized * cornerPadding + normalized * generalPadding;
		float num3 = 0f;
		if ((Object)(object)((RaycastHit)(ref hitA)).collider != (Object)null)
		{
			num3 = Mathf.Max(num3, GetBuildingBlockPadding(((Object)((RaycastHit)(ref hitA)).collider).name, yPadding: false, ((Component)((RaycastHit)(ref hitA)).collider).transform, ((RaycastHit)(ref hitA)).normal));
		}
		if ((Object)(object)((RaycastHit)(ref hitB)).collider != (Object)null)
		{
			num3 = Mathf.Max(num3, GetBuildingBlockPadding(((Object)((RaycastHit)(ref hitB)).collider).name, yPadding: false, ((Component)((RaycastHit)(ref hitB)).collider).transform, ((RaycastHit)(ref hitB)).normal));
		}
		Vector3 val9 = val8 + normalized * num3;
		float num4 = Vector3.Dot(target.position, up);
		float num5 = Vector3.Dot(val9, up);
		return val9 + up * (num4 - num5);
	}

	private SnapResult TryMatchingDeployableSnap(Construction.Target target)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!allowSnappingToSameDeployable)
		{
			return SnapResult.Invalid;
		}
		using (TimeWarning.New("Socket_Free_Snappable.DoPlacement.TryMatchingDeployableSnap"))
		{
			if (TryFindMatchingDeployables(target, out var bestHit))
			{
				Quaternion targetRotation = ((RaycastHit)(ref bestHit)).transform.rotation;
				Vector3 val = ComputeSnappedMatchingDeployablePosition(target, bestHit, targetRotation);
				float num = Vector3Ex.Distance2D(target.position, ((RaycastHit)(ref bestHit)).point);
				if (target.entity.prefabID == staticEntity.prefabID)
				{
					num *= 0.9f;
				}
				return new SnapResult
				{
					Valid = true,
					Score = num,
					Placement = new Construction.Placement(target)
					{
						position = val,
						rotation = targetRotation
					},
					Label = "Deployable"
				};
			}
			return SnapResult.Invalid;
		}
	}

	private Vector3 ComputeSnappedMatchingDeployablePosition(Construction.Target target, RaycastHit bestHit, Quaternion targetRotation)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity entity = RaycastHitEx.GetEntity(bestHit);
		if ((Object)(object)entity == (Object)null)
		{
			return target.position;
		}
		OBB val = entity.WorldSpaceBounds();
		Vector3 val2 = ((RaycastHit)(ref bestHit)).point - val.position;
		Vector3 up = GetUp(target);
		val2 -= up * Vector3.Dot(val2, up);
		Vector3[] obj = new Vector3[4]
		{
			val.right,
			-val.right,
			val.forward,
			-val.forward
		};
		Vector3 val3 = val.forward;
		float num = -1f;
		Vector3[] array = (Vector3[])(object)obj;
		foreach (Vector3 val4 in array)
		{
			float num2 = Vector3.Dot(val2, val4);
			if (num2 > num)
			{
				num = num2;
				val3 = val4;
			}
		}
		Matrix4x4 val5 = Matrix4x4.TRS(target.position, targetRotation, ((Component)staticEntity).transform.lossyScale);
		Matrix4x4 inverse = ((Matrix4x4)(ref val5)).inverse;
		Vector3 val6 = Vector3.Scale(((Matrix4x4)(ref inverse)).MultiplyVector(((Vector3)(ref val3)).normalized), ((Bounds)(ref staticEntity.bounds)).size);
		Vector3 val7 = ((Matrix4x4)(ref val5)).MultiplyVector(val6);
		Vector3 val8 = targetRotation * ((Bounds)(ref staticEntity.bounds)).center;
		val8 -= up * Vector3.Dot(val8, up);
		float num3 = Vector3.Dot(val8, val3);
		return ((Component)entity).transform.position + val3 * sameDeployablePadding + val7 - val3 * num3 + val3 * generalPadding;
	}

	private bool TryFindMatchingDeployables(Construction.Target target, out RaycastHit bestHit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		bestHit = default(RaycastHit);
		PooledList<Vector3> val = Pool.Get<PooledList<Vector3>>();
		try
		{
			AddDirections(target, val, rayAligned: false);
			Vector3 up = GetUp(target);
			Vector3 val2 = target.position + up * 0.05f;
			PooledList<RaycastHit> val3 = Pool.Get<PooledList<RaycastHit>>();
			try
			{
				Ray ray = default(Ray);
				foreach (Vector3 item in (List<Vector3>)(object)val)
				{
					((Ray)(ref ray))._002Ector(val2, item);
					PooledList<RaycastHit> val4 = Pool.Get<PooledList<RaycastHit>>();
					try
					{
						GamePhysics.TraceAll(ray, 0f, (List<RaycastHit>)(object)val4, GetMaxDistance() * 1.5f, 256, (QueryTriggerInteraction)0);
						if (((List<RaycastHit>)(object)val4).Count <= 0)
						{
							continue;
						}
						foreach (RaycastHit item2 in (List<RaycastHit>)(object)val4)
						{
							RaycastHit current2 = item2;
							if (!((Object)(object)((RaycastHit)(ref current2)).collider == (Object)null))
							{
								BaseEntity entity = RaycastHitEx.GetEntity(current2);
								if (!((Object)(object)entity == (Object)null) && ShouldDeployableSnap(staticEntity, entity))
								{
									((List<RaycastHit>)(object)val3).Add(current2);
								}
							}
						}
					}
					finally
					{
						((IDisposable)val4)?.Dispose();
					}
				}
				float num = float.MaxValue;
				foreach (RaycastHit item3 in (List<RaycastHit>)(object)val3)
				{
					RaycastHit current3 = item3;
					if (!(Vector3.Distance(((RaycastHit)(ref current3)).point, ((Ray)(ref target.ray)).origin) > staticConstruction.maxplaceDistance))
					{
						float num2 = Vector3Ex.Distance2D(val2, ((RaycastHit)(ref current3)).point);
						if (num2 < num)
						{
							num = num2;
							bestHit = current3;
						}
					}
				}
				return Math.Abs(num - float.MaxValue) > Mathf.Epsilon;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Vector3 GetUp(Construction.Target target)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target.entity != (Object)null)
		{
			return ((Component)target.entity).transform.up;
		}
		return Vector3.up;
	}

	private bool ShouldDeployableSnap(BaseEntity ent, BaseEntity other)
	{
		if (ent is BaseCombatEntity { pickup: { enabled: not false } pickup } && other is BaseCombatEntity { pickup: { enabled: not false } pickup2 })
		{
			if ((Object)(object)pickup.itemTarget == (Object)null || (Object)(object)pickup2.itemTarget == (Object)null)
			{
				return false;
			}
			ItemDefinition obj = (((Object)(object)pickup.itemTarget.isRedirectOf == (Object)null) ? pickup.itemTarget : pickup.itemTarget.isRedirectOf);
			ItemDefinition itemDefinition = (((Object)(object)pickup2.itemTarget.isRedirectOf == (Object)null) ? pickup2.itemTarget : pickup2.itemTarget.isRedirectOf);
			return (Object)(object)obj == (Object)(object)itemDefinition;
		}
		return ent.prefabID == other.prefabID;
	}
}
