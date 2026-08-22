namespace Carbon;

public class Build
{
	public class Git
	{
		public static readonly string Branch = "production";

		public static readonly string Author = "raul";

		public static readonly string Comment = "Merge branch 'main' into production";

		public static readonly string Date = "2026-08-22 21:46:17 +0200";

		public static readonly string Tag = "production_build";

		public static readonly string HashShort = "7c08250";

		public static readonly string HashLong = "7c082506835127d05ac9be78d6dadaecccec3b09";

		public static readonly string Url = "https://github.com/CarbonCommunity/Carbon/commit/7c082506835127d05ac9be78d6dadaecccec3b09";
	}

	public static bool IsDebug => false;
}
