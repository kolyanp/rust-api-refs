using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Sound Remapping")]
public class SoundRemapping : ScriptableObject
{
	[Serializable]
	public struct Entry
	{
		public SoundDefinition from;

		public SoundDefinition to;
	}

	public Entry[] entries = Array.Empty<Entry>();

	private Dictionary<SoundDefinition, SoundDefinition> lookup;

	public SoundDefinition Remap(SoundDefinition sound)
	{
		if (lookup == null)
		{
			BuildLookup();
		}
		if (!lookup.TryGetValue(sound, out var value) || !((Object)(object)value != (Object)null))
		{
			return sound;
		}
		return value;
	}

	private void BuildLookup()
	{
		lookup = new Dictionary<SoundDefinition, SoundDefinition>(entries.Length);
		Entry[] array = entries;
		for (int i = 0; i < array.Length; i++)
		{
			Entry entry = array[i];
			if ((Object)(object)entry.from != (Object)null)
			{
				lookup[entry.from] = entry.to;
			}
		}
	}

	private void OnValidate()
	{
		lookup = null;
	}
}
