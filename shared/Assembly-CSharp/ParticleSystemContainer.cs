using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemContainer : MonoBehaviour, IPrefabPreProcess
{
	[Serializable]
	public struct ParticleSystemGroup
	{
		public ParticleSystem system;

		public LODComponentParticleSystem[] lodComponents;
	}

	public bool precached;

	public bool includeLights;

	[HideInInspector]
	[SerializeField]
	private ParticleSystemGroup[] particleGroups;

	[HideInInspector]
	[SerializeField]
	private Light[] lights;

	[HideInInspector]
	[SerializeField]
	private LightEx[] lightExs;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	[UnityEvent]
	public void Play()
	{
	}

	[UnityEvent]
	public void Pause()
	{
	}

	[UnityEvent]
	public void Stop()
	{
	}

	[UnityEvent]
	public void Clear()
	{
	}

	private void SetLights(bool on)
	{
		Light[] componentsInChildren;
		LightEx[] componentsInChildren2;
		if (precached)
		{
			componentsInChildren = lights;
			componentsInChildren2 = lightExs;
		}
		else
		{
			componentsInChildren = ((Component)this).GetComponentsInChildren<Light>();
			componentsInChildren2 = ((Component)this).GetComponentsInChildren<LightEx>();
		}
		LightEx[] array = componentsInChildren2;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = on;
		}
		Light[] array2 = componentsInChildren;
		for (int i = 0; i < array2.Length; i++)
		{
			((Behaviour)array2[i]).enabled = on;
		}
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (precached && clientside)
		{
			List<ParticleSystemGroup> list = new List<ParticleSystemGroup>();
			ParticleSystem[] componentsInChildren = ((Component)this).GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem val in componentsInChildren)
			{
				LODComponentParticleSystem[] components = ((Component)val).GetComponents<LODComponentParticleSystem>();
				ParticleSystemGroup item = new ParticleSystemGroup
				{
					system = val,
					lodComponents = components
				};
				list.Add(item);
			}
			particleGroups = list.ToArray();
			if (includeLights)
			{
				lights = ((Component)this).GetComponentsInChildren<Light>();
				lightExs = ((Component)this).GetComponentsInChildren<LightEx>();
			}
		}
	}

	public bool IsPlaying()
	{
		ParticleSystemGroup[] array = particleGroups;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].system.isPlaying)
			{
				return true;
			}
		}
		return false;
	}
}
