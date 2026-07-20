using System;
using API.Commands;
using Carbon.Base;

public class OxideCommand : AuthenticatedCommand
{
	public string Command
	{
		get
		{
			return base.Name;
		}
		set
		{
			base.Name = value;
		}
	}

	public BaseHookable Plugin { get; set; }

	public new Action<BasePlayer, string, string[]> Callback { get; set; }

	public string[] Permissions
	{
		get
		{
			if (base.Auth != null)
			{
				return base.Auth.Permissions;
			}
			return null;
		}
		set
		{
			if (base.Auth != null)
			{
				base.Auth.Permissions = value;
			}
		}
	}

	public string[] Groups
	{
		get
		{
			if (base.Auth != null)
			{
				return base.Auth.Groups;
			}
			return null;
		}
		set
		{
			if (base.Auth != null)
			{
				base.Auth.Groups = value;
			}
		}
	}

	public int AuthLevel
	{
		get
		{
			if (base.Auth != null)
			{
				return base.Auth.AuthLevel;
			}
			return 0;
		}
		set
		{
			if (base.Auth != null)
			{
				base.Auth.AuthLevel = value;
			}
		}
	}

	public int Cooldown
	{
		get
		{
			if (base.Auth != null)
			{
				return base.Auth.Cooldown;
			}
			return 0;
		}
		set
		{
			if (base.Auth != null)
			{
				base.Auth.Cooldown = value;
			}
		}
	}

	public bool DoCooldownPenalty
	{
		get
		{
			if (base.Auth != null)
			{
				return base.Auth.DoCooldownPenalty;
			}
			return false;
		}
		set
		{
			if (base.Auth != null)
			{
				base.Auth.DoCooldownPenalty = value;
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
