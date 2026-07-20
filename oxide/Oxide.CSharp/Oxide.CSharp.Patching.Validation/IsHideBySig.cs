using Mono.Cecil;

namespace Oxide.CSharp.Patching.Validation;

public class IsHideBySig : PatchValidationAttribute
{
	protected override bool IsValid(object item)
	{
		if (item is IMemberDefinition)
		{
			return PatchValidationAttribute.GetPropertyValue(item, "IsHideBySig", defaultValue: false);
		}
		return false;
	}
}
