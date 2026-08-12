using UnityEngine;

public class BackpackInventoryPanel : MonoBehaviour
{
	private ItemIcon icon;

	public GameObject[] ShowWhenSelected;

	public GameObject[] HideWhenSelected;

	public Vector3 BackpackTargetRotation;

	public Vector3 BackpackTargetUIShift;

	public AnimationCurve BackpackModelRotateCurve;

	public AnimationCurve BackpackUIShiftCurve;

	public AnimationCurve BackpackTransparencyCurve;

	public AnimationCurve FadeOutWhenOpenCurve;

	public float OpenScale;

	public float CloseScale;

	public float BackpackAlphaActiveThreshold;

	public RectTransform PreviewModelRectTransform;

	public CanvasGroup BackpackInventoryCanvas;

	public CanvasGroup[] FadeOutWhenOpen;

	public BackpackInventoryPanel()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		BackpackTargetRotation = new Vector3(0f, 0f, 0f);
		BackpackTargetUIShift = Vector2.op_Implicit(new Vector2(160f, 0f));
		OpenScale = 1f;
		CloseScale = 1f;
		BackpackAlphaActiveThreshold = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
