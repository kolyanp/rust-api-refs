using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public class DistanceField
{
	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct GenerateJob : IJob
	{
		public int size;

		public byte threshold;

		public ReadOnly<byte> image;

		public NativeArray<float> distanceField;

		public void Execute()
		{
			int num = size + 2;
			NativeArray<int> val = default(NativeArray<int>);
			val._002Ector(num * num, (Allocator)2, (NativeArrayOptions)1);
			NativeArray<int> val2 = default(NativeArray<int>);
			val2._002Ector(num * num, (Allocator)2, (NativeArrayOptions)1);
			NativeArray<float> val3 = default(NativeArray<float>);
			val3._002Ector(num * num, (Allocator)2, (NativeArrayOptions)1);
			int i = 0;
			int num2 = 0;
			for (; i < num; i++)
			{
				int num3 = 0;
				while (num3 < num)
				{
					val[num2] = -1;
					val2[num2] = -1;
					val3[num2] = float.PositiveInfinity;
					num3++;
					num2++;
				}
			}
			int num4 = 1;
			int num5 = num4 * size;
			int num6 = num4 * num;
			while (num4 < size - 2)
			{
				int num7 = 1;
				int num8 = num5 + num7;
				int num9 = num6 + num7;
				while (num7 < size - 2)
				{
					int num10 = num9 + num + 1;
					bool flag = image[num8] > threshold;
					if (flag && (image[num8 - 1] > threshold != flag || image[num8 + 1] > threshold != flag || image[num8 - size] > threshold != flag || image[num8 + size] > threshold != flag))
					{
						val[num10] = num7 + 1;
						val2[num10] = num4 + 1;
						val3[num10] = 0f;
					}
					num7++;
					num8++;
					num9++;
				}
				num4++;
				num5 += size;
				num6 += num;
			}
			int num11 = 1;
			int num12 = num11 * num;
			while (num11 < num - 1)
			{
				int num13 = 1;
				int num14 = num12 + num13;
				while (num13 < num - 1)
				{
					int num15 = num14 - 1;
					int num16 = num14 - num;
					int num17 = num16 - 1;
					int num18 = num16 + 1;
					float num19 = val3[num14];
					if (val3[num17] + 1.4142135f < num19)
					{
						int num20 = (val[num14] = val[num17]);
						int num22 = num20;
						num20 = (val2[num14] = val2[num17]);
						int num24 = num20;
						float num25 = (val3[num14] = Vector2Ex.Length((float)(num13 - num22), (float)(num11 - num24)));
						num19 = num25;
					}
					if (val3[num16] + 1f < num19)
					{
						int num20 = (val[num14] = val[num16]);
						int num28 = num20;
						num20 = (val2[num14] = val2[num16]);
						int num30 = num20;
						float num25 = (val3[num14] = Vector2Ex.Length((float)(num13 - num28), (float)(num11 - num30)));
						num19 = num25;
					}
					if (val3[num18] + 1.4142135f < num19)
					{
						int num20 = (val[num14] = val[num18]);
						int num33 = num20;
						num20 = (val2[num14] = val2[num18]);
						int num35 = num20;
						float num25 = (val3[num14] = Vector2Ex.Length((float)(num13 - num33), (float)(num11 - num35)));
						num19 = num25;
					}
					if (val3[num15] + 1f < num19)
					{
						int num20 = (val[num14] = val[num15]);
						int num38 = num20;
						num20 = (val2[num14] = val2[num15]);
						int num40 = num20;
						float num25 = (val3[num14] = Vector2Ex.Length((float)(num13 - num38), (float)(num11 - num40)));
						num19 = num25;
					}
					num13++;
					num14++;
				}
				num11++;
				num12 += num;
			}
			int num42 = num - 2;
			int num43 = num42 * num;
			while (num42 >= 1)
			{
				int num44 = num - 2;
				int num45 = num43 + num44;
				while (num44 >= 1)
				{
					int num46 = num45 + 1;
					int num47 = num45 + num;
					int num48 = num47 - 1;
					int num49 = num47 + 1;
					float num50 = val3[num45];
					if (val3[num46] + 1f < num50)
					{
						int num20 = (val[num45] = val[num46]);
						int num52 = num20;
						num20 = (val2[num45] = val2[num46]);
						int num54 = num20;
						float num25 = (val3[num45] = Vector2Ex.Length((float)(num44 - num52), (float)(num42 - num54)));
						num50 = num25;
					}
					if (val3[num48] + 1.4142135f < num50)
					{
						int num20 = (val[num45] = val[num48]);
						int num57 = num20;
						num20 = (val2[num45] = val2[num48]);
						int num59 = num20;
						float num25 = (val3[num45] = Vector2Ex.Length((float)(num44 - num57), (float)(num42 - num59)));
						num50 = num25;
					}
					if (val3[num47] + 1f < num50)
					{
						int num20 = (val[num45] = val[num47]);
						int num62 = num20;
						num20 = (val2[num45] = val2[num47]);
						int num64 = num20;
						float num25 = (val3[num45] = Vector2Ex.Length((float)(num44 - num62), (float)(num42 - num64)));
						num50 = num25;
					}
					if (val3[num49] + 1f < num50)
					{
						int num20 = (val[num45] = val[num49]);
						int num67 = num20;
						num20 = (val2[num45] = val2[num49]);
						int num69 = num20;
						float num25 = (val3[num45] = Vector2Ex.Length((float)(num44 - num67), (float)(num42 - num69)));
						num50 = num25;
					}
					num44--;
					num45--;
				}
				num42--;
				num43 -= num;
			}
			int num71 = 0;
			int num72 = 0;
			int num73 = num;
			while (num71 < size)
			{
				int num74 = 0;
				int num75 = num73 + 1;
				while (num74 < size)
				{
					distanceField[num72] = ((image[num72] > threshold) ? (0f - val3[num75]) : val3[num75]);
					num74++;
					num72++;
					num75++;
				}
				num71++;
				num73 += num;
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct SobelGradientsJob : IJobParallelFor
	{
		public int size;

		public ReadOnly<float> distanceField;

		public NativeArray<Vector4> vectorField;

		public void Execute(int index)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			int num = index % size;
			int num2 = index / size;
			float num3 = SampleClamped(distanceField, size, num, num2);
			float num4 = SampleClamped(distanceField, size, num - 1, num2 - 1);
			float num5 = SampleClamped(distanceField, size, num - 1, num2);
			float num6 = SampleClamped(distanceField, size, num - 1, num2 + 1);
			float num7 = SampleClamped(distanceField, size, num, num2 - 1);
			float num8 = SampleClamped(distanceField, size, num, num2 + 1);
			float num9 = SampleClamped(distanceField, size, num + 1, num2 - 1);
			float num10 = SampleClamped(distanceField, size, num + 1, num2);
			float num11 = SampleClamped(distanceField, size, num + 1, num2 + 1);
			float num12 = num9 + 2f * num10 + num11 - (num4 + 2f * num5 + num6);
			float num13 = num6 + 2f * num8 + num11 - (num4 + 2f * num7 + num9);
			Vector2 val = new Vector2(0f - num12, 0f - num13);
			Vector2 normalized = ((Vector2)(ref val)).normalized;
			vectorField[index] = new Vector4(normalized.x, normalized.y, num3, 0f);
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct FixBoundaryGradientsJob : IJob
	{
		public int size;

		public NativeArray<Vector4> vectorField;

		public void Execute()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 1; i < size - 1; i++)
			{
				vectorField[0 + i] = SampleClamped(vectorField, size, i, 1);
				vectorField[(size - 1) * size + i] = SampleClamped(vectorField, size, i, size - 2);
			}
			for (int j = 0; j < size; j++)
			{
				vectorField[j * size] = SampleClamped(vectorField, size, 1, j);
				vectorField[j * size + size - 1] = SampleClamped(vectorField, size, size - 2, j);
			}
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct BlurHorizontalJob : IJobParallelFor
	{
		public int size;

		public ReadOnly<float> src;

		[WriteOnly]
		public NativeArray<float> dst;

		public void Execute(int index)
		{
			int num = index % size;
			int num2 = index / size;
			int num3 = size - 1;
			int num4 = num2 * size;
			float num5 = 0f;
			for (int i = 0; i < 7; i++)
			{
				int num6 = num + GaussOffsets[i];
				num6 = ((num6 >= 0) ? ((num6 > num3) ? num3 : num6) : 0);
				num5 += src[num4 + num6] * GaussWeights[i];
			}
			dst[index] = num5;
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct BlurVerticalJob : IJobParallelFor
	{
		public int size;

		public ReadOnly<float> src;

		[WriteOnly]
		public NativeArray<float> dst;

		public void Execute(int index)
		{
			int num = index % size;
			int num2 = index / size;
			int num3 = size - 1;
			float num4 = 0f;
			for (int i = 0; i < 7; i++)
			{
				int num5 = num2 + GaussOffsets[i];
				num5 = ((num5 >= 0) ? ((num5 > num3) ? num3 : num5) : 0);
				num4 += src[num5 * size + num] * GaussWeights[i];
			}
			dst[index] = num4;
		}
	}

	[BurstCompile(/*Could not decode attribute arguments.*/)]
	private struct GaussianBlurJob : IJob
	{
		public int size;

		public NativeArray<float> distanceField;

		public int steps;

		public void Execute()
		{
			NativeArray<float> val = default(NativeArray<float>);
			val._002Ector(size * size, (Allocator)2, (NativeArrayOptions)1);
			int num = size - 1;
			for (int i = 0; i < steps; i++)
			{
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				while (num2 < size)
				{
					int num5 = 0;
					while (num5 < size)
					{
						float num6 = 0f;
						for (int j = 0; j < 7; j++)
						{
							int num7 = num5 + GaussOffsets[j];
							num7 = ((num7 >= 0) ? num7 : 0);
							num7 = ((num7 <= num) ? num7 : num);
							num6 += distanceField[num4 + num7] * GaussWeights[j];
						}
						val[num3] = num6;
						num5++;
						num3++;
					}
					num2++;
					num4 += size;
				}
				int k = 0;
				int num8 = 0;
				for (; k < size; k++)
				{
					int num9 = 0;
					while (num9 < size)
					{
						float num10 = 0f;
						for (int l = 0; l < 7; l++)
						{
							int num11 = k + GaussOffsets[l];
							num11 = ((num11 >= 0) ? num11 : 0);
							num11 = ((num11 <= num) ? num11 : num);
							num10 += val[num11 * size + num9] * GaussWeights[l];
						}
						distanceField[num8] = num10;
						num9++;
						num8++;
					}
				}
			}
		}
	}

	private static readonly int[] GaussOffsets = new int[7] { -6, -4, -2, 0, 2, 4, 6 };

	private static readonly float[] GaussWeights = new float[7]
	{
		1f / 32f,
		7f / 64f,
		7f / 32f,
		9f / 32f,
		7f / 32f,
		7f / 64f,
		1f / 32f
	};

	public static JobHandle GenerateNative(in int size, in byte threshold, in ReadOnly<byte> image, in NativeArray<float> distanceFieldOut, JobHandle inputDeps)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		GenerateJob generateJob = new GenerateJob
		{
			size = size,
			threshold = threshold,
			image = image,
			distanceField = distanceFieldOut
		};
		return IJobExtensions.ScheduleByRef<GenerateJob>(ref generateJob, inputDeps);
	}

	public static void Generate(in int size, in byte threshold, in byte[] image, ref float[] distanceField)
	{
		int num = size + 2;
		int[] array = new int[num * num];
		int[] array2 = new int[num * num];
		float[] array3 = new float[num * num];
		int i = 0;
		int num2 = 0;
		for (; i < num; i++)
		{
			int num3 = 0;
			while (num3 < num)
			{
				array[num2] = -1;
				array2[num2] = -1;
				array3[num2] = float.PositiveInfinity;
				num3++;
				num2++;
			}
		}
		int num4 = 1;
		int num5 = num4 * size;
		int num6 = num4 * num;
		while (num4 < size - 2)
		{
			int num7 = 1;
			int num8 = num5 + num7;
			int num9 = num6 + num7;
			while (num7 < size - 2)
			{
				int num10 = num9 + num + 1;
				bool flag = image[num8] > threshold;
				if (flag && (image[num8 - 1] > threshold != flag || image[num8 + 1] > threshold != flag || image[num8 - size] > threshold != flag || image[num8 + size] > threshold != flag))
				{
					array[num10] = num7 + 1;
					array2[num10] = num4 + 1;
					array3[num10] = 0f;
				}
				num7++;
				num8++;
				num9++;
			}
			num4++;
			num5 += size;
			num6 += num;
		}
		int num11 = 1;
		int num12 = num11 * num;
		while (num11 < num - 1)
		{
			int num13 = 1;
			int num14 = num12 + num13;
			while (num13 < num - 1)
			{
				int num15 = num14 - 1;
				int num16 = num14 - num;
				int num17 = num16 - 1;
				int num18 = num16 + 1;
				float num19 = array3[num14];
				if (array3[num17] + 1.4142135f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length((float)(num13 - (array[num14] = array[num17])), (float)(num11 - (array2[num14] = array2[num17]))));
				}
				if (array3[num16] + 1f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length((float)(num13 - (array[num14] = array[num16])), (float)(num11 - (array2[num14] = array2[num16]))));
				}
				if (array3[num18] + 1.4142135f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length((float)(num13 - (array[num14] = array[num18])), (float)(num11 - (array2[num14] = array2[num18]))));
				}
				if (array3[num15] + 1f < num19)
				{
					num19 = (array3[num14] = Vector2Ex.Length((float)(num13 - (array[num14] = array[num15])), (float)(num11 - (array2[num14] = array2[num15]))));
				}
				num13++;
				num14++;
			}
			num11++;
			num12 += num;
		}
		int num20 = num - 2;
		int num21 = num20 * num;
		while (num20 >= 1)
		{
			int num22 = num - 2;
			int num23 = num21 + num22;
			while (num22 >= 1)
			{
				int num24 = num23 + 1;
				int num25 = num23 + num;
				int num26 = num25 - 1;
				int num27 = num25 + 1;
				float num28 = array3[num23];
				if (array3[num24] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length((float)(num22 - (array[num23] = array[num24])), (float)(num20 - (array2[num23] = array2[num24]))));
				}
				if (array3[num26] + 1.4142135f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length((float)(num22 - (array[num23] = array[num26])), (float)(num20 - (array2[num23] = array2[num26]))));
				}
				if (array3[num25] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length((float)(num22 - (array[num23] = array[num25])), (float)(num20 - (array2[num23] = array2[num25]))));
				}
				if (array3[num27] + 1f < num28)
				{
					num28 = (array3[num23] = Vector2Ex.Length((float)(num22 - (array[num23] = array[num27])), (float)(num20 - (array2[num23] = array2[num27]))));
				}
				num22--;
				num23--;
			}
			num20--;
			num21 -= num;
		}
		int num29 = 0;
		int num30 = 0;
		int num31 = num;
		while (num29 < size)
		{
			int num32 = 0;
			int num33 = num31 + 1;
			while (num32 < size)
			{
				distanceField[num30] = ((image[num30] > threshold) ? (0f - array3[num33]) : array3[num33]);
				num32++;
				num30++;
				num33++;
			}
			num29++;
			num31 += num;
		}
	}

	private static float SampleClamped(float[] data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static Vector4 SampleClamped(Vector4[] data, int size, int x, int y)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static ushort SampleClamped(ushort[] data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static float SampleClamped(ReadOnly<float> data, int size, int x, int y)
	{
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	private static Vector4 SampleClamped(NativeArray<Vector4> data, int size, int x, int y)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		x = ((x >= 0) ? x : 0);
		y = ((y >= 0) ? y : 0);
		x = ((x >= size) ? (size - 1) : x);
		y = ((y >= size) ? (size - 1) : y);
		return data[y * size + x];
	}

	public static JobHandle GenerateVectorsNative(in int size, ReadOnly<float> distanceField, NativeArray<Vector4> vectorFieldOut, JobHandle inputDeps)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		SobelGradientsJob jobData = new SobelGradientsJob
		{
			size = size,
			distanceField = distanceField,
			vectorField = vectorFieldOut
		};
		inputDeps = ParallelJobEx.ScheduleParallel<SobelGradientsJob>(ref jobData, vectorFieldOut.Length, inputDeps);
		inputDeps = IJobExtensions.Schedule<FixBoundaryGradientsJob>(new FixBoundaryGradientsJob
		{
			size = size,
			vectorField = vectorFieldOut
		}, inputDeps);
		return inputDeps;
	}

	public static void GenerateVectors(in int size, in float[] distanceField, ref Vector4[] vectorField)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < size - 1; i++)
		{
			for (int j = 1; j < size - 1; j++)
			{
				float num = SampleClamped(distanceField, size, i, j);
				float num2 = SampleClamped(distanceField, size, i - 1, j - 1);
				float num3 = SampleClamped(distanceField, size, i - 1, j);
				float num4 = SampleClamped(distanceField, size, i - 1, j + 1);
				float num5 = SampleClamped(distanceField, size, i, j - 1);
				float num6 = SampleClamped(distanceField, size, i, j + 1);
				float num7 = SampleClamped(distanceField, size, i + 1, j - 1);
				float num8 = SampleClamped(distanceField, size, i + 1, j);
				float num9 = SampleClamped(distanceField, size, i + 1, j + 1);
				float num10 = num7 + 2f * num8 + num9 - (num2 + 2f * num3 + num4);
				float num11 = num4 + 2f * num6 + num9 - (num2 + 2f * num5 + num7);
				Vector2 val = new Vector2(0f - num10, 0f - num11);
				Vector2 normalized = ((Vector2)(ref val)).normalized;
				vectorField[j * size + i] = new Vector4(normalized.x, normalized.y, num, 0f);
			}
		}
		for (int k = 1; k < size - 1; k++)
		{
			vectorField[k] = SampleClamped(vectorField, size, k, 1);
			vectorField[(size - 1) * size + k] = SampleClamped(vectorField, size, k, size - 2);
		}
		for (int l = 0; l < size; l++)
		{
			vectorField[l * size] = SampleClamped(vectorField, size, 1, l);
			vectorField[l * size + size - 1] = SampleClamped(vectorField, size, size - 2, l);
		}
	}

	public static JobHandle ApplyGaussianBlurNative(int size, NativeArray<float> distanceField, int steps = 1, JobHandle inputDeps = default(JobHandle))
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (steps <= 0)
		{
			return inputDeps;
		}
		NativeArray<float> dst = default(NativeArray<float>);
		dst._002Ector(size * size, (Allocator)3, (NativeArrayOptions)1);
		for (int i = 0; i < steps; i++)
		{
			BlurHorizontalJob jobData = new BlurHorizontalJob
			{
				size = size,
				src = distanceField.AsReadOnly(),
				dst = dst
			};
			inputDeps = ParallelJobEx.ScheduleParallel<BlurHorizontalJob>(ref jobData, size * size, inputDeps);
			BlurVerticalJob jobData2 = new BlurVerticalJob
			{
				size = size,
				src = dst.AsReadOnly(),
				dst = distanceField
			};
			inputDeps = ParallelJobEx.ScheduleParallel<BlurVerticalJob>(ref jobData2, size * size, inputDeps);
		}
		dst.Dispose(inputDeps);
		return inputDeps;
	}

	public static void ApplyGaussianBlur(int size, float[] distanceField, int steps = 1)
	{
		if (steps <= 0)
		{
			return;
		}
		float[] array = new float[size * size];
		int num = size - 1;
		for (int i = 0; i < steps; i++)
		{
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			while (num2 < size)
			{
				int num5 = 0;
				while (num5 < size)
				{
					float num6 = 0f;
					for (int j = 0; j < 7; j++)
					{
						int num7 = num5 + GaussOffsets[j];
						num7 = ((num7 >= 0) ? num7 : 0);
						num7 = ((num7 <= num) ? num7 : num);
						num6 += distanceField[num4 + num7] * GaussWeights[j];
					}
					array[num3] = num6;
					num5++;
					num3++;
				}
				num2++;
				num4 += size;
			}
			int k = 0;
			int num8 = 0;
			for (; k < size; k++)
			{
				int num9 = 0;
				while (num9 < size)
				{
					float num10 = 0f;
					for (int l = 0; l < 7; l++)
					{
						int num11 = k + GaussOffsets[l];
						num11 = ((num11 >= 0) ? num11 : 0);
						num11 = ((num11 <= num) ? num11 : num);
						num10 += array[num11 * size + num9] * GaussWeights[l];
					}
					distanceField[num8] = num10;
					num9++;
					num8++;
				}
			}
		}
	}
}
