using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifeInfographicStat : MonoBehaviour
{
	public enum DataType
	{
		None,
		AliveTime_Short,
		SleepingTime_Short,
		KillerName,
		KillerWeapon,
		AliveTime_Long,
		KillerDistance,
		GenericStat,
		DistanceTravelledWalk,
		DistanceTravelledRun,
		DamageTaken,
		DamageHealed,
		WeaponInfo,
		SecondsWilderness,
		SecondsSwimming,
		SecondsInBase,
		SecondsInMonument,
		SecondsFlying,
		SecondsBoating,
		PlayersKilled,
		ScientistsKilled,
		AnimalsKilled,
		SecondsDriving
	}

	public enum WeaponInfoType
	{
		TotalShots,
		ShotsHit,
		ShotsMissed,
		AccuracyPercentage
	}

	public DataType dataSource;

	[Header("Weapon Info")]
	public string targetWeaponName;

	public WeaponInfoType weaponInfoType;

	public TextMeshProUGUI targetText;

	public Image StatImage;

	[NonSerialized]
	public string genericStatKey;
}
