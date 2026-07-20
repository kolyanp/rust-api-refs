using Newtonsoft.Json.Shims;

namespace Newtonsoft.Json.Linq.JsonPath;

[Preserve]
internal enum QueryOperator
{
	None,
	Equals,
	NotEquals,
	Exists,
	LessThan,
	LessThanOrEquals,
	GreaterThan,
	GreaterThanOrEquals,
	And,
	Or
}
