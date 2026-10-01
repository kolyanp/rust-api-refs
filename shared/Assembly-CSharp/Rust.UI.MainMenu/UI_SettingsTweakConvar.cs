using UnityEngine.Events;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakConvar : UI_SettingsTweakBase
{
	public string convarName;

	public bool ApplyImmediatelyOnChange = true;

	public UnityEvent onValueChanged = new UnityEvent();

	public UI_SettingsTweakConvar()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
	}
}
