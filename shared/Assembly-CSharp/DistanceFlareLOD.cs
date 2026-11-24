using System;
using UnityEngine;
using UnityEngine.Serialization;

public class DistanceFlareLOD : FacepunchBehaviour, IClientComponent, IPrefabPreProcess
{
	public const float DefaultMaxDistance = 2000f;

	[FormerlySerializedAs("minEnabledDistance")]
	[Min(0f)]
	public float minDistance = 40f;

	public float maxDistance = 2000f;

	[HideInInspector]
	public Material flareMaterial;

	protected void OnValidate()
	{
		maxDistance = Mathf.Max(maxDistance, minDistance);
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside)
		{
			StripRenderer((Action<Component>)preProcess.RemoveComponent);
		}
	}

	private void StripRenderer(Action<Component> removeComponent)
	{
		Renderer val = default;
		if (((Component)this).TryGetComponent<Renderer>(ref val))
		{
			flareMaterial = val.sharedMaterial;
			removeComponent((Component)(object)val);
		}
		MeshFilter obj = default;
		if (((Component)this).TryGetComponent<MeshFilter>(ref obj))
		{
			removeComponent((Component)(object)obj);
		}
	}
}
