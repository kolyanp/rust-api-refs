using System;
using Rust.UI;
using UnityEngine;

public class PickAFriend : UIDialog
{
	public RustInput rustInput;

	public bool AutoSelectInputField;

	public bool AllowMultiple;

	public Action<ulong, string> onSelected;

	public SteamFriendsList friendsList;

	public Func<ulong, bool> shouldShowPlayer
	{
		set
		{
			if ((Object)(object)friendsList != (Object)null)
			{
				friendsList.shouldShowPlayer = value;
			}
		}
	}
}
