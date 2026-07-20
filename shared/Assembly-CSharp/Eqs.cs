using System;
using System.Collections.Generic;
using UnityEngine;

public static class Eqs
{
	public sealed class PooledScoreList : BasePooledList<(Vector3 pos, float score), PooledScoreList>
	{
		public void SortByScoreDesc(BaseEntity baseEntity = null, string debugCategory = "navigation")
		{
			using (TimeWarning.New("SortByScoreDesc"))
			{
				((List<(Vector3, float)>)(object)this).Sort((Comparison<(Vector3, float)>)(((Vector3 pos, float score) a, (Vector3 pos, float score) b) => b.score.CompareTo(a.score)));
			}
		}

		public void Reorder(List<Vector3> positions)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("Reorder"))
			{
				for (int i = 0; i < positions.Count; i++)
				{
					positions[i] = ((List<(Vector3, float)>)(object)this)[i].Item1;
				}
			}
		}
	}

	public static void SamplePositionsInDonutShape(Vector3 center, List<Vector3> sampledPositions, float radius = 10f, int itemsPerRing = 8)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SamplePositionsInDonutShape"))
		{
			float num = Random.Range(0f, MathF.PI * 2f);
			for (int i = 0; i < itemsPerRing; i++)
			{
				float num2 = MathF.PI * 2f * (float)i / (float)itemsPerRing + num;
				Vector3 item = center + new Vector3(Mathf.Cos(num2), 0f, Mathf.Sin(num2)) * radius;
				sampledPositions.Add(item);
			}
			ListEx.Shuffle<Vector3>(sampledPositions, (uint)Environment.TickCount);
		}
	}

	public static void SamplePositionsInMultiDonutShape(Vector3 center, List<Vector3> sampledPositions, float outerRadius = 10f, float innerRadius = 10f, int numRings = 1, int itemsPerRing = 8)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SamplePositionsInMultiDonutShape"))
		{
			float num = Random.Range(0f, MathF.PI * 2f);
			for (int i = 0; i < numRings; i++)
			{
				float num2 = ((numRings != 1) ? Mathf.Lerp(innerRadius, outerRadius, (float)i / (float)(numRings - 1)) : outerRadius);
				for (int j = 0; j < itemsPerRing; j++)
				{
					float num3 = num + (float)i * MathF.PI / (float)numRings;
					float num4 = MathF.PI * 2f * (float)j / (float)itemsPerRing + num3;
					Vector3 item = center + new Vector3(Mathf.Cos(num4), 0f, Mathf.Sin(num4)) * num2;
					sampledPositions.Add(item);
				}
			}
			ListEx.Shuffle<Vector3>(sampledPositions, (uint)Environment.TickCount);
		}
	}
}
