using UnityEngine.Events;

namespace Rust.UI.MainMenu;

public class UI_SettingsTweakConvar : UI_SettingsTweakBase
{
	public string convarName;

	public bool ApplyImmediatelyOnChange;

	public UnityEvent onValueChanged;

	public UI_SettingsTweakConvar()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		ApplyImmediatelyOnChange = true;
		onValueChanged = new UnityEvent();
		base._002Ector();
	}
}
