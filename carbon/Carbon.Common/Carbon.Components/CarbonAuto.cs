using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using API.Abstracts;
using Carbon.Core;
using Carbon.Extensions;

namespace Carbon.Components;

public class CarbonAuto : API.Abstracts.CarbonAuto
{
	public struct AutoVar
	{
		public CarbonAutoVar Variable;

		public object ReflectionInfo;

		public readonly Type GetVarType()
		{
			object reflectionInfo = ReflectionInfo;
			if (!(reflectionInfo is FieldInfo fieldInfo))
			{
				if (reflectionInfo is PropertyInfo propertyInfo)
				{
					return propertyInfo.PropertyType;
				}
				return null;
			}
			return fieldInfo.FieldType;
		}

		public readonly object GetValue()
		{
			CorePlugin core = Community.Runtime.Core;
			object reflectionInfo = ReflectionInfo;
			if (!(reflectionInfo is FieldInfo fieldInfo))
			{
				if (reflectionInfo is PropertyInfo propertyInfo)
				{
					return propertyInfo.GetValue(core);
				}
				return null;
			}
			return fieldInfo.IsStatic ? fieldInfo.GetValue(null) : fieldInfo.GetValue(core);
		}

		public void SetValue(object value)
		{
			CorePlugin core = Community.Runtime.Core;
			object reflectionInfo = ReflectionInfo;
			if (!(reflectionInfo is FieldInfo fieldInfo))
			{
				if (reflectionInfo is PropertyInfo propertyInfo)
				{
					propertyInfo.SetValue(core, Convert.ChangeType(value, GetVarType()));
				}
			}
			else
			{
				fieldInfo.SetValue(fieldInfo.IsStatic ? null : core, Convert.ChangeType(value, GetVarType()));
			}
		}

		public readonly bool IsChanged()
		{
			object value = GetValue();
			if (value == null)
			{
				return false;
			}
			return !value.Equals(Convert.ChangeType(-1, GetVarType()));
		}
	}

	public static Dictionary<string, AutoVar> AutoCache = new Dictionary<string, AutoVar>();

	internal bool _initialized;

	public static void Init()
	{
		API.Abstracts.CarbonAuto.Singleton = new CarbonAuto();
		API.Abstracts.CarbonAuto.Singleton.Refresh();
	}

	public override void Refresh()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		Type typeFromHandle = typeof(CorePlugin);
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		FieldInfo[] fields = typeFromHandle.GetFields(bindingAttr);
		PropertyInfo[] properties = typeFromHandle.GetProperties(bindingAttr);
		AutoCache.Clear();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			CarbonAutoVar customAttribute = fieldInfo.GetCustomAttribute<CarbonAutoVar>();
			if (customAttribute != null)
			{
				AutoVar value = new AutoVar
				{
					Variable = customAttribute,
					ReflectionInfo = fieldInfo
				};
				AutoCache.Add("c." + customAttribute.Name, value);
			}
		}
		PropertyInfo[] array2 = properties;
		foreach (PropertyInfo propertyInfo in array2)
		{
			CarbonAutoVar customAttribute2 = propertyInfo.GetCustomAttribute<CarbonAutoVar>();
			if (customAttribute2 != null)
			{
				AutoVar value2 = new AutoVar
				{
					Variable = customAttribute2,
					ReflectionInfo = propertyInfo
				};
				AutoCache.Add("c." + customAttribute2.Name, value2);
			}
		}
	}

	public override bool IsForceModded()
	{
		using (TimeMeasure.New("CarbonAuto.IsChanged"))
		{
			foreach (KeyValuePair<string, AutoVar> item in AutoCache)
			{
				if (item.Value.Variable.ForceModded)
				{
					object value = item.Value.GetValue();
					if (value is float && (float)value != -1f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void Save()
	{
		using (TimeMeasure.New("CarbonAuto.Save"))
		{
			try
			{
				Refresh();
				using StringBody stringBody = default(StringBody);
				foreach (KeyValuePair<string, AutoVar> item in AutoCache)
				{
					stringBody.Add($"{item.Key} \"{item.Value.GetValue()}\"");
				}
				OsEx.File.Create(Defines.GetCarbonAutoFile(), stringBody.ToNewLine());
			}
			catch (Exception ex)
			{
				Logger.Error("Failed saving Carbon auto file", ex);
			}
		}
	}

	public override void Load()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		using (TimeMeasure.New("CarbonAuto.Load"))
		{
			try
			{
				Refresh();
				string carbonAutoFile = Defines.GetCarbonAutoFile();
				if (!OsEx.File.Exists(carbonAutoFile))
				{
					Save();
					return;
				}
				string[] array = OsEx.File.ReadTextLines(carbonAutoFile);
				Option server = Option.Server;
				if (!Community.Runtime.Config.Logging.ReducedLogging)
				{
					Logger.Log(string.Format("Initialized Carbon Auto ({0:n0} {1})", array.Length, array.Length.Plural("variable", "variables")));
				}
				string[] array2 = array;
				foreach (string text in array2)
				{
					try
					{
						string[] array3 = text.Split(' ');
						string text2 = ((array3.Length != 0) ? array3[0] : null);
						string value = array3.Skip(1).ToString(" ").Replace("\"", string.Empty);
						if (AutoCache.TryGetValue(text2, out var value2))
						{
							value2.SetValue(value);
							if (!Community.Runtime.Config.Logging.ReducedLogging)
							{
								Logger.Warn(string.Format(" {0} \"{1}\"{2}", text2, value2.GetValue(), value2.Variable.ForceModded ? " [modded]" : string.Empty));
							}
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Failed processing line '" + text + "'", ex);
					}
				}
				if (IsForceModded())
				{
					Logger.Warn("Carbon Auto: Gameplay-significant options have been modified. Please run c.whymodded to see why the server's modded");
				}
			}
			catch (Exception ex2)
			{
				Logger.Error("Failed loading Carbon auto file", ex2);
			}
		}
	}
}
