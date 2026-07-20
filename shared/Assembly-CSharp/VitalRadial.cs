using UnityEngine;

public class VitalRadial : MonoBehaviour
{
	private void Awake()
	{
		Debug.LogWarning((object)("VitalRadial is obsolete " + TransformEx.GetRecursiveName(((Component)this).transform)), (Object)(object)((Component)this).gameObject);
	}
}
