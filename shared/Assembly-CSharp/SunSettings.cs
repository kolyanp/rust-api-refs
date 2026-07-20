using ConVar;
using UnityEngine;

[ExecuteAlways]
public class SunSettings : MonoBehaviour, IClientComponent
{
	private static readonly int mainLightDirectionId = Shader.PropertyToID("_MainLightDir");

	private static readonly int mainLightColorId = Shader.PropertyToID("_MainLightColor");

	private Light light;

	private void OnEnable()
	{
		light = ((Component)this).GetComponent<Light>();
	}

	private void Update()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying)
		{
			LightShadows val = (LightShadows)Mathf.Clamp(Graphics.shadowmode, 1, 2);
			if (light.shadows != val)
			{
				light.shadows = val;
			}
		}
		if ((Object)(object)light != (Object)null)
		{
			int num = mainLightColorId;
			Color color = light.color;
			Shader.SetGlobalColor(num, ((Color)(ref color)).linear * light.intensity);
			Shader.SetGlobalVector(mainLightDirectionId, Vector4.op_Implicit(((Component)light).transform.forward));
		}
	}
}
