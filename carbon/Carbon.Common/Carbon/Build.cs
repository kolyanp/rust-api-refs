namespace Carbon;

public class Build
{
	public class Git
	{
		public static readonly string Branch = "production";

		public static readonly string Author = "raul";

		public static readonly string Comment = "Merge branch 'rust_beta/release' into production";

		public static readonly string Date = "2026-10-02 11:07:29 +0200";

		public static readonly string Tag = "production_build";

		public static readonly string HashShort = "8a81d70";

		public static readonly string HashLong = "8a81d704288f191b7c4017f74dc284f744e220ac";

		public static readonly string Url = "https://github.com/CarbonCommunity/Carbon/commit/8a81d704288f191b7c4017f74dc284f744e220ac";
	}

	public static bool IsDebug => false;
}
