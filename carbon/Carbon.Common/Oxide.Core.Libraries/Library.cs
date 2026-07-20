using System;
using System.Collections.Generic;
using System.Reflection;

namespace Oxide.Core.Libraries;

public class Library : IDisposable
{
	internal IDictionary<string, MethodInfo> functions;

	internal IDictionary<string, PropertyInfo> properties;

	public virtual bool IsGlobal { get; }

	public Exception LastException { get; protected set; }

	public Library()
	{
		string name = GetType().Name;
		if (!OxideMod._libraryCache.ContainsKey(name))
		{
			OxideMod._libraryCache.TryAdd(name, this);
		}
		functions = new Dictionary<string, MethodInfo>();
		properties = new Dictionary<string, PropertyInfo>();
		Type type = GetType();
		MethodInfo[] methods = type.GetMethods();
		foreach (MethodInfo methodInfo in methods)
		{
			LibraryFunction libraryFunction = null;
			try
			{
				libraryFunction = methodInfo.GetCustomAttribute<LibraryFunction>(inherit: true);
				if (libraryFunction == null)
				{
					continue;
				}
			}
			catch (TypeLoadException)
			{
				continue;
			}
			string text = libraryFunction?.Name ?? methodInfo.Name;
			if (functions.ContainsKey(text))
			{
				Interface.Oxide.LogError(type.FullName + " library tried to register an already registered function: " + text);
			}
			else
			{
				functions[text] = methodInfo;
			}
		}
		PropertyInfo[] array = type.GetProperties();
		foreach (PropertyInfo propertyInfo in array)
		{
			LibraryProperty libraryProperty = null;
			try
			{
				libraryProperty = propertyInfo.GetCustomAttribute<LibraryProperty>(inherit: true);
				if (libraryProperty == null)
				{
					continue;
				}
			}
			catch (TypeLoadException)
			{
				continue;
			}
			string text2 = libraryProperty?.Name ?? propertyInfo.Name;
			if (properties.ContainsKey(text2))
			{
				Interface.Oxide.LogError("{0} library tried to register an already registered property: {1}", type.FullName, text2);
			}
			else
			{
				properties[text2] = propertyInfo;
			}
		}
	}

	public static implicit operator bool(Library library)
	{
		return library != null;
	}

	public static bool operator !(Library library)
	{
		return !library;
	}

	public virtual void Dispose()
	{
	}

	public virtual void Shutdown()
	{
	}

	public MethodInfo GetFunction(string name)
	{
		if (!functions.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public PropertyInfo GetProperty(string name)
	{
		if (!properties.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public IEnumerable<string> GetFunctionNames()
	{
		return functions.Keys;
	}

	public IEnumerable<string> GetPropertyNames()
	{
		return properties.Keys;
	}
}
