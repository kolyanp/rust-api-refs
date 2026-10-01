using System.Diagnostics;
using UnityEngine;

public class EntityDebug : EntityComponent<BaseEntity>
{
	internal Stopwatch stopwatch = Stopwatch.StartNew();

	private void Update()
	{
		if (!baseEntity.IsValid() || !baseEntity.IsDebugging())
		{
			((Behaviour)this).enabled = false;
		}
		else if (!(stopwatch.Elapsed.TotalSeconds < 0.5))
		{
			_ = baseEntity.isClient;
			if (baseEntity.isServer)
			{
				baseEntity.DebugServer(1, (float)stopwatch.Elapsed.TotalSeconds);
			}
			stopwatch.Reset();
			stopwatch.Start();
		}
	}
}
