using UnityEngine;

public class RandomScale : MonoBehaviour
{
	[SerializeField]
	private bool multiplyByExistingScale = true;

	[SerializeField]
	private Vector3 minScale = Vector3.one * 0.8f;

	[SerializeField]
	private Vector3 maxScale = Vector3.one * 1.2f;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (multiplyByExistingScale)
		{
			Transform transform = ((Component)this).transform;
			transform.localScale *= Random.Range(minScale.x, maxScale.x);
		}
		else
		{
			((Component)this).transform.localScale = Vector3.one * Random.Range(minScale.x, maxScale.x);
		}
	}

	public RandomScale()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
	}
}
