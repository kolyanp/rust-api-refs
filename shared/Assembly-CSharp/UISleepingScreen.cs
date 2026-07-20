using UnityEngine;

public class UISleepingScreen : SingletonComponent<UISleepingScreen>, IUIScreen
{
	protected Canvas canvas;

	protected CanvasGroup canvasGroup;

	private bool visible;

	protected override void Awake()
	{
		base.Awake();
		canvasGroup = ((Component)this).GetComponent<CanvasGroup>();
		canvas = ((Component)this).GetComponent<Canvas>();
		visible = true;
	}

	public void SetVisible(bool b)
	{
		if (visible != b)
		{
			visible = b;
			if ((Object)(object)canvas != (Object)null)
			{
				((Behaviour)canvas).enabled = b;
			}
			canvasGroup.alpha = (visible ? 1f : 0f);
		}
	}
}
