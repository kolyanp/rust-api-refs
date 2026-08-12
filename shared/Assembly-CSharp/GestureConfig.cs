using ConVar;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "Rust/Gestures/Gesture Config")]
public class GestureConfig : ScriptableObject
{
	public enum GestureType
	{
		Player,
		NPC,
		Cinematic
	}

	public enum PlayerModelLayer
	{
		UpperBody = 3,
		FullBody
	}

	public enum MovementCapabilities
	{
		FullMovement,
		NoMovement
	}

	public enum AnimationType
	{
		OneShot,
		Loop
	}

	public enum GestureActionType
	{
		None,
		ShowNameTag,
		DanceAchievement,
		Surrender,
		RockPaperScissors
	}

	[ReadOnly]
	public uint gestureId;

	public string gestureCommand;

	public string convarName;

	public Phrase gestureName;

	public Phrase gestureDescription;

	public Sprite icon;

	public AnimationType animationType;

	public float duration;

	public bool canCancel;

	public MovementCapabilities movementMode;

	public BasePlayer.CameraMode viewMode;

	public bool hideInWheel;

	public VideoClip previewClip;

	[Header("Player model setup")]
	public PlayerModelLayer playerModelLayer;

	public GestureType gestureType;

	public bool hideHeldEntity;

	public bool unequipHeldEntity;

	public bool canDuckDuringGesture;

	public bool hasViewmodelAnimation;

	public float viewmodelHolsterDelay;

	public bool useRootMotion;

	public bool forceForwardRotation;

	public bool forceAllowSpineMovement;

	[Header("Interaction")]
	public bool hasMultiplayerInteraction;

	public Phrase joinPlayerPhrase;

	public Phrase joinPlayerDescPhrase;

	[Header("Ownership")]
	public GestureActionType actionType;

	public bool forceUnlock;

	public SteamDLCItem dlcItem;

	public SteamInventoryItem inventoryItem;

	public int GetItemId()
	{
		if ((Object)(object)dlcItem != (Object)null)
		{
			return dlcItem.dlcAppID;
		}
		if ((Object)(object)inventoryItem != (Object)null)
		{
			return inventoryItem.id;
		}
		return 0;
	}

	public Phrase GetSteamItemName()
	{
		if ((Object)(object)dlcItem != (Object)null)
		{
			return dlcItem.dlcName;
		}
		if ((Object)(object)inventoryItem != (Object)null)
		{
			return inventoryItem.displayName;
		}
		return null;
	}

	public bool IsOwnedBy(BasePlayer player, bool allowCinematic = false)
	{
		object obj = Interface.CallHook("CanUseGesture", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (forceUnlock)
		{
			return true;
		}
		if (gestureType == GestureType.NPC)
		{
			if ((Object)(object)player != (Object)null)
			{
				return player.IsNpc;
			}
			return false;
		}
		if (gestureType == GestureType.Cinematic)
		{
			if (!allowCinematic && (!((Object)(object)player != (Object)null) || !player.IsAdmin))
			{
				return Server.cinematic;
			}
			return true;
		}
		return IsUnlockedBy(player);
	}

	public bool IsUnlockedBy(BasePlayer player)
	{
		if (forceUnlock)
		{
			return true;
		}
		if ((Object)(object)dlcItem != (Object)null && (Object)(object)player != (Object)null)
		{
			return dlcItem.CanUse(player);
		}
		if ((Object)(object)inventoryItem != (Object)null && (Object)(object)player != (Object)null && player.blueprints.steamInventory.HasItem(inventoryItem.id))
		{
			return true;
		}
		return false;
	}

	public bool CanBeUsedBy(BasePlayer player)
	{
		if (player.isMounted)
		{
			if (playerModelLayer == PlayerModelLayer.FullBody)
			{
				return false;
			}
			if (player.GetMounted().allowedGestures == BaseMountable.MountGestureType.None)
			{
				return false;
			}
		}
		if (player.IsSwimming() && playerModelLayer == PlayerModelLayer.FullBody)
		{
			return false;
		}
		if (playerModelLayer == PlayerModelLayer.FullBody && player.modelState.ducked)
		{
			return false;
		}
		return true;
	}

	public GestureConfig()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		duration = 1.5f;
		canCancel = true;
		playerModelLayer = PlayerModelLayer.UpperBody;
		hideHeldEntity = true;
		hasViewmodelAnimation = true;
		joinPlayerPhrase = new Phrase("", "");
		joinPlayerDescPhrase = new Phrase("", "");
		((ScriptableObject)this)._002Ector();
	}
}
