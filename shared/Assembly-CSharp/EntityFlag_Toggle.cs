using System;
using UnityEngine;
using UnityEngine.Events;

public class EntityFlag_Toggle : EntityComponent<BaseEntity>, IOnPostNetworkUpdate, IOnSendNetworkUpdate, IPrefabPreProcess
{
	private enum FlagCheck
	{
		All,
		Any
	}

	private enum ListenerKind : byte
	{
		GameObjectSetActive,
		BehaviourEnabled,
		ColliderEnabled,
		RendererEnabled,
		RendererSharedMaterial,
		LerpBetweenPointsBoolSetAtOffset,
		ParticleSystemContainerPlay,
		ParticleSystemContainerStop,
		SoundPlayerPlay,
		SoundPlayerStop,
		SoundPlayerPlayOneshot,
		SoundPlayerFadeInAndPlay,
		SoundPlayerFadeOutAndRecycle,
		LODComponentSetVisible,
		EmissionToggleSetEmissionEnabled,
		MaterialSwapSetOverrideEnabled,
		ParticleSystemPlay,
		ParticleSystemStop,
		AnimatorSetTrigger,
		AnimatorResetTrigger,
		EmissionToggleSetEmission
	}

	[Serializable]
	private struct BakedListener
	{
		public ListenerKind kind;

		public Object target;

		public bool boolArgument;

		public float floatArgument;

		public string stringArgument;

		public Object objectArgument;
	}

	public bool runClientside = true;

	[Tooltip("Server-side only works if the EntityFlag_Toggle is on the same GameObject as the Entity")]
	public bool runServerside = true;

	public BaseEntity.Flags flag;

	[Tooltip("If multiple flags are defined in 'flag', should they all be set, or any?")]
	[SerializeField]
	private FlagCheck flagCheck;

	[Tooltip("Specify any flags that must NOT be on for this toggle to be on")]
	[SerializeField]
	private BaseEntity.Flags notFlag;

	[SerializeField]
	private UnityEvent onFlagEnabled = new UnityEvent();

	[SerializeField]
	private UnityEvent onFlagDisabled = new UnityEvent();

	[HideInInspector]
	[SerializeField]
	private BakedListener[] onFlagEnabledListeners;

	[SerializeField]
	[HideInInspector]
	private BakedListener[] onFlagDisabledListeners;

	internal bool hasRunOnce;

	internal bool lastToggleOn;

	bool IPrefabPreProcess.CanPreProcessDuringBundling => true;

	protected void OnDisable()
	{
		hasRunOnce = false;
		lastToggleOn = false;
	}

	public void DoUpdate(BaseEntity entity)
	{
		bool flag = ((flagCheck == FlagCheck.All) ? entity.HasFlag(this.flag) : entity.HasAnyFlag(this.flag));
		if (entity.HasAnyFlag(notFlag))
		{
			flag = false;
		}
		if (!hasRunOnce || flag != lastToggleOn)
		{
			bool isFirstRun = !hasRunOnce;
			hasRunOnce = true;
			lastToggleOn = flag;
			if (flag)
			{
				RunListeners(onFlagEnabled, onFlagEnabledListeners, isFirstRun);
			}
			else
			{
				RunListeners(onFlagDisabled, onFlagDisabledListeners, isFirstRun);
			}
			OnStateToggled(flag);
		}
	}

	private static void RunListeners(UnityEvent unityEvent, BakedListener[] listeners, bool isFirstRun)
	{
		if (listeners == null || listeners.Length == 0)
		{
			unityEvent.Invoke();
			return;
		}
		for (int i = 0; i < listeners.Length; i++)
		{
			BakedListener listener = listeners[i];
			if (listener.target != (Object)null)
			{
				RunListener(in listener, isFirstRun);
			}
		}
	}

	private static void RunListener(in BakedListener listener, bool isFirstRun)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected Obj, but got Unknown
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		switch (listener.kind)
		{
		case ListenerKind.GameObjectSetActive:
			((GameObject)listener.target).SetActive(listener.boolArgument);
			break;
		case ListenerKind.BehaviourEnabled:
			((Behaviour)listener.target).enabled = listener.boolArgument;
			break;
		case ListenerKind.ColliderEnabled:
			((Collider)listener.target).enabled = listener.boolArgument;
			break;
		case ListenerKind.RendererEnabled:
			((Renderer)listener.target).enabled = listener.boolArgument;
			break;
		case ListenerKind.RendererSharedMaterial:
			((Renderer)listener.target).sharedMaterial = (Material)listener.objectArgument;
			break;
		case ListenerKind.LerpBetweenPointsBoolSetAtOffset:
			((LerpBetweenPointsBool)(object)listener.target).SetAtOffset(listener.boolArgument);
			break;
		case ListenerKind.ParticleSystemContainerPlay:
			((ParticleSystemContainer)(object)listener.target).Play();
			break;
		case ListenerKind.ParticleSystemContainerStop:
			((ParticleSystemContainer)(object)listener.target).Stop();
			break;
		case ListenerKind.ParticleSystemPlay:
			((ParticleSystem)listener.target).Play();
			break;
		case ListenerKind.ParticleSystemStop:
			((ParticleSystem)listener.target).Stop();
			break;
		case ListenerKind.AnimatorSetTrigger:
			((Animator)listener.target).SetTrigger(listener.stringArgument);
			break;
		case ListenerKind.AnimatorResetTrigger:
			((Animator)listener.target).ResetTrigger(listener.stringArgument);
			break;
		case ListenerKind.SoundPlayerPlay:
		case ListenerKind.SoundPlayerStop:
		case ListenerKind.SoundPlayerPlayOneshot:
		case ListenerKind.SoundPlayerFadeInAndPlay:
		case ListenerKind.SoundPlayerFadeOutAndRecycle:
		case ListenerKind.LODComponentSetVisible:
		case ListenerKind.EmissionToggleSetEmissionEnabled:
		case ListenerKind.MaterialSwapSetOverrideEnabled:
			break;
		}
	}

	protected virtual void OnStateToggled(bool state)
	{
	}

	public void OnPostNetworkUpdate(BaseEntity entity)
	{
		if (!((Object)(object)baseEntity != (Object)(object)entity) && runClientside)
		{
			DoUpdate(entity);
		}
	}

	public void OnSendNetworkUpdate(BaseEntity entity)
	{
		if (runServerside)
		{
			DoUpdate(entity);
		}
	}

	public void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!((Object)(object)this == (Object)null) && (!clientside || !runClientside) && (!serverside || !runServerside))
		{
			process.RemoveComponent((Component)(object)this);
		}
	}

	public EntityFlag_Toggle()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected Obj, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
	}
}
