using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Facepunch;
using HarmonyLib;

namespace Carbon.Extensions;

public static class AccessToolsEx
{
	private static Dictionary<string, Type> searchCache = new Dictionary<string, Type>();

	public static Type TypeByName(string name)
	{
		if (searchCache.TryGetValue(name, out var value))
		{
			return value;
		}
		return searchCache[name] = Type.GetType(name, throwOnError: false) ?? AccessTools.TypeByName(name) ?? SearchTypeByName(name);
	}

	private static Type SearchTypeByName(string name)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if (type != null)
		{
			return type;
		}
		if (name.Contains('`'))
		{
			if (name.Contains("["))
			{
				name = name.Replace("[", "<").Replace("]", ">");
			}
			int num = name.IndexOf('<');
			string text = name.Substring(0, num);
			string text2 = name.Substring(num);
			PooledList<string> val = Pool.Get<PooledList<string>>();
			try
			{
				((List<string>)(object)val).AddRange((IEnumerable<string>)text2.Replace("<", string.Empty).Replace(">", string.Empty).Split(','));
				Type[] array = ((IEnumerable<string>)val).Select((string arg) => Type.GetType(arg, throwOnError: false) ?? AccessTools.TypeByName(arg)).ToArray();
				if (array.Length != ((List<string>)(object)val).Count)
				{
					Logger.Warn("AccessTools.TypeByName: Failed to resolve one or more generic arguments for " + name);
					return null;
				}
				Type type2 = AccessTools.TypeByName(text);
				if (type2 == null)
				{
					Logger.Warn("AccessTools.TypeByName: Could not find generic type definition " + text);
					return null;
				}
				if (!type2.IsGenericTypeDefinition)
				{
					type2 = type2.GetGenericTypeDefinition();
				}
				try
				{
					return type2.MakeGenericType(array);
				}
				catch (Exception ex)
				{
					Logger.Error("AccessTools.TypeByName: Failed to MakeGenericType for " + name + " - " + ex.Message);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		IEnumerable<Type> source = AccessTools.AllTypes();
		type = source.FirstOrDefault((Type t) => t.FullName == name);
		if (type != null)
		{
			return type;
		}
		return source.FirstOrDefault((Type t) => t.Name == name);
	}

	public static IEnumerable<Type> AllTypes()
	{
		return AllAssemblies().SelectMany((Assembly a) => GetTypesFromAssembly(a));
	}

	public static Type[] GetTypesFromAssembly(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type type) => (object)type != null).ToArray();
		}
	}

	public static IEnumerable<Assembly> AllAssemblies()
	{
		return from a in AppDomain.CurrentDomain.GetAssemblies()
			where !a.FullName.StartsWith("Microsoft.VisualStudio")
			select a;
	}

	public static IEnumerable<Type> GetConstraints(Type type)
	{
		Type[] genericArguments = type.GetGenericArguments();
		if (genericArguments.Count() > 1)
		{
			throw new Exception("GetConstraints only supports generics with one type");
		}
		Type type2 = genericArguments.First();
		return type2.GetGenericParameterConstraints();
	}

	public static IEnumerable<Type> MatchConstrains(IEnumerable<Type> constrains)
	{
		IEnumerable<Type> interfaces = constrains.Where((Type x) => x.IsInterface);
		Type @base = constrains.Single((Type x) => !x.IsInterface);
		return from type in AllTypes()
			where type.IsSubclassOf(@base) && type.GetInterfaces().Intersect(interfaces).Any()
			select type;
	}

	public static CodeInstruction WithLabel(this CodeInstruction instruction, Label label)
	{
		instruction.labels.Add(label);
		return instruction;
	}
}
