using UnityEngine;

public class TriggerParachuteForceVolume : TriggerBase
{
	[SerializeField]
	private float SpeedMultiplier = 0.8f;

	internal override GameObject InterestedInObject(GameObject obj)
	{
		if ((Object)(object)obj.GetComponent<Parachute>() == (Object)null)
		{
			return null;
		}
		return base.InterestedInObject(obj);
	}

	public float GetSpeedMultiplierForParachute(Parachute p)
	{
		return SpeedMultiplier;
	}
}
