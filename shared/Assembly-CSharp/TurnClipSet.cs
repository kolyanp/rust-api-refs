using System;
using UnityEngine;

public class TurnClipSet : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		public string ClipName;

		public float Angle;

		public float Length;

		public float YawCompletePhase;
	}

	public Entry[] Entries = Array.Empty<Entry>();

	public bool TryGetClipInfo(float signedAngle, out float length, out float yawCompletePhase)
	{
		length = 0f;
		yawCompletePhase = 1f;
		if (Entries == null || Entries.Length == 0)
		{
			return false;
		}
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		bool flag = false;
		bool flag2 = false;
		Entry entry = default;
		Entry entry2 = default;
		for (int i = 0; i < Entries.Length; i++)
		{
			Entry entry3 = Entries[i];
			if (!(entry3.Length <= 0f))
			{
				if (entry3.Angle <= signedAngle && entry3.Angle > num)
				{
					num = entry3.Angle;
					entry = entry3;
					flag = true;
				}
				if (entry3.Angle >= signedAngle && entry3.Angle < num2)
				{
					num2 = entry3.Angle;
					entry2 = entry3;
					flag2 = true;
				}
			}
		}
		if (!flag && !flag2)
		{
			return false;
		}
		if (!flag)
		{
			length = entry2.Length;
			yawCompletePhase = entry2.YawCompletePhase;
			return true;
		}
		if (!flag2)
		{
			length = entry.Length;
			yawCompletePhase = entry.YawCompletePhase;
			return true;
		}
		float num3 = num2 - num;
		float num4 = ((num3 <= 0.0001f) ? 0f : ((signedAngle - num) / num3));
		length = Mathf.Lerp(entry.Length, entry2.Length, num4);
		yawCompletePhase = Mathf.Lerp(entry.YawCompletePhase, entry2.YawCompletePhase, num4);
		return true;
	}
}
