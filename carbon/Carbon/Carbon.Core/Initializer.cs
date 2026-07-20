using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using API.Assembly;
using UnityEngine;

namespace Carbon.Core;

public class Initializer : ICarbonComponent, ICarbonAddon
{
	public void Awake(EventArgs args)
	{
	}

	public void OnEnable(EventArgs args)
	{
	}

	public void OnDisable(EventArgs args)
	{
	}

	public void OnLoaded(EventArgs args)
	{
		try
		{
			OSPlatform windows = OSPlatform.Windows;
			OSPlatform linux = OSPlatform.Linux;
			if (!RuntimeInformation.IsOSPlatform(windows))
			{
				Logger.Log(Environment.NewLine + "                                                          " + Environment.NewLine + "  ________ _______ ______ _______ _______ _______ _______ " + Environment.NewLine + " |  |  |  |   _   |   __ \\    |  |_     _|    |  |     __|" + Environment.NewLine + " |  |  |  |       |      <       |_|   |_|       |    |  |" + Environment.NewLine + " |________|___|___|___|__|__|____|_______|__|____|_______|" + Environment.NewLine + "                                                          " + Environment.NewLine + $"    YOU'RE TRYING TO RUN CARBON FOR {windows} ON A {linux}   " + Environment.NewLine + "    MACHINE. THIS CANNOT HAPPEN.                          " + Environment.NewLine + "                                                          " + Environment.NewLine + "    PLEASE VERIFY SERVER FILES WITH STEAM, DOWNLOAD THE   " + Environment.NewLine + $"    {linux} CARBON BUILD, THEN TRY AGAIN.               " + Environment.NewLine + "                                                          " + Environment.NewLine + "    IF THIS STILL PERSISTS, PLEASE REACH OUT TO US.       " + Environment.NewLine + "    THANK YOU <3                                          " + Environment.NewLine + "                                                          " + Environment.NewLine);
				Thread.Sleep(60000);
				Application.Quit();
				return;
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Unable to assert operating system status.", ex);
			return;
		}
		try
		{
			if ((object)Type.GetType("Oxide.Core.Interface, Oxide.Core") != null)
			{
				Logger.Log(Environment.NewLine + "                                                          " + Environment.NewLine + "  ________ _______ ______ _______ _______ _______ _______ " + Environment.NewLine + " |  |  |  |   _   |   __ \\    |  |_     _|    |  |     __|" + Environment.NewLine + " |  |  |  |       |      <       |_|   |_|       |    |  |" + Environment.NewLine + " |________|___|___|___|__|__|____|_______|__|____|_______|" + Environment.NewLine + "                                                          " + Environment.NewLine + "    WE HAVE DETECTED YOUR SERVER IS STILL PATCHED WITH    " + Environment.NewLine + "    OXIDE. CARBON WILL NOT WORK IN THIS ENVIRONMENT.\t\t" + Environment.NewLine + "                                                          " + Environment.NewLine + "    PLEASE VERIFY YOUR GAME FILES WITH STEAMCMD THEN\t\t" + Environment.NewLine + "    REBOOT THE SERVER.\t\t\t\t\t\t\t\t\t" + Environment.NewLine + "                                                          " + Environment.NewLine + "    THIS SERVER WILL BE TERMINATED IN 60 SECONDS.         " + Environment.NewLine + "    THANK YOU <3                                          " + Environment.NewLine + "                                                          " + Environment.NewLine);
				Thread.Sleep(60000);
				Application.Quit();
				return;
			}
		}
		catch (Exception ex2)
		{
			Logger.Error("Unable to assert assembly status.", ex2);
			return;
		}
		try
		{
			Type type = Type.GetType("ServerMgr, Assembly-CSharp");
			MethodInfo methodInfo = type.GetMethod("Shutdown", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? null;
			if (methodInfo == null || !methodInfo.IsPublic)
			{
				Logger.Log(Environment.NewLine + "                                                          " + Environment.NewLine + "  ________ _______ ______ _______ _______ _______ _______ " + Environment.NewLine + " |  |  |  |   _   |   __ \\    |  |_     _|    |  |     __|" + Environment.NewLine + " |  |  |  |       |      <       |_|   |_|       |    |  |" + Environment.NewLine + " |________|___|___|___|__|__|____|_______|__|____|_______|" + Environment.NewLine + "                                                          " + Environment.NewLine + "    THE SERVER ASSEMBLY CODE IS NOT PUBLICIZED.           " + Environment.NewLine + "    CARBON WILL NOT WORK PROPERLY.                        " + Environment.NewLine + "                                                          " + Environment.NewLine + "    PLEASE MAKE SURE UNITY DOORSTOP IS BEING EXECUTED.    " + Environment.NewLine + "    IF THE PROBLEM PRESISTS, PLEASE OPEN A NEW ISSUE AT   " + Environment.NewLine + "    GITHUB OR ASK FOR SUPPORT ON OUR DISCORD.             " + Environment.NewLine + "\t\t\t\tDISCORD.GG/CARBONMOD\t\t\t\t\t\t" + Environment.NewLine + "                                                          " + Environment.NewLine + "    THIS SERVER WILL BE TERMINATED IN 60 SECONDS.         " + Environment.NewLine + "    THANK YOU <3                                          " + Environment.NewLine + "                                                          " + Environment.NewLine);
				Thread.Sleep(60000);
				Application.Quit();
				return;
			}
		}
		catch (Exception ex3)
		{
			Logger.Error("Unable to assert assembly status.", ex3);
			return;
		}
		try
		{
			if (CommunityInternal.InternalRuntime == null)
			{
				CommunityInternal.InternalRuntime = new CommunityInternal();
			}
			else
			{
				CommunityInternal.InternalRuntime?.Uninitialize();
			}
			CommunityInternal.InternalRuntime.Initialize();
		}
		catch (Exception ex4)
		{
			Logger.Error("Unable to initialize.", ex4.InnerException ?? ex4);
		}
	}

	public void OnUnloaded(EventArgs args)
	{
		Logger.Log("Uninitalizing...");
		CommunityInternal.InternalRuntime?.Uninitialize();
		CommunityInternal.InternalRuntime = null;
	}
}
