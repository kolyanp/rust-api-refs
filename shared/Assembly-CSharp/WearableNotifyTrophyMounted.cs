using UnityEngine;
using UnityEngine.Events;

public class WearableNotifyTrophyMounted : WearableNotify, IPrefabPreProcess
{
	public UnityEvent OnMounted = new UnityEvent();

	public Renderer[] EmissionToggles;

	[Tooltip("Parts skinned to the upper torso that should keep their size on a trophy, like a helmet resting on the shoulders")]
	public SkinnedMeshRenderer[] NeckAnchoredRenderers;

	[HideInInspector]
	public Transform[] NeckProxies;

	[HideInInspector]
	public string[] NeckProxyBones;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}

	public WearableNotifyTrophyMounted()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
