using UnityEngine;

namespace ConVar.Misc;

public class NoBuildZoneMaterialController : SingletonComponent<NoBuildZoneMaterialController>
{
	[SerializeField]
	private Material noBuildZoneMaterial;

	private static readonly int DistanceWarningStrength = Shader.PropertyToID("_DistanceWarningStrength");

	private static readonly int DistanceWarningStrengthNight = Shader.PropertyToID("_DistanceWarningStrengthNight");

	private static readonly int DistanceWarningMinHeight = Shader.PropertyToID("_DistanceWarningMinHeight");

	[ClientVar(ClientAdmin = true, Help = "[0.0-1.0]")]
	public static void SetStrengthDay(ConsoleSystem.Arg arg)
	{
		float def = SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.GetFloat(DistanceWarningStrength);
		float num = arg.GetFloat(0, def);
		SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.SetFloat(DistanceWarningStrength, num);
	}

	[ClientVar(ClientAdmin = true, Help = "[0.0-1.0]")]
	public static void SetStrengthNight(ConsoleSystem.Arg arg)
	{
		float def = SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.GetFloat(DistanceWarningStrengthNight);
		float num = arg.GetFloat(0, def);
		SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.SetFloat(DistanceWarningStrengthNight, num);
	}

	[ClientVar(ClientAdmin = true, Help = "[0.0-10]")]
	public static void SetHeight(ConsoleSystem.Arg arg)
	{
		float def = SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.GetFloat(DistanceWarningMinHeight);
		float num = arg.GetFloat(0, def);
		SingletonComponent<NoBuildZoneMaterialController>.Instance.noBuildZoneMaterial.SetFloat(DistanceWarningMinHeight, num);
	}
}
