using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlurManager : MonoBehaviour
{
	public PostProcessVolume postProcessVolume;

	public PostProcessProfile standardBlurProfile;

	public PostProcessProfile heavyBlurProfile;
}
