using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[ExecuteAlways]
public class RawImagePreserveAspect : MonoBehaviour, IClientComponent
{
	public RawImage rawImage;

	public RectTransform rectTransform;

	private Texture lastTexture;
}
