using UnityEngine;
using UnityEngine.Events;

public class ItemEventFlag : MonoBehaviour, IItemUpdate
{
	public Item.Flag flag;

	public UnityEvent onEnabled;

	public UnityEvent onDisable;

	internal bool firstRun;

	internal bool lastState;

	public virtual void OnItemUpdate(Item item)
	{
		bool flag = item.HasFlag(this.flag);
		if (firstRun || flag != lastState)
		{
			if (flag)
			{
				onEnabled.Invoke();
			}
			else
			{
				onDisable.Invoke();
			}
			lastState = flag;
			firstRun = false;
		}
	}

	public ItemEventFlag()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		onEnabled = new UnityEvent();
		onDisable = new UnityEvent();
		firstRun = true;
		((MonoBehaviour)this)._002Ector();
	}
}
