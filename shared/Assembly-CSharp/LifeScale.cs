using System;
using UnityEngine;

public class LifeScale : BaseMonoBehaviour
{
	[NonSerialized]
	private bool initialized;

	[NonSerialized]
	private Vector3 initialScale;

	public Vector3 finalScale;

	private Vector3 targetLerpScale;

	private Action updateScaleAction;

	protected void Awake()
	{
		updateScaleAction = UpdateScale;
	}

	public void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Init();
		((Component)this).transform.localScale = initialScale;
	}

	public void SetProgress(float progress)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Init();
		targetLerpScale = Vector3.Lerp(initialScale, finalScale, progress);
		InvokeRepeating(updateScaleAction, 0f, 0.015f);
	}

	public void Init()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			initialScale = ((Component)this).transform.localScale;
			initialized = true;
		}
	}

	public void UpdateScale()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = Vector3.Lerp(((Component)this).transform.localScale, targetLerpScale, Time.deltaTime);
		if (((Component)this).transform.localScale == targetLerpScale)
		{
			targetLerpScale = Vector3.zero;
			CancelInvoke(updateScaleAction);
		}
	}

	public LifeScale()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		finalScale = Vector3.one;
		targetLerpScale = Vector3.zero;
		base._002Ector();
	}
}
