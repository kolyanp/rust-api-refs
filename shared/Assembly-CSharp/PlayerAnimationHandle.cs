using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public struct PlayerAnimationHandle : IEquatable<PlayerAnimationHandle>
{
	[CompilerGenerated]
	private AnimationClipPlayable _003CPlayable_003Ek__BackingField;

	[CompilerGenerated]
	private AnimatorControllerPlayable _003CController_003Ek__BackingField;

	[CompilerGenerated]
	private AnimationLayerMixerPlayable _003CLayerMixer_003Ek__BackingField;

	[CompilerGenerated]
	private PlayableGraph _003CGraph_003Ek__BackingField;

	public AnimationClipPlayable Playable
	{
		[CompilerGenerated]
		readonly get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CPlayable_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CPlayable_003Ek__BackingField = value;
		}
	}

	public AnimatorControllerPlayable Controller
	{
		[CompilerGenerated]
		readonly get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CController_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CController_003Ek__BackingField = value;
		}
	}

	public AnimationLayerMixerPlayable LayerMixer
	{
		[CompilerGenerated]
		readonly get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CLayerMixer_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CLayerMixer_003Ek__BackingField = value;
		}
	}

	public PlayableGraph Graph
	{
		[CompilerGenerated]
		readonly get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CGraph_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CGraph_003Ek__BackingField = value;
		}
	}

	public float Length { get; private set; }

	public readonly bool Valid
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
			{
				return PlayableExtensions.IsValid<AnimatorControllerPlayable>(Controller);
			}
			return true;
		}
	}

	public int InputPort { get; private set; }

	public AvatarMask CurrentMask { get; private set; }

	public static PlayerAnimationHandle InvalidHandle => default(PlayerAnimationHandle);

	public bool Equals(PlayerAnimationHandle other)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		AnimationClipPlayable playable = Playable;
		if (((AnimationClipPlayable)(ref playable)).Equals(other.Playable))
		{
			AnimationLayerMixerPlayable layerMixer = LayerMixer;
			if (((AnimationLayerMixerPlayable)(ref layerMixer)).Equals(other.LayerMixer) && ((object)Graph/*cast due to constrained. prefix*/).Equals((object?)other.Graph) && Length.Equals(other.Length))
			{
				return InputPort == other.InputPort;
			}
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlayerAnimationHandle other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return HashCode.Combine<AnimationClipPlayable, AnimationLayerMixerPlayable, PlayableGraph, float, int>(Playable, LayerMixer, Graph, Length, InputPort);
	}

	public readonly void SetProgress(float progress)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			PlayableGraph graph = Graph;
			if (((PlayableGraph)(ref graph)).IsValid())
			{
				progress = Mathf.Clamp01(progress);
				PlayableExtensions.SetTime<AnimationClipPlayable>(Playable, (double)(progress * Length));
			}
		}
	}

	public readonly void SetTime(float time)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			time = Mathf.Clamp(time, 0f, Length);
			PlayableExtensions.SetTime<AnimationClipPlayable>(Playable, (double)time);
		}
	}

	public readonly void Play()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			PlayableExtensions.Play<AnimationClipPlayable>(Playable);
		}
	}

	public readonly void PlayFromStart()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			PlayableExtensions.SetTime<AnimationClipPlayable>(Playable, 0.0);
			PlayableExtensions.Play<AnimationClipPlayable>(Playable);
		}
	}

	public readonly void SetWeight(float weight)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationLayerMixerPlayable>(LayerMixer))
		{
			PlayableExtensions.SetInputWeight<AnimationLayerMixerPlayable>(LayerMixer, InputPort, weight);
		}
	}

	public float GetWeight()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!Valid)
		{
			return 0f;
		}
		return PlayableExtensions.GetInputWeight<AnimationLayerMixerPlayable>(LayerMixer, InputPort);
	}

	public void SetMask(AvatarMask newMask)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)newMask == (Object)null) && Valid && PlayableExtensions.IsValid<AnimationLayerMixerPlayable>(LayerMixer))
		{
			AnimationLayerMixerPlayable layerMixer = LayerMixer;
			((AnimationLayerMixerPlayable)(ref layerMixer)).SetLayerMaskFromAvatarMask((uint)InputPort, newMask);
			CurrentMask = newMask;
		}
	}

	public readonly void Pause()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			PlayableExtensions.Pause<AnimationClipPlayable>(Playable);
		}
	}

	public readonly bool IsPlaying()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		if (!Valid)
		{
			return false;
		}
		return (int)PlayableExtensions.GetPlayState<AnimationClipPlayable>(Playable) == 1;
	}

	public void Dispose()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!Valid)
		{
			return;
		}
		PlayableGraph graph = Graph;
		if (((PlayableGraph)(ref graph)).IsValid())
		{
			PlayableExtensions.DisconnectInput<AnimationLayerMixerPlayable>(LayerMixer, InputPort);
			if (PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
			{
				graph = Graph;
				((PlayableGraph)(ref graph)).DestroyPlayable<AnimationClipPlayable>(Playable);
			}
			if (PlayableExtensions.IsValid<AnimatorControllerPlayable>(Controller))
			{
				graph = Graph;
				((PlayableGraph)(ref graph)).DestroyPlayable<AnimatorControllerPlayable>(Controller);
			}
		}
	}

	public readonly float GetNormalizedTime()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && PlayableExtensions.IsValid<AnimationClipPlayable>(Playable))
		{
			PlayableGraph graph = Graph;
			if (((PlayableGraph)(ref graph)).IsValid())
			{
				return (float)(PlayableExtensions.GetTime<AnimationClipPlayable>(Playable) / (double)Length);
			}
		}
		return 0f;
	}

	public static PlayerAnimationHandle Create(AnimationClip clip, PlayableGraph playableGraph, AnimationLayerMixerPlayable layerMixer, int inputPort, AvatarMask mask, bool additive, bool autoPlay = true, float initialWeight = 1f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		PlayerAnimationHandle result = default(PlayerAnimationHandle);
		PlayableExtensions.DisconnectInput<AnimationLayerMixerPlayable>(layerMixer, inputPort);
		result.Length = clip.length;
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Playable = AnimationClipPlayable.Create(playableGraph, clip);
		result.CurrentMask = mask;
		PlayableExtensions.ConnectInput<AnimationLayerMixerPlayable, AnimationClipPlayable>(layerMixer, inputPort, result.Playable, 0);
		PlayableExtensions.SetInputWeight<AnimationLayerMixerPlayable>(layerMixer, inputPort, initialWeight);
		((AnimationLayerMixerPlayable)(ref layerMixer)).SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		((AnimationLayerMixerPlayable)(ref layerMixer)).SetLayerAdditive((uint)inputPort, additive);
		if (!autoPlay)
		{
			PlayableExtensions.Pause<AnimationClipPlayable>(result.Playable);
		}
		else
		{
			PlayableExtensions.Play<AnimationClipPlayable>(result.Playable);
		}
		return result;
	}

	public static PlayerAnimationHandle Create(RuntimeAnimatorController controller, PlayableGraph playableGraph, AnimationLayerMixerPlayable layerMixer, int inputPort, AvatarMask mask, bool additive, float initialWeight = 1f)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		PlayerAnimationHandle result = default(PlayerAnimationHandle);
		PlayableExtensions.DisconnectInput<AnimationLayerMixerPlayable>(layerMixer, inputPort);
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Controller = AnimatorControllerPlayable.Create(playableGraph, controller);
		result.CurrentMask = mask;
		PlayableExtensions.ConnectInput<AnimationLayerMixerPlayable, AnimatorControllerPlayable>(layerMixer, inputPort, result.Controller, 0);
		PlayableExtensions.SetInputWeight<AnimationLayerMixerPlayable>(layerMixer, inputPort, initialWeight);
		((AnimationLayerMixerPlayable)(ref layerMixer)).SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		((AnimationLayerMixerPlayable)(ref layerMixer)).SetLayerAdditive((uint)inputPort, additive);
		return result;
	}
}
