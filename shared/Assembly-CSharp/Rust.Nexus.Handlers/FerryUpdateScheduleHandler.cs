using ProtoBuf.Nexus;
using UnityEngine;

namespace Rust.Nexus.Handlers;

public class FerryUpdateScheduleHandler : BaseNexusRequestHandler<FerryUpdateScheduleRequest>
{
	protected override void Handle()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		NexusFerry nexusFerry = NexusFerry.Get(base.Request.entityId, base.Request.timestamp);
		if ((Object)(object)nexusFerry != (Object)null)
		{
			nexusFerry.UpdateSchedule(base.Request.schedule);
		}
		SendSuccess();
	}
}
