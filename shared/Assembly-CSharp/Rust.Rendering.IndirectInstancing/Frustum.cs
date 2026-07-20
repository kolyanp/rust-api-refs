using System;
using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

internal struct Frustum
{
	public Plane left;

	public Plane right;

	public Plane down;

	public Plane up;

	public Plane near;

	public Plane far;

	[ThreadStatic]
	private static Plane[] reusable_plane_array;

	public Frustum(Camera camera)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		GeometryUtility.CalculateFrustumPlanes(camera, reusable_plane_array);
		left = reusable_plane_array[0];
		right = reusable_plane_array[1];
		down = reusable_plane_array[2];
		up = reusable_plane_array[3];
		near = reusable_plane_array[4];
		far = reusable_plane_array[5];
	}

	static Frustum()
	{
		reusable_plane_array = (Plane[])(object)new Plane[6];
	}

	public static implicit operator Rust.Rendering.IndirectInstancing.Frustum(Camera camera)
	{
		return new Rust.Rendering.IndirectInstancing.Frustum(camera);
	}
}
