using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Carbon.Components;

public class ModifierBank : List<Modifier>
{
	public bool HasPlugin(string name)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (Path.GetFileNameWithoutExtension(base[i].Path).Equals(name, StringComparison.CurrentCulture))
			{
				return true;
			}
		}
		return false;
	}

	public ModifierBank WithModifier(Modifier modifier)
	{
		Add(modifier);
		return this;
	}

	public string ToJson(Formatting formatting = (Formatting)1)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return JsonConvert.SerializeObject((object)this, formatting);
	}

	public void ToFile(string path, Formatting formatting = (Formatting)1)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		File.WriteAllText(path, ToJson(formatting));
	}
}
