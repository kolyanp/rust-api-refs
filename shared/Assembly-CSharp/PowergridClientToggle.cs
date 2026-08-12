using System;
using UnityEngine;
using UnityEngine.Events;

public class PowergridClientToggle : FacepunchBehaviour, IClientComponent
{
	[Serializable]
	private class StageChangeEvent
	{
		public int requiredPowergridStage;

		public UnityEvent onPowergridStageReached;

		public UnityEvent onPowergridStageLost;

		public StageChangeEvent()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			onPowergridStageReached = new UnityEvent();
			onPowergridStageLost = new UnityEvent();
			base._002Ector();
		}
	}

	public int requiredPowergridStage;

	[SerializeField]
	private UnityEvent onPowergridStageReached;

	[SerializeField]
	private UnityEvent onPowergridStageLost;

	[SerializeField]
	private StageChangeEvent[] expandedStageChangeEvents;

	public PowergridClientToggle()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		requiredPowergridStage = 1;
		onPowergridStageReached = new UnityEvent();
		onPowergridStageLost = new UnityEvent();
		expandedStageChangeEvents = Array.Empty<StageChangeEvent>();
		base._002Ector();
	}
}
