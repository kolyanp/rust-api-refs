using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.BurstCloth;
using Rust.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using VLB;

public class PrefabPreProcess : IPrefabProcessor
{
	public static Type[] clientsideOnlyTypes = new Type[41]
	{
		typeof(IClientComponent),
		typeof(SkeletonSkinLod),
		typeof(SkeletonSkin),
		typeof(ImageEffectLayer),
		typeof(NGSS_Directional),
		typeof(VolumetricDustParticles),
		typeof(VolumetricLightBeam),
		typeof(Cloth),
		typeof(TextMeshPro),
		typeof(MeshFilter),
		typeof(Renderer),
		typeof(AudioLowPassFilter),
		typeof(AudioSource),
		typeof(AudioListener),
		typeof(ParticleSystemRenderer),
		typeof(ParticleSystem),
		typeof(ParticleEmitFromParentObject),
		typeof(ImpostorShadows),
		typeof(Light),
		typeof(LODGroup),
		typeof(Animator),
		typeof(AnimationEvents),
		typeof(PlayerVoiceSpeaker),
		typeof(VoiceProcessor),
		typeof(PlayerVoiceRecorder),
		typeof(ParticleScaler),
		typeof(PostEffectsBase),
		typeof(TOD_ImageEffect),
		typeof(TOD_Scattering),
		typeof(TOD_Rays),
		typeof(Tree),
		typeof(Projector),
		typeof(HttpImage),
		typeof(EventTrigger),
		typeof(InputSystemUIInputModule),
		typeof(UIBehaviour),
		typeof(Canvas),
		typeof(CanvasRenderer),
		typeof(CanvasGroup),
		typeof(GraphicRaycaster),
		typeof(BurstClothConstraint)
	};

	public static Type[] serversideOnlyTypes = new Type[5]
	{
		typeof(IServerComponent),
		typeof(NavMeshLink),
		typeof(NavMeshSurface),
		typeof(NavMeshObstacle),
		typeof(NavMeshModifierVolume)
	};

	public bool isClientside;

	public bool isServerside;

	public bool isBundling;

	public Dictionary<string, GameObject> prefabList = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

	private GameObject parentObject;

	public List<Component> destroyList = new List<Component>();

	public List<GameObject> cleanupList = new List<GameObject>();

	private List<GameObject> deleteList = new List<GameObject>();

	public PrefabPreProcess(bool clientside, bool serverside, bool bundling = false)
	{
		isClientside = clientside;
		isServerside = serverside;
		isBundling = bundling;
	}

	public GameObject Find(string strPrefab)
	{
		if (prefabList.TryGetValue(strPrefab, out var value))
		{
			if ((Object)(object)value == (Object)null)
			{
				prefabList.Remove(strPrefab);
				return null;
			}
			return value;
		}
		return null;
	}

	public bool NeedsProcessing(GameObject go, PreProcessPrefabOptions options)
	{
		if (go.CompareTag("NoPreProcessing"))
		{
			return false;
		}
		if (options.PreProcess && HasComponents<IPrefabPreProcess>(go.transform))
		{
			return true;
		}
		if (options.PostProcess && HasComponents<IPrefabPostProcess>(go.transform))
		{
			return true;
		}
		if (options.StripComponents && HasComponents<IEditorComponent>(go.transform))
		{
			return true;
		}
		if (!isClientside)
		{
			if (options.StripComponents && clientsideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (options.StripComponents && HasComponents<IClientComponentEx>(go.transform))
			{
				return true;
			}
		}
		if (!isServerside)
		{
			if (options.StripComponents && serversideOnlyTypes.Any((Type type) => HasComponents(go.transform, type)))
			{
				return true;
			}
			if (options.StripComponents && HasComponents<IServerComponentEx>(go.transform))
			{
				return true;
			}
		}
		return false;
	}

	public void ProcessObject(string name, GameObject go, PreProcessPrefabOptions options)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Expected O, but got Unknown
		StringPool.Get(name);
		StripEmptyChildren stripEmptyChildren = default(StripEmptyChildren);
		bool flag = go.TryGetComponent<StripEmptyChildren>(ref stripEmptyChildren) && Render.IsInstancingEnabled;
		if (options.StripComponents)
		{
			if (!isClientside)
			{
				Type[] array = clientsideOnlyTypes;
				foreach (Type t in array)
				{
					DestroyComponents(t, go, isClientside, isServerside);
				}
				foreach (IClientComponentEx item in FindIComponents<IClientComponentEx>(go.transform))
				{
					item.PreClientComponentCull((IPrefabProcessor)(object)this);
				}
			}
			if (!isServerside)
			{
				Type[] array = serversideOnlyTypes;
				foreach (Type t2 in array)
				{
					DestroyComponents(t2, go, isClientside, isServerside);
				}
				foreach (IServerComponentEx item2 in FindIComponents<IServerComponentEx>(go.transform))
				{
					item2.PreServerComponentCull((IPrefabProcessor)(object)this);
				}
			}
			DestroyComponents(typeof(IEditorComponent), go, isClientside, isServerside);
		}
		if (options.ResetLocalTransform)
		{
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
		}
		List<Transform> list = this.FindComponents<Transform>(go.transform);
		list.Reverse();
		if (options.UpdateMeshCooking)
		{
			MeshColliderCookingOptions val = (MeshColliderCookingOptions)14;
			MeshColliderCookingOptions cookingOptions = (MeshColliderCookingOptions)30;
			MeshColliderCookingOptions val2 = (MeshColliderCookingOptions)(-1);
			foreach (MeshCollider item3 in this.FindComponents<MeshCollider>(go.transform))
			{
				if (item3.cookingOptions == val || item3.cookingOptions == val2)
				{
					item3.cookingOptions = cookingOptions;
				}
			}
		}
		if (options.DisableInstancingOnParticleSystems)
		{
			foreach (ParticleSystemRenderer item4 in this.FindComponents<ParticleSystemRenderer>(go.transform))
			{
				if (!((Object)(object)item4 == (Object)null))
				{
					item4.enableGPUInstancing = false;
				}
			}
		}
		if (options.PreProcess)
		{
			foreach (IPrefabPreProcess item5 in FindIComponents<IPrefabPreProcess>(go.transform))
			{
				if (!isBundling || item5.CanRunDuringBundling)
				{
					item5.PreProcess((IPrefabProcessor)(object)this, go, name, isServerside, isClientside, isBundling);
					MarkPropertiesDirty((Object)item5);
				}
			}
			if (isClientside && !isServerside)
			{
				RemoveChildEntities(go);
			}
		}
		if (options.StripEmptyChildren)
		{
			BaseEntity baseEntity = default(BaseEntity);
			foreach (Transform item6 in list)
			{
				if (!Object.op_Implicit((Object)(object)item6) || !Object.op_Implicit((Object)(object)((Component)item6).gameObject))
				{
					continue;
				}
				if (isServerside && ((Component)item6).gameObject.CompareTag("Server Cull"))
				{
					RemoveComponents(((Component)item6).gameObject);
					NominateForDeletion(((Component)item6).gameObject);
				}
				if (isClientside)
				{
					bool num = ((Component)item6).gameObject.CompareTag("Client Cull");
					bool flag2 = (Object)(object)item6 != (Object)(object)go.transform && ((Component)item6).gameObject.TryGetComponent<BaseEntity>(ref baseEntity);
					if (num | flag2)
					{
						RemoveComponents(((Component)item6).gameObject);
						NominateForDeletion(((Component)item6).gameObject);
					}
					else if (flag)
					{
						NominateForDeletion(((Component)item6).gameObject);
					}
				}
			}
		}
		RunCleanupQueue(go);
		if (!options.PostProcess)
		{
			return;
		}
		foreach (IPrefabPostProcess item7 in FindIComponents<IPrefabPostProcess>(go.transform))
		{
			item7.PostProcess((IPrefabProcessor)(object)this, go, name, isServerside, isClientside, isBundling);
		}
	}

	public void Process(string name, GameObject go, bool forceInPlace = false)
	{
		PreProcessPrefabOptions assetSceneRuntime = PreProcessPrefabOptions.AssetSceneRuntime;
		GameObject val = go;
		if (Application.isPlaying && !val.CompareTag("NoPreProcessing"))
		{
			bool num = PrefabNeedsCopy(val);
			bool flag = NeedsProcessing(val, assetSceneRuntime);
			bool flag2 = num & flag;
			if (!forceInPlace & flag2)
			{
				Transform val2 = (TryGetHierarchyGroup(out var obj) ? obj.transform : null);
				go = Instantiate.GameObject(val, val2);
				((Object)go).name = ((Object)val).name;
			}
			if (flag)
			{
				ProcessObject(name, go, assetSceneRuntime);
			}
			if (!forceInPlace)
			{
				AddPrefab(name, go);
			}
		}
		static bool PrefabNeedsCopy(GameObject val3)
		{
			Wearable wearable = default(Wearable);
			if (val3.TryGetComponent<Wearable>(ref wearable) && !wearable.disableRigStripping)
			{
				return true;
			}
			return false;
		}
	}

	public void Invalidate(string name)
	{
		if (prefabList.TryGetValue(name, out var value))
		{
			prefabList.Remove(name);
			if ((Object)(object)value != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)value, true);
			}
		}
	}

	public void InvalidateAll()
	{
		foreach (var (_, val2) in prefabList)
		{
			if ((Object)(object)val2 != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)val2, true);
			}
		}
		prefabList.Clear();
	}

	private void RemoveChildEntities(GameObject go)
	{
		if ((Object)(object)go.GetComponent<BaseNetworkable>() == (Object)null)
		{
			return;
		}
		BaseNetworkable[] componentsInChildren = go.GetComponentsInChildren<BaseNetworkable>();
		foreach (BaseNetworkable baseNetworkable in componentsInChildren)
		{
			if (!((Object)(object)baseNetworkable == (Object)null) && !((Object)(object)((Component)baseNetworkable).gameObject == (Object)(object)go))
			{
				Object.DestroyImmediate((Object)(object)((Component)baseNetworkable).gameObject, true);
			}
		}
	}

	private bool TryGetHierarchyGroup(out GameObject obj)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		if (isBundling || !Application.isPlaying)
		{
			obj = null;
			return false;
		}
		if ((Object)(object)parentObject == (Object)null)
		{
			string text = ((isClientside && isServerside) ? "PrefabPreProcess - Generic" : (isServerside ? "PrefabPreProcess - Server" : "PrefabPreProcess - Client"));
			parentObject = new GameObject(text);
			parentObject.SetActive(false);
			Object.DontDestroyOnLoad((Object)(object)parentObject);
		}
		obj = parentObject;
		return true;
	}

	public void AddPrefab(string name, GameObject go)
	{
		go.SetActive(false);
		prefabList.Add(name, go);
	}

	private void DestroyComponents(Type t, GameObject go, bool client, bool server)
	{
		List<Component> list = new List<Component>();
		FindComponents(go.transform, list, t);
		list.Reverse();
		RealmedRemove realmedRemove = default(RealmedRemove);
		foreach (Component item in list)
		{
			if (!item.TryGetComponent<RealmedRemove>(ref realmedRemove) || realmedRemove.ShouldDelete(item, client, server))
			{
				if (!item.gameObject.CompareTag("persist"))
				{
					NominateForDeletion(item.gameObject);
				}
				Object.DestroyImmediate((Object)(object)item, true);
			}
		}
	}

	private bool ShouldExclude(Transform transform)
	{
		BaseEntity baseEntity = default(BaseEntity);
		if (((Component)transform).TryGetComponent<BaseEntity>(ref baseEntity))
		{
			return true;
		}
		return false;
	}

	private void GatherExcludedTransf(Transform root, HashSet<Transform> excludeSet)
	{
		List<BaseEntity> list = new List<BaseEntity>();
		((Component)root).GetComponentsInChildren<BaseEntity>(true, list);
		int i = 0;
		if (list.Count > 0 && (Object)(object)((Component)list[0]).transform == (Object)(object)root)
		{
			i = 1;
		}
		for (; i < list.Count; i++)
		{
			ExcludeChildHierarchy(((Component)list[i]).transform, excludeSet);
		}
		static void ExcludeChildHierarchy(Transform transf, HashSet<Transform> set)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			set.Add(transf);
			foreach (Transform item in transf)
			{
				ExcludeChildHierarchy(item, set);
			}
		}
	}

	private bool HasComponents<T>(Transform transform)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		T val = default(T);
		if (((Component)transform).TryGetComponent<T>(ref val))
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			Transform transform2 = item;
			if (!ShouldExclude(transform2) && HasComponents<T>(transform2))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasComponents(Transform transform, Type t)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Component val = default(Component);
		if (((Component)transform).TryGetComponent(t, ref val))
		{
			return true;
		}
		foreach (Transform item in transform)
		{
			Transform transform2 = item;
			if (!ShouldExclude(transform2) && HasComponents(transform2, t))
			{
				return true;
			}
		}
		return false;
	}

	public List<T> FindComponents<T>(Transform transform) where T : Component
	{
		List<T> list = new List<T>();
		FindComponents(transform, list);
		return list;
	}

	public void FindComponents<T>(Transform transform, List<T> list) where T : Component
	{
		List<T> list2 = new List<T>();
		((Component)transform).GetComponentsInChildren<T>(true, list2);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		GatherExcludedTransf(transform, hashSet);
		try
		{
			foreach (T item in list2)
			{
				Transform transform2 = ((Component)item).transform;
				if (!hashSet.Contains(transform2))
				{
					list.Add(item);
				}
			}
		}
		catch
		{
			throw;
		}
	}

	public List<T> FindIComponents<T>(Transform transform)
	{
		List<T> list = new List<T>();
		FindIComponents(transform, list);
		return list;
	}

	public void FindIComponents<T>(Transform transform, List<T> list)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		list.AddRange(((Component)transform).GetComponents<T>());
		foreach (Transform item in transform)
		{
			Transform transform2 = item;
			if (!ShouldExclude(transform2))
			{
				FindIComponents(transform2, list);
			}
		}
	}

	public List<Component> FindComponents(Transform transform, Type t)
	{
		List<Component> list = new List<Component>();
		FindComponents(transform, list, t);
		return list;
	}

	public void FindComponents(Transform transform, List<Component> list, Type t)
	{
		Component[] componentsInChildren = ((Component)transform).GetComponentsInChildren(t, true);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		GatherExcludedTransf(transform, hashSet);
		Component[] array = componentsInChildren;
		foreach (Component val in array)
		{
			Transform transform2 = val.transform;
			if (!hashSet.Contains(transform2))
			{
				list.Add(val);
			}
		}
	}

	public void RemoveComponent(Component c)
	{
		if (!((Object)(object)c == (Object)null))
		{
			destroyList.Add(c);
		}
	}

	public void RemoveComponents(GameObject gameObj)
	{
		Component[] components = gameObj.GetComponents<Component>();
		foreach (Component val in components)
		{
			if (!(val is Transform))
			{
				destroyList.Add(val);
			}
		}
	}

	public void NominateForDeletion(GameObject gameObj)
	{
		cleanupList.Add(gameObj);
	}

	public void DeleteGameObject(GameObject obj)
	{
		if (!((Object)(object)obj == (Object)null))
		{
			deleteList.Add(obj);
		}
	}

	public void MarkPropertiesDirty(Object obj)
	{
	}

	public void RunCleanupQueue(GameObject rootGo)
	{
		foreach (Component destroy in destroyList)
		{
			Object.DestroyImmediate((Object)(object)destroy, true);
		}
		destroyList.Clear();
		foreach (GameObject delete in deleteList)
		{
			Object.DestroyImmediate((Object)(object)delete, true);
		}
		deleteList.Clear();
		foreach (GameObject cleanup in cleanupList)
		{
			if (cleanup != rootGo)
			{
				DoCleanup(cleanup);
			}
		}
		cleanupList.Clear();
	}

	public void DoCleanup(GameObject go)
	{
		if (!((Object)(object)go == (Object)null) && go.GetComponentsInChildren<Component>(true).Length <= 1)
		{
			Object.DestroyImmediate((Object)(object)go, true);
		}
	}
}
