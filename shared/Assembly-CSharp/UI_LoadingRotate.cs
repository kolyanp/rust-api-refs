using UnityEngine;

public class UI_LoadingRotate : MonoBehaviour
{
	[SerializeField]
	private Transform RotateImage;

	private bool _keepRotating;

	public void Toggle()
	{
		_keepRotating = !_keepRotating;
	}

	public void ContinuouslyRotate(bool state)
	{
		_keepRotating = state;
	}

	public void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (_keepRotating && !((Object)(object)RotateImage == (Object)null))
		{
			((Component)RotateImage).transform.localEulerAngles = new Vector3(0f, 0f, RotateImage.localEulerAngles.z - Time.deltaTime * 500f);
		}
	}

	public void RotateOnce()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float z = RotateImage.localEulerAngles.z;
		float to = z + 360f;
		LeanTween.value(((Component)RotateImage).gameObject, z, to, 0.5f).setEase(LeanTweenType.linear).setOnUpdate(delegate(float angle, object obj)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Transform val = (Transform)((obj is Transform) ? obj : null);
			if (val != null)
			{
				val.localEulerAngles = new Vector3(0f, 0f, angle);
			}
		}, RotateImage);
	}

	public void Reset()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RotateImage.localEulerAngles = Vector3.zero;
	}
}
