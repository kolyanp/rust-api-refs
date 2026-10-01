using System;
using UnityEngine;
using UnityEngine.Events;

public class PowergridClientToggle : FacepunchBehaviour, IClientComponent
{
	[Serializable]
	private class StageChangeEvent
	{
		public int requiredPowergridStage;

		public UnityEvent onPowergridStageReached = new UnityEvent();

		public UnityEvent onPowergridStageLost = new UnityEvent();

		public StageChangeEvent()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected Obj, but got Unknown
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected Obj, but got Unknown
		}
	}

	public int requiredPowergridStage = 1;

	[SerializeField]
	private UnityEvent onPowergridStageReached = new UnityEvent();

	[SerializeField]
	private UnityEvent onPowergridStageLost = new UnityEvent();

	[SerializeField]
	private StageChangeEvent[] expandedStageChangeEvents = Array.Empty<StageChangeEvent>();

	public PowergridClientToggle()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
	}
}
