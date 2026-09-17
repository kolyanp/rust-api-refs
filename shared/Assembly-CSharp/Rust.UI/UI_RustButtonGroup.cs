using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Rust.UI;

public class UI_RustButtonGroup : MonoBehaviour
{
	[Header("Button Group")]
	[SerializeField]
	protected List<RustButton> _buttons = new List<RustButton>();

	[SerializeField]
	protected bool _unpressSiblings;

	[SerializeField]
	[Tooltip("This button will appear 'pressed' at the beginning.")]
	private RustButton _defaultButton;

	[SerializeField]
	private bool _defaultButtonFiresEvent = true;

	[SerializeField]
	private bool _allowToggleOff;

	public List<RustButton> Buttons => _buttons;

	private void Start()
	{
		SetupButtons();
	}

	public void AddListenerToGroup(UnityAction action)
	{
		if (_buttons.Count <= 0)
		{
			Debug.LogError((object)"No Buttons found in group.");
			return;
		}
		foreach (RustButton button in _buttons)
		{
			if (!((Object)(object)button == (Object)null))
			{
				button.OnPressed.AddListener(action);
			}
		}
	}

	public void AddListenerToIndex(int index, UnityAction action)
	{
		if (_buttons.Count <= 0)
		{
			Debug.LogError((object)"No Buttons found in group.");
		}
		else if (_buttons.Count - 1 < index)
		{
			Debug.LogError((object)$"No Buttons found at index {index}.");
		}
		else if ((Object)(object)_buttons[index] == (Object)null)
		{
			Debug.LogError((object)$"Button at index {index} is null.");
		}
		else
		{
			_buttons[index].OnPressed.AddListener(action);
		}
	}

	public void EnableButton(int index)
	{
		if (_buttons.Count <= index)
		{
			Debug.LogError((object)$"Button with index {index} doesn't exist.");
		}
		else
		{
			_buttons[index].SetToggleTrue(true);
		}
	}

	public virtual void SetupButtons()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		if (_buttons.Count <= 0)
		{
			Debug.LogError((object)"No Buttons found in group.");
			return;
		}
		foreach (RustButton button in _buttons)
		{
			if ((Object)(object)button == (Object)null)
			{
				continue;
			}
			if ((Object)(object)_defaultButton != (Object)null && (Object)(object)button == (Object)(object)_defaultButton)
			{
				button.PreventToggleOff = false;
				button.SetToggleTrue(_defaultButtonFiresEvent);
			}
			if (!_unpressSiblings)
			{
				continue;
			}
			button.OnToggleEnabled.AddListener((UnityAction)delegate
			{
				if (!_allowToggleOff)
				{
					button.PreventToggleOff = true;
				}
				UnpressSiblings(button);
			});
		}
	}

	public void UnpressSiblings(RustButton thisButton)
	{
		foreach (RustButton button in _buttons)
		{
			if (!((Object)(object)thisButton == (Object)(object)button) && !((Object)(object)button == (Object)null))
			{
				button.PreventToggleOff = false;
				button.SetToggleFalse(true);
			}
		}
	}

	public void AddButton(RustButton button)
	{
		_buttons.Add(button);
	}
}
