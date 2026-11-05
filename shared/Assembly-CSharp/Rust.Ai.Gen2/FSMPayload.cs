using System;
using Facepunch;

namespace Rust.Ai.Gen2;

public struct FSMPayload : IDisposable
{
	public Nullable position;

	public Nullable velocity;

	public BaseEntity entity;

	public HitInfo hitInfo;

	public void CopyFrom(FSMPayload other)
	{
		position = other.position;
		velocity = other.velocity;
		entity = other.entity;
		hitInfo = other.hitInfo;
	}

	public void Dispose()
	{
		if (hitInfo != null)
		{
			Pool.Free<HitInfo>(ref hitInfo);
		}
	}
}
