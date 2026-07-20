using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.Metrics;

internal delegate void MeasurementCallback<T>([_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(1)] Instrument instrument, T measurement, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(new byte[] { 0, 0, 1, 2 })] ReadOnlySpan<KeyValuePair<string, object>> tags, [_003Ccb19cdbf_002D9746_002D4655_002D867e_002D0e010acfd54a_003ENullable(2)] object state) where T : struct;
