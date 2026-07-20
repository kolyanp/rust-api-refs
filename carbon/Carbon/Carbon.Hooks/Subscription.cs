namespace Carbon.Hooks;

public struct Subscription(string id, string sub)
{
	public string Identifier = id;

	public string Subscriber = sub;
}
