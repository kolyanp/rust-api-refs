using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class TubeRenderer : FacepunchBehaviour
{
	[Range(3f, 64f)]
	[Header("Settings")]
	public int Segments = 12;

	public float Radius = 0.1f;

	public bool useLocalPositions;

	[Header("Caps")]
	public bool EnableCaps = true;

	[Range(1f, 8f)]
	public int HemisphereRings = 4;

	[NonSerialized]
	public List<Vector3> points = new List<Vector3>();

	private Mesh mesh;

	public void ClearPositions()
	{
		points.Clear();
	}

	public void SetPosition(int index, Vector3 position)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (index >= 0 && index < points.Count)
		{
			points[index] = position;
		}
	}

	public void SetPositions(List<Vector3> positions)
	{
		points.Clear();
		points.AddRange(positions);
	}

	public void UpdateRenderer()
	{
		GenerateTube(points, Radius, Segments, HemisphereRings);
	}

	private void SetupMesh()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		mesh = new Mesh
		{
			name = "Tube Mesh"
		};
		mesh.MarkDynamic();
		MeshFilter val = ((Component)this).GetComponent<MeshFilter>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)this).gameObject.AddComponent<MeshFilter>();
		}
		val.mesh = mesh;
		if ((Object)(object)((Component)this).GetComponent<MeshRenderer>() == (Object)null)
		{
			((Component)this).gameObject.AddComponent<MeshRenderer>();
		}
	}

	private void GenerateTube(List<Vector3> points, float radius, int segments, int rings)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (points == null || points.Count < 2)
		{
			return;
		}
		using (TimeWarning.New("TubeRenderer.GenerateTube"))
		{
			if ((Object)(object)mesh == (Object)null)
			{
				SetupMesh();
			}
			List<Vector3> list = Pool.Get<List<Vector3>>();
			List<int> list2 = Pool.Get<List<int>>();
			List<Vector2> list3 = Pool.Get<List<Vector2>>();
			List<Quaternion> list4 = Pool.Get<List<Quaternion>>();
			List<float> list5 = Pool.Get<List<float>>();
			int vertOffset = 0;
			ComputeParallelTransportFrames(points, list4);
			list5.Add(0f);
			float num = 0f;
			for (int i = 1; i < points.Count; i++)
			{
				float num2 = Vector3.Distance(points[i - 1], points[i]);
				num += num2;
				list5.Add(num);
			}
			for (int j = 0; j < points.Count; j++)
			{
				Vector3 center = (useLocalPositions ? points[j] : ((Component)this).transform.InverseTransformPoint(points[j]));
				float v = ((num > 0f) ? (list5[j] / num) : 0f);
				AppendRing(list, list3, list4[j], center, radius, segments, v);
				if (j > 0)
				{
					BridgeLastTwoRings(list2, vertOffset, segments);
				}
				vertOffset += segments + 1;
			}
			if (EnableCaps)
			{
				AppendHemisphereCap(list, list2, list3, ref vertOffset, points[0], list4[0], radius, segments, rings, -1);
				List<Vector3> verts = list;
				List<int> tris = list2;
				List<Vector2> uvs = list3;
				Vector3 position = points[points.Count - 1];
				List<Quaternion> list6 = list4;
				AppendHemisphereCap(verts, tris, uvs, ref vertOffset, position, list6[list6.Count - 1], radius, segments, rings, 1);
			}
			else
			{
				AppendFlatCap(list, list2, list3, ref vertOffset, points[0], list4[0], radius, segments, -1);
				List<Vector3> verts2 = list;
				List<int> tris2 = list2;
				List<Vector2> uvs2 = list3;
				Vector3 position2 = points[points.Count - 1];
				List<Quaternion> list7 = list4;
				AppendFlatCap(verts2, tris2, uvs2, ref vertOffset, position2, list7[list7.Count - 1], radius, segments, 1);
			}
			mesh.Clear();
			mesh.SetVertices(list);
			mesh.SetUVs(0, list3);
			mesh.SetTriangles(list2, 0);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			Pool.FreeUnmanaged<Vector3>(ref list);
			Pool.FreeUnmanaged<int>(ref list2);
			Pool.FreeUnmanaged<Quaternion>(ref list4);
			Pool.FreeUnmanaged<Vector2>(ref list3);
			Pool.FreeUnmanaged<float>(ref list5);
		}
	}

	private void ComputeParallelTransportFrames(List<Vector3> points, List<Quaternion> rotations)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		rotations.Clear();
		if (points.Count >= 2)
		{
			Vector3 val = points[1] - points[0];
			Vector3 val2 = ((Vector3)(ref val)).normalized;
			Vector3 val3 = ((Mathf.Abs(Vector3.Dot(val2, Vector3.up)) < 0.99f) ? Vector3.up : Vector3.right);
			val = Vector3.Cross(val2, val3);
			Vector3 val4 = ((Vector3)(ref val)).normalized;
			val = Vector3.Cross(val2, val4);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			rotations.Add(Quaternion.LookRotation(val2, normalized));
			for (int i = 1; i < points.Count; i++)
			{
				val = points[i] - points[i - 1];
				Vector3 normalized2 = ((Vector3)(ref val)).normalized;
				Vector3 val5 = Quaternion.FromToRotation(val2, normalized2) * val4;
				val = Vector3.Cross(normalized2, val5);
				Vector3 normalized3 = ((Vector3)(ref val)).normalized;
				rotations.Add(Quaternion.LookRotation(normalized2, normalized3));
				val2 = normalized2;
				val4 = val5;
			}
		}
	}

	private void AppendRing(List<Vector3> verts, List<Vector2> uvs, Quaternion rotation, Vector3 center, float radius, int segments, float v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = rotation * Vector3.right;
		Vector3 val2 = rotation * Vector3.up;
		for (int i = 0; i <= segments; i++)
		{
			float num = (float)i / (float)segments * MathF.PI * 2f;
			Vector3 val3 = Mathf.Cos(num) * val + Mathf.Sin(num) * val2;
			verts.Add(center + val3 * radius);
			uvs.Add(new Vector2((float)i / (float)segments, v));
		}
	}

	private void BridgeLastTwoRings(List<int> tris, int vertOffset, int segments)
	{
		int num = vertOffset - (segments + 1);
		for (int i = 0; i < segments; i++)
		{
			int item = num + i;
			int item2 = num + i + 1;
			int item3 = vertOffset + i;
			int item4 = vertOffset + i + 1;
			tris.Add(item);
			tris.Add(item2);
			tris.Add(item3);
			tris.Add(item2);
			tris.Add(item4);
			tris.Add(item3);
		}
	}

	private void AppendFlatCap(List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int vertOffset, Vector3 position, Quaternion rotation, float radius, int segments, int direction)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		position = (useLocalPositions ? position : ((Component)this).transform.InverseTransformPoint(position));
		Vector3 val = rotation * Vector3.right;
		Vector3 val2 = rotation * Vector3.up;
		int count = verts.Count;
		verts.Add(position);
		uvs.Add(new Vector2(0.5f, 0.5f));
		for (int i = 0; i <= segments; i++)
		{
			float num = (float)i / (float)segments * MathF.PI * 2f;
			Vector3 val3 = Mathf.Cos(num) * val + Mathf.Sin(num) * val2;
			verts.Add(position + val3 * radius);
			float num2 = Mathf.Cos(num) * 0.5f + 0.5f;
			float num3 = Mathf.Sin(num) * 0.5f + 0.5f;
			uvs.Add(new Vector2(num2, num3));
		}
		for (int j = 0; j < segments; j++)
		{
			int item = count + j + 1;
			int item2 = count + j + 2;
			if (direction == 1)
			{
				tris.Add(count);
				tris.Add(item);
				tris.Add(item2);
			}
			else
			{
				tris.Add(count);
				tris.Add(item2);
				tris.Add(item);
			}
		}
		vertOffset += segments + 2;
	}

	private void AppendHemisphereCap(List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int vertOffset, Vector3 position, Quaternion rotation, float radius, int segments, int rings, int direction)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		position = (useLocalPositions ? position : ((Component)this).transform.InverseTransformPoint(position));
		List<Vector3> list = Pool.Get<List<Vector3>>();
		List<Vector2> list2 = Pool.Get<List<Vector2>>();
		Vector3 val = default(Vector3);
		for (int i = 0; i <= rings; i++)
		{
			float num = (float)i / (float)rings * MathF.PI / 2f;
			for (int j = 0; j <= segments; j++)
			{
				float num2 = (float)j / (float)segments * MathF.PI * 2f;
				float num3 = Mathf.Sin(num);
				((Vector3)(ref val))._002Ector(Mathf.Cos(num2) * num3, Mathf.Sin(num2) * num3, Mathf.Cos(num) * (float)direction);
				Vector3 item = rotation * (val * radius) + position;
				list.Add(item);
				float num4 = Mathf.Cos(num2) * num3 * 0.5f + 0.5f;
				float num5 = Mathf.Sin(num2) * num3 * 0.5f + 0.5f;
				list2.Add(new Vector2(num4, num5));
			}
		}
		int count = verts.Count;
		verts.AddRange(list);
		uvs.AddRange(list2);
		for (int k = 0; k < rings; k++)
		{
			for (int l = 0; l < segments; l++)
			{
				int num6 = count + k * (segments + 1) + l;
				int item2 = num6 + 1;
				int num7 = num6 + (segments + 1);
				int item3 = num7 + 1;
				if (direction == 0)
				{
					tris.Add(num6);
					tris.Add(item2);
					tris.Add(num7);
					tris.Add(item2);
					tris.Add(item3);
					tris.Add(num7);
				}
				else
				{
					tris.Add(num6);
					tris.Add(num7);
					tris.Add(item2);
					tris.Add(item2);
					tris.Add(num7);
					tris.Add(item3);
				}
			}
		}
		vertOffset += list.Count;
		Pool.FreeUnmanaged<Vector3>(ref list);
		Pool.FreeUnmanaged<Vector2>(ref list2);
	}
}
