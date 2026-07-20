using System;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class VertexNormalDebugger : MonoBehaviour
{
	private static readonly int positionsBufferId = Shader.PropertyToID("_VertexPositionsBuffer");

	private static readonly int modelMatrixId = Shader.PropertyToID("_ModelMatrix");

	private static readonly int invModelMatrixId = Shader.PropertyToID("_InvModelMatrix");

	[SerializeField]
	private Mesh mesh;

	[SerializeField]
	private Material material;

	private GraphicsBuffer vertexPositionsBuffer;

	private Vector3[] vertexPositions;

	private void OnEnable()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mesh == (Object)null) && !((Object)(object)material == (Object)null))
		{
			vertexPositionsBuffer = new GraphicsBuffer((Target)16, (UsageFlags)0, mesh.vertexCount * 2, 12);
			vertexPositions = (Vector3[])(object)new Vector3[mesh.vertexCount * 2];
			for (int i = 0; i < vertexPositions.Length; i += 2)
			{
				int num = i / 2;
				vertexPositions[i] = mesh.vertices[num];
				vertexPositions[i + 1] = mesh.vertices[num] + mesh.normals[num];
			}
		}
	}

	private void LateUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		vertexPositionsBuffer.SetData((Array)vertexPositions);
		Shader.SetGlobalBuffer(positionsBufferId, vertexPositionsBuffer);
		Shader.SetGlobalMatrix(modelMatrixId, ((Component)this).transform.localToWorldMatrix);
		Shader.SetGlobalMatrix(invModelMatrixId, ((Component)this).transform.worldToLocalMatrix);
		Graphics.DrawProcedural(material, new Bounds(Vector3.zero, Vector3.one), (MeshTopology)3, mesh.vertexCount * 2, 1, (Camera)null, (MaterialPropertyBlock)null, (ShadowCastingMode)1, true, 0);
	}

	private void OnDisable()
	{
		GraphicsBuffer obj = vertexPositionsBuffer;
		if (obj != null)
		{
			obj.Dispose();
		}
		vertexPositionsBuffer = null;
		vertexPositions = null;
	}
}
