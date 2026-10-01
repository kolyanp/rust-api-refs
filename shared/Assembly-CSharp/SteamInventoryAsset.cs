using System;
using UnityEngine;

public abstract class SteamInventoryAsset : ScriptableObject
{
	public int id;

	public Sprite icon;

	public Phrase displayName;

	public Phrase displayDescription;

	[Tooltip("Images to show on the Steam store page for this item. Should all be square and hosted on https://files.facepunch.com/")]
	public string[] storeImages = Array.Empty<string>();

	public SteamInventoryCategory steamCategory;

	[Tooltip("If true then this will be placed under the Limited tab, otherwise it goes under General.")]
	public bool isLimitedTimeOffer = true;
}
