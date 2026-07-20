namespace Oxide.Pooling;

public interface IPoolProvider
{
	void Return(object item);
}
public interface IPoolProvider<out T> : IPoolProvider
{
	T Take();
}
