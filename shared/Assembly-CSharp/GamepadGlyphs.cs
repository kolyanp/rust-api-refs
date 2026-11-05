using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class GamepadGlyphs
{
	public enum ControllerType
	{
		Xbox,
		PlayStation
	}

	public struct Glyph
	{
		public string Label;

		public Sprite Icon;

		public static readonly Glyph Empty = new Glyph
		{
			Label = null
		};

		public bool IsValid => !string.IsNullOrEmpty(Label);

		public bool HasIcon => (Object)(object)Icon != (Object)null;

		public override string ToString()
		{
			return Label ?? string.Empty;
		}
	}

	private static ControllerType _currentType;

	private static readonly Dictionary<ControllerType, Dictionary<string, Glyph>> _glyphSets;

	static GamepadGlyphs()
	{
		_currentType = ControllerType.Xbox;
		_glyphSets = new Dictionary<ControllerType, Dictionary<string, Glyph>>();
		RegisterDefaults(ControllerType.Xbox);
		RegisterDefaults(ControllerType.PlayStation);
	}

	public static void SetControllerType(ControllerType type)
	{
		_currentType = type;
	}

	public static ControllerType GetControllerType()
	{
		return _currentType;
	}

	public static Glyph GetGlyph(string buttonName)
	{
		if (string.IsNullOrEmpty(buttonName))
		{
			return Glyph.Empty;
		}
		if (_glyphSets.TryGetValue(_currentType, out var value) && value.TryGetValue(buttonName, out var value2))
		{
			return value2;
		}
		return Glyph.Empty;
	}

	public static Glyph GetActionGlyph(string action)
	{
		for (int i = 0; i < GamepadActions.All.Count; i++)
		{
			GamepadActions.Definition definition = GamepadActions.All[i];
			if (!(definition.Action != action))
			{
				return new Glyph
				{
					Label = ((action == "Move") ? "LS" : ((_currentType == ControllerType.PlayStation) ? definition.PlayStationLabel : definition.XboxLabel))
				};
			}
		}
		return Glyph.Empty;
	}

	public static bool IsGamepadButton(string buttonName)
	{
		GamepadActions.Definition definition;
		return GamepadActions.TryGet(buttonName, out definition);
	}

	public static void RegisterGlyphSet(ControllerType type, Dictionary<string, Glyph> glyphs)
	{
		_glyphSets[type] = glyphs;
	}

	private static void RegisterDefaults(ControllerType type)
	{
		Dictionary<string, Glyph> dictionary = new Dictionary<string, Glyph>(StringComparer.OrdinalIgnoreCase);
		foreach (GamepadActions.Definition item in GamepadActions.All)
		{
			if (item.Button != null)
			{
				dictionary.Add(item.Button, new Glyph
				{
					Label = ((type == ControllerType.PlayStation) ? item.PlayStationLabel : item.XboxLabel)
				});
			}
		}
		_glyphSets[type] = dictionary;
	}
}
