using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerEyes : EntityComponent<BasePlayer>
{
	public static readonly Vector3 EyeOffset;

	public static readonly Vector3 DuckOffset;

	public static readonly Vector3 CrawlOffset;

	public static readonly Vector3 ParachuteOffset;

	public Vector3 thirdPersonSleepingOffset;

	public LazyAimProperties defaultLazyAim;

	private EncryptedValue<Vector3> viewOffset;

	[CompilerGenerated]
	private Quaternion _003CbodyRotation_003Ek__BackingField;

	public Vector3 worldMountedPosition
	{
		get
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)base.baseEntity) && base.baseEntity.isMounted)
			{
				Vector3 val = base.baseEntity.GetMounted().EyePositionForPlayer(base.baseEntity, GetLookRotation());
				if (val != Vector3.zero)
				{
					return val;
				}
			}
			return worldStandingPosition;
		}
	}

	public Vector3 worldStandingPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position + EyeOffset;
		}
	}

	public Vector3 worldCrouchedPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return worldStandingPosition + DuckOffset;
		}
	}

	public Vector3 worldCrawlingPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return worldStandingPosition + CrawlOffset;
		}
	}

	public Vector3 position
	{
		get
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)base.baseEntity) && base.baseEntity.isMounted)
			{
				Vector3 val = base.baseEntity.GetMounted().EyePositionForPlayer(base.baseEntity, GetLookRotation());
				if (val != Vector3.zero)
				{
					return val;
				}
				return GetMountedPos(((Component)this).transform.position, ((Component)this).transform.rotation);
			}
			return GetUnmountedPos(((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	private Vector3 BodyLeanOffset
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.zero;
		}
	}

	public Vector3 center
	{
		get
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)base.baseEntity) && base.baseEntity.isMounted)
			{
				Vector3 val = base.baseEntity.GetMounted().EyeCenterForPlayer(base.baseEntity, GetLookRotation());
				if (val != Vector3.zero)
				{
					return val;
				}
			}
			return ((Component)this).transform.position + ((Component)this).transform.up * (EyeOffset.y + DuckOffset.y);
		}
	}

	public Vector3 offset
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.up * (EyeOffset.y + viewOffset.Get().y);
		}
	}

	public Quaternion rotation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return parentRotation * bodyRotation;
		}
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			bodyRotation = Quaternion.Inverse(parentRotation) * value;
		}
	}

	public Quaternion bodyRotation
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CbodyRotation_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CbodyRotation_003Ek__BackingField = value;
		}
	}

	public Quaternion parentRotation
	{
		get
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if (!base.baseEntity.isMounted)
			{
				Transform parent = ((Component)this).transform.parent;
				if (parent != null)
				{
					Quaternion val = parent.rotation;
					return Quaternion.Euler(0f, ((Quaternion)(ref val)).eulerAngles.y, 0f);
				}
			}
			return Quaternion.identity;
		}
	}

	public Vector3 GetPos(Vector3 pos, Quaternion rot, bool isMounted, BaseMountable mounted)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.baseEntity) & isMounted)
		{
			Vector3 val = mounted.EyePositionForPlayer(base.baseEntity, rotation);
			if (val != Vector3.zero)
			{
				return val;
			}
			return GetMountedPos(pos, rot);
		}
		return GetUnmountedPos(pos, rot);
	}

	public Vector3 GetMountedPos(Vector3 pos, Quaternion rot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		return pos + rot * Vector3.up * (EyeOffset.y + viewOffset.Get().y) + BodyLeanOffset;
	}

	public Vector3 GetUnmountedPos(Vector3 pos, Quaternion rot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return pos + rot * (EyeOffset + (Vector3)viewOffset) + BodyLeanOffset;
	}

	public void NetworkUpdate(Quaternion rot)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (base.baseEntity.IsCrawling())
		{
			viewOffset = CrawlOffset;
		}
		else
		{
			viewOffset = Vector3.zero;
			viewOffset = Vector3.Lerp((Vector3)viewOffset, DuckOffset, base.baseEntity.modelState.ducking);
		}
		bodyRotation = rot;
	}

	public Vector3 PositionWithOverride(Vector3 position)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.baseEntity) && base.baseEntity.isMounted)
		{
			Vector3 val = base.baseEntity.GetMounted().EyePositionForPlayer(base.baseEntity, GetLookRotation());
			if (val != Vector3.zero)
			{
				return val;
			}
			return position + ((Component)this).transform.up * (EyeOffset.y + viewOffset.Get().y) + BodyLeanOffset;
		}
		return position + ((Component)this).transform.rotation * (EyeOffset + (Vector3)viewOffset) + BodyLeanOffset;
	}

	public Vector3 MovementForward()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = rotation;
		return Quaternion.Euler(new Vector3(0f, ((Quaternion)(ref val)).eulerAngles.y, 0f)) * Vector3.forward;
	}

	public Vector3 MovementRight()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = rotation;
		return Quaternion.Euler(new Vector3(0f, ((Quaternion)(ref val)).eulerAngles.y, 0f)) * Vector3.right;
	}

	public Ray BodyRay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Ray(position, BodyForward());
	}

	public Vector3 BodyForward()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return rotation * Vector3.forward;
	}

	public Vector3 BodyRight()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return rotation * Vector3.right;
	}

	public Vector3 BodyUp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return rotation * Vector3.up;
	}

	public Ray HeadRay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Ray(position, HeadForward());
	}

	public Vector3 HeadForward()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetLookRotation() * Vector3.forward;
	}

	public Vector3 HeadRight()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetLookRotation() * Vector3.right;
	}

	public Vector3 HeadUp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetLookRotation() * Vector3.up;
	}

	public Quaternion GetLookRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return rotation;
	}

	public Quaternion GetAimRotation()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return rotation;
	}

	public PlayerEyes()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		thirdPersonSleepingOffset = new Vector3(0.43f, 1.25f, 0.7f);
		viewOffset = Vector3.zero;
		base._002Ector();
	}

	static PlayerEyes()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		EyeOffset = new Vector3(0f, 1.5f, 0f);
		DuckOffset = new Vector3(0f, -0.6f, 0f);
		CrawlOffset = new Vector3(0f, -1.15f, 0.175f);
		ParachuteOffset = new Vector3(0f, -1.45f, 0.3f);
	}
}
