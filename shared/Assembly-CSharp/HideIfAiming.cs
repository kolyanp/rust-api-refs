using UnityEngine;

public class HideIfAiming : BaseMonoBehaviour, IEffect, IPrefabPreProcess
{
	public ParticleSystem[] systems;

	[HideInInspector]
	public float lifetime;

	public bool CanRunDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		lifetime = 0f;
		ParticleSystem[] componentsInChildren = ((Component)this).GetComponentsInChildren<ParticleSystem>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MainModule main = componentsInChildren[i].main;
			if (main.loop)
			{
				lifetime = 0f;
				break;
			}
			float num = lifetime;
			MinMaxCurve val = main.startDelay;
			float num2 = val.constantMax + main.duration;
			val = main.startLifetime;
			lifetime = Mathf.Max(num, num2 + val.constantMax);
		}
	}
}
