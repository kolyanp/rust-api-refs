using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public class IndependantNavmesh : MonoBehaviour, IServerComponent
{
	public Vector3 size;

	public bool canMove;

	public bool buildOnEnable;

	[Tooltip("Bake this navmesh with the hi res build params. Needed for detailed structures like ghost ships, the tile builder only picks hi res by itself for building blocks and monument prevent building volumes")]
	public bool forceHiRes;

	private static SparseGridWithBounds<IndependantNavmesh> navmeshLookup = new SparseGridWithBounds<IndependantNavmesh>();

	private Matrix4x4 buildTimeTransform;

	private Bounds lastBounds;

	public RustNavmesh Navmesh { get; private set; }

	public Matrix4x4 WorldToNavMatrix
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return Matrix4x4.identity;
			}
			if (!canMove)
			{
				return Matrix4x4.identity;
			}
			return buildTimeTransform * ((Component)this).transform.worldToLocalMatrix;
		}
	}

	public Matrix4x4 NavToWorldMatrix
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			if (!RustNavigation.EnsureNewNavmesh())
			{
				return Matrix4x4.identity;
			}
			if (!canMove)
			{
				return Matrix4x4.identity;
			}
			Matrix4x4 worldToNavMatrix = WorldToNavMatrix;
			return ((Matrix4x4)(ref worldToNavMatrix)).inverse;
		}
	}

	public bool IsBuilt()
	{
		if (Navmesh != null)
		{
			return Navmesh.IsBuilt();
		}
		return false;
	}

	private void OnEnable()
	{
		if (!AI.useUnityNavmesh && buildOnEnable && (Object)(object)RustNavigation.Instance != (Object)null)
		{
			RustNavigation.Instance.AddNavmesh(this);
		}
	}

	private void OnDisable()
	{
		if (!AI.useUnityNavmesh)
		{
			navmeshLookup.Remove(this);
			if ((Object)(object)RustNavigation.Instance != (Object)null)
			{
				RustNavigation.Instance.RemoveNavmesh(this);
			}
			if (Navmesh != null)
			{
				Navmesh.Dispose();
				Navmesh = null;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.useUnityNavmesh && canMove && Navmesh != null)
		{
			Bounds bounds = GetBounds();
			if (bounds != lastBounds)
			{
				navmeshLookup.Remove(this);
				lastBounds = bounds;
				navmeshLookup.Add(lastBounds, this);
			}
		}
	}

	public void Rebuild(BackgroundTileBuilder tileBuilder, bool synchronous = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return;
		}
		buildTimeTransform = ((Component)this).transform.localToWorldMatrix;
		RustNavmesh rustNavmesh = new RustNavmesh(tileBuilder, null, null, GetBounds(), shouldBuild: true, canMove || synchronous, forceHiRes);
		if (rustNavmesh == null || !rustNavmesh.IsValid())
		{
			RustNavigation.LogError("Failed to build independent navmesh for object " + ((Object)this).name);
			return;
		}
		rustNavmesh.debugName = ((Object)((Component)this).transform.root).name;
		if (Navmesh != null)
		{
			Navmesh.Dispose();
		}
		Navmesh = rustNavmesh;
		navmeshLookup.Remove(this);
		lastBounds = GetBounds();
		navmeshLookup.Add(lastBounds, this);
	}

	public void RebuildTilesInBounds(Bounds bounds, bool synchronous = false)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || Navmesh == null || !Navmesh.IsValid())
		{
			return;
		}
		if (canMove)
		{
			if (AI.logIssues)
			{
				RustNavigation.LogError("Rebuilding single tiles of moving navmesh is not supported.");
			}
		}
		else
		{
			Navmesh.RebuildTilesInBounds(bounds, canMove || synchronous);
		}
	}

	public NavVector3 TransformPointFromWorldSpaceToNavSpace(Vector3 worldPoint)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || !canMove)
		{
			return new NavVector3(worldPoint);
		}
		Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
		Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(worldPoint);
		return new NavVector3(((Matrix4x4)(ref buildTimeTransform)).MultiplyPoint3x4(val));
	}

	public Vector3 TransformPointFromNavSpaceToWorldSpace(NavVector3 navSpacePoint)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || !canMove)
		{
			return navSpacePoint.Value;
		}
		Matrix4x4 val = ((Matrix4x4)(ref buildTimeTransform)).inverse;
		Vector3 val2 = ((Matrix4x4)(ref val)).MultiplyPoint3x4(navSpacePoint.Value);
		val = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref val)).MultiplyPoint3x4(val2);
	}

	public Vector3 TransformDirectionFromNavSpaceToWorldSpace(NavVector3 navSpaceDirection)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || !canMove)
		{
			return navSpaceDirection.Value;
		}
		Matrix4x4 val = ((Matrix4x4)(ref buildTimeTransform)).inverse;
		Vector3 val2 = ((Matrix4x4)(ref val)).MultiplyVector(navSpaceDirection.Value);
		val = ((Component)this).transform.localToWorldMatrix;
		return ((Matrix4x4)(ref val)).MultiplyVector(val2);
	}

	public NavVector3 TransformDirectionFromWorldSpaceToNavSpace(Vector3 worldSpaceDirection)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || !canMove)
		{
			return new NavVector3(worldSpaceDirection);
		}
		Matrix4x4 worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
		Vector3 val = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyVector(worldSpaceDirection);
		return new NavVector3(((Matrix4x4)(ref buildTimeTransform)).MultiplyVector(val));
	}

	public bool FillDebugDrawProto(NavMeshData navMeshData, Bounds bounds)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh() || Navmesh == null || !Navmesh.IsValid())
		{
			return false;
		}
		OBB val = default(OBB);
		((OBB)(ref val))._002Ector(bounds);
		if (!canMove)
		{
			Navmesh.FillDebugDrawProto(navMeshData, ((OBB)(ref val)).ToBounds());
			return true;
		}
		Matrix4x4 worldToNavMatrix = WorldToNavMatrix;
		((OBB)(ref val)).Transform(((Matrix4x4)(ref worldToNavMatrix)).GetPosition(), ((Matrix4x4)(ref worldToNavMatrix)).lossyScale, ((Matrix4x4)(ref worldToNavMatrix)).rotation);
		Navmesh.FillDebugDrawProto(navMeshData, ((OBB)(ref val)).ToBounds(), ((Matrix4x4)(ref worldToNavMatrix)).inverse);
		return true;
	}

	public static IndependantNavmesh FindNavmeshAtPosition(Vector3 worldPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return null;
		}
		PooledHashSet<IndependantNavmesh> val = Pool.Get<PooledHashSet<IndependantNavmesh>>();
		try
		{
			navmeshLookup.FindAll(new Bounds(worldPosition, Vector3.one), (HashSet<IndependantNavmesh>)(object)val);
			foreach (IndependantNavmesh item in (HashSet<IndependantNavmesh>)(object)val)
			{
				if (item.Navmesh != null && item.Navmesh.IsValid())
				{
					Bounds bounds = item.GetBounds();
					if (((Bounds)(ref bounds)).Contains(worldPosition))
					{
						return item;
					}
				}
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void FindNavmeshesInBounds(Bounds bounds, List<IndependantNavmesh> results)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return;
		}
		PooledHashSet<IndependantNavmesh> val = Pool.Get<PooledHashSet<IndependantNavmesh>>();
		try
		{
			navmeshLookup.FindAll(bounds, (HashSet<IndependantNavmesh>)(object)val);
			foreach (IndependantNavmesh item in (HashSet<IndependantNavmesh>)(object)val)
			{
				results.Add(item);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private Bounds GetBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		OBB val = new OBB(((Component)this).transform.position, size, ((Component)this).transform.rotation);
		return ((OBB)(ref val)).ToBounds();
	}

	public IndependantNavmesh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		size = Vector3.one * 50f;
		((MonoBehaviour)this)._002Ector();
	}
}
