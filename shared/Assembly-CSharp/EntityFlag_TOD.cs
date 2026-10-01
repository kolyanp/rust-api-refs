using UnityEngine;

public class EntityFlag_TOD : EntityComponent<BaseEntity>
{
	public BaseEntity.Flags desiredFlag;

	public bool onAtNight = true;

	public void Start()
	{
		Invoke(Initialize, 1f);
	}

	public void Initialize()
	{
		if (!((Object)(object)baseEntity == (Object)null) && !baseEntity.isClient)
		{
			InvokeRandomized(DoTimeCheck, 0f, 5f, 1f);
		}
	}

	public bool WantsOn()
	{
		if ((Object)(object)TOD_Sky.Instance == (Object)null)
		{
			return false;
		}
		bool isNight = TOD_Sky.Instance.IsNight;
		if (onAtNight == isNight)
		{
			return true;
		}
		return false;
	}

	private void DoTimeCheck()
	{
		bool flag = baseEntity.HasFlag(desiredFlag);
		bool flag2 = WantsOn();
		if (flag != flag2)
		{
			using (BaseEntity.FlagsUpdateScope flagsUpdateScope = baseEntity.StartSetFlags(BaseEntity.FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(desiredFlag, flag2);
			}
		}
	}
}
