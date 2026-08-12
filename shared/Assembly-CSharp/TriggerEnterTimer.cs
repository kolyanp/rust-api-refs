using System.Runtime.CompilerServices;

public class TriggerEnterTimer : TriggerBase
{
	[CompilerGenerated]
	private TimeSince _003CEnterTime_003Ek__BackingField;

	public TimeSince EnterTime
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CEnterTime_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CEnterTime_003Ek__BackingField = value;
		}
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		bool hasAnyEntityContents = base.HasAnyEntityContents;
		base.OnEntityEnter(ent);
		if (!hasAnyEntityContents && base.HasAnyEntityContents)
		{
			EnterTime = TimeSince.op_Implicit(0f);
		}
	}
}
