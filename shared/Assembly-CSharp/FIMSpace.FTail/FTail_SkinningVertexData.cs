using System;
using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail;

[Serializable]
public class FTail_SkinningVertexData
{
	public Vector3 position;

	public int[] bonesIndexes;

	public int allMeshBonesCount;

	public float[] weights;

	public float[] debugDists;

	public float[] debugDistWeights;

	public float[] debugWeights;

	public FTail_SkinningVertexData(Vector3 pos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		position = pos;
	}

	public float DistanceToLine(Vector3 pos, Vector3 lineStart, Vector3 lineEnd)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = pos - lineStart;
		Vector3 val2 = lineEnd - lineStart;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		float num = Vector3.Distance(lineStart, lineEnd);
		float num2 = Vector3.Dot(normalized, val);
		if (num2 <= 0f)
		{
			return Vector3.Distance(pos, lineStart);
		}
		if (num2 >= num)
		{
			return Vector3.Distance(pos, lineEnd);
		}
		Vector3 val3 = normalized * num2;
		Vector3 val4 = lineStart + val3;
		return Vector3.Distance(pos, val4);
	}

	public void CalculateVertexParameters(Vector3[] bonesPos, Quaternion[] bonesRot, Vector3[] boneAreas, int maxWeightedBones, float spread, Vector3 spreadOffset, float spreadPower = 1f)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		allMeshBonesCount = bonesPos.Length;
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < bonesPos.Length; i++)
		{
			Vector3 val = ((i == bonesPos.Length - 1) ? Vector3.Lerp(bonesPos[i], bonesPos[i] + (bonesPos[i] - bonesPos[i - 1]), 0.9f) : Vector3.Lerp(bonesPos[i], bonesPos[i + 1], 0.9f));
			val += bonesRot[i] * spreadOffset;
			float num = DistanceToLine(position, bonesPos[i], val);
			list.Add(new Vector2((float)i, num));
		}
		list.Sort(delegate(Vector2 a, Vector2 b)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return a.y.CompareTo(b.y);
		});
		int num2 = Mathf.Min(maxWeightedBones, bonesPos.Length);
		bonesIndexes = new int[num2];
		float[] array = new float[num2];
		for (int num3 = 0; num3 < num2; num3++)
		{
			bonesIndexes[num3] = (int)list[num3].x;
			array[num3] = list[num3].y;
		}
		float[] array2 = new float[num2];
		AutoSetBoneWeights(array2, array, spread, spreadPower, boneAreas);
		float num4 = 1f;
		weights = new float[num2];
		for (int num5 = 0; num5 < num2 && (spread != 0f || num5 <= 0); num5++)
		{
			if (num4 <= 0f)
			{
				weights[num5] = 0f;
				continue;
			}
			float num6 = array2[num5];
			num4 -= num6;
			if (num4 <= 0f)
			{
				num6 += num4;
			}
			else if (num5 == num2 - 1)
			{
				num6 += num4;
			}
			weights[num5] = num6;
		}
	}

	public void AutoSetBoneWeights(float[] weightForBone, float[] distToBone, float spread, float spreadPower, Vector3[] boneAreas)
	{
		int num = weightForBone.Length;
		float[] array = new float[num];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ((Vector3)(ref boneAreas[i])).magnitude;
		}
		float[] array2 = new float[num];
		for (int j = 0; j < weightForBone.Length; j++)
		{
			weightForBone[j] = 0f;
		}
		float num2 = 0f;
		for (int k = 0; k < num; k++)
		{
			num2 += distToBone[k];
		}
		for (int l = 0; l < num; l++)
		{
			array2[l] = 1f - distToBone[l] / num2;
		}
		debugDists = distToBone;
		if (num == 1 || spread == 0f)
		{
			weightForBone[0] = 1f;
			return;
		}
		if (num == 2)
		{
			float num3 = 1f;
			weightForBone[0] = 1f;
			float num4 = Mathf.InverseLerp(distToBone[0] + array[0] / 1.25f * spread, distToBone[0], distToBone[1]);
			debugDists[0] = num4;
			num3 += (weightForBone[1] = DistributionIn(Mathf.Lerp(0f, 1f, num4), Mathf.Lerp(1.5f, 16f, spreadPower)));
			debugDistWeights = new float[weightForBone.Length];
			weightForBone.CopyTo(debugDistWeights, 0);
			for (int m = 0; m < num; m++)
			{
				weightForBone[m] /= num3;
			}
			debugWeights = weightForBone;
			return;
		}
		float num5 = array[0] / 10f;
		float num6 = array[0] / 2f;
		float num7 = 0f;
		for (int n = 0; n < num; n++)
		{
			float num8 = Mathf.InverseLerp(0f, num5 + num6 * spread, distToBone[n]);
			float num9 = Mathf.Lerp(1f, 0f, num8);
			if (n == 0 && num9 == 0f)
			{
				num9 = 1f;
			}
			weightForBone[n] = num9;
			num7 += num9;
		}
		debugDistWeights = new float[weightForBone.Length];
		weightForBone.CopyTo(debugDistWeights, 0);
		for (int num10 = 0; num10 < num; num10++)
		{
			weightForBone[num10] /= num7;
		}
		debugWeights = weightForBone;
	}

	public static float DistributionIn(float k, float power)
	{
		return Mathf.Pow(k, power + 1f);
	}

	public static Color GetBoneIndicatorColor(int boneIndex, int bonesCount, float s = 0.9f, float v = 0.9f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return Color.HSVToRGB(((float)boneIndex * 1.125f / (float)bonesCount + 0.125f * (float)boneIndex + 0.3f) % 1f, s, v);
	}

	public Color GetWeightColor()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		Color val = GetBoneIndicatorColor(bonesIndexes[0], allMeshBonesCount, 1f, 1f);
		for (int i = 1; i < bonesIndexes.Length; i++)
		{
			val = Color.Lerp(val, GetBoneIndicatorColor(bonesIndexes[i], allMeshBonesCount, 1f, 1f), weights[i]);
		}
		return val;
	}
}
