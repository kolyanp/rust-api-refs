using System;
using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json;

[Preserve]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class JsonRequiredAttribute : Attribute
{
}
