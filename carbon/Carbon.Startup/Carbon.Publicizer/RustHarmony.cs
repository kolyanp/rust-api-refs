using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Carbon.Publicizer;

public class RustHarmony : Patch
{
	public RustHarmony()
		: base(Patch.RustManagedDirectory, "Rust.Harmony.dll")
	{
	}

	public override bool Execute()
	{
		if (!base.Execute())
		{
			return false;
		}
		try
		{
			PatchLoadHarmonyMods();
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			return false;
		}
		return true;
	}

	private void PatchLoadHarmonyMods()
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		TypeDefinition type = assembly.MainModule.GetType("HarmonyLoader");
		MethodDefinition val = ((IEnumerable<MethodDefinition>)type.Methods).FirstOrDefault((MethodDefinition x) => ((MemberReference)x).Name == "LoadHarmonyMods");
		TypeDefinition type2 = Patch.facepunchSystem.MainModule.GetType("Facepunch.CommandLine");
		MethodDefinition val2 = ((type2 != null) ? ((IEnumerable<MethodDefinition>)type2.Methods).FirstOrDefault((MethodDefinition x) => ((MemberReference)x).Name == "GetSwitch") : null);
		if (val2 != null && val != null && val.HasBody)
		{
			Collection<Instruction> instructions = ((IEnumerable<MethodDefinition>)((IEnumerable<TypeDefinition>)type.NestedTypes).FirstOrDefault((TypeDefinition x) => ((MemberReference)x).FullName == "HarmonyLoader/<>c").Methods).FirstOrDefault((MethodDefinition x) => ((MemberReference)x).Name == "<LoadHarmonyMods>b__12_0").Body.Instructions;
			for (int num = 0; num < 5; num++)
			{
				instructions.RemoveAt(0);
			}
			MethodReference val3 = assembly.MainModule.ImportReference((MethodReference)(object)val2);
			MethodReference val4 = assembly.MainModule.ImportReference((MethodBase)typeof(Path).GetMethod("Combine", new Type[2]
			{
				typeof(string),
				typeof(string)
			}));
			val.Body.Instructions.RemoveAt(21);
			val.Body.Instructions.RemoveAt(21);
			val.Body.Instructions.RemoveAt(21);
			val.Body.Instructions.Insert(21, Instruction.Create(OpCodes.Ldstr, "-harmonydir"));
			val.Body.Instructions.Insert(22, Instruction.Create(OpCodes.Ldloc_0));
			val.Body.Instructions.Insert(23, Instruction.Create(OpCodes.Ldstr, "HarmonyMods"));
			val.Body.Instructions.Insert(24, Instruction.Create(OpCodes.Call, val4));
			val.Body.Instructions.Insert(25, Instruction.Create(OpCodes.Call, val3));
		}
	}
}
