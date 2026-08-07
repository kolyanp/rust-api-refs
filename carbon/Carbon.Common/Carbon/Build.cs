namespace Carbon;

public class Build
{
	public class Git
	{
		public static readonly string Branch = "production";

		public static readonly string Author = "raul";

		public static readonly string Comment = "Merge branch 'rust_beta/release' into production";

		public static readonly string Date = "2026-08-06 16:53:42 +0200";

		public static readonly string Tag = "production_build";

		public static readonly string HashShort = "eb247144";

		public static readonly string HashLong = "eb247144eb058398569142d102764edf074fb05c";

		public static readonly string Url = "https://github.com/CarbonCommunity/Carbon.git/commit/eb247144eb058398569142d102764edf074fb05c";
	}

	public static bool IsDebug => false;
}
