using System;
using System.Diagnostics;
using System.Linq;
using API.Abstracts;
using API.Assembly;
using API.Events;
using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Serialized;
using Carbon.Compat.Converters;
using Facepunch;

namespace Carbon.Compat;

public class CompatManager : CarbonBehaviour, ICompatManager
{
	private readonly BaseConverter oxideConverter = new OxideConverter();

	private readonly BaseConverter harmonyConverter = new HarmonyConverter();

	private static readonly ModuleReaderParameters readerArgs;

	private static readonly Version zeroVersion;

	public static readonly AssemblyReference SDK;

	public static readonly AssemblyReference Common;

	public static readonly AssemblyReference Newtonsoft;

	public static readonly AssemblyReference protobuf;

	public static readonly AssemblyReference protobufCore;

	public static readonly AssemblyReference wsSharp;

	private bool ConvertAssembly(ModuleDefinition md, BaseConverter converter, ref byte[] buffer, bool noEntrypoint = false)
	{
		Stopwatch stopwatch = Pool.Get<Stopwatch>();
		stopwatch.Restart();
		md.DebugData.Clear();
		BaseConverter.Context ctx = new BaseConverter.Context
		{
			Buffer = buffer,
			NoEntrypoint = noEntrypoint
		};
		try
		{
			buffer = converter.Convert(md, ctx);
		}
		catch (Exception ex)
		{
			Logger.Error($"Failed to convert assembly {md.Name}", ex);
			buffer = null;
			stopwatch.Reset();
			Pool.FreeUnsafe<Stopwatch>(ref stopwatch);
			return false;
		}
		if (buffer == ctx.Buffer)
		{
			Logger.Log($"{converter.Name} assembly doesn't need any conversion [for '{md.Name}'], skipping..");
		}
		else
		{
			Logger.Log($"{converter.Name} assembly conversion for '{md.Name}' took {stopwatch.ElapsedMilliseconds:0}ms");
		}
		stopwatch.Reset();
		Pool.FreeUnsafe<Stopwatch>(ref stopwatch);
		return true;
	}

	ConversionResult ICompatManager.AttemptOxideConvert(ref byte[] data)
	{
		ModuleDefinition val = ModuleDefinition.FromBytes(data, readerArgs);
		if (!val.AssemblyReferences.Any(Helpers.IsOxideASM))
		{
			return ConversionResult.Skip;
		}
		if (!ConvertAssembly(val, oxideConverter, ref data))
		{
			return ConversionResult.Fail;
		}
		return ConversionResult.Success;
	}

	bool ICompatManager.ConvertHarmonyMod(ref byte[] data, bool noEntrypoint)
	{
		return ConvertAssembly(ModuleDefinition.FromBytes(data, readerArgs), harmonyConverter, ref data, noEntrypoint);
	}

	public void Init()
	{
		Community.Runtime.Events.Subscribe(CarbonEvent.HookFetchStart, delegate
		{
			HookProcessor.HookClear();
		});
		Community.Runtime.Events.Subscribe(CarbonEvent.HookFetchEnd, delegate
		{
			HookProcessor.HookReload();
		});
	}

	static CompatManager()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		readerArgs = new ModuleReaderParameters((IErrorListener)(object)EmptyErrorListener.Instance);
		zeroVersion = new Version(0, 0, 0, 0);
		SDK = new AssemblyReference(Utf8String.op_Implicit("Carbon.SDK"), zeroVersion);
		Common = new AssemblyReference(Utf8String.op_Implicit("Carbon.Common"), zeroVersion);
		Newtonsoft = new AssemblyReference(Utf8String.op_Implicit("Newtonsoft.Json"), zeroVersion);
		protobuf = new AssemblyReference(Utf8String.op_Implicit("protobuf-net"), zeroVersion);
		protobufCore = new AssemblyReference(Utf8String.op_Implicit("protobuf-net.Core"), zeroVersion);
		wsSharp = new AssemblyReference(Utf8String.op_Implicit("websocket-sharp"), zeroVersion);
	}
}
