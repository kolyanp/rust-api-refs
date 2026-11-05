using Development.Attributes;
using Network;

[ResetStaticFields]
public abstract class EntityComponentProxyBase
{
	public abstract BaseEntity GetBaseEntity();

	public virtual bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		return false;
	}
}
