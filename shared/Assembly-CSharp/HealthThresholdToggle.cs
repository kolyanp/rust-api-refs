using System;
using System.Collections.Generic;
using UnityEngine;

public class HealthThresholdToggle : FacepunchBehaviour, IPrefabPreProcess
{
	public enum PanelSide
	{
		Front,
		Back,
		Left,
		Right
	}

	[Serializable]
	public struct ThresholdEntry
	{
		[Tooltip("A panel detaches while health fraction is at or below this. Which panel it is depends on damage direction, not on this entry.")]
		[Range(0f, 1f)]
		public float disableAtHealthFraction;

		[Tooltip("The side of the vehicle these objects cover, matched against incoming damage direction.")]
		public PanelSide side;

		public GameObject[] targets;
	}

	[SerializeField]
	private ThresholdEntry[] entries;

	private int lastAppliedMask = -1;

	private readonly float[] sideDamage = new float[4];

	private readonly List<int> detachOrder = new List<int>();

	public bool CanRunDuringBundling => true;

	public void ApplyMask(int detachedMask)
	{
		if (detachedMask == lastAppliedMask)
		{
			return;
		}
		int num = lastAppliedMask;
		lastAppliedMask = detachedMask;
		for (int i = 0; i < entries.Length; i++)
		{
			int num2 = 1 << i;
			bool flag = (detachedMask & num2) == 0;
			GameObject[] targets = entries[i].targets;
			foreach (GameObject val in targets)
			{
				if (!((Object)(object)val == (Object)null) && val.activeSelf != flag)
				{
					val.SetActive(flag);
				}
			}
		}
	}

	public void ResetState()
	{
		lastAppliedMask = -1;
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject[] targets = array[i].targets;
			foreach (GameObject val in targets)
			{
				if ((Object)(object)val != (Object)null && !val.activeSelf)
				{
					val.SetActive(true);
				}
			}
		}
	}

	public void NoteDamage(HitInfo info, Transform vehicle)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (info != null && !(info.HitPositionWorld == Vector3.zero))
		{
			Vector3 val = vehicle.InverseTransformPoint(info.HitPositionWorld);
			if (!(val.sqrMagnitude < 0.01f))
			{
				PanelSide panelSide;
				if (Mathf.Abs(val.x) > Mathf.Abs(val.z))
				{
					panelSide = ((val.x < 0f) ? PanelSide.Left : PanelSide.Right);
				}
				else
				{
					panelSide = ((!(val.z >= 0f)) ? PanelSide.Back : PanelSide.Front);
				}
				sideDamage[(int)panelSide] += info.damageTypes.Total();
			}
		}
	}

	public void NoteHealing(float healedHealthFraction)
	{
		float num = Mathf.Clamp01(1f - healedHealthFraction);
		for (int i = 0; i < sideDamage.Length; i++)
		{
			sideDamage[i] *= num;
		}
	}

	public int UpdateDetachedMask(int currentMask, float healthFraction)
	{
		RebuildDetachOrder(currentMask);
		int num = 0;
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			ThresholdEntry thresholdEntry = array[i];
			if (healthFraction <= thresholdEntry.disableAtHealthFraction)
			{
				num++;
			}
		}
		while (detachOrder.Count > num)
		{
			detachOrder.RemoveAt(detachOrder.Count - 1);
		}
		while (detachOrder.Count < num)
		{
			detachOrder.Add(PickNextDetach());
		}
		int num2 = 0;
		foreach (int item in detachOrder)
		{
			num2 |= 1 << item;
		}
		return num2;
	}

	private void RebuildDetachOrder(int mask)
	{
		if (detachOrder.Count > 0 || mask == 0)
		{
			return;
		}
		for (int i = 0; i < entries.Length; i++)
		{
			if ((mask & (1 << i)) != 0)
			{
				detachOrder.Add(i);
			}
		}
		detachOrder.Sort((int a, int b) => entries[b].disableAtHealthFraction.CompareTo(entries[a].disableAtHealthFraction));
	}

	private int PickNextDetach()
	{
		int num = -1;
		for (int i = 0; i < entries.Length; i++)
		{
			if (!detachOrder.Contains(i) && (num == -1 || IsBetterDetach(i, num)))
			{
				num = i;
			}
		}
		return num;
	}

	private bool IsBetterDetach(int candidate, int best)
	{
		float num = sideDamage[(int)entries[candidate].side];
		float num2 = sideDamage[(int)entries[best].side];
		if (!Mathf.Approximately(num, num2))
		{
			return num > num2;
		}
		return entries[candidate].disableAtHealthFraction > entries[best].disableAtHealthFraction;
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		ThresholdEntry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject[] targets = array[i].targets;
			foreach (GameObject val in targets)
			{
				if (!((Object)(object)val == (Object)null))
				{
					Gibbable component = val.GetComponent<Gibbable>();
					if (component != null)
					{
						component.isConditional = true;
					}
				}
			}
		}
	}
}
