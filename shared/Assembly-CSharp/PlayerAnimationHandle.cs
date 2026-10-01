using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public struct PlayerAnimationHandle : IEquatable<PlayerAnimationHandle>
{
	public SafePlayable<AnimationClipPlayable> Playable { get; private set; }

	public SafePlayable<AnimatorControllerPlayable> Controller { get; private set; }

	public SafePlayable<AnimationLayerMixerPlayable> LayerMixer { get; private set; }

	public PlayableGraph Graph
	{
		[CompilerGenerated]
		readonly get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public float Length { get; private set; }

	public readonly bool Valid
	{
		get
		{
			if (!Playable.IsValid())
			{
				return Controller.IsValid();
			}
			return true;
		}
	}

	public int InputPort { get; private set; }

	public AvatarMask CurrentMask { get; private set; }

	public static PlayerAnimationHandle InvalidHandle => default;

	public bool Equals(PlayerAnimationHandle other)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Playable.Equals(other.Playable) && LayerMixer.Equals(other.LayerMixer) && ((object)Graph/*cast due to constrained. prefix*/).Equals((object?)other.Graph) && Length.Equals(other.Length))
		{
			return InputPort == other.InputPort;
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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return HashCode.Combine<SafePlayable<AnimationClipPlayable>, SafePlayable<AnimationLayerMixerPlayable>, PlayableGraph, float, int>(Playable, LayerMixer, Graph, Length, InputPort);
	}

	public readonly void SetProgress(float progress)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && Playable.IsValid())
		{
			PlayableGraph graph = Graph;
			if (graph.IsValid())
			{
				progress = Mathf.Clamp01(progress);
				Playable.SetTime(progress * Length);
			}
		}
	}

	public readonly void SetApplyFootIK(bool apply)
	{
		if (Playable.IsValid())
		{
			Playable.SetApplyFootIK(apply);
		}
	}

	public readonly void SetTime(float time)
	{
		if (Valid && Playable.IsValid())
		{
			time = Mathf.Clamp(time, 0f, Length);
			Playable.SetTime(time);
		}
	}

	public readonly void Play()
	{
		if (Valid && Playable.IsValid())
		{
			Playable.Play();
		}
	}

	public readonly void PlayFromStart()
	{
		if (Playable.IsValid())
		{
			Playable.SetTime(0.0);
			Playable.Play();
		}
	}

	public readonly void SetWeight(float weight)
	{
		if (Valid && LayerMixer.IsValid())
		{
			LayerMixer.SetInputWeight(InputPort, weight);
		}
	}

	public float GetWeight()
	{
		if (!Valid)
		{
			return 0f;
		}
		return LayerMixer.GetInputWeight(InputPort);
	}

	public void SetMask(AvatarMask newMask)
	{
		if (!((Object)(object)newMask == (Object)null) && Valid && LayerMixer.IsValid())
		{
			LayerMixer.SetLayerMaskFromAvatarMask((uint)InputPort, newMask);
			CurrentMask = newMask;
		}
	}

	public readonly void Pause()
	{
		if (Valid && Playable.IsValid())
		{
			Playable.Pause();
		}
	}

	public readonly bool IsPlaying()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if (!Valid)
		{
			return false;
		}
		return (int)Playable.GetPlayState() == 1;
	}

	public void Dispose()
	{
		if (Valid)
		{
			LayerMixer.DisconnectInput(InputPort);
			Playable.Destroy();
			Controller.Destroy();
		}
	}

	public readonly float GetNormalizedTime()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (Valid && Playable.IsValid())
		{
			PlayableGraph graph = Graph;
			if (graph.IsValid() && !(Length <= 0f))
			{
				return (float)(Playable.GetTime() / (double)Length);
			}
		}
		return 0f;
	}

	public static PlayerAnimationHandle Create(AnimationClip clip, PlayableGraph playableGraph, SafePlayable<AnimationLayerMixerPlayable> layerMixer, int inputPort, AvatarMask mask, bool additive, bool autoPlay = true, float initialWeight = 1f)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)clip == (Object)null || !playableGraph.IsValid() || !layerMixer.HasInput(inputPort))
		{
			return InvalidHandle;
		}
		PlayerAnimationHandle result = default;
		layerMixer.DisconnectInput(inputPort);
		result.Length = clip.length;
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Playable = AnimationClipPlayable.Create(playableGraph, clip);
		result.CurrentMask = mask;
		layerMixer.ConnectInput<AnimationClipPlayable>(inputPort, result.Playable, 0);
		layerMixer.SetInputWeight(inputPort, initialWeight);
		layerMixer.SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		layerMixer.SetLayerAdditive((uint)inputPort, additive);
		if (!autoPlay)
		{
			result.Playable.Pause();
		}
		else
		{
			result.Playable.Play();
		}
		return result;
	}

	public static PlayerAnimationHandle Create(RuntimeAnimatorController controller, PlayableGraph playableGraph, SafePlayable<AnimationLayerMixerPlayable> layerMixer, int inputPort, AvatarMask mask, bool additive, float initialWeight = 1f)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)controller == (Object)null || !playableGraph.IsValid() || !layerMixer.HasInput(inputPort))
		{
			return InvalidHandle;
		}
		PlayerAnimationHandle result = default;
		layerMixer.DisconnectInput(inputPort);
		result.Graph = playableGraph;
		result.LayerMixer = layerMixer;
		result.InputPort = inputPort;
		result.Controller = AnimatorControllerPlayable.Create(playableGraph, controller);
		result.CurrentMask = mask;
		layerMixer.ConnectInput<AnimatorControllerPlayable>(inputPort, result.Controller, 0);
		layerMixer.SetInputWeight(inputPort, initialWeight);
		layerMixer.SetLayerMaskFromAvatarMask((uint)inputPort, mask);
		layerMixer.SetLayerAdditive((uint)inputPort, additive);
		return result;
	}
}
