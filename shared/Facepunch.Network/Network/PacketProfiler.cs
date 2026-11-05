using System.Runtime.CompilerServices;
using System.Threading;

namespace Network;

public static class PacketProfiler
{
	public static class AnalyticsKeys
	{
		public static string[] MessageType;

		static AnalyticsKeys()
		{
			MessageType = new string[30];
			for (int i = 0; i < 30; i++)
			{
				MessageType[i] = ((Message.Type)i/*cast due to constrained. prefix*/).ToString();
			}
		}
	}

	public static bool enabled = false;

	public static int[] inboundCount = new int[30];

	public static int[] inboundBytes = new int[30];

	public static int[] outboundCount = new int[30];

	public static int[] outboundMsgBytes = new int[30];

	public static int[] outboundSum = new int[30];

	public static int[] outboundBytes = new int[30];

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogSimpleInbound(Message.Type type, int length)
	{
		if (enabled && (int)type < 30)
		{
			inboundCount[(uint)type]++;
			inboundBytes[(uint)type] += length;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LogSimpleOutbound(int type, int connectionCount, int length, int totalLength)
	{
		if (enabled && type < 30)
		{
			Interlocked.Increment(ref outboundCount[type]);
			Interlocked.Add(ref outboundSum[type], connectionCount);
			Interlocked.Add(ref outboundMsgBytes[type], length);
			Interlocked.Add(ref outboundBytes[type], totalLength);
		}
	}

	public static void Reset()
	{
		for (int i = 0; i < 30; i++)
		{
			inboundCount[i] = 0;
			inboundBytes[i] = 0;
			outboundCount[i] = 0;
			outboundSum[i] = 0;
			outboundMsgBytes[i] = 0;
			outboundBytes[i] = 0;
		}
	}
}
