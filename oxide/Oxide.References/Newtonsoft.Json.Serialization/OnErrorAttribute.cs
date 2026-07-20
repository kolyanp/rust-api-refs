using System;
using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Serialization;

[Preserve]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnErrorAttribute : Attribute
{
}
