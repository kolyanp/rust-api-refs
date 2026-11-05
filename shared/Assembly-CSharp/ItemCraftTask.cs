using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

public class ItemCraftTask : IPooled
{
	public ItemBlueprint blueprint;

	public float endTime;

	public int taskUID;

	public bool cancelled;

	public InstanceData instanceData;

	public int amount = 1;

	public int skinID;

	public List<Item> takenItems;

	public int numCrafted;

	public float conditionScale = 1f;

	public BaseEntity workbenchEntity;

	public int attachmentID;

	public void EnterPool()
	{
		blueprint = null;
		endTime = 0f;
		taskUID = 0;
		cancelled = false;
		instanceData = null;
		amount = 1;
		skinID = 0;
		takenItems = null;
		numCrafted = 0;
		conditionScale = 1f;
		workbenchEntity = null;
		attachmentID = 0;
	}

	public void LeavePool()
	{
	}
}
