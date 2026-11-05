using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.BurstCloth;
using Facepunch.Extend;
using Rust;
using Rust.RenderPipeline.Runtime;
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
	public static Type[] clientsideOnlyTypes = new Type[43]
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
		typeof(TMP_SubMesh),
		typeof(MeshFilter),
		typeof(Renderer),
		typeof(AudioLowPassFilter),
		typeof(AudioSource),
		typeof(AudioListener),
		typeof(ParticleSystemRenderer),
		typeof(ParticleSystem),
		typeof(ParticleEmitFromParentObject),
		typeof(ImpostorShadows),
		typeof(RustAdditionalLightData),
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

	private Dictionary<string, GameObject> foundExactCase = new Dictionary<string, GameObject>();

	private GameObject parentObject;

	public List<Component> destroyList = new List<Component>();

	public List<GameObject> cleanupList = new List<GameObject>();

	private List<(int Depth, GameObject GameObject)> cleanupOrder = new List<(int, GameObject)>();

	private List<GameObject> deleteList = new List<GameObject>();

	public static bool IsProcessingBundledPrefab { get; private set; }

	public PrefabPreProcess(bool clientside, bool serverside, bool bundling = false)
	{
		isClientside = clientside;
		isServerside = serverside;
		isBundling = bundling;
	}

	public GameObject Find(string strPrefab)
	{
		if (foundExactCase.TryGetValue(strPrefab, out var value) && (Object)(object)value != (Object)null)
		{
			return value;
		}
		if (prefabList.TryGetValue(strPrefab, out value))
		{
			if ((Object)(object)value == (Object)null)
			{
				prefabList.Remove(strPrefab);
				foundExactCase.Clear();
				return null;
			}
			foundExactCase[strPrefab] = value;
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
		if (options.PostProcess && HasComponents(go.transform, (IPrefabPostProcess postProcess) => ShouldPostProcess(postProcess, options)))
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
		bool isProcessingBundledPrefab = IsProcessingBundledPrefab;
		IsProcessingBundledPrefab = options.PostProcessedDuringBundling;
		try
		{
			ProcessPass(name, go, options);
		}
		finally
		{
			IsProcessingBundledPrefab = isProcessingBundledPrefab;
		}
	}

	private void ProcessPass(string name, GameObject go, PreProcessPrefabOptions options)
	{
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Expected Obj, but got Unknown
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Expected Obj, but got Unknown
		StringPool.Get(name);
		StripEmptyChildren stripEmptyChildren = default;
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
				if (!isBundling || item5.CanPreProcessDuringBundling)
				{
					item5.PreProcess((IPrefabProcessor)(object)this, go, name, isServerside, isClientside, isBundling);
					MarkPropertiesDirty((Object)item5);
				}
			}
			if (isClientside && !isServerside && options.StripComponents)
			{
				RemoveChildEntities(go);
			}
		}
		if (!isServerside)
		{
			DisableWheelColliders(go);
		}
		if (options.StripEmptyChildren)
		{
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
					if (((Component)item6).gameObject.CompareTag("Client Cull"))
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
		if (options.PostProcess)
		{
			foreach (IPrefabPostProcess item7 in FindIComponents<IPrefabPostProcess>(go.transform))
			{
				if (ShouldPostProcess(item7, options))
				{
					item7.PostProcess((IPrefabProcessor)(object)this, go, name, isServerside, isClientside, isBundling);
					MarkPropertiesDirty((Object)item7);
				}
			}
		}
		BaseEntity baseEntity = default;
		if (options.StripComponents && isServerside && !isClientside && go.TryGetComponent<BaseEntity>(ref baseEntity))
		{
			baseEntity.ServerPreProcessed = true;
			MarkPropertiesDirty((Object)(object)baseEntity);
		}
	}

	private bool ShouldPostProcess(IPrefabPostProcess postProcess, PreProcessPrefabOptions options)
	{
		if (isBundling)
		{
			return postProcess.CanPostProcessDuringBundling;
		}
		if (postProcess.CanPostProcessDuringBundling)
		{
			return !options.PostProcessedDuringBundling;
		}
		return true;
	}

	public void Process(string name, GameObject go, bool forceInPlace = false)
	{
		PreProcessPrefabOptions assetSceneRuntime = PreProcessPrefabOptions.AssetSceneRuntime;
		GameObject val = go;
		if (Application.isPlaying && !Application.isQuitting && !val.CompareTag("NoPreProcessing"))
		{
			bool flag = PrefabNeedsCopy(val);
			bool flag2 = NeedsProcessing(val, assetSceneRuntime);
			bool flag3 = flag & flag2;
			if (!forceInPlace & flag3)
			{
				Transform val2 = (TryGetHierarchyGroup(out var obj) ? obj.transform : null);
				go = Instantiate.GameObject(val, val2);
				((Object)go).name = ((Object)val).name;
			}
			if (flag2)
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
			Wearable wearable = default;
			if (val3.TryGetComponent<Wearable>(ref wearable) && !wearable.disableRigStripping)
			{
				return true;
			}
			return false;
		}
	}

	public void ProcessForSpawning(BaseEntity entity)
	{
		if (!entity.ServerPreProcessed || !((Object)(object)((Component)entity).transform.parent == (Object)null))
		{
			PreProcessPrefabOptions options = (entity.ServerPreProcessed ? PreProcessPrefabOptions.AssetSceneRuntime : PreProcessPrefabOptions.Default_NoResetPosition);
			if (NeedsProcessing(((Component)entity).gameObject, options))
			{
				ProcessObject(null, ((Component)entity).gameObject, options);
			}
		}
	}

	public void Invalidate(string name)
	{
		if (prefabList.TryGetValue(name, out var value))
		{
			prefabList.Remove(name);
			foundExactCase.Clear();
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
		foundExactCase.Clear();
	}

	private void RemoveChildEntities(GameObject go)
	{
		BaseNetworkable[] componentsInChildren = go.GetComponentsInChildren<BaseNetworkable>();
		foreach (BaseNetworkable baseNetworkable in componentsInChildren)
		{
			if (!((Object)(object)baseNetworkable == (Object)null) && !((Object)(object)((Component)baseNetworkable).gameObject == (Object)(object)go))
			{
				Object.DestroyImmediate((Object)(object)((Component)baseNetworkable).gameObject, true);
			}
		}
	}

	private void DisableWheelColliders(GameObject go)
	{
		List<WheelCollider> list = Pool.Get<List<WheelCollider>>();
		this.FindComponents<WheelCollider>(go.transform, list);
		foreach (WheelCollider item in list)
		{
			if (!((Object)(object)((Component)item).GetComponentInParent<Rigidbody>(true) != (Object)null))
			{
				((Collider)item).enabled = false;
			}
		}
		Pool.FreeUnmanaged<WheelCollider>(ref list);
	}

	private bool TryGetHierarchyGroup(out GameObject obj)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
		if (isBundling || !Application.isPlaying)
		{
			obj = null;
			return false;
		}
		if ((Object)(object)parentObject == (Object)null)
		{
			string text;
			if (isClientside && isServerside)
			{
				text = "PrefabPreProcess - Generic";
			}
			else
			{
				text = (isServerside ? "PrefabPreProcess - Server" : "PrefabPreProcess - Client");
			}
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
		RealmedRemove realmedRemove = default;
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

	private static bool ShouldExclude(Transform transform)
	{
		BaseEntity baseEntity = default;
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
			//IL_0022: Expected Obj, but got Unknown
			set.Add(transf);
			foreach (Transform item in transf)
			{
				ExcludeChildHierarchy(item, set);
			}
		}
	}

	private bool HasComponents<T>(Transform transform, Func<T, bool> match = null)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected Obj, but got Unknown
		if (match == null)
		{
			T val = default;
			if (((Component)transform).TryGetComponent<T>(ref val))
			{
				return true;
			}
		}
		else
		{
			PooledList<T> val2 = Pool.Get<PooledList<T>>();
			try
			{
				((Component)transform).GetComponents<T>((List<T>)(object)val2);
				foreach (T item in (List<T>)(object)val2)
				{
					if (match(item))
					{
						return true;
					}
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		foreach (Transform item2 in transform)
		{
			Transform transform2 = item2;
			if (!ShouldExclude(transform2) && HasComponents(transform2, match))
			{
				return true;
			}
		}
		return false;
	}

	private bool HasComponents(Transform transform, Type t)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected Obj, but got Unknown
		Component val = default;
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
		//IL_0021: Expected Obj, but got Unknown
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
			cleanupOrder.Add(((!((Object)(object)cleanup == (Object)null)) ? cleanup.transform.GetDepth() : 0, cleanup));
		}
		cleanupOrder.Sort(((int Depth, GameObject GameObject) a, (int Depth, GameObject GameObject) b) => b.Depth.CompareTo(a.Depth));
		try
		{
			foreach (var item2 in cleanupOrder)
			{
				GameObject item = item2.GameObject;
				if (item != rootGo)
				{
					DoCleanup(item);
				}
			}
		}
		finally
		{
			cleanupOrder.Clear();
			cleanupList.Clear();
		}
	}

	public void DoCleanup(GameObject go)
	{
		if (!((Object)(object)go == (Object)null) && go.GetComponentsInChildren<Component>(true).Length <= 1)
		{
			Object.DestroyImmediate((Object)(object)go, true);
		}
	}
}
