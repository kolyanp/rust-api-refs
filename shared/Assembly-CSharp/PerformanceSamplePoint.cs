using System;

public struct PerformanceSamplePoint
{
	public int UpdateCount;

	public int FixedUpdateCount;

	public int RenderCount;

	public TimeSpan PreCull;

	public TimeSpan Update;

	public TimeSpan LateUpdate;

	public TimeSpan PhysicsUpdate;

	public TimeSpan Render;

	public TimeSpan FixedUpdate;

	public TimeSpan PreLateUpdate;

	public TimeSpan TotalCPU;

	public int CpuUpdateCount;

	public PerformanceSamplePoint Add(PerformanceSamplePoint other)
	{
		return new PerformanceSamplePoint
		{
			UpdateCount = UpdateCount + other.UpdateCount,
			FixedUpdateCount = FixedUpdateCount + other.FixedUpdateCount,
			RenderCount = RenderCount + other.RenderCount,
			PreCull = PreCull + other.PreCull,
			Update = Update + other.Update,
			LateUpdate = LateUpdate + other.LateUpdate,
			PhysicsUpdate = PhysicsUpdate + other.PhysicsUpdate,
			Render = Render + other.Render,
			FixedUpdate = FixedUpdate + other.FixedUpdate,
			PreLateUpdate = PreLateUpdate + other.PreLateUpdate,
			TotalCPU = TotalCPU + other.TotalCPU,
			CpuUpdateCount = CpuUpdateCount + other.CpuUpdateCount
		};
	}
}
