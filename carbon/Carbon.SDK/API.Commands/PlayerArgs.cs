namespace API.Commands;

public class PlayerArgs : Command.Args
{
	public object Player { get; set; }

	public bool GetPlayer<T>(out T value) where T : class
	{
		return (value = (T)Player) != null;
	}

	public override void EnterPool()
	{
		base.EnterPool();
		Player = null;
	}
}
