using System;
using System.Linq;
using HarmonyLib;

namespace API.Hooks;

[AttributeUsage(AttributeTargets.Class)]
public class HookAttribute : Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Patch : Attribute
	{
		public string Name { get; }

		public string FullName { get; }

		public string Target { get; }

		public string Method { get; }

		public string[] MethodArgs { get; }

		public MethodType MethodType { get; }

		public Patch(string name, string fullName)
			: this(name, fullName, (string)null, (string)null)
		{
		}

		public Patch(string name, string fullName, string target, string method, string[] args)
			: this(name, fullName, target, method, (MethodType)0)
		{
			MethodArgs = args;
		}

		public Patch(string name, string fullName, string target, string method, string[] args, MethodType type)
			: this(name, fullName, target, method, type)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			MethodArgs = args;
		}

		public Patch(string name, string fullName, Type target, string method, Type[] args)
			: this(name, fullName, target.FullName, method, (MethodType)0)
		{
			MethodArgs = ((args == null) ? Array.Empty<string>() : args.Select((Type x) => x.FullName).ToArray());
		}

		public Patch(string name, string fullName, Type target, string method, Type[] args, MethodType type)
			: this(name, fullName, target.FullName, method, type)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			MethodArgs = ((args == null) ? Array.Empty<string>() : args.Select((Type x) => x.FullName).ToArray());
		}

		public Patch(string name, string fullName, string target, string method)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			FullName = fullName;
			Method = method;
			Name = name;
			Target = target;
			MethodType = (MethodType)0;
		}

		public Patch(string name, string fullName, string target, string method, MethodType type)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			FullName = fullName;
			Method = method;
			Name = name;
			Target = target;
			MethodType = type;
		}

		public Patch(string name, string fullName, Type target, string method)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			FullName = fullName;
			Method = method;
			Name = name;
			Target = target.FullName;
			MethodType = (MethodType)0;
		}

		public Patch(string name, string fullName, Type target, string method, MethodType type)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			FullName = fullName;
			Method = method;
			Name = name;
			Target = target.FullName;
			MethodType = type;
		}

		public Patch(string name, string fullName, string target)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
			Target = target;
			FullName = fullName;
			MethodType = (MethodType)0;
		}

		public Patch(string name, string fullName, string target, MethodType type)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
			Target = target;
			FullName = fullName;
			MethodType = type;
		}

		public Patch(string name, string fullName, Type target)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
			Target = target.FullName;
			FullName = fullName;
			MethodType = (MethodType)0;
		}

		public Patch(string name, string fullName, Type target, MethodType type)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			Name = name;
			Target = target.FullName;
			FullName = fullName;
			MethodType = type;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Options : Attribute
	{
		public HookFlags Value { get; }

		public Options(HookFlags value)
		{
			Value = value;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Identifier : Attribute
	{
		public string Value { get; }

		public Identifier(string value)
		{
			Value = value;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Dependencies : Attribute
	{
		public string[] Value { get; }

		public Dependencies(string[] value)
		{
			Value = value;
		}
	}
}
