using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAchievementAdvanced : TriggerBase
{
	public enum AchievementTriggerMode
	{
		Enter,
		Exit
	}

	[Flags]
	public enum ExitSideMask
	{
		Any = 0,
		Front = 1,
		Bottom = 2,
		Left = 4,
		Right = 8,
		Back = 0x10,
		Top = 0x20
	}

	public string statToIncrease = "";

	public string achievementName = "";

	public string requiredVehicleName = "";

	public bool allowDuringTutorial;

	[SerializeField]
	private BoxCollider boxCollider;

	public AchievementTriggerMode triggerMode;

	public ExitSideMask requiredExitSides;

	public BasePlayer.PlayerFlags requiredPlayerFlags;

	public string[] requireWearingItemNames = Array.Empty<string>();

	[NonSerialized]
	private List<ulong> triggeredPlayers = new List<ulong>();

	public void OnPuzzleReset()
	{
		Reset();
	}

	public void Reset()
	{
		triggeredPlayers.Clear();
	}

	internal override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}

	protected virtual bool VerifyAdditionalConditions(BasePlayer ply, bool onExit)
	{
		if (requireWearingItemNames.Length != 0 && !IsWearingRequiredItems(ply))
		{
			return false;
		}
		if (requiredPlayerFlags != 0 && !ply.HasPlayerFlag(requiredPlayerFlags))
		{
			return false;
		}
		return true;
	}

	private bool IsWearingRequiredItems(BasePlayer ply)
	{
		if ((Object)(object)ply == (Object)null || (Object)(object)ply.inventory == (Object)null || ply.inventory.containerWear == null)
		{
			return false;
		}
		bool result = true;
		string[] array = requireWearingItemNames;
		foreach (string name in array)
		{
			if (ply.inventory.containerWear.FindItemByItemName(name) == null)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal override void OnEntityEnter(BaseEntity ent)
	{
		base.OnEntityEnter(ent);
		TryGrant(ent, onExit: false);
	}

	internal override void OnEntityLeave(BaseEntity ent)
	{
		base.OnEntityLeave(ent);
		TryGrant(ent, onExit: true);
	}

	private void TryGrant(BaseEntity ent, bool onExit)
	{
		if ((!onExit || triggerMode == AchievementTriggerMode.Exit) && (onExit || triggerMode == AchievementTriggerMode.Enter) && !((Object)(object)ent == (Object)null) && !ent.isClient && IsValidPlayer(ent, out var ply) && (requiredExitSides == ExitSideMask.Any || IsAllowedExitSide(ply)) && VerifyAdditionalConditions(ply, onExit))
		{
			GrantToPlayer(ply);
		}
	}

	private bool IsValidPlayer(BaseEntity ent, out BasePlayer ply)
	{
		ply = null;
		if ((Object)(object)ent == (Object)null)
		{
			return false;
		}
		ply = ((Component)ent).GetComponent<BasePlayer>();
		if ((Object)(object)ply == (Object)null || !ply.IsAlive() || ply.IsSleeping() || ply.IsNpc)
		{
			return false;
		}
		if (triggeredPlayers.Contains(ply.userID))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(requiredVehicleName))
		{
			BaseVehicle mountedVehicle = ply.GetMountedVehicle();
			if ((Object)(object)mountedVehicle == (Object)null)
			{
				return false;
			}
			if (!mountedVehicle.ShortPrefabName.Contains(requiredVehicleName))
			{
				return false;
			}
		}
		return true;
	}

	private void GrantToPlayer(BasePlayer ply)
	{
		if (!ply.isClient)
		{
			if (!string.IsNullOrEmpty(achievementName))
			{
				ply.GiveAchievement(achievementName, allowDuringTutorial);
			}
			if (!string.IsNullOrEmpty(statToIncrease))
			{
				ply.stats.Add(statToIncrease, 1);
				ply.stats.Save(forceSteamSave: true);
			}
			triggeredPlayers.Add(ply.userID);
		}
	}

	private bool IsAllowedExitSide(BasePlayer ply)
	{
		if (requiredExitSides == ExitSideMask.Any)
		{
			return true;
		}
		ExitSideMask exitSide = GetExitSide(ply);
		if (exitSide == ExitSideMask.Any)
		{
			return false;
		}
		return (requiredExitSides & exitSide) != 0;
	}

	private ExitSideMask GetExitSide(BasePlayer ply)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)boxCollider))
		{
			return ExitSideMask.Any;
		}
		Vector3 val = ((Component)boxCollider).transform.InverseTransformPoint(((Component)ply).transform.position) - boxCollider.center;
		Vector3 val2 = boxCollider.size * 0.5f;
		float num = ((val2.x > 0f) ? (Mathf.Abs(val.x) / val2.x) : (-1f));
		float num2 = ((val2.y > 0f) ? (Mathf.Abs(val.y) / val2.y) : (-1f));
		float num3 = ((val2.z > 0f) ? (Mathf.Abs(val.z) / val2.z) : (-1f));
		if (num >= num2 && num >= num3)
		{
			if (!(val.x >= 0f))
			{
				return ExitSideMask.Left;
			}
			return ExitSideMask.Right;
		}
		if (num2 >= num && num2 >= num3)
		{
			if (!(val.y >= 0f))
			{
				return ExitSideMask.Bottom;
			}
			return ExitSideMask.Top;
		}
		if (!(val.z >= 0f))
		{
			return ExitSideMask.Back;
		}
		return ExitSideMask.Front;
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		BoxCollider val = (Object.op_Implicit((Object)(object)boxCollider) ? boxCollider : ((Component)this).GetComponent<BoxCollider>());
		if (Object.op_Implicit((Object)(object)val))
		{
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = ((Component)val).transform.localToWorldMatrix;
			Vector3 center = val.center;
			Vector3 size = val.size;
			float markerSize = Mathf.Max(Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.08f, 0.03f);
			Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.75f);
			Gizmos.DrawWireCube(center, size);
			DrawSideGizmo(center, size, Vector3.forward, ExitSideMask.Front, markerSize);
			DrawSideGizmo(center, size, Vector3.back, ExitSideMask.Back, markerSize);
			DrawSideGizmo(center, size, Vector3.left, ExitSideMask.Left, markerSize);
			DrawSideGizmo(center, size, Vector3.right, ExitSideMask.Right, markerSize);
			DrawSideGizmo(center, size, Vector3.up, ExitSideMask.Top, markerSize);
			DrawSideGizmo(center, size, Vector3.down, ExitSideMask.Bottom, markerSize);
			Gizmos.matrix = matrix;
		}
	}

	private void DrawSideGizmo(Vector3 center, Vector3 size, Vector3 normal, ExitSideMask side, float markerSize)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = ((requiredExitSides == ExitSideMask.Any || (requiredExitSides & side) != ExitSideMask.Any) ? new Color(0.2f, 1f, 0.35f, 0.9f) : new Color(1f, 0.25f, 0.25f, 0.7f));
		Vector3 val = size * 0.5f;
		Vector3 val2 = center + Vector3.Scale(normal, val);
		Vector3 val3 = val2 + normal * (markerSize * 3f);
		Gizmos.DrawLine(val2, val3);
		Gizmos.DrawSphere(val3, markerSize);
	}
}
