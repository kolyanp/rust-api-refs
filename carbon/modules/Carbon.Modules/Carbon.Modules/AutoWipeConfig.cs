using System.Collections.Generic;

namespace Carbon.Modules;

public class AutoWipeConfig
{
	public string WipeChatCommand;

	public AutoWipeModule.WipeConfig FullWipe;

	public AutoWipeModule.WipeConfig MapWipe;

	public List<AutoWipeModule.WipeMap> Maps = new List<AutoWipeModule.WipeMap>();

	public List<AutoWipeModule.Wipe> AvailableWipes = new List<AutoWipeModule.Wipe>();

	public AutoWipeModule.WipeConfig GetWipeConfig(AutoWipeModule.Wipe wipe)
	{
		return wipe.Type switch
		{
			AutoWipeModule.WipeTypes.FullWipe => FullWipe, 
			AutoWipeModule.WipeTypes.MapWipe => MapWipe, 
			_ => default(AutoWipeModule.WipeConfig), 
		};
	}
}
