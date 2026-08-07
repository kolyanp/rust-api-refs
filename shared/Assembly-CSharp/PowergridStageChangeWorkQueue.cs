using System;
using System.Collections.Generic;
using System.Diagnostics;
using Facepunch;
using UnityEngine;

public class PowergridStageChangeWorkQueue : PersistentObjectWorkQueue
{
	private int currentIndex;

	private bool isRunning;

	private double lastUpdateTime;

	private double cooldownTimer;

	private readonly HashSet<IPowergridEntity> skipEntities = new HashSet<IPowergridEntity>();

	private Stopwatch stopwatch = new Stopwatch();

	private List<PowergridManager.PowergridEntityEntry> powergridEntities { get; }

	public override int ListLength => powergridEntities?.Count ?? 0;

	public bool IsRunning => isRunning;

	public PowergridStageChangeWorkQueue(List<PowergridManager.PowergridEntityEntry> entitiesList)
	{
		base.Name = ((object)this).GetType().FullName;
		PersistentObjectWorkQueue.All.Add((PersistentObjectWorkQueue)(object)this);
		powergridEntities = entitiesList;
	}

	public void StopWorkQueue()
	{
		currentIndex = 0;
		isRunning = false;
		skipEntities.Clear();
	}

	public void StartWorkQueue()
	{
		isRunning = true;
		lastUpdateTime = Time.timeAsDouble;
	}

	public void RestartWorkQueue()
	{
		StopWorkQueue();
		StartWorkQueue();
	}

	public void OnEntityInserted(int index, IPowergridEntity entity)
	{
		if (isRunning)
		{
			skipEntities.Add(entity);
			if (index < currentIndex)
			{
				currentIndex++;
			}
		}
	}

	public void OnEntityRemoved(int index, IPowergridEntity entity)
	{
		if (isRunning)
		{
			skipEntities.Remove(entity);
			if (index < currentIndex)
			{
				currentIndex--;
			}
		}
	}

	public void RunList(double maximumMilliseconds)
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		double timeAsDouble = Time.timeAsDouble;
		double num = timeAsDouble - lastUpdateTime;
		lastUpdateTime = timeAsDouble;
		((WorkQueueTelemStats)(ref base.Stats)).Clear();
		base.Stats.BudgetTime = ((maximumMilliseconds >= 1000.0) ? default(TimeSpan) : TimeSpanExt.FromMicroseconds(maximumMilliseconds));
		if (!isRunning)
		{
			return;
		}
		if (Powergrid.stageChangeWorkQueueDelayBetweenJobs > 0f)
		{
			cooldownTimer += num;
			if (cooldownTimer < (double)Powergrid.stageChangeWorkQueueDelayBetweenJobs)
			{
				return;
			}
		}
		cooldownTimer = 0.0;
		int listLength = ((PersistentObjectWorkQueue)this).ListLength;
		if (currentIndex >= listLength || listLength == 0)
		{
			StopWorkQueue();
			return;
		}
		int num2 = listLength;
		using (TimeWarning.New(base.Name, (int)base.WarningThreshold.TotalMilliseconds))
		{
			stopwatch.Restart();
			Vector3? val = null;
			while (currentIndex < num2)
			{
				IPowergridEntity entity = powergridEntities[currentIndex].Entity;
				_ = powergridEntities[currentIndex];
				if (entity != null && !skipEntities.Contains(entity))
				{
					PowergridManager serverInstance = PointEntity<PowergridManager>.ServerInstance;
					entity.Server_OnPowergridStageChanged(serverInstance.CurrentStage);
					Vector3 valueOrDefault = val.GetValueOrDefault();
					if (!val.HasValue)
					{
						valueOrDefault = ((Component)entity.GetEntity()).transform.position;
						val = valueOrDefault;
					}
					base.Stats.ProcessedCount++;
				}
				currentIndex++;
				if (currentIndex >= num2)
				{
					StopWorkQueue();
					break;
				}
				if (base.Stats.ProcessedCount > 0 && Powergrid.stageChangeWorkQueueDelayBetweenJobs > 0f)
				{
					bool flag = true;
					if (Powergrid.stageChangeWorkQueueGroupJobsDistance > 0f)
					{
						IPowergridEntity entity2 = powergridEntities[currentIndex].Entity;
						if (Vector3.SqrMagnitude(val.Value - ((Component)entity2.GetEntity()).transform.position) <= Powergrid.stageChangeWorkQueueGroupJobsSqrDistance)
						{
							flag = false;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (stopwatch.Elapsed.TotalMilliseconds >= maximumMilliseconds)
				{
					break;
				}
			}
		}
		base.Stats.QueueCount = num2;
		base.Stats.ExecutionTime = stopwatch.Elapsed;
		base.TotalExecutionTime += base.Stats.ExecutionTime;
	}

	public string Info()
	{
		return string.Format("{0:n0}, lastCount: {1:n0}, lastMS: {2:R}, totMS: {3:n0}", new object[4]
		{
			((PersistentObjectWorkQueue)this).ListLength,
			base.Stats.ProcessedCount,
			base.Stats.ExecutionTime.TotalMilliseconds,
			base.TotalExecutionTime.TotalMilliseconds
		});
	}
}
