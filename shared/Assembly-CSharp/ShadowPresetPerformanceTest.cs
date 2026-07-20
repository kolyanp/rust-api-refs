using System.Text;
using System.Threading.Tasks;
using ConVar;
using UnityEngine;

public static class ShadowPresetPerformanceTest
{
	private struct TestResultAverages
	{
		public float totalCpuTime;

		public float totalGpuTime;

		public float cpuRenderThreadTime;

		public float cpuMainThreadPresentWaitTime;
	}

	private const int FRAMES_TO_CAPTURE = 100;

	public static async void RunAll()
	{
		Debug.Log((object)"Shadow Preset Performance Test Started...");
		int originalPreset = GraphicsSettings.shadowQualityPreset;
		StringBuilder performanceLog = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			ChangePreset(i);
			await Task.Delay(3000);
			ResultsToStringBuilder(CaptureTimings(), performanceLog);
		}
		ChangePreset(originalPreset);
		Debug.Log((object)performanceLog.ToString());
	}

	public static void RunTestWithCurrentPreset()
	{
		TestResultAverages resultAverages = CaptureTimings();
		StringBuilder stringBuilder = new StringBuilder();
		ResultsToStringBuilder(resultAverages, stringBuilder);
		Debug.Log((object)stringBuilder.ToString());
	}

	private static void ResultsToStringBuilder(TestResultAverages resultAverages, StringBuilder performanceLog)
	{
		performanceLog.AppendLine("Timings for Shadow Preset " + GraphicsSettings.shadowQualityPreset + ":");
		performanceLog.AppendLine($"CPU Time: {resultAverages.totalCpuTime} ms");
		performanceLog.AppendLine($"GPU Time: {resultAverages.totalGpuTime} ms");
		performanceLog.AppendLine($"CPU Render Thread Time: {resultAverages.cpuRenderThreadTime} ms");
		performanceLog.AppendLine($"CPU Main Thread Present Wait Time: {resultAverages.cpuMainThreadPresentWaitTime} ms");
	}

	public static void ChangePreset(int value)
	{
		ConsoleSystem.Run(ConsoleSystem.Option.Client.Quiet(), "graphicssettings.shadowqualitypreset", value);
	}

	private static TestResultAverages CaptureTimings()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		FrameTiming[] array = (FrameTiming[])(object)new FrameTiming[100];
		FrameTimingManager.CaptureFrameTimings();
		FrameTimingManager.GetLatestTimings(100u, array);
		TestResultAverages result = new TestResultAverages
		{
			totalCpuTime = 0f,
			totalGpuTime = 0f
		};
		FrameTiming[] array2 = array;
		foreach (FrameTiming val in array2)
		{
			result.totalCpuTime += (float)val.cpuFrameTime;
			result.totalGpuTime += (float)val.cpuFrameTime;
			result.cpuRenderThreadTime += (float)val.cpuRenderThreadFrameTime;
			result.cpuMainThreadPresentWaitTime = (float)val.cpuMainThreadPresentWaitTime;
		}
		result.totalCpuTime /= array.Length;
		result.totalGpuTime /= array.Length;
		result.cpuRenderThreadTime /= array.Length;
		result.cpuMainThreadPresentWaitTime /= array.Length;
		return result;
	}
}
