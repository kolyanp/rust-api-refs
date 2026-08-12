using System.Collections.Generic;
using Oxide.Core;
using UnityEngine;

public class RFManager
{
	private static readonly Dictionary<int, HashSet<IRFObject>> _listeners;

	private static readonly Dictionary<int, HashSet<IRFObject>> _broadcasters;

	private static readonly Dictionary<int, bool> _isFrequencyBroadcasting;

	public static int minFreq;

	public static int maxFreq;

	private static int reserveRangeMin;

	private static int reserveRangeMax;

	public static Phrase reservedFrequencyPhrase;

	public static int ClampFrequency(int freq)
	{
		return Mathf.Clamp(freq, minFreq, maxFreq);
	}

	public static HashSet<IRFObject> GetListenerSet(int frequency)
	{
		frequency = ClampFrequency(frequency);
		if (!_listeners.TryGetValue(frequency, out var value))
		{
			value = new HashSet<IRFObject>();
			_listeners[frequency] = value;
		}
		return value;
	}

	public static HashSet<IRFObject> GetBroadcasterSet(int frequency)
	{
		frequency = ClampFrequency(frequency);
		if (!_broadcasters.TryGetValue(frequency, out var value))
		{
			value = new HashSet<IRFObject>();
			_broadcasters[frequency] = value;
		}
		return value;
	}

	public static void AddListener(int frequency, IRFObject obj)
	{
		frequency = ClampFrequency(frequency);
		if (Interface.CallHook("OnRfListenerAdd", obj, frequency) == null && GetListenerSet(frequency).Add(obj))
		{
			bool flag = _isFrequencyBroadcasting.TryGetValue(frequency, out var value) & value;
			obj.RFSignalUpdate(flag);
			Interface.CallHook("OnRfListenerAdded", obj, frequency);
		}
	}

	public static void RemoveListener(int frequency, IRFObject obj)
	{
		frequency = ClampFrequency(frequency);
		if (Interface.CallHook("OnRfListenerRemove", obj, frequency) == null && GetListenerSet(frequency).Remove(obj))
		{
			obj.RFSignalUpdate(on: false);
			Interface.CallHook("OnRfListenerRemoved", obj, frequency);
		}
	}

	public static void AddBroadcaster(int frequency, IRFObject obj)
	{
		frequency = ClampFrequency(frequency);
		if (Interface.CallHook("OnRfBroadcasterAdd", obj, frequency) != null)
		{
			return;
		}
		HashSet<IRFObject> broadcasterSet = GetBroadcasterSet(frequency);
		if (broadcasterSet.RemoveWhere((IRFObject b) => b == null || !BaseEntityEx.IsValidEntityReference(b)) > 0)
		{
			Debug.LogWarning((object)$"Found null entries in the RF broadcaster set for frequency {frequency}... cleaning up.");
		}
		if (broadcasterSet.Add(obj))
		{
			Interface.CallHook("OnRfBroadcasterAdded", obj, frequency);
			if (!_isFrequencyBroadcasting.TryGetValue(frequency, out var value) || !value)
			{
				_isFrequencyBroadcasting[frequency] = true;
				UpdateListenersForFrequency(frequency, isBroadcasting: true);
			}
		}
	}

	public static void RemoveBroadcaster(int frequency, IRFObject obj)
	{
		frequency = ClampFrequency(frequency);
		if (Interface.CallHook("OnRfBroadcasterRemove", obj, frequency) != null)
		{
			return;
		}
		HashSet<IRFObject> broadcasterSet = GetBroadcasterSet(frequency);
		if (broadcasterSet.RemoveWhere((IRFObject b) => b == null || !BaseEntityEx.IsValidEntityReference(b)) > 0)
		{
			Debug.LogWarning((object)$"Found null entries in the RF broadcaster set for frequency {frequency}... cleaning up.");
		}
		if (broadcasterSet.Remove(obj))
		{
			Interface.CallHook("OnRfBroadcasterRemoved", obj, frequency);
			if (broadcasterSet.Count == 0)
			{
				_isFrequencyBroadcasting[frequency] = false;
				UpdateListenersForFrequency(frequency, isBroadcasting: false);
			}
		}
	}

	private static void UpdateListenersForFrequency(int frequency, bool isBroadcasting)
	{
		HashSet<IRFObject> listenerSet = GetListenerSet(frequency);
		listenerSet.RemoveWhere((IRFObject l) => l == null || !BaseEntityEx.IsValidEntityReference(l));
		foreach (IRFObject item in listenerSet)
		{
			item.RFSignalUpdate(isBroadcasting);
		}
	}

	public static bool IsReserved(int frequency)
	{
		if (frequency >= reserveRangeMin && frequency <= reserveRangeMax)
		{
			return true;
		}
		return false;
	}

	public static void ReserveErrorPrint(BasePlayer player)
	{
		player.ShowToast(GameTip.Styles.Error, reservedFrequencyPhrase, false, reserveRangeMin.ToString(), reserveRangeMax.ToString());
	}

	public static void ChangeFrequency(int oldFrequency, int newFrequency, IRFObject obj, bool isListener, bool isOn = true)
	{
		newFrequency = ClampFrequency(newFrequency);
		if (isListener)
		{
			RemoveListener(oldFrequency, obj);
			if (isOn)
			{
				AddListener(newFrequency, obj);
			}
		}
		else
		{
			RemoveBroadcaster(oldFrequency, obj);
			if (isOn)
			{
				AddBroadcaster(newFrequency, obj);
			}
		}
	}

	static RFManager()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		_listeners = new Dictionary<int, HashSet<IRFObject>>();
		_broadcasters = new Dictionary<int, HashSet<IRFObject>>();
		_isFrequencyBroadcasting = new Dictionary<int, bool>();
		minFreq = 1;
		maxFreq = 999999;
		reserveRangeMin = 4760;
		reserveRangeMax = 4790;
		reservedFrequencyPhrase = new Phrase("rf.reservedfrequency", "Channels {0} to {1} are restricted");
	}
}
