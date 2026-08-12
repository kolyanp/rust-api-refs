using System;
using Unity.Collections;
using UnityEngine;

public class TickInterpolatorCache
{
	public struct PlayerInfo
	{
		public int Count;

		public float Length;
	}

	public struct Segment
	{
		public Vector3 point;

		public readonly float length;

		public Segment(Vector3 a, Vector3 b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			point = b;
			length = Vector3.Distance(a, b);
		}

		public Segment(Vector3 b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			point = b;
			length = 0f;
		}
	}

	public struct ReadOnlyState
	{
		public readonly ReadOnly<Segment> Segments;

		public readonly ReadOnly<PlayerInfo> Infos;

		public readonly int BufferSize;

		public ReadOnlyState(ReadOnly<Segment> playerSegments, ReadOnly<PlayerInfo> playerInfos, int bufferSize)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Segments = playerSegments;
			Infos = playerInfos;
			BufferSize = bufferSize;
		}
	}

	public struct PlayerTickIterator
	{
		private readonly ReadOnlyState state;

		private readonly int playerIndex;

		private Vector3 currPoint;

		private int segmentIndex;

		public Vector3 CurrentPoint
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return currPoint;
			}
		}

		public Vector3 StartPoint
		{
			get
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return GetStartPoint(state, playerIndex);
			}
		}

		public Vector3 EndPoint
		{
			get
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return GetEndPoint(state, playerIndex);
			}
		}

		public float Length
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return state.Infos[playerIndex].Length;
			}
		}

		public PlayerTickIterator(ReadOnlyState state, int playerIndex)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			this.state = state;
			this.playerIndex = playerIndex;
			segmentIndex = 0;
			currPoint = GetStartPoint(state, playerIndex);
		}

		public bool MoveNext(float distance)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			int num2 = playerIndex * state.BufferSize + 1;
			while (num < distance && HasNext())
			{
				Segment segment = state.Segments[num2 + segmentIndex];
				currPoint = segment.point;
				num += segment.length;
				segmentIndex++;
			}
			return num > 0f;
		}

		public void Reset()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			segmentIndex = 0;
			currPoint = StartPoint;
		}

		public bool HasNext()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return segmentIndex < state.Infos[playerIndex].Count;
		}
	}

	private NativeArray<Segment> playerSegments;

	private NativeArray<PlayerInfo> playerInfos;

	private int bufferSize;

	public ReadOnlyState ReadOnly
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			return new ReadOnlyState(playerSegments.AsReadOnly(), playerInfos.AsReadOnly(), bufferSize);
		}
	}

	public TickInterpolatorCache(int capacity = 32)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		bufferSize = 9;
		base._002Ector();
		playerSegments = new NativeArray<Segment>(bufferSize * capacity, (Allocator)4, (NativeArrayOptions)0);
		playerInfos = new NativeArray<PlayerInfo>(capacity, (Allocator)4, (NativeArrayOptions)1);
	}

	public void Dispose()
	{
		NativeArrayEx.SafeDispose(ref playerSegments);
		NativeArrayEx.SafeDispose(ref playerInfos);
	}

	public void ReplacePlayer(int index)
	{
		playerInfos[index] = default(PlayerInfo);
	}

	public void MovePlayer(int from, int to)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		PlayerInfo playerInfo = playerInfos[from];
		playerInfos[to] = playerInfo;
		int num = from * bufferSize;
		int num2 = playerInfo.Count + 1;
		NativeArray<Segment> subArray = playerSegments.GetSubArray(num, num2);
		int num3 = to * bufferSize;
		NativeArray<Segment> subArray2 = playerSegments.GetSubArray(num3, num2);
		subArray.CopyTo(subArray2);
	}

	public void AddTick(BasePlayer player, Vector3 point)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		int activePlayerInd = player.ActivePlayerInd;
		AddTick(activePlayerInd, point);
	}

	public void AddTick(int playerIndex, Vector3 point)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		ref PlayerInfo reference = ref playerInfos.AsSpan()[playerIndex];
		int num = reference.Count + 1;
		if (num >= bufferSize)
		{
			GrowSegments(playerInfos.Length);
		}
		int num2 = playerIndex * bufferSize;
		Vector3 point2 = playerSegments[num2 + num - 1].point;
		Segment segment = new Segment(point2, point);
		reference.Length += segment.length;
		playerSegments[num2 + num] = segment;
		reference.Count++;
	}

	public void Reset(BasePlayer player, Vector3 point)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		int activePlayerInd = player.ActivePlayerInd;
		Reset(activePlayerInd, point);
	}

	public void Reset(int playerIndex, Vector3 point)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		playerInfos[playerIndex] = default(PlayerInfo);
		int num = playerIndex * bufferSize;
		playerSegments[num] = new Segment(point);
	}

	public void Expand(int newCap)
	{
		int length = playerInfos.Length;
		if (newCap > length)
		{
			NativeArrayEx.Expand(ref playerInfos, newCap, (NativeArrayOptions)1);
			GrowSegments(length);
		}
	}

	public static Vector3 GetStartPoint(ReadOnlyState state, int playerIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return state.Segments[playerIndex * state.BufferSize].point;
	}

	public static Vector3 GetEndPoint(ReadOnlyState state, int playerIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		PlayerInfo info = state.Infos[playerIndex];
		return GetEndPoint(state, playerIndex, info);
	}

	public static Vector3 GetEndPoint(ReadOnlyState state, int playerIndex, PlayerInfo info)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		return state.Segments[playerIndex * state.BufferSize + info.Count].point;
	}

	public void TransformEntries(int playerIndex, in Matrix4x4 matrix)
	{
		PlayerInfo info = playerInfos[playerIndex];
		TransformEntries(playerIndex, info, in matrix);
	}

	public void TransformEntries(int playerIndex, PlayerInfo info, in Matrix4x4 matrix)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Span<Segment> span = playerSegments.GetSubArray(playerIndex * bufferSize, info.Count + 1).AsSpan();
		for (int i = 0; i < span.Length; i++)
		{
			ref Segment reference = ref span[i];
			reference.point = ((Matrix4x4)(ref matrix)).MultiplyPoint3x4(reference.point);
		}
	}

	public static PlayerTickIterator GetPlayerTickIterator(ReadOnlyState state, int playerIndex)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Debug.Assert(playerIndex >= 0 && playerIndex < state.Infos.Length);
		return new PlayerTickIterator(state, playerIndex);
	}

	private void GrowSegments(int oldPlayerCap)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		int length = playerInfos.Length;
		int num = bufferSize;
		if (length == oldPlayerCap)
		{
			bufferSize += 4;
		}
		NativeArray<Segment> val = default(NativeArray<Segment>);
		val._002Ector(length * bufferSize, (Allocator)4, (NativeArrayOptions)0);
		for (int i = 0; i < oldPlayerCap; i++)
		{
			int count = playerInfos[i].Count;
			if (count > 0)
			{
				NativeArray<Segment> subArray = playerSegments.GetSubArray(i * num, count + 1);
				NativeArray<Segment> subArray2 = val.GetSubArray(i * bufferSize, count + 1);
				subArray.CopyTo(subArray2);
			}
			else
			{
				val[i * bufferSize] = playerSegments[i * num];
			}
		}
		playerSegments.Dispose();
		playerSegments = val;
	}
}
