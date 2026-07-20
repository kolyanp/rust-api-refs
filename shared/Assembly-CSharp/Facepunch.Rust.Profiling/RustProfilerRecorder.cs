using Unity.Profiling;

namespace Facepunch.Rust.Profiling;

public struct RustProfilerRecorder(string column, ProfilerCategory category, string sample, int sampleCount = 1, ProfilerRecorderOptions options = (ProfilerRecorderOptions)24)
{
	public string ColumnName = column;

	public ProfilerRecorder Recorder = ProfilerRecorder.StartNew(category, sample, sampleCount, options);
}
