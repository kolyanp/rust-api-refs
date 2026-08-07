using UnityEngine;

public class TriggerMonumentIOArea : TriggerBase
{
	internal override GameObject InterestedInObject(GameObject obj)
	{
		if ((Object)(object)obj.GetComponent<BasePlayer>() == (Object)null)
		{
			return null;
		}
		return base.InterestedInObject(obj);
	}
}
