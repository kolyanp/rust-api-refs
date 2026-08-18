using UnityEngine;

public class DeepSeaBuoy : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color colorOpen;

	[SerializeField]
	[ColorUsage(true, true)]
	private Color colorClosed;

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

	public DeepSeaBuoy()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		colorOpen = Color.green;
		colorClosed = Color.red;
		((MonoBehaviour)this)._002Ector();
	}
}
