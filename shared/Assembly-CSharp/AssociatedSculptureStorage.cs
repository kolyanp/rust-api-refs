public class AssociatedSculptureStorage : BaseEntity
{
	public override bool ShouldTransferAssociatedFiles => true;

	public void InitFromSculpture(NetworkableId id)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.Reassign(id, net.ID);
	}

	internal override void DoServerDestroy()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		FileStorage.server.RemoveAll(net.ID);
	}
}
