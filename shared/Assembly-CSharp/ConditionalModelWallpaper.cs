using System;
using Rust.Workshop;
using UnityEngine;

public class ConditionalModelWallpaper : ConditionalModel
{
	public bool softSide;

	public override GameObject InstantiateSkin(BaseEntity parent)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (!onServer && isServer)
		{
			return null;
		}
		GameObject val = gameManager.CreatePrefab(prefab.resourcePath, ((Component)parent).transform, active: false);
		if ((Object)(object)val != (Object)null)
		{
			val.transform.localPosition = worldPosition;
			val.transform.localRotation = worldRotation;
			BuildingBlock buildingBlock = parent as BuildingBlock;
			if ((Object)(object)buildingBlock != (Object)null)
			{
				ItemDefinition itemDefForCategory = WallpaperSettings.GetItemDefForCategory(WallpaperPlanner.Settings.GetCategory(buildingBlock, (!softSide) ? 1 : 0));
				WorkshopSkin.Reset(val);
				SkinHelpers.SetSkin(val, itemDefForCategory, softSide ? buildingBlock.wallpaperID : buildingBlock.wallpaperID2);
				float num = (softSide ? buildingBlock.wallpaperRotation : buildingBlock.wallpaperRotation2);
				if (num != 0f)
				{
					Vector3 localEulerAngles = val.transform.localEulerAngles;
					localEulerAngles.y += num;
					val.transform.localRotation = Quaternion.Euler(localEulerAngles);
				}
				PoolableEx.AwakeFromInstantiate(val);
			}
		}
		return val;
	}

	protected override Type GetIndexedType()
	{
		return typeof(ConditionalModel);
	}
}
