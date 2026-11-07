using UnityEngine;

public class WearableNotifyTrophyMounted : WearableNotify, IPrefabPreProcess
{
	public GameObject[] HideOnTrophy;

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
}
