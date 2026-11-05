using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class SoundModulation : MonoBehaviour, IClientComponent
{
	public enum Parameter
	{
		Gain,
		Pitch,
		Spread,
		MaxDistance
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public readonly struct Modulator : IDisposable
	{
		public void Dispose()
		{
		}
	}

	private const int parameterCount = 4;
}
