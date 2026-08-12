using UnityEngine;

public class RotateObject : MonoBehaviour
{
	public float rotateSpeed_X;

	public float rotateSpeed_Y;

	public float rotateSpeed_Z;

	public bool localSpace;

	public bool randomizeRotation;

	public Vector3 randomVariationRange;

	private void Start()
	{
		if (randomizeRotation)
		{
			rotateSpeed_Y = Random.Range(0f - Mathf.Abs(rotateSpeed_X), Mathf.Abs(rotateSpeed_X));
			rotateSpeed_Z = Random.Range(0f - Mathf.Abs(rotateSpeed_X), Mathf.Abs(rotateSpeed_X));
		}
		rotateSpeed_X *= 1f + Random.Range(0f - randomVariationRange.x, randomVariationRange.x);
		rotateSpeed_Y *= 1f + Random.Range(0f - randomVariationRange.y, randomVariationRange.y);
		rotateSpeed_Z *= 1f + Random.Range(0f - randomVariationRange.z, randomVariationRange.z);
	}

	protected void Update()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		if (localSpace)
		{
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(rotateSpeed_X, rotateSpeed_Y, rotateSpeed_Z);
			((Component)this).transform.Rotate(val * Time.deltaTime, (Space)1);
			return;
		}
		if (rotateSpeed_X != 0f)
		{
			((Component)this).transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed_X);
		}
		if (rotateSpeed_Y != 0f)
		{
			((Component)this).transform.Rotate(((Component)this).transform.forward, Time.deltaTime * rotateSpeed_Y);
		}
		if (rotateSpeed_Z != 0f)
		{
			((Component)this).transform.Rotate(((Component)this).transform.right, Time.deltaTime * rotateSpeed_Z);
		}
	}

	public RotateObject()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		rotateSpeed_X = 1f;
		rotateSpeed_Y = 1f;
		rotateSpeed_Z = 1f;
		randomVariationRange = new Vector3(0.5f, 0.5f, 0.5f);
		((MonoBehaviour)this)._002Ector();
	}
}
