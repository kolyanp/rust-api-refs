using System.Collections.Generic;

public static class RoomSkins
{
	private static HashSet<ulong> TransparentSkins = new HashSet<ulong>
	{
		2907132404uL, 2894956771uL, 809253752uL, 3542220717uL, 3073282711uL, 3067032045uL, 3124659970uL, 3250674853uL, 3394884613uL, 3451053222uL,
		3496390798uL, 3536700831uL, 3367691413uL, 3041586492uL, 1180981036uL, 1209586977uL, 2853221257uL, 3041343020uL, 3080563689uL, 3269931728uL,
		3429486913uL, 801937986uL, 2996049308uL, 3090452448uL, 2950760834uL
	};

	public static bool IsTransparentDoor(uint prefabId, ulong skinId)
	{
		if (prefabId == 358326125)
		{
			return true;
		}
		if (TransparentSkins.Contains(skinId))
		{
			return true;
		}
		return false;
	}
}
