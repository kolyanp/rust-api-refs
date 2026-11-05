using System;
using System.Collections.Generic;

namespace Facepunch;

public static class GamepadActions
{
	public enum Read
	{
		None,
		Digital,
		StickUp,
		StickDown,
		StickLeft,
		StickRight,
		Accelerate,
		Brake
	}

	public readonly struct Definition(string button, string action, string xbox, string playStation, Read read = Read.None, bool analog = false)
	{
		public readonly string Button = button;

		public readonly string Action = action;

		public readonly string XboxLabel = xbox;

		public readonly string PlayStationLabel = playStation;

		public readonly bool Analog = analog;

		public readonly Read Read = read;
	}

	public static readonly IReadOnlyList<Definition> All = new Definition[40]
	{
		new Definition(null, "Pause", "Start", "Options"),
		new Definition(null, "MenuConfirm", "A", "Cross"),
		new Definition(null, "MenuBack", "B", "Circle"),
		new Definition(null, "MenuOption", "X", "Square"),
		new Definition(null, "MenuHold", "Y", "Triangle"),
		new Definition(null, "MenuTabPrev", "LB", "L1"),
		new Definition(null, "MenuTabNext", "RB", "R1"),
		new Definition(null, "MenuSection", "RT", "R2"),
		new Definition(null, "MenuUp", "D-Pad Up", "D-Pad Up"),
		new Definition(null, "MenuDown", "D-Pad Down", "D-Pad Down"),
		new Definition(null, "MenuLeft", "D-Pad Left", "D-Pad Left"),
		new Definition(null, "MenuRight", "D-Pad Right", "D-Pad Right"),
		new Definition(null, "Camera", "RS", "RS", Read.None, analog: true),
		new Definition(null, "Steer", "LS", "LS", Read.None, analog: true),
		new Definition(null, "MenuSlow", "LT", "L2", Read.None, analog: true),
		new Definition("pad_attackalt", "AttackAlt", "LT", "L2", Read.Digital),
		new Definition("pad_attack", "Attack", "RT", "R2", Read.Digital),
		new Definition("pad_jump", "Jump", "A", "Cross", Read.Digital),
		new Definition("pad_duck", "Duck", "B", "Circle", Read.Digital),
		new Definition("pad_use", "Use", "X", "Square", Read.Digital),
		new Definition("pad_flashlight", "Flashlight", "Y", "Triangle", Read.Digital),
		new Definition("pad_reload", "Reload", "LB", "L1", Read.Digital),
		new Definition("pad_firemode", "FireMode", "RB", "R1", Read.Digital),
		new Definition("pad_sprint", "Sprint", "LS", "L3", Read.Digital),
		new Definition("pad_gestures", "Gestures", "D-Pad Down", "D-Pad Down", Read.Digital),
		new Definition("pad_inventory", "Inventory", "Back", "Share", Read.Digital),
		new Definition("pad_crafting", "Crafting", "D-Pad Down", "D-Pad Down", Read.Digital),
		new Definition("pad_voice", "Voice", "RS", "R3", Read.Digital),
		new Definition("pad_map", "Map", "D-Pad Up", "D-Pad Up", Read.Digital),
		new Definition("pad_invprev", "InvPrev", "D-Pad Left", "D-Pad Left", Read.Digital),
		new Definition("pad_invnext", "InvNext", "D-Pad Right", "D-Pad Right", Read.Digital),
		new Definition("pad_prevskin", "InvPrev", "D-Pad Left", "D-Pad Left", Read.Digital),
		new Definition("pad_nextskin", "InvNext", "D-Pad Right", "D-Pad Right", Read.Digital),
		new Definition("pad_horn", "Horn", "Horn", "Horn", Read.Digital),
		new Definition("pad_accelerate", "Accelerate", "RT", "R2", Read.Accelerate, analog: true),
		new Definition("pad_brake", "Brake", "LT", "L2", Read.Brake, analog: true),
		new Definition("pad_move_up", "Move", "LS Up", "LS Up", Read.StickUp, analog: true),
		new Definition("pad_move_down", "Move", "LS Down", "LS Down", Read.StickDown, analog: true),
		new Definition("pad_move_left", "Move", "LS Left", "LS Left", Read.StickLeft, analog: true),
		new Definition("pad_move_right", "Move", "LS Right", "LS Right", Read.StickRight, analog: true)
	};

	private static readonly Dictionary<string, Definition> byButton = CreateLookup();

	private static Dictionary<string, Definition> CreateLookup()
	{
		Dictionary<string, Definition> dictionary = new Dictionary<string, Definition>(StringComparer.OrdinalIgnoreCase);
		foreach (Definition item in All)
		{
			if (item.Button != null)
			{
				dictionary.Add(item.Button, item);
			}
		}
		return dictionary;
	}

	public static bool IsAnalogAction(string action)
	{
		for (int i = 0; i < All.Count; i++)
		{
			if (All[i].Action == action)
			{
				return All[i].Analog;
			}
		}
		return false;
	}

	public static bool TryGet(string button, out Definition definition)
	{
		definition = default;
		if (button != null)
		{
			return byButton.TryGetValue(button, out definition);
		}
		return false;
	}
}
