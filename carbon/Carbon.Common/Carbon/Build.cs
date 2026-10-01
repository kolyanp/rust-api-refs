namespace Carbon;

public class Build
{
	public class Git
	{
		public static readonly string Branch = "production";

		public static readonly string Author = "raul";

		public static readonly string Comment = "Merge branch 'rust_beta/release' into production";

		public static readonly string Date = "2026-10-01 18:53:35 +0200";

		public static readonly string Tag = "production_build";

		public static readonly string HashShort = "c74c4ca";

		public static readonly string HashLong = "c74c4ca8d0f7a9c8e8a431b077c0b3010f476e44";

		public static readonly string Url = "https://github.com/CarbonCommunity/Carbon/commit/c74c4ca8d0f7a9c8e8a431b077c0b3010f476e44";
	}

	public static bool IsDebug => false;
}
