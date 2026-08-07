using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[BurstCompile]
internal static class SDFChunkJobs
{
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal struct CleanupIslandsJob : IJob
	{
		public QuantizedFloatData3DArray DataArray;

		public float Iso;

		public void Execute()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_0181: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			int length = DataArray.FlatArray.Length;
			NativeArray<byte> touched = default(NativeArray<byte>);
			touched._002Ector(length, (Allocator)2, (NativeArrayOptions)1);
			NativeArray<int4> queue = default(NativeArray<int4>);
			queue._002Ector(length, (Allocator)2, (NativeArrayOptions)0);
			int num = 0;
			int tail = 0;
			int num2 = 1;
			int num3 = DataArray.Width - 2;
			int num4 = 1;
			int num5 = DataArray.Height - 2;
			int num6 = 1;
			int num7 = DataArray.Depth - 2;
			for (int i = num6; i <= num7; i++)
			{
				for (int j = num2; j <= num3; j++)
				{
					int num8 = DataArray.ToIndex(j, 0, i);
					touched[num8] = 1;
					queue[tail++] = new int4(j, 0, i, num8);
				}
			}
			int width = DataArray.Width;
			int widthHeight = DataArray.WidthHeight;
			while (num < tail)
			{
				int4 val = queue[num++];
				int w = val.w;
				if (val.x > num2)
				{
					TryTouch(w - 1, val.x - 1, val.y, val.z, touched, queue, ref tail);
				}
				if (val.x < num3)
				{
					TryTouch(w + 1, val.x + 1, val.y, val.z, touched, queue, ref tail);
				}
				if (val.y > num4)
				{
					TryTouch(w - width, val.x, val.y - 1, val.z, touched, queue, ref tail);
				}
				if (val.y < num5)
				{
					TryTouch(w + width, val.x, val.y + 1, val.z, touched, queue, ref tail);
				}
				if (val.z > num6)
				{
					TryTouch(w - widthHeight, val.x, val.y, val.z - 1, touched, queue, ref tail);
				}
				if (val.z < num7)
				{
					TryTouch(w + widthHeight, val.x, val.y, val.z + 1, touched, queue, ref tail);
				}
			}
			for (int k = 0; k < length; k++)
			{
				if (touched[k] == 0)
				{
					DataArray.FlatArray[k] = byte.MaxValue;
				}
			}
			queue.Dispose();
			touched.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void TryTouch(int idx, int x, int y, int z, NativeArray<byte> touched, NativeArray<int4> queue, ref int tail)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (touched[idx] == 0)
			{
				touched[idx] = 1;
				if (DataArray.Sample(idx) < Iso)
				{
					queue[tail++] = new int4(x, y, z, idx);
				}
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal struct AccumulateCensorBoundsJob : IJobParallelForBatch
	{
		[NativeDisableContainerSafetyRestriction]
		public QuantizedFloatData3DArray SrcData;

		public Writer ShapeStream;

		public int SegmentsX;

		public int SegmentsY;

		public int SegmentsZ;

		public float iso;

		public int batchSize;

		public void Execute(int startIndex, int count)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0200: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0221: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0288: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02be: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0171: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0323: Unknown result type (might be due to invalid IL or missing references)
			//IL_0327: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			//IL_0330: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0339: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0347: Unknown result type (might be due to invalid IL or missing references)
			//IL_0349: Unknown result type (might be due to invalid IL or missing references)
			//IL_034b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0350: Unknown result type (might be due to invalid IL or missing references)
			//IL_0352: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Unknown result type (might be due to invalid IL or missing references)
			//IL_0356: Unknown result type (might be due to invalid IL or missing references)
			//IL_035b: Unknown result type (might be due to invalid IL or missing references)
			//IL_036e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_0372: Unknown result type (might be due to invalid IL or missing references)
			//IL_037c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_0383: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_0387: Unknown result type (might be due to invalid IL or missing references)
			//IL_0391: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Unknown result type (might be due to invalid IL or missing references)
			//IL_0398: Unknown result type (might be due to invalid IL or missing references)
			//IL_039a: Unknown result type (might be due to invalid IL or missing references)
			//IL_039c: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_03af: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_03de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			int num = startIndex / batchSize;
			((Writer)(ref ShapeStream)).PatchMinMaxRange(num);
			((Writer)(ref ShapeStream)).BeginForEachIndex(num);
			NativeList<int3> val = default(NativeList<int3>);
			val._002Ector(AllocatorHandle.op_Implicit((Allocator)2));
			int num2 = SrcData.Width / SegmentsX;
			int num3 = SrcData.Height / SegmentsY;
			int num4 = SrcData.Depth / SegmentsZ;
			int3 val2 = default(int3);
			((int3)(ref val2))._002Ector(num2, num3, num4);
			int3 val3 = default(int3);
			((int3)(ref val3))._002Ector(1);
			int num5 = SegmentsX * SegmentsY;
			int3 val6 = default(int3);
			int3 val7 = default(int3);
			int3 val9 = default(int3);
			float3 val15 = default(float3);
			float3 val16 = default(float3);
			float3 val18 = default(float3);
			for (int i = startIndex; i < startIndex + count; i++)
			{
				int num6 = i % SegmentsX;
				int num7 = i / SegmentsX % SegmentsY;
				int num8 = i / num5;
				int3 val4 = math.max(val2 * new int3(num6, num7, num8) - val3, int3.op_Implicit(0));
				int3 val5 = math.min(val2 * new int3(num6, num7, num8) + val2 + val3, SrcData.Bounds - 1);
				val.Clear();
				((int3)(ref val6))._002Ector(int.MaxValue);
				((int3)(ref val7))._002Ector(int.MinValue);
				int3 val8 = int3.zero;
				for (int j = val4.z; j <= val5.z; j++)
				{
					for (int k = val4.y; k <= val5.y; k++)
					{
						for (int l = val4.x; l <= val5.x; l++)
						{
							((int3)(ref val9))._002Ector(l, k, j);
							if (SrcData.Sample(val9) < iso)
							{
								val.Add(ref val9);
								val6 = math.min(val6, val9);
								val7 = math.max(val7, val9);
								val8 += val9;
							}
						}
					}
				}
				if (val.Length >= 3)
				{
					float3 val10 = float3.op_Implicit(val8 / val.Length);
					float3x3 zero = float3x3.zero;
					for (int m = 0; m < val.Length; m++)
					{
						float3 val11 = float3.op_Implicit(val[m]) - val10;
						ref float3 c = ref zero.c0;
						c += val11 * val11.x;
						ref float3 c2 = ref zero.c1;
						c2 += val11 * val11.y;
						ref float3 c3 = ref zero.c2;
						c3 += val11 * val11.z;
					}
					zero /= (float)(val.Length - 1);
					EigenDecomposition(zero, out var V);
					float3 val12 = math.normalize(V.c0);
					float3 val13 = math.normalize(V.c1);
					float3 val14 = math.normalize(V.c2);
					((float3)(ref val15))._002Ector(float.MaxValue);
					((float3)(ref val16))._002Ector(float.MinValue);
					for (int n = 0; n < val.Length; n++)
					{
						float3 val17 = float3.op_Implicit(val[n]) - val10;
						((float3)(ref val18))._002Ector(math.dot(val17, val12), math.dot(val17, val13), math.dot(val17, val14));
						val15 = math.min(val15, val18);
						val16 = math.max(val16, val18);
					}
					float3 extents = (val16 - val15) * 0.5f;
					float3 val19 = (val16 + val15) * 0.5f;
					val10 = val10 + val12 * val19.x + val13 * val19.y + val14 * val19.z;
					((Writer)(ref ShapeStream)).Write<Shape>(new Shape(ShapeType.OBB, val10, extents, quaternion.LookRotation(val14, val13), isAdditive: true, 0.2f));
				}
			}
			((Writer)(ref ShapeStream)).EndForEachIndex();
		}

		private static void EigenDecomposition(float3x3 A, out float3x3 V)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0152: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			V = float3x3.identity;
			for (int i = 0; i < 32; i++)
			{
				float num = math.abs(A.c1.x);
				float num2 = math.abs(A.c2.x);
				float num3 = math.abs(A.c2.y);
				int num4;
				int num5;
				if (num > num2 && num > num3)
				{
					num4 = 0;
					num5 = 1;
				}
				else if (num2 > num3)
				{
					num4 = 0;
					num5 = 2;
				}
				else
				{
					num4 = 1;
					num5 = 2;
				}
				if (!(math.abs(((float3)(ref ((float3x3)(ref A))[num4]))[num5]) < 1E-10f))
				{
					float num6 = ((float3)(ref ((float3x3)(ref A))[num4]))[num4];
					float num7 = ((float3)(ref ((float3x3)(ref A))[num5]))[num5];
					float num8 = ((float3)(ref ((float3x3)(ref A))[num4]))[num5];
					float num9 = 0.5f * math.atan2(2f * num8, num7 - num6);
					float num10 = math.cos(num9);
					float num11 = math.sin(num9);
					float3x3 identity = float3x3.identity;
					((float3)(ref ((float3x3)(ref identity))[num4]))[num4] = num10;
					((float3)(ref ((float3x3)(ref identity))[num5]))[num5] = num10;
					((float3)(ref ((float3x3)(ref identity))[num4]))[num5] = num11;
					((float3)(ref ((float3x3)(ref identity))[num5]))[num4] = 0f - num11;
					A = math.mul(math.transpose(identity), math.mul(A, identity));
					V = math.mul(V, identity);
					continue;
				}
				break;
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal struct ApplyCensorBoundsJob : IJobParallelFor
	{
		[NativeDisableContainerSafetyRestriction]
		public QuantizedFloatData3DArray OutputArray;

		public Reader ShapeStream;

		public unsafe void Execute(int z)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			byte* unsafePtr = (byte*)NativeArrayUnsafeUtility.GetUnsafePtr<byte>(OutputArray.FlatArray);
			UnsafeUtility.MemSet((void*)(unsafePtr + z * OutputArray.WidthHeight), byte.MaxValue, (long)OutputArray.WidthHeight);
			for (int i = 0; i < ((Reader)(ref ShapeStream)).ForEachCount; i++)
			{
				((Reader)(ref ShapeStream)).BeginForEachIndex(i);
				while (((Reader)(ref ShapeStream)).RemainingItemCount > 0)
				{
					ref Shape reference = ref ((Reader)(ref ShapeStream)).Read<Shape>();
					Bounds worldFloatBounds = reference.GetBounds();
					OutputArray.ToLocalIntBounds(in worldFloatBounds, out var min, out var max);
					if (z < min.z || z >= max.z)
					{
						continue;
					}
					for (int j = min.y; j < max.y; j++)
					{
						for (int k = min.x; k < max.x; k++)
						{
							float num = reference.OBBDistance(new float3((float)k, (float)j, (float)z));
							if (!(num > 2.5f))
							{
								byte val = OutputArray.Compress(num);
								byte b = Math.Min(OutputArray.GetByte(k, j, z), val);
								OutputArray.SetByte(k, j, z, b);
							}
						}
					}
				}
				((Reader)(ref ShapeStream)).EndForEachIndex();
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal struct ClearBoundariesJob : IJob
	{
		public QuantizedFloatData3DArray DataArray;

		public void Execute()
		{
			for (int i = 0; i < DataArray.Width; i++)
			{
				for (int j = 0; j < DataArray.Height; j++)
				{
					DataArray[i, j, 0] = 255f;
					DataArray[i, j, DataArray.Depth - 1] = 255f;
				}
			}
			for (int k = 0; k < DataArray.Width; k++)
			{
				for (int l = 0; l < DataArray.Depth; l++)
				{
					DataArray[k, 0, l] = 255f;
					DataArray[k, DataArray.Height - 1, l] = 255f;
				}
			}
			for (int m = 0; m < DataArray.Height; m++)
			{
				for (int n = 0; n < DataArray.Depth; n++)
				{
					DataArray[0, m, n] = 255f;
					DataArray[DataArray.Width - 1, m, n] = 255f;
				}
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	internal struct CalculateDistanceFieldJob : IJob
	{
		public float3 Origin;

		public Bounds ChunkBounds;

		public ReadOnly<Shape> Mods;

		public QuantizedFloatData3DArray DataArray;

		private const float TaubinInflateScale = -1.03f;

		public void Execute()
		{
			for (int i = 0; i < Mods.Length; i++)
			{
				Apply(Mods[i]);
			}
		}

		private void Apply(in Shape mod)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			Bounds worldFloatBounds = mod.GetBounds();
			if (((Bounds)(ref ChunkBounds)).Intersects(worldFloatBounds))
			{
				DataArray.ToLocalIntBounds(in worldFloatBounds, out var min, out var max);
				switch (mod.Type)
				{
				case ShapeType.Sphere:
					ApplyDistanceOps<Facepunch.MarchingCubes.SphereSdf>(in mod, in min, in max);
					break;
				case ShapeType.AABB:
					ApplyDistanceOps<Facepunch.MarchingCubes.AABBSdf>(in mod, in min, in max);
					break;
				case ShapeType.OBB:
					ApplyDistanceOps<Facepunch.MarchingCubes.OBBSdf>(in mod, in min, in max);
					break;
				case ShapeType.SharpOBB:
					ApplyDistanceOps<Facepunch.MarchingCubes.SharpOBBSdf>(in mod, in min, in max);
					break;
				case ShapeType.Cylinder:
					ApplyDistanceOps<Facepunch.MarchingCubes.CylinderSdf>(in mod, in min, in max);
					break;
				case ShapeType.Capsule:
					ApplyDistanceOps<Facepunch.MarchingCubes.CapsuleSdf>(in mod, in min, in max);
					break;
				case ShapeType.Cone:
					ApplyDistanceOps<Facepunch.MarchingCubes.ConeSdf>(in mod, in min, in max);
					break;
				case ShapeType.HexPrism:
					ApplyDistanceOps<Facepunch.MarchingCubes.HexPrismSdf>(in mod, in min, in max);
					break;
				case ShapeType.Bulge:
					ApplyBulgeOp(in mod, in min, in max);
					break;
				case ShapeType.Smooth:
					ApplySmoothOp(in mod, in min, in max);
					break;
				}
			}
		}

		private void ApplyHardOp(int x, int y, int z, float d, bool isAddtive)
		{
			byte b = DataArray.Compress(d);
			byte val = DataArray.GetByte(x, y, z);
			byte b2 = (isAddtive ? Math.Min(val, b) : Math.Max(val, (byte)(255 - b)));
			DataArray.SetByte(x, y, z, b2);
		}

		private void ApplySmoothedOp(int x, int y, int z, float d, float k, bool isAdditive)
		{
			float num = Decompress(DataArray.GetByte(x, y, z));
			float f = (isAdditive ? SmoothMin(num, d, k) : SmoothMax(d, num, k));
			byte b = DataArray.Compress(f);
			DataArray.SetByte(x, y, z, b);
		}

		private void ApplyBulgeOp(in Shape mod, in int3 min, in int3 max)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			float num = 1f / mod.Extents.x;
			float y = mod.Extents.y;
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					for (int k = min.z; k <= max.z; k++)
					{
						float num2 = math.length(new float3((float)i, (float)j, (float)k) + Origin - mod.Position) * num;
						if (!(num2 >= 1f))
						{
							float num3 = 1f - num2 * num2 * (3f - 2f * num2);
							float num4 = math.select(y * num3, (0f - y) * num3, mod.IsAdditive);
							byte b = DataArray.GetByte(i, j, k);
							byte b2 = DataArray.Compress(Decompress(b) + num4);
							DataArray.SetByte(i, j, k, b2);
						}
					}
				}
			}
		}

		private static float Decompress(byte b)
		{
			return ((float)(int)b / 255f - 0.5f) / 0.2f;
		}

		private void ApplySmoothOp(in Shape mod, in int3 min, in int3 max)
		{
			float invRadius = 1f / mod.Extents.x;
			float y = mod.Extents.y;
			SmoothPass(in mod, in min, in max, invRadius, y);
			SmoothPass(in mod, in min, in max, invRadius, y * -1.03f);
		}

		private void SmoothPass(in Shape mod, in int3 min, in int3 max, float invRadius, float strength)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_0245: Unknown result type (might be due to invalid IL or missing references)
			float num = invRadius * invRadius;
			int width = DataArray.Width;
			int widthHeight = DataArray.WidthHeight;
			int3 val = DataArray.Bounds - 1;
			float3 val2 = Origin - mod.Position;
			int3 val3 = default(int3);
			for (int i = min.z; i <= max.z; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					float num2 = (float)i + val2.z;
					float num3 = (float)j + val2.y;
					float num4 = (num2 * num2 + num3 * num3) * num;
					if (num4 >= 1f)
					{
						continue;
					}
					int num5 = DataArray.ToIndex(min.x, j, i);
					int num6 = min.x;
					while (num6 <= max.x)
					{
						float num7 = (float)num6 + val2.x;
						float num8 = num4 + num7 * num7 * num;
						if (!(num8 >= 1f))
						{
							float num9 = strength * (1f - num8 * (3f - 2f * math.sqrt(num8)));
							((int3)(ref val3))._002Ector(num6, j, i);
							float num10 = (int)DataArray.FlatArray[num5];
							float num11 = ((!(math.all(val3 > int3.zero) & math.all(val3 < val))) ? (num10 + ClampedTap(val3 + new int3(1, 0, 0)) + ClampedTap(val3 - new int3(1, 0, 0)) + ClampedTap(val3 + new int3(0, 1, 0)) + ClampedTap(val3 - new int3(0, 1, 0)) + ClampedTap(val3 + new int3(0, 0, 1)) + ClampedTap(val3 - new int3(0, 0, 1))) : (num10 + (float)(int)DataArray.FlatArray[num5 - 1] + (float)(int)DataArray.FlatArray[num5 + 1] + (float)(int)DataArray.FlatArray[num5 - width] + (float)(int)DataArray.FlatArray[num5 + width] + (float)(int)DataArray.FlatArray[num5 - widthHeight] + (float)(int)DataArray.FlatArray[num5 + widthHeight]));
							float num12 = math.clamp(math.lerp(num10, num11 * (1f / 7f), num9), 0f, 255f);
							DataArray.FlatArray[num5] = (byte)math.round(num12);
						}
						num6++;
						num5++;
					}
				}
			}
		}

		private float ClampedTap(int3 c)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			c = math.clamp(c, int3.zero, DataArray.Bounds - 1);
			return (int)DataArray.GetByte(c.x, c.y, c.z);
		}

		private void ApplyDistanceOps<TSdf>(in Shape mod, in int3 min, in int3 max) where TSdf : struct, Facepunch.MarchingCubes.ISdf
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			TSdf val = default(TSdf);
			for (int i = min.x; i <= max.x; i++)
			{
				for (int j = min.y; j <= max.y; j++)
				{
					for (int k = min.z; k <= max.z; k++)
					{
						float num = val.Distance(in mod, new float3((float)i, (float)j, (float)k) + Origin);
						if (!(num > 2.5f))
						{
							if (mod.Smoothing > 0f)
							{
								ApplySmoothedOp(i, j, k, num, mod.Smoothing, mod.IsAdditive);
							}
							else
							{
								ApplyHardOp(i, j, k, num, mod.IsAdditive);
							}
						}
					}
				}
			}
		}

		private float SmoothMin(float a, float b, float k)
		{
			k *= 4f;
			float num = math.max(k - math.abs(a - b), 0f);
			return math.min(a, b) - num * num * 0.25f / k;
		}

		private float SmoothMax(float a, float b, float k)
		{
			return 0f - SmoothMin(a, 0f - b, k);
		}
	}
}
