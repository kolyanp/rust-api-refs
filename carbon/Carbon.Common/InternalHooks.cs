public static class InternalHooks
{
	public static int OnEntitySaved;

	public static void Handle(string hookName, bool subscribed)
	{
		if (hookName == "OnEntitySaved")
		{
			OnEntitySaved += (subscribed ? 1 : (-1));
		}
	}
}
