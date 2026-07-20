using System;

namespace Carbon.Extensions;

public static class ExceptionEx
{
	public static string GetFullStackTrace(this Exception exception, bool mainMessage = true)
	{
		string text = (mainMessage ? exception.ToString() : exception.StackTrace);
		for (Exception innerException = exception.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			text = text + "\n  Inner exception:\n  " + innerException;
		}
		return text;
	}
}
