using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbon.Components;

public class ConVarSnapshots
{
	public struct Snapshot
	{
		public object Value;

		public KeyValuePair<FieldInfo, ServerVar> Field;

		public KeyValuePair<Type, Factory> Factory;
	}

	public static readonly Dictionary<string, Snapshot> Snapshots = new Dictionary<string, Snapshot>();

	public static void TakeSnapshot()
	{
		try
		{
			Snapshots.Clear();
			using (TimeMeasure.New("ConVarSnapshots.TakeSnapshot", 150))
			{
				Type[] exportedTypes = typeof(BasePlayer).Assembly.GetExportedTypes();
				Type[] array = exportedTypes;
				foreach (Type type in array)
				{
					Factory customAttribute = ((MemberInfo)type).GetCustomAttribute<Factory>();
					string text = ((customAttribute == null) ? type.Name.ToLower() : customAttribute.Name);
					IEnumerable<FieldInfo> enumerable = from x in type.GetFields(BindingFlags.Static | BindingFlags.Public)
						where ((MemberInfo)x).GetCustomAttribute<ServerVar>() != null
						select x;
					foreach (FieldInfo item in enumerable)
					{
						Snapshot value = new Snapshot
						{
							Value = item.GetValue(null),
							Field = new KeyValuePair<FieldInfo, ServerVar>(item, ((MemberInfo)item).GetCustomAttribute<ServerVar>()),
							Factory = new KeyValuePair<Type, Factory>(type, customAttribute)
						};
						Snapshots.Add(text + "." + item.Name, value);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("Failed taking snapshot of all default Rust ConVar values", ex);
		}
	}
}
