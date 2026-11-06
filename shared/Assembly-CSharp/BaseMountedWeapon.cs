using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust.Ai.Gen2;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class BaseMountedWeapon : BaseMountedUseable
{
	[Serializable]
	private struct Ammo
	{
		public ItemDefinition item;

		public GameObject go;
	}

	[Serializable]
	private struct FiringEffect
	{
		public ItemDefinition item;

		public GameObjectRef effectPrefab;
	}

	[Header("Loaded Gun")]
	[SerializeField]
	private bool syncAimDirOnFire = true;

	[SerializeField]
	private float reloadTime = 3f;

	[SerializeField]
	private bool syncAimDirOnReload = true;

	[SerializeField]
	private bool reloadPreventsAiming;

	[SerializeField]
	protected BUTTON reloadButton = BUTTON.RELOAD;

	[SerializeField]
	private SoundDefinition reloadedSound;

	[SerializeField]
	protected BaseProjectile.Magazine magazine;

	[Space]
	[SerializeField]
	protected Animator animator;

	[SerializeField]
	private GameObject ammoParent;

	[SerializeField]
	private Ammo[] ammoPrefabs;

	[SerializeField]
	[Header("Effects")]
	private FiringEffect[] muzzleFireEffects;

	[SerializeField]
	[Space]
	private bool runSideChecks;

	[SerializeField]
	private Transform leftGroundCheckTransform;

	[SerializeField]
	private Transform rightGroundCheckTransform;

	[SerializeField]
	private Transform[] leftSideCheckPositions;

	[SerializeField]
	private Transform[] rightSideCheckPositions;

	[SerializeField]
	private Vector3 originalLocalMountPos;

	[SerializeField]
	private bool runBoundsChecks;

	[SerializeField]
	private Bounds[] areaChecks;

	protected static readonly int Reload = Animator.StringToHash("Reload");

	public const Flags Flag_Reloading = Flags.Reserved4;

	public const Flags Flag_Loaded = Flags.Reserved5;

	[HideInInspector]
	public float reloadProgress;

	private TimeSince lastReloadStartTime;

	private BasePlayer reloadingPlayer;

	private readonly float progressTickRate = 0.1f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMountedWeapon.OnRpcMessage"))
		{
			if (rpc == 1188838966 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_CancelReload"));
				}
				using (TimeWarning.New("SERVER_CancelReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1188838966u, "SERVER_CancelReload", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(1188838966u, "SERVER_CancelReload", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_CancelReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_CancelReload");
					}
				}
				return true;
			}
			if (rpc == 2817383917u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_ReloadStart"));
				}
				using (TimeWarning.New("SERVER_ReloadStart"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2817383917u, "SERVER_ReloadStart", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(2817383917u, "SERVER_ReloadStart", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_ReloadStart(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_ReloadStart");
					}
				}
				return true;
			}
			if (rpc == 4118009042u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SwitchAmmoTo"));
				}
				using (TimeWarning.New("SERVER_SwitchAmmoTo"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4118009042u, "SERVER_SwitchAmmoTo", this, player, 5uL))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SERVER_SwitchAmmoTo(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_SwitchAmmoTo");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsLoaded()
	{
		if (HasFlag(Flags.Reserved5) && magazine.contents == magazine.capacity)
		{
			return reloadProgress >= 0.94f;
		}
		return false;
	}

	private bool CanFire()
	{
		if (IsLoaded() && !HasFlag(Flags.Reserved4))
		{
			return !OwnerIsWaterlogged();
		}
		return false;
	}

	protected virtual bool CanReload()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsLoaded() && TimeSince.op_Implicit(lastReloadStartTime) > 1f)
		{
			return !OwnerIsWaterlogged();
		}
		return false;
	}

	protected override bool CanAim()
	{
		if (reloadPreventsAiming)
		{
			return !HasFlag(Flags.Reserved4);
		}
		return true;
	}

	public virtual bool OwnerIsWaterlogged()
	{
		return false;
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void InitShared()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.InitShared();
		originalLocalMountPos = mountAnchor.localPosition;
	}

	protected override void LimitAimInput(ref float pitchAdjustAmount, ref float yawAdjustAmount)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		float y = mountAnchor.position.y;
		MoveMountAnchor();
		float num = y - mountAnchor.position.y;
		if (Mathf.Abs(num) > 0.0001f)
		{
			pitchAdjustAmount += (0f - num) * 50f;
		}
		if (!CanRotateInDirection(yawAdjustAmount > 0f))
		{
			yawAdjustAmount = 0f;
		}
	}

	private bool CanRotateInDirection(bool rotatingLeft)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Transform checkTransform = (rotatingLeft ? leftGroundCheckTransform : rightGroundCheckTransform);
		Transform[] array = (rotatingLeft ? leftSideCheckPositions : rightSideCheckPositions);
		if (!HasGround(checkTransform))
		{
			return false;
		}
		if (runSideChecks)
		{
			Transform[] array2 = array;
			foreach (Transform val in array2)
			{
				if (HasColliderBlockingRotation(val.position, val.forward))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override bool TryRotate(Transform rotatingTransform, Quaternion targetRotation)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)rotatingTransform != (Object)(object)yawTransform || !HasGroundOnBothSides())
		{
			return RotateUntilBlocked(rotatingTransform, targetRotation);
		}
		Quaternion rotation = rotatingTransform.rotation;
		bool result = RotateUntilBlocked(rotatingTransform, targetRotation);
		if (HasGroundOnBothSides())
		{
			return result;
		}
		rotatingTransform.rotation = rotation;
		return false;
	}

	private bool HasGroundOnBothSides()
	{
		if (HasGround(leftGroundCheckTransform))
		{
			return HasGround(rightGroundCheckTransform);
		}
		return false;
	}

	private bool RotateUntilBlocked(Transform rotatingTransform, Quaternion targetRotation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		if (!runBoundsChecks)
		{
			rotatingTransform.rotation = targetRotation;
			return true;
		}
		Quaternion startRotation = rotatingTransform.rotation;
		Quaternion inverseStartRotation = Quaternion.Inverse(startRotation);
		Vector3 pivot = rotatingTransform.position;
		Vector3 pitchPosition = pitchTransform.position;
		Quaternion pitchRotation = pitchTransform.rotation;
		float num = 0f;
		Bounds[] array = areaChecks;
		for (int i = 0; i < array.Length; i++)
		{
			Bounds val = array[i];
			Vector3 val2 = pitchPosition + pitchRotation * val.center;
			float num2 = num;
			float num3 = Vector3.Distance(pivot, val2);
			Vector3 extents = val.extents;
			num = Mathf.Max(num2, num3 + extents.magnitude);
		}
		float num4 = Quaternion.Angle(startRotation, targetRotation) * (MathF.PI / 180f) * num;
		int num5 = Mathf.Max(1, Mathf.CeilToInt(num4 / 0.025f));
		float stepPadding = num4 / (float)num5 * 0.5f;
		float num6 = AreaCheckOverlap(pitchPosition, pitchRotation);
		if (num6 > 0f)
		{
			float num7 = 0f;
			for (int j = 1; j <= Mathf.Min(num5, 16); j++)
			{
				float num8 = (float)j / (float)num5;
				Quaternion val3 = Quaternion.Slerp(startRotation, targetRotation, num8) * inverseStartRotation;
				float num9 = AreaCheckOverlap(pivot + val3 * (pitchPosition - pivot), val3 * pitchRotation);
				if (num9 > num6)
				{
					break;
				}
				num7 = num8;
				if (num9 == 0f)
				{
					break;
				}
			}
			rotatingTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, num7);
			return num7 >= 1f;
		}
		for (int k = 1; k <= num5; k++)
		{
			if (!CheckRotationStep(((float)k - 1f) / (float)num5, (float)k / (float)num5, stepPadding, 8))
			{
				rotatingTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, ((float)k - 1f) / (float)num5);
				return false;
			}
		}
		rotatingTransform.rotation = targetRotation;
		return true;
		bool CheckRotationStep(float from, float to, float num11, int remainingSubdivisions)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			float num10 = (from + to) * 0.5f;
			Quaternion val4 = Quaternion.Slerp(startRotation, targetRotation, num10) * inverseStartRotation;
			Vector3 position = pivot + val4 * (pitchPosition - pivot);
			Quaternion rotation = val4 * pitchRotation;
			if (AreaChecksAreClear(position, rotation, num11, 1f))
			{
				return true;
			}
			if (remainingSubdivisions == 0)
			{
				return false;
			}
			if (CheckRotationStep(from, num10, num11 * 0.5f, remainingSubdivisions - 1))
			{
				return CheckRotationStep(num10, to, num11 * 0.5f, remainingSubdivisions - 1);
			}
			return false;
		}
	}

	private float AreaCheckOverlap(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (AreaChecksAreClear(position, rotation, 0f, 1f))
		{
			return 0f;
		}
		float num = 0f;
		float num2 = 1f;
		for (int i = 0; i < 6; i++)
		{
			float num3 = (num + num2) * 0.5f;
			if (AreaChecksAreClear(position, rotation, 0f, num3))
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return 1f - num;
	}

	private bool AreaChecksAreClear(Vector3 position, Quaternion rotation, float padding, float scale)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Bounds[] array = areaChecks;
		for (int i = 0; i < array.Length; i++)
		{
			Bounds val = array[i];
			val.extents *= scale;
			val.Expand(padding * 2f);
			if (SocketMod_AreaCheck.IsInArea(position, new OBB(position, rotation, val), LayerMask.op_Implicit(1503731969), out var _, wantsInside: true, shouldParent: false, null, this))
			{
				return false;
			}
		}
		return true;
	}

	protected Item GetAmmoFromPlayerInventory(BasePlayer player)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Item item = player.inventory.FindItemByItemID(magazine.ammoType.itemid);
		if (item == null && !magazine.allowAmmoSwitching)
		{
			return null;
		}
		if (item == null)
		{
			Item item2 = player.inventory.FindAmmo(magazine.definition.ammoTypes);
			if (item2 == null)
			{
				return null;
			}
			item = player.inventory.FindItemByItemID(item2.info.itemid);
			if (item == null)
			{
				return null;
			}
		}
		return item;
	}

	public ItemDefinition GetDisplayedAmmoType(BasePlayer player)
	{
		Item ammoFromPlayerInventory = GetAmmoFromPlayerInventory(player);
		if (ammoFromPlayerInventory == null)
		{
			return magazine.ammoType;
		}
		return ammoFromPlayerInventory.info;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	protected virtual bool HasGround(Transform checkTransform)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		float num = 1f;
		Vector3 up = checkTransform.up;
		Vector3 val2 = -up;
		Vector3 val3 = up * 0.6f;
		if (Physics.SphereCast(checkTransform.position + val3, 0.5f, val2, ref val, num, 1503731969))
		{
			return val.normal.y > 0f;
		}
		return false;
	}

	private bool HasColliderBlockingRotation(Vector3 origin, Vector3 direction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.2f;
		RaycastHit val = default;
		return Physics.SphereCast(origin, 0.05f, direction, ref val, num, 1503731969);
	}

	protected virtual void MoveMountAnchor()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		float num = 2f;
		Vector3 val = mountAnchor.parent.TransformPoint(originalLocalMountPos) + Vector3.up * 0.8f;
		Vector3 down = Vector3.down;
		RaycastHit val2 = default;
		if (Physics.SphereCast(val, 0.05f, down, ref val2, num, 1503731969))
		{
			float y = val2.point.y;
			float y2 = mountAnchor.parent.TransformPoint(originalLocalMountPos).y;
			if (Mathf.Abs(y - y2) < 0.5f)
			{
				mountAnchor.position = val2.point;
			}
		}
	}

	protected bool CanSeeFirePoint(BasePlayer player, float radius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = player.eyes.center;
		Vector3 position = player.eyes.position;
		Vector3 position2 = muzzle.position;
		int layerMask = 2162689;
		if (GamePhysics.LineOfSightRadius(center, position, layerMask, radius, this))
		{
			return GamePhysics.LineOfSightRadius(position, position2, layerMask, radius, this);
		}
		return false;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (HasGroundOnBothSides())
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if (HasFlag(Flags.Reserved4))
		{
			StopReload();
		}
	}

	public bool ServerStartReload(BasePlayer player)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null || (Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)mounted || HasFlag(Flags.Reserved4) || UnableToStartReloadServer(player))
		{
			return false;
		}
		object obj = Interface.CallHook("OnMountedWeaponReload", this, player);
		if (obj != null)
		{
			if (!(obj is bool))
			{
				return false;
			}
			return (bool)obj;
		}
		reloadingPlayer = player;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved4, b: true);
		}
		ClientRPC(RpcTarget.Player("CLIENT_StartReloading", reloadingPlayer), reloadingPlayer.net.ID);
		InvokeRepeating(ReloadProgress, 0f, progressTickRate);
		Server_OnReloadStarted();
		return true;
	}

	protected virtual void ReloadProgress()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)reloadingPlayer == (Object)null || (Object)(object)reloadingPlayer != (Object)(object)GetMounted() || reloadingPlayer.IsDead() || reloadingPlayer.IsSleeping() || Vector3Ex.Distance2D(((Component)reloadingPlayer).transform.position, ((Component)this).transform.position) > 3f)
		{
			StopReload();
			return;
		}
		reloadProgress += progressTickRate / reloadTime;
		if (reloadProgress >= 1f)
		{
			reloadProgress = 1f;
			LoadAmmo(reloadingPlayer);
			if (syncAimDirOnReload)
			{
				Invoke(() =>
				{
					SendAimDirImmediate(force: true);
				}, 0.5f);
			}
			StopReload();
		}
		else
		{
			SendNetworkUpdateImmediate();
		}
	}

	public virtual void StopReload()
	{
		CancelInvoke(ReloadProgress);
		reloadingPlayer = null;
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved4, b: false);
	}

	protected virtual bool UnableToStartReloadServer(BasePlayer player)
	{
		return false;
	}

	[RPC_Server.FromMounted]
	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	private void SERVER_ReloadStart(RPCMessage msg)
	{
		if (!ServerStartReload(msg.player))
		{
			SendNetworkUpdate();
		}
	}

	protected virtual void Server_OnReloadStarted()
	{
	}

	[RPC_Server.CallsPerSecond(3uL)]
	[RPC_Server]
	[RPC_Server.FromMounted]
	public void SERVER_CancelReload(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)reloadingPlayer))
		{
			StopReload();
		}
	}

	protected virtual void LoadAmmo(BasePlayer player)
	{
		if ((Object)(object)player != (Object)(object)GetMounted() || IsLoaded())
		{
			return;
		}
		Item ammoFromPlayerInventory = GetAmmoFromPlayerInventory(player);
		if (ammoFromPlayerInventory != null)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved5, b: true);
			}
			magazine.ammoType = ammoFromPlayerInventory.info;
			magazine.contents = 1;
			ammoFromPlayerInventory.UseItem();
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	private void SERVER_SwitchAmmoTo(RPCMessage msg)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null || (Object)(object)player == (Object)null || (Object)(object)player != (Object)(object)mounted)
		{
			return;
		}
		int num = msg.read.Int32();
		if (num == magazine.ammoType.itemid)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(num);
		if ((Object)(object)itemDefinition == (Object)null)
		{
			return;
		}
		ItemModProjectile component = ((Component)itemDefinition).GetComponent<ItemModProjectile>();
		if (!Object.op_Implicit((Object)(object)component) || !component.IsAmmo(magazine.definition.ammoTypes))
		{
			return;
		}
		if (magazine.contents > 0)
		{
			mounted.GiveItem(ItemManager.CreateByItemID(magazine.ammoType.itemid, magazine.contents, 0uL, 0uL));
			magazine.contents = 0;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		magazine.ammoType = itemDefinition;
		SendNetworkUpdateImmediate();
		ItemManager.DoRemoves();
		mounted.inventory.ServerUpdate(0f);
	}

	protected virtual bool IsFireRPCInvalid(RPCMessage msg, BasePlayer player, out ItemDefinition ammoItem, out ItemModProjectile itemModProjectile)
	{
		if ((Object)(object)player == (Object)null)
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!VerifyClientRPC(player))
		{
			SendNetworkUpdate();
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!IsLoaded() || magazine.contents != 1)
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if ((Object)(object)player != (Object)(object)GetMounted())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (!CanFire())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		if (player.InSafeZone())
		{
			ammoItem = null;
			itemModProjectile = null;
			return true;
		}
		ammoItem = magazine.ammoType;
		if ((Object)(object)ammoItem == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "item_missing");
			itemModProjectile = null;
			return true;
		}
		itemModProjectile = ((Component)ammoItem).GetComponent<ItemModProjectile>();
		if ((Object)(object)itemModProjectile == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "mod_missing");
			return true;
		}
		return false;
	}

	protected virtual void SERVER_OnProjectileFired(Connection connection, BasePlayer player)
	{
		if (syncAimDirOnFire)
		{
			SendAimDirImmediate(force: true);
		}
		player.MarkHostileFor();
		SignalBroadcast(Signal.Attack, string.Empty, connection);
		magazine.contents = 0;
		reloadProgress = 0f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved5, b: false);
		}
		SingletonComponent<NpcNoiseManager>.Instance.OnWeaponShot(player, null);
	}

	protected bool VerifyClientRPC(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			Debug.LogWarning((object)"Received RPC from null player");
			return false;
		}
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Owner not found (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "owner_missing");
			return false;
		}
		if ((Object)(object)mounted != (Object)(object)player)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player mismatch (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_mismatch");
			return false;
		}
		if (player.IsDead())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player dead (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_dead");
			return false;
		}
		if (player.IsWounded())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player down (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_down");
			return false;
		}
		if (player.IsSleeping())
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player sleeping (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "player_sleeping");
			return false;
		}
		if (player.desyncTimeRaw > ConVar.AntiHack.maxdesync)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Player stalled (" + ShortPrefabName + " with " + player.desyncTimeRaw + "s)");
			player.stats.combat.LogInvalid(player, null, "player_stalled");
			return false;
		}
		if ((Object)(object)magazine.ammoType == (Object)null)
		{
			AntiHack.Log(player, AntiHackType.AttackHack, "Item not found (" + ShortPrefabName + ")");
			player.stats.combat.LogInvalid(player, null, "item_missing");
			return false;
		}
		return true;
	}

	protected override bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return signal == Signal.Attack;
	}

	public void AdminReload(int ammo)
	{
		reloadProgress = 1f;
		StopReload();
		ItemDefinition itemDefinition = null;
		ammo = Mathf.Clamp(ammo, 0, ammoPrefabs.Length - 1);
		itemDefinition = ammoPrefabs[ammo].item;
		SetFlagLocal(Flags.Reserved5, b: true);
		magazine.ammoType = itemDefinition;
		magazine.contents = 1;
		SendNetworkUpdateImmediate();
	}

	[ServerVar(Help = "Forces the ballistas, cannons and mortars within 10m of the calling admin player to reload immediately; admin-only")]
	public static void reload(ConsoleSystem.Arg arg)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
		}
		else
		{
			if (!basePlayer.IsAdmin)
			{
				return;
			}
			int ammo = arg.GetInt(0);
			BaseMountedWeapon[] array = Util.FindAll<BaseMountedWeapon>();
			int num = 0;
			BaseMountedWeapon[] array2 = array;
			foreach (BaseMountedWeapon baseMountedWeapon in array2)
			{
				if (baseMountedWeapon.isServer && Vector3.Distance(((Component)baseMountedWeapon).transform.position, ((Component)basePlayer).transform.position) <= 10f)
				{
					baseMountedWeapon.AdminReload(ammo);
					num++;
				}
			}
			arg.ReplyWith($"Reloaded {num} guns.");
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.ballistaGun != null)
		{
			info.msg.ballistaGun.magazine = magazine.Save();
			info.msg.ballistaGun.reloadProgress = reloadProgress;
		}
	}

	public override void Load(LoadInfo info)
	{
		if (info.msg.ballistaGun != null)
		{
			reloadProgress = info.msg.ballistaGun.reloadProgress;
		}
		if (info.msg.ballistaGun != null && info.msg.ballistaGun.magazine != null)
		{
			magazine.Load(info.msg.ballistaGun.magazine);
		}
		base.Load(info);
	}
}
