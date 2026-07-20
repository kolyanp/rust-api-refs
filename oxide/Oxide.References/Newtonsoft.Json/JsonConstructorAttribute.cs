using System;
using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json;

[Preserve]
[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
public sealed class JsonConstructorAttribute : Attribute
{
}
