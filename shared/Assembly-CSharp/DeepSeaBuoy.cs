using UnityEngine;

public class DeepSeaBuoy : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color colorOpen = Color.green;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color colorClosed = Color.red;

	[SerializeField]
	private GameObject lightClosed;

	[SerializeField]
	private GameObject lightOpen;

	private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

	private static MaterialPropertyBlock propertyBlock;

	private void OnEnable()
	{
		UpdateLights();
	}

	public void UpdateLights()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		DeepSeaManager deepSeaManager = DeepSeaManager.Get(server: false);
		bool flag = (Object)(object)deepSeaManager != (Object)null && deepSeaManager.IsAccessible();
		if ((Object)(object)lightClosed != (Object)null)
		{
			lightClosed.SetActive(!flag);
		}
		if ((Object)(object)lightOpen != (Object)null)
		{
			lightOpen.SetActive(flag);
		}
		if (propertyBlock == null)
		{
			propertyBlock = new MaterialPropertyBlock();
		}
		propertyBlock.SetColor(EmissionColor, flag ? colorOpen : colorClosed);
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer val in array)
		{
			if ((Object)(object)val != (Object)null)
			{
				((Renderer)val).SetPropertyBlock(propertyBlock);
			}
		}
	}
}
