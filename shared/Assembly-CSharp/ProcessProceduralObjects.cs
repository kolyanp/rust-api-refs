using System.Collections.Generic;
using UnityEngine;

public class ProcessProceduralObjects : ProceduralComponent
{
	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		List<ProceduralObject> proceduralObjects = SingletonComponent<WorldSetup>.Instance.ProceduralObjects;
		foreach (ProceduralObject item in proceduralObjects)
		{
			if (Object.op_Implicit((Object)(object)item))
			{
				if (World.Cached)
				{
					item.ProcessCached();
				}
				else
				{
					item.Process();
				}
			}
		}
		proceduralObjects.Clear();
	}
}
