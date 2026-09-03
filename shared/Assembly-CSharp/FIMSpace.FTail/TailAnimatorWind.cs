using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FTail;

[AddComponentMenu("FImpossible Creations/Tail Animator Utilities/Tail Animator Wind")]
public class TailAnimatorWind : MonoBehaviour, IDropHandler, IEventSystemHandler, IFHierarchyIcon
{
	[Header("In playmode you will find this object in DontDestroyOnLoad")]
	[FPD_Header("Main Wind Setings", 2f, 4f, 2)]
	public float power;

	public float additionalTurbulence;

	public float additionalTurbSpeed;

	[Space(7f)]
	public WindZone SyncWithUnityWindZone;

	public float UnityWindZonePowerMul;

	public float UnityWindZoneTurbMul;

	[Header("Overriding wind if value below different than 0,0,0")]
	public Vector3 overrideWind;

	[Range(0.1f, 1f)]
	[FPD_Header("Procedural Wind Settings (if not syncing and not overriding)", 6f, 4f, 2)]
	public float rapidness;

	[FPD_Suffix(0f, 360f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°", true, 0)]
	public float changesPower;

	[Range(0f, 10f)]
	[Header("Extra")]
	public float turbulenceSpeed;

	[FPD_Header("World Position Turbulence", 6f, 4f, 2)]
	[Tooltip("Increase to make objects next to each other wave in slightly different way")]
	public float worldTurb;

	[Tooltip("If higher no performance cost, it is just a number")]
	public float worldTurbScale;

	public float worldTurbSpeed;

	[FPD_Header("Tail Compoenents Related", 6f, 4f, 2)]
	[Tooltip("When tail is longer then power of wind should be higher")]
	public bool powerDependOnTailLength;

	[Tooltip("Don't destroy on load")]
	public bool persistThroughAllScenes;

	private Vector3 targetWind;

	private Vector3 smoothWind;

	private Vector3 windVeloHelper;

	private Quaternion windOrientation;

	private Quaternion smoothWindOrient;

	private Quaternion smoothWindOrientHelper;

	private float[] randNumbers;

	private float[] randTimes;

	private float[] randSpeeds;

	private int frameOffset;

	private Vector3 finalAddTurbulence;

	private Vector3 addTurbHelper;

	private Vector3 smoothAddTurbulence;

	public string EditorIconPath => "Tail Animator/TailAnimatorWindIconSmall";

	public static TailAnimatorWind Instance { get; private set; }

	public void OnDrop(PointerEventData data)
	{
	}

	private void Awake()
	{
		if (Application.isPlaying)
		{
			Instance = this;
			if (persistThroughAllScenes)
			{
				Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
			}
		}
	}

	public void OnValidate()
	{
		Instance = this;
	}

	private void Update()
	{
		if (frameOffset > 0)
		{
			frameOffset--;
		}
		else
		{
			ComputeWind();
		}
	}

	public static void Refresh()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Debug.Log((object)"[Tail Animator Wind] No Tail Animator Wind component on the scene!");
			Debug.LogWarning((object)"[Tail Animator Wind] No Tail Animator Wind component on the scene!");
		}
	}

	public void AffectTailWithWind(TailAnimator2 t)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		if (!t.UseWind || t.WindEffectPower <= 0f || t.TailSegments.Count <= 0)
		{
			return;
		}
		float num = 1f;
		if (powerDependOnTailLength)
		{
			num = t._TC_TailLength * t.TailSegments[0].transform.lossyScale.z / 5f;
			if (t.TailSegments.Count > 3)
			{
				num *= Mathf.Lerp(0.7f, 3f, (float)t.TailSegments.Count / 14f);
			}
		}
		if (t.WindWorldNoisePower > 0f)
		{
			float num2 = worldTurbSpeed;
			if (Object.op_Implicit((Object)(object)SyncWithUnityWindZone))
			{
				num2 *= SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul;
			}
			float num3 = 0.5f + Mathf.Sin(Time.time * num2 + t.TailSegments[0].ProceduralPosition.x * worldTurbScale) / 2f + (0.5f + Mathf.Cos(Time.time * num2 + t.TailSegments[0].ProceduralPosition.z * worldTurbScale) / 2f);
			num += num3 * worldTurb * t.WindWorldNoisePower;
		}
		num *= t.WindEffectPower;
		if (t.WindTurbulencePower > 0f)
		{
			t.WindEffect = new Vector3(targetWind.x * num + finalAddTurbulence.x * t.WindTurbulencePower, targetWind.y * num + finalAddTurbulence.y * t.WindTurbulencePower, targetWind.z * num + finalAddTurbulence.z * t.WindTurbulencePower);
		}
		else
		{
			t.WindEffect = new Vector3(targetWind.x * num, targetWind.y * num, targetWind.z * num);
		}
	}

	private void Start()
	{
		int num = 10;
		randNumbers = new float[num];
		randTimes = new float[num];
		randSpeeds = new float[num];
		for (int i = 0; i < 10; i++)
		{
			randNumbers[i] = Random.Range(-1000f, 1000f);
			randTimes[i] = Random.Range(-1000f, 1000f);
			randSpeeds[i] = Random.Range(0.18f, 0.7f);
		}
	}

	private void ComputeWind()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		if (Object.op_Implicit((Object)(object)SyncWithUnityWindZone))
		{
			val = ((Component)SyncWithUnityWindZone).transform.forward * SyncWithUnityWindZone.windMain * UnityWindZonePowerMul;
			((Component)this).transform.rotation = ((Component)SyncWithUnityWindZone).transform.rotation;
		}
		else if (overrideWind != Vector3.zero)
		{
			val = overrideWind;
		}
		else
		{
			for (int i = 0; i < 4; i++)
			{
				randTimes[i] += Time.deltaTime * randSpeeds[i] * turbulenceSpeed;
			}
			Quaternion val2 = windOrientation;
			float num = -1f + Mathf.PerlinNoise(randTimes[0], 256f + randTimes[1]) * 2f;
			float num2 = -1f + Mathf.PerlinNoise(0f - randTimes[1], 55f + randTimes[2]) * 2f;
			float num3 = -1f + Mathf.PerlinNoise(0f - randTimes[3], 55f + randTimes[0]) * 2f;
			val2 *= Quaternion.Euler(new Vector3(0f, num2, 0f) * changesPower);
			val2 = Quaternion.Euler(num * (changesPower / 6f), ((Quaternion)(ref val2)).eulerAngles.y, num3 * (changesPower / 6f));
			smoothWindOrient = FEngineering.SmoothDampRotation(smoothWindOrient, val2, ref smoothWindOrientHelper, 1f - rapidness, Time.deltaTime);
			((Component)this).transform.rotation = smoothWindOrient;
			val = smoothWindOrient * Vector3.forward;
		}
		smoothAddTurbulence = Vector3.SmoothDamp(smoothAddTurbulence, GetAddTurbulence() * additionalTurbulence, ref addTurbHelper, 0.05f, float.PositiveInfinity, Time.deltaTime);
		smoothWind = Vector3.SmoothDamp(smoothWind, val, ref windVeloHelper, 0.1f, float.PositiveInfinity, Time.deltaTime);
		for (int j = 7; j < 10; j++)
		{
			randTimes[j] += Time.deltaTime * randSpeeds[j] * turbulenceSpeed;
		}
		float num4 = power * 0.015f;
		num4 *= 0.5f + Mathf.PerlinNoise(randTimes[7] * 2f, 25f + randTimes[8] * 0.5f);
		finalAddTurbulence = smoothAddTurbulence * num4;
		targetWind = smoothWind * num4;
	}

	private Vector3 GetAddTurbulence()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		float num = additionalTurbSpeed;
		if (Object.op_Implicit((Object)(object)SyncWithUnityWindZone))
		{
			num *= SyncWithUnityWindZone.windTurbulence * UnityWindZoneTurbMul;
		}
		for (int i = 4; i < 7; i++)
		{
			randTimes[i] += Time.deltaTime * randSpeeds[i] * num;
		}
		float num2 = -1f + Mathf.PerlinNoise(randTimes[4] + 7.123f, -2.324f + Time.time * 0.24f) * 2f;
		float num3 = -1f + Mathf.PerlinNoise(randTimes[5] - 4.7523f, -25.324f + Time.time * 0.54f) * 2f;
		float num4 = -1f + Mathf.PerlinNoise(randTimes[6] + 1.123f, -63.324f + Time.time * -0.49f) * 2f;
		return new Vector3(num2, num3, num4);
	}

	public TailAnimatorWind()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		power = 1f;
		additionalTurbulence = 1f;
		additionalTurbSpeed = 1f;
		UnityWindZonePowerMul = 2f;
		UnityWindZoneTurbMul = 1f;
		overrideWind = Vector3.zero;
		rapidness = 0.95f;
		changesPower = 90f;
		turbulenceSpeed = 1f;
		worldTurb = 1f;
		worldTurbScale = 512f;
		worldTurbSpeed = 5f;
		powerDependOnTailLength = true;
		targetWind = Vector3.zero;
		smoothWind = Vector3.zero;
		windVeloHelper = Vector3.zero;
		windOrientation = Quaternion.identity;
		smoothWindOrient = Quaternion.identity;
		smoothWindOrientHelper = Quaternion.identity;
		frameOffset = 2;
		finalAddTurbulence = Vector3.zero;
		addTurbHelper = Vector3.zero;
		smoothAddTurbulence = Vector3.zero;
		((MonoBehaviour)this)._002Ector();
	}
}
