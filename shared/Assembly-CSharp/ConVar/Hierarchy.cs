using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("hierarchy")]
public class Hierarchy : ConsoleSystem
{
	private static GameObject currentDir;

	private static Transform[] GetCurrent()
	{
		if ((Object)(object)currentDir == (Object)null)
		{
			return TransformUtil.GetRootObjects().ToArray();
		}
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < currentDir.transform.childCount; i++)
		{
			list.Add(currentDir.transform.GetChild(i));
		}
		return list.ToArray();
	}

	[ServerVar(Help = "(Generated) Lists all GameObjects in the current hierarchy context, similar to the Unix ls command; used for navigating the scene hierarchy from the console")]
	public static void ls(Arg args)
	{
		string text = "";
		string filter = args.GetString(0);
		text = ((!Object.op_Implicit((Object)(object)currentDir)) ? (text + "Listing .\n\n") : (text + "Listing " + TransformEx.GetRecursiveName(currentDir.transform) + "\n\n"));
		foreach (Transform item in (from x in GetCurrent()
			where string.IsNullOrEmpty(filter) || ((Object)x).name.Contains(filter)
			select x).Take(40))
		{
			text += $"   {((Object)item).name} [{item.childCount}]\n";
		}
		text += "\n";
		args.ReplyWith(text);
	}

	[ServerVar(Help = "(Generated) Changes the current hierarchy context to the named child GameObject, similar to the Unix cd command; allows drilling into nested scene objects")]
	public static void cd(Arg args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (args.FullString == StringView.op_Implicit("."))
		{
			currentDir = null;
			args.ReplyWith("Changed to .");
			return;
		}
		if (args.FullString == StringView.op_Implicit(".."))
		{
			if (Object.op_Implicit((Object)(object)currentDir))
			{
				currentDir = (Object.op_Implicit((Object)(object)currentDir.transform.parent) ? ((Component)currentDir.transform.parent).gameObject : null);
			}
			currentDir = null;
			if (Object.op_Implicit((Object)(object)currentDir))
			{
				args.ReplyWith("Changed to " + TransformEx.GetRecursiveName(currentDir.transform));
			}
			else
			{
				args.ReplyWith("Changed to .");
			}
			return;
		}
		string argsStringLower = ((object)Unsafe.As<StringView, StringView>(ref args.FullString)/*cast due to constrained. prefix*/).ToString().ToLower();
		Transform val = GetCurrent().FirstOrDefault((Transform x) => ((Object)x).name.ToLower() == argsStringLower);
		if ((Object)(object)val == (Object)null)
		{
			val = GetCurrent().FirstOrDefault((Transform x) => ((Object)x).name.StartsWith(argsStringLower, StringComparison.CurrentCultureIgnoreCase));
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			currentDir = ((Component)val).gameObject;
			args.ReplyWith("Changed to " + TransformEx.GetRecursiveName(currentDir.transform));
		}
		else
		{
			args.ReplyWith("Couldn't find \"" + ((object)Unsafe.As<StringView, StringView>(ref args.FullString)/*cast due to constrained. prefix*/).ToString() + "\"");
		}
	}

	[ServerVar(Help = "(Generated) Deletes the named GameObject from the scene hierarchy; use with caution as this permanently removes the object")]
	public static void del(Arg args)
	{
		if (!args.HasArgs())
		{
			return;
		}
		string argsStringLower = ((object)Unsafe.As<StringView, StringView>(ref args.FullString)/*cast due to constrained. prefix*/).ToString().ToLower();
		IEnumerable<Transform> enumerable = from x in GetCurrent()
			where ((Object)x).name.ToLower() == argsStringLower
			select x;
		if (enumerable.Count() == 0)
		{
			enumerable = from x in GetCurrent()
				where ((Object)x).name.StartsWith(argsStringLower, StringComparison.CurrentCultureIgnoreCase)
				select x;
		}
		if (enumerable.Count() == 0)
		{
			args.ReplyWith("Couldn't find  " + ((object)Unsafe.As<StringView, StringView>(ref args.FullString)/*cast due to constrained. prefix*/).ToString());
			return;
		}
		foreach (Transform item in enumerable)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((Component)item).gameObject);
			if (baseEntity.IsValid())
			{
				if (baseEntity.isServer)
				{
					baseEntity.Kill();
				}
			}
			else
			{
				GameManager.Destroy(((Component)item).gameObject);
			}
		}
		args.ReplyWith("Deleted " + enumerable.Count() + " objects");
	}
}
