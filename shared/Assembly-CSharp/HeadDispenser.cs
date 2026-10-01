using UnityEngine;

public class HeadDispenser : EntityComponent<BaseEntity>
{
	public ItemDefinition HeadDef;

	public GameObjectRef SourceEntity;

	public ItemDefinition OverrideClothing;

	private bool hasDispensed;

	public BaseEntity overrideEntity { get; set; }

	public void DispenseHead(HitInfo info, BaseCorpse corpse)
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (hasDispensed || !(info.Weapon is BaseMelee baseMelee) || !baseMelee.gathering.ProduceHeadItem)
		{
			return;
		}
		if ((Object)(object)info.InitiatorPlayer != (Object)null)
		{
			Item item = ItemManager.CreateByItemID(HeadDef.itemid, 1, 0uL, 0uL);
			item.SetItemOwnership(info.InitiatorPlayer, ItemOwnershipPhrases.Beheaded);
			HeadEntity associatedEntity = ItemModAssociatedEntity<HeadEntity>.GetAssociatedEntity(item);
			BaseEntity baseEntity = (((Object)(object)overrideEntity != (Object)null) ? overrideEntity : SourceEntity.GetEntity());
			overrideEntity = null;
			if ((Object)(object)associatedEntity != (Object)null && (Object)(object)baseEntity != (Object)null)
			{
				associatedEntity.SetupSourceId(baseEntity.prefabID);
				if ((!((Object)(object)corpse != (Object)null) || !corpse.FillHeadData(associatedEntity)) && (Object)(object)OverrideClothing != (Object)null)
				{
					associatedEntity.AssignClothing(OverrideClothing);
				}
			}
			if (info.InitiatorPlayer.inventory.GiveItem(item))
			{
				info.InitiatorPlayer.Command("note.inv", HeadDef.itemid, 1);
			}
			else
			{
				item.DropAndTossUpwards(info.HitPositionWorld);
			}
		}
		hasDispensed = true;
	}
}
