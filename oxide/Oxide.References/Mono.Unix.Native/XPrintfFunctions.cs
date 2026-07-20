namespace Mono.Unix.Native;

internal class XPrintfFunctions
{
	internal delegate object XPrintf(object[] parameters);

	internal static XPrintf printf;

	internal static XPrintf fprintf;

	internal static XPrintf snprintf;

	internal static XPrintf syslog;

	static XPrintfFunctions()
	{
		CdeclFunction cdeclFunction = new CdeclFunction("msvcrt", "printf", typeof(int));
		printf = cdeclFunction.Invoke;
		CdeclFunction cdeclFunction2 = new CdeclFunction("msvcrt", "fprintf", typeof(int));
		fprintf = cdeclFunction2.Invoke;
		CdeclFunction cdeclFunction3 = new CdeclFunction("MonoPosixHelper", "Mono_Posix_Stdlib_snprintf", typeof(int));
		snprintf = cdeclFunction3.Invoke;
		CdeclFunction cdeclFunction4 = new CdeclFunction("MonoPosixHelper", "Mono_Posix_Stdlib_syslog2", typeof(int));
		syslog = cdeclFunction4.Invoke;
	}
}
