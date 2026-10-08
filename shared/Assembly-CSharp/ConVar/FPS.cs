using UnityEngine;

namespace ConVar;

[Factory("fps")]
public class FPS : ConsoleSystem
{
	private static int _limit = 240;

	private static int m_graph;

	[ServerVar(Saved = true)]
	[ClientVar(Saved = true, Help = "(Generated) Target application frame rate cap; 0 = unlimited; on client clamped to 240; on server limits tick dispatch rate; saved between sessions")]
	public static int limit
	{
		get
		{
			if (_limit == -1)
			{
				_limit = Application.targetFrameRate;
			}
			return _limit;
		}
		set
		{
			_limit = value;
			Application.targetFrameRate = _limit;
		}
	}

	[ClientVar(Help = "(Generated) Controls the FPS graph overlay mode; 0 = off, higher values show increasingly detailed frame time graphs and performance metrics")]
	public static int graph
	{
		get
		{
			return m_graph;
		}
		set
		{
			m_graph = value;
			if (Object.op_Implicit((Object)(object)MainCamera.mainCamera))
			{
				FPSGraph component = ((Component)MainCamera.mainCamera).GetComponent<FPSGraph>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.Refresh();
				}
			}
		}
	}
}
