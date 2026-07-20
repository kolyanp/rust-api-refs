namespace Oxide.CSharp.Patching;

public interface IPatch
{
	void Patch(PatchContext context);
}
