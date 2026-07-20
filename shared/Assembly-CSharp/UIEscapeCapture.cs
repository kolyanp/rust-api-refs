using System;
using UnityEngine;
using UnityEngine.Events;

public class UIEscapeCapture : ListComponent<UIEscapeCapture>
{
	public UnityEvent onEscape = new UnityEvent();

	[Tooltip("If true, pressing escape will call only this callback and not any others.")]
	public bool blockOtherCallbacks = true;

	[Tooltip("Set this to true if you want this EscapeCapture to take priority over any older EscapeCapture when enabled. Surely this should be default?")]
	public bool insertAtTop = true;

	[ClientVar(ClientAdmin = true, Help = "(Generated) When enabled, draws debug visualisations for this system (seismic sensor range sphere, escape capture state, etc.); editor/admin-only")]
	public static bool debug;

	public override void Setup()
	{
		if (!ListComponent<UIEscapeCapture>.InstanceList.Contains(this))
		{
			if (insertAtTop && ListComponent<UIEscapeCapture>.InstanceList.Count > 0)
			{
				ListComponent<UIEscapeCapture>.InstanceList.Insert(0, this);
			}
			else
			{
				ListComponent<UIEscapeCapture>.InstanceList.Add(this);
			}
		}
	}

	[UnityEvent]
	public static bool EscapePressed()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<UIEscapeCapture> enumerator = ListComponent<UIEscapeCapture>.InstanceList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				UIEscapeCapture current = enumerator.Current;
				if (debug)
				{
					Debug.Log((object)("Escape key pressed by: " + ((object)current).GetType().Name + " - " + ((Object)current).name));
				}
				current.onEscape.Invoke();
				if (current.blockOtherCallbacks)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}
}
