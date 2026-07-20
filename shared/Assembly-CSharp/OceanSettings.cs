using System.IO;
using Rust.Water5;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ocean Settings", menuName = "Water5/Ocean Settings")]
public class OceanSettings : ScriptableObject
{
	[Header("Compute Shaders")]
	public ComputeShader waveSpectrumCompute;

	public ComputeShader fftCompute;

	public ComputeShader waveMergeCompute;

	public ComputeShader waveInitialSpectrum;

	[Header("Global Ocean Params")]
	public float[] octaveScales;

	public float lamda;

	public float windDirection;

	public float distanceAttenuationFactor;

	public float depthAttenuationFactor;

	[Header("Ocean Spectra")]
	public OceanSpectrumSettings[] spectrumSettings;

	[HideInInspector]
	public float[] spectrumRanges;

	public unsafe OceanDisplacementShort3[,,] LoadSimData()
	{
		OceanDisplacementShort3[,,] array = new OceanDisplacementShort3[spectrumSettings.Length, 72, 65536];
		string path = Application.streamingAssetsPath + "/" + ((Object)this).name + ".physicsdata.dat";
		if (!File.Exists(path))
		{
			Debug.Log((object)"Simulation Data not found");
			return array;
		}
		byte[] array2 = File.ReadAllBytes(path);
		fixed (byte* ptr = array2)
		{
			fixed (OceanDisplacementShort3* ptr2 = array)
			{
				UnsafeUtility.MemCpy((void*)ptr2, (void*)ptr, (long)array2.Length);
			}
		}
		return array;
	}

	internal unsafe Rust.Water5.NativeOceanDisplacementShort3 LoadNativeSimData()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Rust.Water5.NativeOceanDisplacementShort3 result = Rust.Water5.NativeOceanDisplacementShort3.Create(spectrumSettings.Length, 72, 65536);
		string text = Application.streamingAssetsPath + "/" + ((Object)this).name + ".physicsdata.dat";
		if (!File.Exists(text))
		{
			Debug.Log((object)"Simulation Data not found");
			return result;
		}
		NativeArray<byte> val = FileEx.ReadAllBytesNative(text, (Allocator)2);
		void* unsafePtr = NativeArrayUnsafeUtility.GetUnsafePtr<OceanDisplacementShort3>(result.GetNativeRaw());
		void* unsafePtr2 = NativeArrayUnsafeUtility.GetUnsafePtr<byte>(val);
		UnsafeUtility.MemCpy(unsafePtr, unsafePtr2, (long)val.Length);
		return result;
	}
}
