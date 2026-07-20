using System.Collections.Generic;
using System.Linq;
using Rust.UI.MainMenu;
using UnityEngine;

public class KeybindCategory : MonoBehaviour
{
	private List<UI_SettingsTweakKeyBind> keybinds = new List<UI_SettingsTweakKeyBind>();

	private void Awake()
	{
		for (int i = ((Component)this).transform.GetSiblingIndex() + 1; i < ((Component)this).transform.parent.childCount; i++)
		{
			Transform child = ((Component)this).transform.parent.GetChild(i);
			if (!((Object)(object)((Component)child).GetComponent<KeybindCategory>() != (Object)null))
			{
				UI_SettingsTweakKeyBind component = ((Component)child).GetComponent<UI_SettingsTweakKeyBind>();
				if (!((Object)(object)component == (Object)null))
				{
					keybinds.Add(component);
				}
				continue;
			}
			break;
		}
	}

	public void UpdateVisibility()
	{
		((Component)this).gameObject.SetActive(keybinds.Any((UI_SettingsTweakKeyBind x) => ((Behaviour)x).isActiveAndEnabled));
	}
}
