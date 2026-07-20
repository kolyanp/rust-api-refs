namespace System.Numerics.Hashing;

internal static class _003Cc139fa64_002Dff40_002D4487_002D9647_002D1f385a5dbff6_003EHashHelpers
{
	public static readonly int RandomSeed = Guid.NewGuid().GetHashCode();

	public static int Combine(int h1, int h2)
	{
		uint num = (uint)((h1 << 5) | (h1 >>> 27));
		return ((int)num + h1) ^ h2;
	}
}
