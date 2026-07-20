using System;

namespace API.Assembly;

public interface ICarbonComponent : ICarbonAddon
{
	void OnEnable(EventArgs args);

	void OnDisable(EventArgs args);
}
