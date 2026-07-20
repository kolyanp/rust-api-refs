using System.Runtime.CompilerServices;

namespace System.Diagnostics.Metrics;

[_003Cb470df79_002Da5b8_002D442e_002D9a19_002Ddc7ef3b9a2cb_003EIsReadOnly]
internal struct QuantileValue(double quantile, double value)
{
	public double Quantile { get; } = quantile;

	public double Value { get; } = value;
}
