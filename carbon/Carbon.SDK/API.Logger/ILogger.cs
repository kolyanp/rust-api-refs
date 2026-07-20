using System;

namespace API.Logger;

public interface ILogger
{
	void Console(string message, Severity severity = Severity.Notice, Exception exception = null);
}
