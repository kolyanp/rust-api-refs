using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

[Serializable]
public struct MountMontageWindow
{
	[Range(0f, 1f)]
	public float start;

	[Range(0f, 1f)]
	public float end;

	public static MountMontageWindow Whole => new MountMontageWindow
	{
		start = 0f,
		end = 1f
	};

	public bool IsAuthored => end > start;
}
