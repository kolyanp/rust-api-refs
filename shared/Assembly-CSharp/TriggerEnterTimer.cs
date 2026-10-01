using System.Runtime.CompilerServices;

public class TriggerEnterTimer : TriggerBase
{
	public TimeSince EnterTime
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		bool hasAnyEntityContents = HasAnyEntityContents;
		base.OnEntityEnter(ent);
		if (!hasAnyEntityContents && HasAnyEntityContents)
		{
			EnterTime = TimeSince.op_Implicit(0f);
		}
	}
}
