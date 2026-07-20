using Mono.Cecil;

namespace Oxide.CSharp.Patching.Validation;

public class IsSpecialNameAttribute : PatchValidationAttribute
{
	protected override bool IsValid(object item)
	{
		if (item is IMemberDefinition memberDefinition)
		{
			return memberDefinition.IsSpecialName;
		}
		return false;
	}
}
