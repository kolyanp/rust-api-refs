namespace CompanionServer;

public static class BackhaulProtocol
{
	public const string PathPrefix = "/backhaul/";

	public const int HeaderSize = 5;

	public const int MaxFrameSize = 2097152;

	public const int MaxPayloadSize = 2097147;

	public const int MaxPlayerMessageSize = 1048576;

	public const byte OpData = 0;

	public const byte OpOpen = 1;

	public const byte OpClose = 2;
}
