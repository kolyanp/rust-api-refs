using System;

namespace API.Assembly;

public interface ICarbonAddon
{
	void Awake(EventArgs args);

	void OnLoaded(EventArgs args);

	void OnUnloaded(EventArgs args);
}
