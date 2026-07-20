using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Facepunch.NativeMeshSimplification;

[BurstCompile]
internal struct SimplifyMeshJob : IJob
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct ProfilerMarkerStub
	{
		[BurstDiscard]
		public void Begin()
		{
		}

		[BurstDiscard]
		public void End()
		{
		}
	}

	public int MaxIterations;

	public float ReductionTarget;

	public int Aggressiveness;

	public NativeList<NativeMeshSimplifier.Triangle> Triangles;

	public NativeList<NativeMeshSimplifier.Vertex> Vertices;

	public NativeList<NativeMeshSimplifier.Ref> Refs;

	private static readonly ProfilerMarkerStub k_Iteration;

	private static readonly ProfilerMarkerStub k_UpdateMesh;

	private static readonly ProfilerMarkerStub k_FirstUpdate;

	private static readonly ProfilerMarkerStub k_CompactMesh;

	private static readonly ProfilerMarkerStub k_Flipped;

	private static readonly ProfilerMarkerStub k_UpdateTriangles;

	private static readonly ProfilerMarkerStub k_MemCpy;

	public void Execute()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		Refs.Clear();
		NativeList<int> deleted = default(NativeList<int>);
		deleted._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
		NativeList<int> deleted2 = default(NativeList<int>);
		deleted2._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
		int deletedTriangles = 0;
		int length = Triangles.Length;
		int num = (int)((float)length * ReductionTarget);
		NativeSlice<NativeMeshSimplifier.Ref> val2 = default(NativeSlice<NativeMeshSimplifier.Ref>);
		NativeSlice<NativeMeshSimplifier.Ref> val3 = default(NativeSlice<NativeMeshSimplifier.Ref>);
		for (int i = 0; i < MaxIterations; i++)
		{
			k_Iteration.Begin();
			if (length - deletedTriangles <= num)
			{
				k_Iteration.End();
				break;
			}
			if (i % 5 == 0)
			{
				k_UpdateMesh.Begin();
				UpdateMesh(i);
				k_UpdateMesh.End();
			}
			for (int j = 0; j < Triangles.Length; j++)
			{
				NativeListAccessExtensions.Get(in Triangles, j).dirty = false;
			}
			float num2 = 1E-09f * math.pow((float)i + 3f, (float)Aggressiveness);
			for (int k = 0; k < Triangles.Length; k++)
			{
				ref readonly NativeMeshSimplifier.Triangle reference = ref NativeListAccessExtensions.GetReadonly(in Triangles, k);
				float4 err = reference.err;
				if (math.any(new bool3(((float4)(ref err))[3] > num2, reference.deleted, reference.dirty)))
				{
					continue;
				}
				for (int l = 0; l < 3; l++)
				{
					err = reference.err;
					if (((float4)(ref err))[l] > num2)
					{
						continue;
					}
					int3 vIndex = reference.vIndex;
					int num3 = ((int3)(ref vIndex))[l];
					vIndex = reference.vIndex;
					int num4 = ((int3)(ref vIndex))[(l + 1) % 3];
					ref NativeMeshSimplifier.Vertex reference2 = ref NativeListAccessExtensions.Get(in Vertices, num3);
					ref readonly NativeMeshSimplifier.Vertex reference3 = ref NativeListAccessExtensions.GetReadonly(in Vertices, num4);
					if (reference2.border != reference3.border)
					{
						continue;
					}
					CalculateError(in reference2, in reference3, out var pResult);
					deleted.Length = reference2.tCount;
					deleted2.Length = reference3.tCount;
					if (Flipped(pResult, num4, in reference2, ref deleted) || Flipped(pResult, num3, in reference3, ref deleted2))
					{
						continue;
					}
					reference2.p = pResult;
					reference2.q = reference3.q + reference2.q;
					int length2 = Refs.Length;
					k_UpdateTriangles.Begin();
					UpdateTriangles(num3, in reference2, in deleted, ref deletedTriangles);
					UpdateTriangles(num3, in reference3, in deleted2, ref deletedTriangles);
					k_UpdateTriangles.End();
					int num5 = Refs.Length - length2;
					if (num5 <= reference2.tCount)
					{
						if (num5 > 0)
						{
							k_MemCpy.Begin();
							NativeArray<NativeMeshSimplifier.Ref> val = Refs.AsArray();
							val2._002Ector(val, reference2.tStart, num5);
							val3._002Ector(val, length2, num5);
							val2.CopyFrom(val3);
							k_MemCpy.End();
						}
					}
					else
					{
						reference2.tStart = length2;
					}
					reference2.tCount = num5;
					break;
				}
				if (length - deletedTriangles <= num)
				{
					break;
				}
			}
			k_Iteration.End();
		}
		k_CompactMesh.Begin();
		CompactMesh();
		k_CompactMesh.End();
		deleted.Dispose();
		deleted2.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float VertexError(in NativeMeshSimplifier.SymmetricMatrix q, in float3 v)
	{
		float x = v.x;
		float y = v.y;
		float z = v.z;
		return q[0] * x * x + 2f * q[1] * x * y + 2f * q[2] * x * z + 2f * q[3] * x + q[4] * y * y + 2f * q[5] * y * z + 2f * q[6] * y + q[7] * z * z + 2f * q[8] * z + q[9];
	}

	private float CalculateError(int idV1, int idV2)
	{
		float3 pResult;
		return CalculateError(in NativeListAccessExtensions.Get(in Vertices, idV1), in NativeListAccessExtensions.Get(in Vertices, idV2), out pResult);
	}

	private float CalculateError(in NativeMeshSimplifier.Vertex v1, in NativeMeshSimplifier.Vertex v2, out float3 pResult)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		pResult = default(float3);
		NativeMeshSimplifier.SymmetricMatrix q = v1.q + v2.q;
		bool flag = v1.border & v2.border;
		float num = 0f;
		float num2 = q.Det(0, 1, 2, 1, 4, 5, 2, 5, 7);
		if (num2 != 0f && !flag)
		{
			float num3 = math.rcp(num2);
			pResult.x = (0f - num3) * q.Det(1, 2, 3, 4, 5, 6, 5, 7, 8);
			pResult.y = num3 * q.Det(0, 2, 3, 1, 5, 6, 2, 7, 8);
			pResult.z = (0f - num3) * q.Det(0, 1, 3, 1, 4, 6, 2, 5, 8);
			num = VertexError(in q, in pResult);
		}
		else
		{
			float3 v3 = v1.p;
			float3 v4 = v2.p;
			float3 v5 = (v3 + v4) * 0.5f;
			float num4 = VertexError(in q, in v3);
			float num5 = VertexError(in q, in v4);
			float num6 = VertexError(in q, in v5);
			num = math.min(num4, math.min(num5, num6));
			if (num4 == num)
			{
				pResult = v3;
			}
			if (num5 == num)
			{
				pResult = v4;
			}
			if (num6 == num)
			{
				pResult = v5;
			}
		}
		return num;
	}

	private bool Flipped(float3 p, int i1, in NativeMeshSimplifier.Vertex v0, ref NativeList<int> deleted)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		k_Flipped.Begin();
		for (int j = 0; j < v0.tCount; j++)
		{
			ref readonly NativeMeshSimplifier.Ref reference = ref NativeListAccessExtensions.GetReadonly(in Refs, v0.tStart + j);
			ref readonly NativeMeshSimplifier.Triangle reference2 = ref NativeListAccessExtensions.GetReadonly(in Triangles, reference.tId);
			if (reference2.deleted)
			{
				continue;
			}
			int tVertex = reference.tVertex;
			int3 vIndex = reference2.vIndex;
			int num = ((int3)(ref vIndex))[(tVertex + 1) % 3];
			vIndex = reference2.vIndex;
			int num2 = ((int3)(ref vIndex))[(tVertex + 2) % 3];
			if (num == i1 || num2 == i1)
			{
				deleted[j] = 1;
				continue;
			}
			float3 val = Vertices[num].p - p;
			float3 val2 = Vertices[num2].p - p;
			float3 val3 = math.cross(val, val2);
			if (math.lengthsq(val3) == 0f)
			{
				k_Flipped.End();
				return true;
			}
			float3 val4 = math.normalize(val3);
			deleted[j] = 0;
			if (math.dot(val4, reference2.n) < 0.2f)
			{
				k_Flipped.End();
				return true;
			}
		}
		k_Flipped.End();
		return false;
	}

	private void UpdateTriangles(int i0, in NativeMeshSimplifier.Vertex v, in NativeList<int> deleted, ref int deletedTriangles)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		for (int j = 0; j < v.tCount; j++)
		{
			ref readonly NativeMeshSimplifier.Ref reference = ref NativeListAccessExtensions.GetReadonly(in Refs, v.tStart + j);
			ref NativeMeshSimplifier.Triangle reference2 = ref NativeListAccessExtensions.Get(in Triangles, reference.tId);
			if (!reference2.deleted)
			{
				if (deleted[j] > 0)
				{
					reference2.deleted = true;
					deletedTriangles++;
					continue;
				}
				ref readonly NativeMeshSimplifier.Vertex reference3 = ref NativeListAccessExtensions.GetReadonly(in Vertices, ((int3)(ref reference2.vIndex))[0]);
				ref readonly NativeMeshSimplifier.Vertex reference4 = ref NativeListAccessExtensions.GetReadonly(in Vertices, ((int3)(ref reference2.vIndex))[1]);
				ref readonly NativeMeshSimplifier.Vertex reference5 = ref NativeListAccessExtensions.GetReadonly(in Vertices, ((int3)(ref reference2.vIndex))[2]);
				((int3)(ref reference2.vIndex))[reference.tVertex] = i0;
				reference2.dirty = true;
				((float4)(ref reference2.err))[0] = CalculateError(in reference3, in reference4, out var pResult);
				((float4)(ref reference2.err))[1] = CalculateError(in reference4, in reference5, out pResult);
				((float4)(ref reference2.err))[2] = CalculateError(in reference5, in reference3, out pResult);
				((float4)(ref reference2.err))[3] = math.cmin(((float4)(ref reference2.err)).xyz);
				Refs.Add(ref reference);
			}
		}
	}

	private void UpdateMesh(int iteration)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = iteration == 0;
		if (!flag)
		{
			int length = 0;
			for (int i = 0; i < Triangles.Length; i++)
			{
				ref readonly NativeMeshSimplifier.Triangle reference = ref NativeListAccessExtensions.GetReadonly(in Triangles, i);
				if (!reference.deleted)
				{
					Triangles[length++] = reference;
				}
			}
			Triangles.Length = length;
		}
		for (int j = 0; j < Vertices.Length; j++)
		{
			ref NativeMeshSimplifier.Vertex reference2 = ref NativeListAccessExtensions.Get(in Vertices, j);
			reference2.tStart = 0;
			reference2.tCount = 0;
		}
		int3 vIndex;
		for (int k = 0; k < Triangles.Length; k++)
		{
			ref readonly NativeMeshSimplifier.Triangle reference3 = ref NativeListAccessExtensions.GetReadonly(in Triangles, k);
			for (int l = 0; l < 3; l++)
			{
				ref NativeList<NativeMeshSimplifier.Vertex> vertices = ref Vertices;
				vIndex = reference3.vIndex;
				NativeListAccessExtensions.Get(in vertices, ((int3)(ref vIndex))[l]).tCount++;
			}
		}
		int num = 0;
		for (int m = 0; m < Vertices.Length; m++)
		{
			ref NativeMeshSimplifier.Vertex reference4 = ref NativeListAccessExtensions.Get(in Vertices, m);
			reference4.tStart = num;
			num += reference4.tCount;
			reference4.tCount = 0;
		}
		Refs.Length = Triangles.Length * 3;
		for (int n = 0; n < Triangles.Length; n++)
		{
			ref readonly NativeMeshSimplifier.Triangle reference5 = ref NativeListAccessExtensions.GetReadonly(in Triangles, n);
			for (int num2 = 0; num2 < 3; num2++)
			{
				ref NativeList<NativeMeshSimplifier.Vertex> vertices2 = ref Vertices;
				vIndex = reference5.vIndex;
				ref NativeMeshSimplifier.Vertex reference6 = ref NativeListAccessExtensions.Get(in vertices2, ((int3)(ref vIndex))[num2]);
				ref NativeMeshSimplifier.Ref reference7 = ref NativeListAccessExtensions.Get(in Refs, reference6.tStart + reference6.tCount);
				reference7.tId = n;
				reference7.tVertex = num2;
				reference6.tCount++;
			}
		}
		if (!flag)
		{
			return;
		}
		k_FirstUpdate.Begin();
		NativeList<int> list = default(NativeList<int>);
		list._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
		NativeList<int> val = default(NativeList<int>);
		val._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
		for (int num3 = 0; num3 < Vertices.Length; num3++)
		{
			ref readonly NativeMeshSimplifier.Vertex reference8 = ref NativeListAccessExtensions.GetReadonly(in Vertices, num3);
			list.Clear();
			val.Clear();
			for (int num4 = 0; num4 < reference8.tCount; num4++)
			{
				ref readonly NativeMeshSimplifier.Triangle reference9 = ref NativeListAccessExtensions.GetReadonly(in Triangles, Refs[reference8.tStart + num4].tId);
				for (int num5 = 0; num5 < 3; num5++)
				{
					int num6 = 0;
					vIndex = reference9.vIndex;
					int num7;
					for (num7 = ((int3)(ref vIndex))[num5]; num6 < list.Length && val[num6] != num7; num6++)
					{
					}
					if (num6 == list.Length)
					{
						int num8 = 1;
						list.Add(ref num8);
						val.Add(ref num7);
					}
					else
					{
						NativeListAccessExtensions.Get(in list, num6)++;
					}
				}
			}
			for (int num9 = 0; num9 < list.Length; num9++)
			{
				if (list[num9] == 1)
				{
					NativeListAccessExtensions.Get(in Vertices, val[num9]).border = true;
				}
			}
		}
		list.Dispose();
		val.Dispose();
		float3 val2 = default(float3);
		float3x3 val3 = default(float3x3);
		for (int num10 = 0; num10 < Triangles.Length; num10++)
		{
			ref NativeMeshSimplifier.Triangle reference10 = ref NativeListAccessExtensions.Get(in Triangles, num10);
			for (int num11 = 0; num11 < 3; num11++)
			{
				ref readonly NativeMeshSimplifier.Vertex reference11 = ref NativeListAccessExtensions.GetReadonly(in Vertices, ((int3)(ref reference10.vIndex))[num11]);
				((float3x3)(ref val3))[num11] = reference11.p;
			}
			val2 = (reference10.n = math.normalizesafe(math.cross(((float3x3)(ref val3))[1] - ((float3x3)(ref val3))[0], ((float3x3)(ref val3))[2] - ((float3x3)(ref val3))[0]), math.right()));
			for (int num12 = 0; num12 < 3; num12++)
			{
				NativeListAccessExtensions.Get(in Vertices, ((int3)(ref reference10.vIndex))[num12]).q += NativeMeshSimplifier.SymmetricMatrix.Plane(val2.x, val2.y, val2.z, 0f - math.dot(val2, ((float3x3)(ref val3))[0]));
			}
		}
		for (int num13 = 0; num13 < Triangles.Length; num13++)
		{
			ref NativeMeshSimplifier.Triangle reference12 = ref NativeListAccessExtensions.Get(in Triangles, num13);
			for (int num14 = 0; num14 < 3; num14++)
			{
				((float4)(ref reference12.err))[num14] = CalculateError(((int3)(ref reference12.vIndex))[num14], ((int3)(ref reference12.vIndex))[(num14 + 1) % 3]);
			}
			((float4)(ref reference12.err))[3] = math.cmin(((float4)(ref reference12.err)).xyz);
		}
		k_FirstUpdate.End();
	}

	private void CompactMesh()
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Vertices.Length; i++)
		{
			NativeListAccessExtensions.Get(in Vertices, i).tCount = 0;
		}
		int length = 0;
		for (int j = 0; j < Triangles.Length; j++)
		{
			ref readonly NativeMeshSimplifier.Triangle reference = ref NativeListAccessExtensions.GetReadonly(in Triangles, j);
			if (!reference.deleted)
			{
				Triangles[length++] = reference;
				for (int k = 0; k < 3; k++)
				{
					ref NativeList<NativeMeshSimplifier.Vertex> vertices = ref Vertices;
					int3 vIndex = reference.vIndex;
					NativeListAccessExtensions.Get(in vertices, ((int3)(ref vIndex))[k]).tCount = 1;
				}
			}
		}
		Triangles.Length = length;
		length = 0;
		for (int l = 0; l < Vertices.Length; l++)
		{
			ref NativeMeshSimplifier.Vertex reference2 = ref NativeListAccessExtensions.Get(in Vertices, l);
			if (reference2.tCount != 0)
			{
				reference2.tStart = length;
				NativeListAccessExtensions.Get(in Vertices, length).p = reference2.p;
				length++;
			}
		}
		for (int m = 0; m < Triangles.Length; m++)
		{
			ref NativeMeshSimplifier.Triangle reference3 = ref NativeListAccessExtensions.Get(in Triangles, m);
			for (int n = 0; n < 3; n++)
			{
				((int3)(ref reference3.vIndex))[n] = Vertices[((int3)(ref reference3.vIndex))[n]].tStart;
			}
		}
		Vertices.Length = length;
	}
}
