using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

internal static class InstancingUtil
{
	public static readonly int PositionBufferProperty;

	public static readonly int RenderBufferProperty;

	public static readonly int IndirectExtraArgProperty;

	public static readonly int Param_MeshOverrideBuffer;

	public static readonly int Param_RenderSliceIndexes;

	public static readonly int DrawCallIndexProperty;

	public static readonly int Param_RendererIndex;

	public static readonly int Param_Verticies;

	public static readonly int Param_Triangles;

	public static readonly GlobalKeyword Keyword_Rust_Procedural_Rendering;

	public const int CullingGPUThreads = 1024;

	public static float MB(int bytes)
	{
		return (float)Math.Round((float)(bytes / 100000) / 10f, 1);
	}

	public static StringBuilder MemoryUsage(this StringBuilder builder, string name, ComputeBuffer buffer)
	{
		builder.AppendLine($"[ComputeBuffer] {name} {buffer.count} | {MB(buffer.count * buffer.stride)}MB");
		return builder;
	}

	public static StringBuilder MemoryUsage(this StringBuilder builder, string name, GraphicsBuffer buffer)
	{
		builder.AppendLine($"[ComputeBuffer] {name} {buffer.count} | {MB(buffer.count * buffer.stride)}MB");
		return builder;
	}

	public static StringBuilder MemoryUsage<T>(this StringBuilder builder, string name, NativeArray<T> array, int count = -1) where T : unmanaged
	{
		int num = Marshal.SizeOf<T>();
		builder.AppendLine(string.Format("[NativeArray] {0}{1} Capacity: {2} | {3}MB", new object[4]
		{
			name,
			(count >= 0) ? (" Count: " + count) : "",
			array.Length,
			MB(array.Length * num)
		}));
		return builder;
	}

	public static StringBuilder MemoryUsage<T>(this StringBuilder builder, string name, ICollection<T> array)
	{
		Type type = (array.GetType().IsGenericType ? array.GetType() : array.GetType().GetGenericTypeDefinition());
		string arg = "Collection";
		if (type == typeof(Dictionary<, >))
		{
			arg = "Dictionary";
		}
		else if (type == typeof(List<>))
		{
			arg = "List";
		}
		else if (type == typeof(HashSet<>))
		{
			arg = "HashSet";
		}
		else if (type == typeof(Array))
		{
			arg = "Array";
		}
		int count = array.Count;
		builder.AppendLine($"[{arg}] {name} Size: {count}");
		return builder;
	}

	public static int GetIterationCount(int count, int threads)
	{
		return count / threads + ((count % threads != 0) ? 1 : 0);
	}

	static InstancingUtil()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		PositionBufferProperty = Shader.PropertyToID("_PositionBuffer");
		RenderBufferProperty = Shader.PropertyToID("_PostCullBuffer");
		IndirectExtraArgProperty = Shader.PropertyToID("_IndirectExtraArgsBuffer");
		Param_MeshOverrideBuffer = Shader.PropertyToID("_MeshOverrideBuffer");
		Param_RenderSliceIndexes = Shader.PropertyToID("_RenderSliceIndexes");
		DrawCallIndexProperty = Shader.PropertyToID("_DrawCallIndex");
		Param_RendererIndex = Shader.PropertyToID("_RendererIndex");
		Param_Verticies = Shader.PropertyToID("_Verticies");
		Param_Triangles = Shader.PropertyToID("_Triangles");
		Keyword_Rust_Procedural_Rendering = GlobalKeyword.Create("RUST_PROCEDURAL_INSTANCING");
	}
}
