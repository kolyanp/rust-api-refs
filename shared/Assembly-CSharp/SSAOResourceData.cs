using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class SSAOResourceData : ContextItem
{
	public uint SampleStep;

	public TextureHandle OcclusionDepthHandle;

	public Matrix4x4 InvViewProjLeft;

	public Matrix4x4 PrevViewProjLeft;

	public Matrix4x4 PrevInvViewProjLeft;

	public float TemporalDirections;

	public float TemporalOffsets;

	private static Mesh s_quadMesh;

	public static Mesh FullscreenQuad
	{
		get
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)s_quadMesh != (Object)null)
			{
				return s_quadMesh;
			}
			Mesh val = new Mesh();
			((Object)val).hideFlags = (HideFlags)52;
			((Object)val).name = "SSAO Fullscreen Quad";
			val.vertices = (Vector3[])(object)new Vector3[4]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 1f, 0f),
				new Vector3(1f, 0f, 0f)
			};
			val.uv = (Vector2[])(object)new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			};
			val.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
			s_quadMesh = val;
			s_quadMesh.normals = Array.Empty<Vector3>();
			s_quadMesh.tangents = Array.Empty<Vector4>();
			return s_quadMesh;
		}
	}

	public override void Reset()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		OcclusionDepthHandle = TextureHandle.nullHandle;
		InvViewProjLeft = Matrix4x4.identity;
		PrevViewProjLeft = Matrix4x4.identity;
		PrevInvViewProjLeft = Matrix4x4.identity;
		TemporalDirections = 0f;
		TemporalOffsets = 0f;
	}
}
