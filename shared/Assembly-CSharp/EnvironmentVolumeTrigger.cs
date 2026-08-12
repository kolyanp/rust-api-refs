using UnityEngine;

public class EnvironmentVolumeTrigger : MonoBehaviour
{
	[HideInInspector]
	public Vector3 Center;

	[HideInInspector]
	public Vector3 Size;

	public EnvironmentVolume volume { get; private set; }

	public void Awake()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		volume = ((Component)this).gameObject.GetComponent<EnvironmentVolume>();
		if ((Object)(object)volume == (Object)null)
		{
			volume = ((Component)this).gameObject.AddComponent<EnvironmentVolume>();
			volume.Center = Center;
			volume.Size = Size;
		}
		volume.UpdateTrigger();
		OnVolumeTriggerUpdate();
	}

	protected virtual void OnVolumeTriggerUpdate()
	{
	}

	public EnvironmentVolumeTrigger()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Center = Vector3.zero;
		Size = Vector3.one;
		((MonoBehaviour)this)._002Ector();
	}
}
