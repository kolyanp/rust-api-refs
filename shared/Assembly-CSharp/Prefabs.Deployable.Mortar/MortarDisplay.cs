using UnityEngine;

namespace Prefabs.Deployable.Mortar;

public class MortarDisplay : FacepunchBehaviour
{
	[SerializeField]
	private RectTransform distanceBar;

	[SerializeField]
	private Vector2 minMaxY;

	[SerializeField]
	private GameObject fragImage;

	[SerializeField]
	private GameObject heImage;

	public void SetPitch(float minMax01)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 anchoredPosition = distanceBar.anchoredPosition;
		anchoredPosition.y = Mathf.Lerp(minMaxY.x, minMaxY.y, minMax01);
		distanceBar.anchoredPosition = anchoredPosition;
	}

	public void SetAmmoIcon(ItemDefinition ammo)
	{
		if (!((Object)(object)ammo == (Object)null))
		{
			bool flag = ammo.shortname.EndsWith("fragment");
			fragImage.SetActive(flag);
			heImage.SetActive(!flag);
		}
	}
}
