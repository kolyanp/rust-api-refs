using System;
using System.Collections;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace Carbon;

internal static class OxidePlugins
{
	private static readonly PropertyInfo Oxide = AccessTools.Property("Oxide.Core.Interface, Oxide.Core:Oxide");

	private static readonly PropertyInfo RootPluginManager = AccessTools.Property("Oxide.Core.OxideMod, Oxide.Core:RootPluginManager");

	private static readonly PropertyInfo PluginDirectory = AccessTools.Property("Oxide.Core.OxideMod, Oxide.Core:PluginDirectory");

	private static readonly MethodInfo ReloadPlugin = AccessTools.Method("Oxide.Core.OxideMod, Oxide.Core:ReloadPlugin", new Type[1] { typeof(string) }, (Type[])null);

	private static readonly MethodInfo GetPlugin = AccessTools.Method("Oxide.Core.Plugins.PluginManager, Oxide.Core:GetPlugin", new Type[1] { typeof(string) }, (Type[])null);

	private static readonly PropertyInfo Filename = AccessTools.Property("Oxide.Core.Plugins.Plugin, Oxide.Core:Filename");

	private static readonly MethodInfo GetCompilablePlugin = AccessTools.Method("Oxide.Plugins.CSharpPluginLoader, Oxide.CSharp:GetCompilablePlugin", (Type[])null, (Type[])null);

	private static readonly FieldInfo CompilablePlugins = AccessTools.Field("Oxide.Plugins.CSharpPluginLoader, Oxide.CSharp:plugins");

	private static readonly FieldInfo LastModifiedAt = AccessTools.Field("Oxide.Plugins.CompilableFile, Oxide.CSharp:LastModifiedAt");

	public static string FindFileName(string name)
	{
		object value = RootPluginManager.GetValue(Oxide.GetValue(null));
		object obj = GetPlugin.Invoke(value, new object[1] { name });
		string text = ((obj == null) ? null : (Filename.GetValue(obj) as string));
		return string.IsNullOrEmpty(text) ? null : Path.GetFileNameWithoutExtension(text);
	}

	public static void MarkForRecompile(string name)
	{
		if (name == "*")
		{
			foreach (object value in ((IDictionary)CompilablePlugins.GetValue(null)).Values)
			{
				LastModifiedAt.SetValue(value, default(DateTime));
			}
			return;
		}
		object obj = GetCompilablePlugin.Invoke(null, new object[2]
		{
			PluginDirectory.GetValue(Oxide.GetValue(null)),
			name
		});
		LastModifiedAt.SetValue(obj, default(DateTime));
	}

	public static bool Recompile(string name)
	{
		MarkForRecompile(name);
		object obj = ReloadPlugin.Invoke(Oxide.GetValue(null), new object[1] { name });
		return obj is bool && (bool)obj;
	}
}
