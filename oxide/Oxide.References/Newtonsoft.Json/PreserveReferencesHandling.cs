using System;
using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json;

[Preserve]
[Flags]
public enum PreserveReferencesHandling
{
	None = 0,
	Objects = 1,
	Arrays = 2,
	All = 3
}
