using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class GlobalEx
{
	public static bool IsSteamId(this string id)
	{
		if (ulong.TryParse(id, out var result))
		{
			return result.IsSteamId();
		}
		return false;
	}

	public static bool IsSteamId(this ulong id)
	{
		return id > 76561197960265728L;
	}

	public static bool IsSteamId(this EncryptedValue<ulong> userID)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EncryptedValue<ulong>.op_Implicit(userID).IsSteamId();
	}

	[Obsolete("This method is deprecated! Use effect.Clear() instead.")]
	public static void Clear(this Effect effect, bool _)
	{
		effect.Clear();
	}

	public static void CancelAllInvokes(this FacepunchBehaviour behaviour)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		PooledList<InvokeAction> val = Pool.Get<PooledList<InvokeAction>>();
		try
		{
			InvokeHandler.FindInvokes((Behaviour)(object)behaviour, (List<InvokeAction>)(object)val);
			for (int i = 0; i < ((List<InvokeAction>)(object)val).Count; i++)
			{
				InvokeAction val2 = ((List<InvokeAction>)(object)val)[i];
				if (val2.action != null)
				{
					behaviour.CancelInvokeFixedTime(val2.action);
					behaviour.CancelInvoke(val2.action);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
