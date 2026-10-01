using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public static class SafeAnimatorControllerPlayableEx
{
	public static int GetLayerCount(this SafePlayable<AnimatorControllerPlayable> playable)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetLayerCount();
	}

	public static bool HasLayer(this SafePlayable<AnimatorControllerPlayable> playable, int layer)
	{
		if (layer >= 0)
		{
			return layer < playable.GetLayerCount();
		}
		return false;
	}

	public static float GetLayerWeight(this SafePlayable<AnimatorControllerPlayable> playable, int layer)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.HasLayer(layer))
		{
			return 0f;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetLayerWeight(layer);
	}

	public static void SetLayerWeight(this SafePlayable<AnimatorControllerPlayable> playable, int layer, float weight)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.HasLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetLayerWeight(layer, weight);
		}
	}

	public static AnimatorStateInfo GetCurrentAnimatorStateInfo(this SafePlayable<AnimatorControllerPlayable> playable, int layer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.HasLayer(layer))
		{
			return default;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetCurrentAnimatorStateInfo(layer);
	}

	public static AnimatorStateInfo GetNextAnimatorStateInfo(this SafePlayable<AnimatorControllerPlayable> playable, int layer)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.HasLayer(layer))
		{
			return default;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetNextAnimatorStateInfo(layer);
	}

	public static void GetCurrentAnimatorClipInfo(this SafePlayable<AnimatorControllerPlayable> playable, int layer, List<AnimatorClipInfo> clips)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		clips.Clear();
		if (playable.HasLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.GetCurrentAnimatorClipInfo(layer, clips);
		}
	}

	public static void GetNextAnimatorClipInfo(this SafePlayable<AnimatorControllerPlayable> playable, int layer, List<AnimatorClipInfo> clips)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		clips.Clear();
		if (playable.HasLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.GetNextAnimatorClipInfo(layer, clips);
		}
	}

	public static bool IsInTransition(this SafePlayable<AnimatorControllerPlayable> playable, int layer)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.HasLayer(layer))
		{
			return false;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.IsInTransition(layer);
	}

	public static bool CanPlayOnLayer(this SafePlayable<AnimatorControllerPlayable> playable, int layerOrAnyLayerWhenNegative)
	{
		if (layerOrAnyLayerWhenNegative >= 0)
		{
			return playable.HasLayer(layerOrAnyLayerWhenNegative);
		}
		return playable.IsValid();
	}

	public static void Play(this SafePlayable<AnimatorControllerPlayable> playable, string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.CanPlayOnLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.Play(stateName, layer, normalizedTime);
		}
	}

	public static void Play(this SafePlayable<AnimatorControllerPlayable> playable, int stateNameHash, int layer = -1, float normalizedTime = float.NegativeInfinity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.CanPlayOnLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.Play(stateNameHash, layer, normalizedTime);
		}
	}

	public static void CrossFade(this SafePlayable<AnimatorControllerPlayable> playable, string stateName, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.CanPlayOnLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.CrossFade(stateName, transitionDuration, layer, normalizedTime);
		}
	}

	public static void CrossFade(this SafePlayable<AnimatorControllerPlayable> playable, int stateNameHash, float transitionDuration, int layer = -1, float normalizedTime = float.NegativeInfinity)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.CanPlayOnLayer(layer))
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.CrossFade(stateNameHash, transitionDuration, layer, normalizedTime);
		}
	}

	public static float GetFloat(this SafePlayable<AnimatorControllerPlayable> playable, string name)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0f;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetFloat(name);
	}

	public static float GetFloat(this SafePlayable<AnimatorControllerPlayable> playable, int id)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0f;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetFloat(id);
	}

	public static void SetFloat(this SafePlayable<AnimatorControllerPlayable> playable, string name, float value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetFloat(name, value);
		}
	}

	public static void SetFloat(this SafePlayable<AnimatorControllerPlayable> playable, int id, float value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetFloat(id, value);
		}
	}

	public static void SetFloatFixed(this SafePlayable<AnimatorControllerPlayable> playable, int id, float value, float dampTime, float deltaTime)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		float num = raw.GetFloat(id);
		if (num != value)
		{
			float num2 = Mathf.Lerp(num, value, deltaTime / Mathf.Max(dampTime, 0.0001f));
			if (Mathf.Abs(num2 - value) < 0.0001f)
			{
				num2 = value;
			}
			if (num2 != num)
			{
				raw = playable.Raw;
				raw.SetFloat(id, num2);
			}
		}
	}

	public static bool GetBool(this SafePlayable<AnimatorControllerPlayable> playable, string name)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return false;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetBool(name);
	}

	public static bool GetBool(this SafePlayable<AnimatorControllerPlayable> playable, int id)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return false;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetBool(id);
	}

	public static void SetBool(this SafePlayable<AnimatorControllerPlayable> playable, string name, bool value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetBool(name, value);
		}
	}

	public static void SetBool(this SafePlayable<AnimatorControllerPlayable> playable, int id, bool value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetBool(id, value);
		}
	}

	public static void SetBoolChecked(this SafePlayable<AnimatorControllerPlayable> playable, int id, bool value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			if (raw.GetBool(id) != value)
			{
				raw = playable.Raw;
				raw.SetBool(id, value);
			}
		}
	}

	public static int GetInteger(this SafePlayable<AnimatorControllerPlayable> playable, string name)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetInteger(name);
	}

	public static int GetInteger(this SafePlayable<AnimatorControllerPlayable> playable, int id)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetInteger(id);
	}

	public static void SetInteger(this SafePlayable<AnimatorControllerPlayable> playable, string name, int value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetInteger(name, value);
		}
	}

	public static void SetInteger(this SafePlayable<AnimatorControllerPlayable> playable, int id, int value)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetInteger(id, value);
		}
	}

	public static void SetTrigger(this SafePlayable<AnimatorControllerPlayable> playable, string name)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetTrigger(name);
		}
	}

	public static void SetTrigger(this SafePlayable<AnimatorControllerPlayable> playable, int id)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.SetTrigger(id);
		}
	}

	public static void ResetTrigger(this SafePlayable<AnimatorControllerPlayable> playable, string name)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.ResetTrigger(name);
		}
	}

	public static void ResetTrigger(this SafePlayable<AnimatorControllerPlayable> playable, int id)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid())
		{
			AnimatorControllerPlayable raw = playable.Raw;
			raw.ResetTrigger(id);
		}
	}

	public static int GetParameterCount(this SafePlayable<AnimatorControllerPlayable> playable)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!playable.IsValid())
		{
			return 0;
		}
		AnimatorControllerPlayable raw = playable.Raw;
		return raw.GetParameterCount();
	}

	public static AnimatorControllerParameter GetParameter(this SafePlayable<AnimatorControllerPlayable> playable, int index)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (playable.IsValid() && index >= 0)
		{
			AnimatorControllerPlayable raw = playable.Raw;
			if (index < raw.GetParameterCount())
			{
				raw = playable.Raw;
				return raw.GetParameter(index);
			}
		}
		return null;
	}
}
