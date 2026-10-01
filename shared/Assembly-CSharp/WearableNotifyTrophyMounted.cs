using Facepunch.BurstCloth;
using UnityEngine;
using UnityEngine.Events;

public class WearableNotifyTrophyMounted : WearableNotify
{
	public UnityEvent OnMounted = new UnityEvent();

	public Renderer[] EmissionToggles;

	public BurstCloth[] BurstCloths;

	public WearableNotifyTrophyMounted()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
	}
}
