using System;
using API.Commands;
using Carbon.Base;

public class OxideCommand : AuthenticatedCommand
{
	public string Command
	{
		get
		{
			return Name;
		}
		set
		{
			Name = value;
		}
	}

	public BaseHookable Plugin { get; set; }

	public new Action<BasePlayer, string, string[]> Callback { get; set; }

	public string[] Permissions
	{
		get
		{
			if (Auth != null)
			{
				return Auth.Permissions;
			}
			return null;
		}
		set
		{
			if (Auth != null)
			{
				Auth.Permissions = value;
			}
		}
	}

	public string[] Groups
	{
		get
		{
			if (Auth != null)
			{
				return Auth.Groups;
			}
			return null;
		}
		set
		{
			if (Auth != null)
			{
				Auth.Groups = value;
			}
		}
	}

	public int AuthLevel
	{
		get
		{
			if (Auth != null)
			{
				return Auth.AuthLevel;
			}
			return 0;
		}
		set
		{
			if (Auth != null)
			{
				Auth.AuthLevel = value;
			}
		}
	}

	public int Cooldown
	{
		get
		{
			if (Auth != null)
			{
				return Auth.Cooldown;
			}
			return 0;
		}
		set
		{
			if (Auth != null)
			{
				Auth.Cooldown = value;
			}
		}
	}

	public bool DoCooldownPenalty
	{
		get
		{
			if (Auth != null)
			{
				return Auth.DoCooldownPenalty;
			}
			return false;
		}
		set
		{
			if (Auth != null)
			{
				Auth.DoCooldownPenalty = value;
			}
		}
	}

	public bool IsHidden
	{
		get
		{
			return HasFlag(CommandFlags.Hidden);
		}
		set
		{
			SetFlag(CommandFlags.Hidden, value);
		}
	}

	public bool Protected
	{
		get
		{
			return HasFlag(CommandFlags.Protected);
		}
		set
		{
			SetFlag(CommandFlags.Protected, value);
		}
	}
}
