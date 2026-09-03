using System;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(RustNavMeshAgent))]
public class RootMotionPlayer : EntityComponent<BaseEntity>, IServerComponent
{
	public struct Warp
	{
		public float startTime;

		public float endTime;

		public Vector3 translationScale;

		public float rotationScale;

		public Warp(float startTime, float endTime, Vector3 translationScale, float rotationScale = 1f)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			this.startTime = startTime;
			this.endTime = endTime;
			this.translationScale = translationScale;
			this.rotationScale = rotationScale;
		}
	}

	public class PlayServerState : IPooled
	{
		public AnimationClip animClip;

		public RootMotionData rmData;

		public float elapsedTime;

		public Vector3 initialLocation;

		public Quaternion initialRotation;

		public Action ServerTickAction;

		public Warp[] warps;

		public bool constrainToNavmesh;

		public Vector3 lastUnscaledOffset;

		public float lastUnscaledRotation;

		public bool isPlaying;

		public void Reset()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			isPlaying = false;
			rmData = null;
			animClip = null;
			initialRotation = Quaternion.identity;
			warps = null;
			constrainToNavmesh = true;
			elapsedTime = 0f;
			ServerTickAction = null;
			lastUnscaledOffset = Vector3.zero;
			lastUnscaledRotation = 0f;
		}

		void IPooled.EnterPool()
		{
			Reset();
		}

		void IPooled.LeavePool()
		{
			Reset();
		}

		public static PlayServerState TakeFromPool(RootMotionData data, Transform transform)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			PlayServerState playServerState = Pool.Get<PlayServerState>();
			playServerState.rmData = data;
			playServerState.initialLocation = transform.position;
			playServerState.initialRotation = transform.rotation;
			return playServerState;
		}

		public static PlayServerState TakeFromPool(AnimationClip data, Transform transform)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			PlayServerState playServerState = Pool.Get<PlayServerState>();
			playServerState.animClip = data;
			playServerState.initialLocation = transform.position;
			playServerState.initialRotation = transform.rotation;
			return playServerState;
		}

		public int GetAnimHash()
		{
			if (rmData != null)
			{
				if ((Object)(object)rmData.inPlaceAnimation == (Object)null)
				{
					Debug.LogError((object)("RootMotionPlayer.PlayServer: rmData.inPlaceAnimation is null for " + ((Object)rmData).name));
				}
				return Animator.StringToHash(((Object)rmData.inPlaceAnimation).name);
			}
			if ((Object)(object)animClip == (Object)null)
			{
				Debug.LogError((object)"RootMotionPlayer.PlayServer: animClip is null");
			}
			return Animator.StringToHash(((Object)animClip).name);
		}

		public float GetAnimLength()
		{
			if (!(rmData != null))
			{
				return animClip.length;
			}
			return rmData.inPlaceAnimation.length;
		}

		public bool Step(float deltaTime, ref Vector3 location, ref Quaternion rotation, float rootBoneLocalZOffset = 0f)
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			if (elapsedTime >= GetAnimLength())
			{
				return false;
			}
			if (rmData == null)
			{
				elapsedTime += deltaTime;
				return elapsedTime < GetAnimLength() - 0.25f;
			}
			Vector3 zero = Vector3.zero;
			zero.x = rmData.xMotionCurve.Evaluate(elapsedTime);
			zero.y = (constrainToNavmesh ? 0f : rmData.yMotionCurve.Evaluate(elapsedTime));
			zero.z = rmData.zMotionCurve.Evaluate(elapsedTime);
			float num = rmData.yRotationCurve.Evaluate(elapsedTime);
			Vector3 val = zero - lastUnscaledOffset;
			float num2 = num - lastUnscaledRotation;
			lastUnscaledOffset = zero;
			lastUnscaledRotation = num;
			if (warps != null)
			{
				Warp[] array = warps;
				for (int i = 0; i < array.Length; i++)
				{
					Warp warp = array[i];
					if (warp.startTime <= elapsedTime && elapsedTime <= warp.endTime)
					{
						val.x *= warp.translationScale.x;
						val.y *= warp.translationScale.y;
						val.z *= warp.translationScale.z;
						num2 *= warp.rotationScale;
					}
				}
			}
			Vector3 val2 = initialRotation * val;
			location -= rotation * (Vector3.forward * (0f - rootBoneLocalZOffset));
			location += val2;
			rotation *= Quaternion.Euler(0f, num2, 0f);
			location += rotation * (Vector3.forward * (0f - rootBoneLocalZOffset));
			elapsedTime += deltaTime;
			return elapsedTime < GetAnimLength() - 0.25f;
		}

		public Quaternion Track(Vector3 ownerPos, Vector3 targetPos, Quaternion rotation, float trackingSpeed, float deltaTime)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = Vector3Ex.NormalizeXZ(targetPos - ownerPos);
			initialRotation = Quaternion.RotateTowards(initialRotation, Quaternion.LookRotation(val, Vector3.up), trackingSpeed * deltaTime);
			rotation *= Quaternion.Inverse(rotation) * initialRotation;
			return rotation;
		}
	}

	[SerializeField]
	private float rootBoneLocalZOffset;

	private RustNavMeshAgent _agent;

	private PlayServerState currentPlayState;

	private Action _playServerTickAction;

	private RustNavMeshAgent Agent => _agent ?? (_agent = ((Component)base.baseEntity).GetComponent<RustNavMeshAgent>());

	private Action PlayServerTickAction => PlayServerTick;

	public PlayServerState PlayServerAndTakeFromPool(RootMotionData data)
	{
		PlayServerState playServerState = PlayServerState.TakeFromPool(data, ((Component)base.baseEntity).transform);
		PlayServer(playServerState);
		return playServerState;
	}

	public PlayServerState PlayServerAndTakeFromPool(AnimationClip data)
	{
		PlayServerState playServerState = PlayServerState.TakeFromPool(data, ((Component)base.baseEntity).transform);
		PlayServer(playServerState);
		return playServerState;
	}

	public void PlayServer(PlayServerState state)
	{
		if (AI.logIssues && state.rmData == null && (Object)(object)state.animClip == (Object)null)
		{
			Debug.LogError((object)"RootMotionPlayer.PlayServer: state.rmData and state.animClip are both null");
			return;
		}
		if (currentPlayState != null)
		{
			StopServer(currentPlayState);
		}
		currentPlayState = state;
		currentPlayState.isPlaying = true;
		base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_PlayMontageDelayed"), currentPlayState.GetAnimHash());
		Agent.Pause(this);
		base.baseEntity.InvokeRepeating(PlayServerTickAction, 0f, 0f);
	}

	public void PlayServerAdditive(AnimationClip animClip)
	{
		base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_PlayAdditiveMontage"), Animator.StringToHash(((Object)animClip).name));
	}

	private void PlayServerTick()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RootMotionPlayer:PlayServerTick"))
		{
			Vector3 location = ((Component)base.baseEntity).transform.position;
			Quaternion rotation = ((Component)base.baseEntity).transform.rotation;
			bool num = !currentPlayState.Step(Time.deltaTime, ref location, ref rotation, rootBoneLocalZOffset);
			if (currentPlayState.rmData != null)
			{
				if (currentPlayState.constrainToNavmesh)
				{
					Agent.Move(Agent.WorldToNavSpace(location) - Agent.WorldToNavSpace(((Component)base.baseEntity).transform.position));
				}
				else
				{
					((Component)base.baseEntity).transform.position = location;
				}
				((Component)base.baseEntity).transform.rotation = rotation;
			}
			if (num)
			{
				StopServer(currentPlayState, interrupt: false);
				currentPlayState = null;
			}
		}
	}

	public void Track(Vector3 targetPos, float trackingSpeed = 45f, float? timeStep = null)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (currentPlayState != null && !(currentPlayState.rmData == null))
		{
			if (!timeStep.HasValue)
			{
				timeStep = Time.deltaTime;
			}
			((Component)base.baseEntity).transform.rotation = currentPlayState.Track(((Component)this).transform.position, targetPos, ((Component)base.baseEntity).transform.rotation, trackingSpeed, timeStep.Value);
		}
	}

	private void StopServer(PlayServerState state, bool interrupt = true)
	{
		if (state != null && state.isPlaying)
		{
			state.isPlaying = false;
			if (state == currentPlayState)
			{
				base.baseEntity.CancelInvoke(PlayServerTickAction);
				Agent.Unpause(this);
				currentPlayState = null;
			}
			if (interrupt)
			{
				base.baseEntity.ClientRPC(RpcTarget.NetworkGroup("CL_StopMontage"));
			}
		}
	}

	public void StopServerAndReturnToPool(ref PlayServerState state, bool interrupt = true)
	{
		if (state != null)
		{
			StopServer(state, interrupt);
			Pool.Free<PlayServerState>(ref state);
		}
	}
}
