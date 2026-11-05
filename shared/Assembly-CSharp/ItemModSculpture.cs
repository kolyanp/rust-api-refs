using UnityEngine;

public class ItemModSculpture : ItemModAssociatedEntity<AssociatedSculptureStorage>
{
	protected override bool AllowNullParenting => true;

	protected override bool ShouldAutoCreateEntity => false;

	public void OnSculpturePickUp(NetworkableId id, Item item)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		AssociatedSculptureStorage associatedSculptureStorage = CreateAssociatedEntity(item);
		if ((Object)(object)associatedSculptureStorage != (Object)null)
		{
			associatedSculptureStorage.InitFromSculpture(id);
		}
	}
}
