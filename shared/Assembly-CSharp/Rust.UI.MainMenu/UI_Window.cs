using System;
using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Window : BaseMonoBehaviour
{
	[SerializeField]
	private bool _skipAutoFixState;

	[SerializeField]
	[Header("Window - Transitions")]
	private FlexTransition _openTransition;

	[SerializeField]
	private bool _oneShotTransition;

	[SerializeField]
	[Header("Window - Components")]
	protected CanvasGroup _group;

	[SerializeField]
	protected UIEscapeCapture _escape;

	[SerializeField]
	protected FlexElement _flex;

	protected bool _firstTimeOpened = true;

	protected bool _opened;

	public event Action OnOpen;

	public event Action OnClose;

	protected virtual void Awake()
	{
		if (!_opened && !_skipAutoFixState)
		{
			FixBrokenState();
		}
	}

	private void FixBrokenState()
	{
		using (TimeWarning.New("UI_Window.FixBrokenState"))
		{
			if ((Object)(object)_group == (Object)null)
			{
				if (((Component)this).gameObject.activeSelf)
				{
					((Component)this).gameObject.SetActive(false);
				}
				return;
			}
			if (!((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
			SetUI(state: false);
		}
	}

	[UnityEvent]
	public virtual void Open()
	{
		if (!_opened)
		{
			_opened = true;
			SetUI(state: true);
			OnOpened();
			if (_firstTimeOpened)
			{
				_firstTimeOpened = false;
			}
		}
	}

	[UnityEvent]
	public virtual void Close()
	{
		if (_opened)
		{
			_opened = false;
			SetUI(state: false);
			OnClosed();
		}
	}

	public bool IsOpen()
	{
		return _opened;
	}

	protected virtual void OnOpened()
	{
		this.OnOpen?.Invoke();
		if ((Object)(object)_openTransition != (Object)null)
		{
			if (_oneShotTransition)
			{
				_openTransition.PlayOneOff();
			}
			else
			{
				_openTransition.SwitchState(true, true);
			}
		}
	}

	protected virtual void OnClosed()
	{
		this.OnClose?.Invoke();
		if (Object.op_Implicit((Object)(object)_openTransition))
		{
			_openTransition.SwitchState(false, false);
		}
	}

	public virtual void SetUI(bool state)
	{
		if ((Object)(object)_group == (Object)null)
		{
			((Component)this).gameObject.SetActive(state);
		}
		else
		{
			if (state && !((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
			_group.alpha = (state ? 1 : 0);
			_group.interactable = state;
			_group.blocksRaycasts = state;
		}
		if ((Object)(object)_escape != (Object)null)
		{
			((Behaviour)_escape).enabled = state;
		}
		if ((Object)(object)_flex != (Object)null)
		{
			((Behaviour)_flex).enabled = state;
		}
	}
}
