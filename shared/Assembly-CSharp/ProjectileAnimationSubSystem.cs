using UnityEngine;

[RequireComponent(typeof(BaseProjectile))]
public class ProjectileAnimationSubSystem : GenericChildAnimatorSubSystem, IPrefabPreProcess
{
	[SerializeField]
	[Tooltip("Length of the 3p reload clip, baked from the child controller during preprocess")]
	private float ReloadClipLength;

	public bool CanRunDuringBundling => true;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if ((Object)(object)ChildController == (Object)null)
		{
			return;
		}
		AnimationClip[] animationClips = ChildController.animationClips;
		for (int i = 0; i < animationClips.Length; i++)
		{
			if (!((Object)(object)animationClips[i] == (Object)null) && ((Object)animationClips[i]).name.EndsWith("_reload"))
			{
				ReloadClipLength = animationClips[i].length;
				break;
			}
		}
	}
}
