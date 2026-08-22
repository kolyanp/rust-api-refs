using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Carbon.Publicizer;

public class AssemblyCSharp : Patch
{
	public AssemblyCSharp()
		: base(Patch.RustManagedDirectory, "Assembly-CSharp.dll")
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
			InjectBootstrap();
			InjectIPlayer();
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			return false;
		}
		return true;
	}

	private void InjectBootstrap()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		if (Patch.bootstrap != null)
		{
			TypeDefinition val = Patch.bootstrap.MainModule.GetType("Carbon", "Bootstrap") ?? throw new Exception("Unable to get a type for 'Carbon.Bootstrap'");
			MethodDefinition val2 = ((IEnumerable<MethodDefinition>)val.Methods).Single((MethodDefinition x) => ((MemberReference)x).Name == "Initialize") ?? throw new Exception("Unable to get a method definition for 'Tier0'");
			TypeDefinition val3 = assembly.MainModule.GetType("Bootstrap") ?? throw new Exception("Unable to get a type for 'Bootstrap'");
			MethodDefinition val4 = ((IEnumerable<MethodDefinition>)val3.Methods).Single((MethodDefinition x) => ((MemberReference)x).Name == "Init_Tier0") ?? throw new Exception("Unable to get a method definition for 'Init_Tier0'");
			if (!((IEnumerable<Instruction>)val4.Body.Instructions).Any(delegate(Instruction x)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return x.OpCode == OpCodes.Call && x.Operand.ToString().Contains("Carbon.Bootstrap::Initialize");
			}))
			{
				ILProcessor iLProcessor = val4.Body.GetILProcessor();
				Instruction val5 = iLProcessor.Create(OpCodes.Call, assembly.MainModule.ImportReference((MethodReference)(object)val2));
				val4.Body.Instructions[val4.Body.Instructions.Count - 1] = val5;
				val4.Body.Instructions.Insert(val4.Body.Instructions.Count, iLProcessor.Create(OpCodes.Ret));
				MethodBodyRocks.OptimizeMacros(val4.Body);
			}
		}
	}

	private void InjectIPlayer()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		FieldDefinition val = ((IEnumerable<FieldDefinition>)assembly.MainModule.GetType("BasePlayer").Fields).FirstOrDefault((FieldDefinition x) => ((MemberReference)x).Name == "IPlayer");
		if (val != null)
		{
			return;
		}
		try
		{
			TypeDefinition val2 = Patch.common.MainModule.GetType("Oxide.Core.Libraries.Covalence", "IPlayer") ?? throw new Exception("Unable to get a type for 'API.Contracts.IPlayer'");
			TypeDefinition val3 = assembly.MainModule.GetType("BasePlayer") ?? throw new Exception("Unable to get a type for 'BasePlayer'");
			val3.Fields.Add(new FieldDefinition("IPlayer", (FieldAttributes)134, assembly.MainModule.ImportReference((TypeReference)(object)val2)));
		}
		catch
		{
		}
	}
}
