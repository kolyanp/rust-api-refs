using System;
using Oxide.Core;
using UnityEngine;

public class ScientistNPC : HumanNPC, IAIMounted
{
	public enum RadioChatterType
	{
		NONE,
		Idle,
		Alert
	}

	public GameObjectRef[] RadioChatterEffects;

	public GameObjectRef[] DeathEffects;

	public string deathStatName;

	public static readonly Phrase ScientistName;

	public Vector2 IdleChatterRepeatRange;

	public RadioChatterType radioChatterType;

	public float lastAlertedTime;

	private Action _playRadioChatter;

	public override string displayName => ScientistName.translated;

	public void SetChatterType(RadioChatterType newType)
	{
		if (newType != radioChatterType)
		{
			if (newType == RadioChatterType.Idle)
			{
				QueueRadioChatter();
			}
			else if (_playRadioChatter != null)
			{
				CancelInvoke(_playRadioChatter);
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetChatterType(RadioChatterType.Idle);
		InvokeRandomized(IdleCheck, 0f, 20f, 1f);
	}

	public override void OnKilled()
	{
		EnsureDismounted();
		base.OnKilled();
	}

	public void IdleCheck()
	{
		if (Time.time > lastAlertedTime + 20f)
		{
			SetChatterType(RadioChatterType.Idle);
		}
	}

	public void QueueRadioChatter()
	{
		if (IsAlive() && !base.IsDestroyed)
		{
			if (_playRadioChatter == null)
			{
				_playRadioChatter = PlayRadioChatter;
			}
			Invoke(_playRadioChatter, Random.Range(IdleChatterRepeatRange.x, IdleChatterRepeatRange.y));
		}
	}

	public override bool ShotTest(float targetDist)
	{
		bool result = base.ShotTest(targetDist);
		if (Time.time - lastGunShotTime < 5f)
		{
			Alert();
		}
		return result;
	}

	public void Alert()
	{
		if (Interface.CallHook("OnNpcAlert", this) == null)
		{
			lastAlertedTime = Time.time;
			SetChatterType(RadioChatterType.Alert);
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		Alert();
	}

	public override void OnDied(HitInfo info)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.OnDied(info);
		SetChatterType(RadioChatterType.NONE);
		if (DeathEffects.Length != 0)
		{
			Effect.server.Run(DeathEffects[Random.Range(0, DeathEffects.Length)].resourcePath, ServerPosition, Vector3.up);
		}
		if (info != null && (Object)(object)info.InitiatorPlayer != (Object)null && !info.InitiatorPlayer.IsNpc)
		{
			info.InitiatorPlayer.stats.Add(deathStatName, 1, (Stats)5);
		}
	}

	public void PlayRadioChatter()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (RadioChatterEffects.Length != 0)
		{
			if ((base.IsDestroyed || (Object)(object)((Component)this).transform == (Object)null) && _playRadioChatter != null)
			{
				CancelInvoke(_playRadioChatter);
			}
			else if (Interface.CallHook("OnNpcRadioChatter", this) == null)
			{
				Effect.server.Run(RadioChatterEffects[Random.Range(0, RadioChatterEffects.Length)].resourcePath, this, StringPool.Get("head"), Vector3.zero, Vector3.zero);
				QueueRadioChatter();
			}
		}
	}

	public override void EquipWeapon(bool skipDeployDelay = false)
	{
		base.EquipWeapon(skipDeployDelay);
		HeldEntity heldEntity = GetHeldEntity();
		if (!((Object)(object)heldEntity != (Object)null))
		{
			return;
		}
		Item item = heldEntity.GetItem();
		if (item == null || item.contents == null || Interface.CallHook("OnNpcEquipWeapon", this, item) != null)
		{
			return;
		}
		if (Random.Range(0, 3) == 0)
		{
			Item item2 = ItemManager.CreateByName("weapon.mod.flashlight", 1, 0uL);
			if (!item2.MoveToContainer(item.contents))
			{
				item2.Remove();
				return;
			}
			lightsOn = false;
			InvokeRandomized(base.LightCheck, 0f, 30f, 5f);
			LightCheck();
		}
		else
		{
			Item item3 = ItemManager.CreateByName("weapon.mod.lasersight", 1, 0uL);
			if (!item3.MoveToContainer(item.contents))
			{
				item3.Remove();
			}
			LightToggle();
			lightsOn = true;
		}
	}

	public bool IsMounted()
	{
		return base.isMounted;
	}

	public ScientistNPC()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		deathStatName = "kill_scientist";
		IdleChatterRepeatRange = new Vector2(10f, 15f);
		lastAlertedTime = -100f;
		base._002Ector();
	}

	static ScientistNPC()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		ScientistName = new Phrase("npc_scientist", "Scientist");
	}
}
