using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Facepunch;

public class VirtualScroll : MonoBehaviour
{
	public interface IDataSource
	{
		int GetItemCount();

		float GetItemSize(int i);

		void SetItemData(int i, GameObject obj);
	}

	public interface IVisualUpdate
	{
		void OnVisualUpdate(int i, GameObject obj);
	}

	public int ItemHeight = 40;

	public int ItemSpacing = 10;

	public RectOffset Padding;

	[Tooltip("Optional, we'll try to GetComponent IDataSource from this object on awake")]
	public GameObject DataSourceObject;

	public GameObject SourceObject;

	public ScrollRect ScrollRect;

	public RectTransform OverrideContentRoot;

	private IDataSource dataSource;

	private Dictionary<int, GameObject> ActivePool = new Dictionary<int, GameObject>();

	private Stack<GameObject> InactivePool = new Stack<GameObject>();

	private int BlockHeight => ItemHeight + ItemSpacing;

	public void Awake()
	{
		((UnityEvent<Vector2>)(object)ScrollRect.onValueChanged).AddListener((UnityAction<Vector2>)OnScrollChanged);
		if ((Object)(object)DataSourceObject != (Object)null)
		{
			SetDataSource(DataSourceObject.GetComponent<IDataSource>());
		}
	}

	public void OnDestroy()
	{
		((UnityEvent<Vector2>)(object)ScrollRect.onValueChanged).RemoveListener((UnityAction<Vector2>)OnScrollChanged);
	}

	[UnityEvent]
	private void OnScrollChanged(Vector2 pos)
	{
		Rebuild();
	}

	public void SetDataSource(IDataSource source, bool forceRebuild = false)
	{
		if (dataSource != source || forceRebuild)
		{
			dataSource = source;
			FullRebuild();
		}
	}

	private float GetItemHeight(int i)
	{
		if (dataSource != null)
		{
			float itemSize = dataSource.GetItemSize(i);
			if (itemSize > 0f)
			{
				return itemSize;
			}
		}
		return BlockHeight;
	}

	public void FullRebuild()
	{
		int[] array = ActivePool.Keys.ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
		Rebuild();
	}

	public void DataChanged()
	{
		foreach (KeyValuePair<int, GameObject> item in ActivePool)
		{
			dataSource.SetItemData(item.Key, item.Value);
		}
		Rebuild();
	}

	protected virtual float GetContentHeight(int itemCount)
	{
		float result = BlockHeight * itemCount - ItemSpacing + Padding.top + Padding.bottom;
		if (dataSource == null)
		{
			return result;
		}
		float num = Padding.top + Padding.bottom - ItemSpacing;
		for (int i = 0; i < itemCount; i++)
		{
			num += GetItemHeight(i) + (float)ItemSpacing;
		}
		return num;
	}

	protected virtual float SetCanvasSize(int items)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (!((Object)(object)OverrideContentRoot != (Object)null))
		{
			Transform child = ((Transform)ScrollRect.viewport).GetChild(0);
			obj = ((child is RectTransform) ? child : null);
		}
		else
		{
			obj = OverrideContentRoot;
		}
		((RectTransform)obj).SetSizeWithCurrentAnchors((Axis)1, GetContentHeight(items));
		return ((RectTransform)obj).anchoredPosition.y;
	}

	public void Rebuild()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (dataSource == null)
		{
			return;
		}
		int itemCount = dataSource.GetItemCount();
		if (itemCount <= 0)
		{
			return;
		}
		float num = SetCanvasSize(itemCount);
		Rect rect = ScrollRect.viewport.rect;
		int num2 = Mathf.Max(2, Mathf.CeilToInt(((Rect)(ref rect)).height / (float)BlockHeight));
		int num3 = Mathf.FloorToInt((num - (float)Padding.top) / (float)BlockHeight);
		int num4 = num3 + num2;
		RecycleOutOfRange(num3, num4);
		for (int i = num3; i <= num4; i++)
		{
			if (i >= 0 && i < itemCount)
			{
				BuildItem(i);
			}
		}
	}

	public void Update()
	{
		if (!(dataSource is IVisualUpdate visualUpdate))
		{
			return;
		}
		foreach (KeyValuePair<int, GameObject> item in ActivePool)
		{
			visualUpdate.OnVisualUpdate(item.Key, item.Value);
		}
	}

	private void RecycleOutOfRange(int startVisible, float endVisible)
	{
		int[] array = (from x in ActivePool.Keys
			where x < startVisible || (float)x > endVisible
			select (x)).ToArray();
		foreach (int key in array)
		{
			Recycle(key);
		}
	}

	private void Recycle(int key)
	{
		GameObject val = ActivePool[key];
		val.SetActive(false);
		ActivePool.Remove(key);
		InactivePool.Push(val);
	}

	private void BuildItem(int i)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (i >= 0 && !ActivePool.ContainsKey(i))
		{
			GameObject item = GetItem();
			item.SetActive(true);
			dataSource.SetItemData(i, item);
			Transform transform = item.transform;
			Transform obj = ((transform is RectTransform) ? transform : null);
			((RectTransform)obj).anchorMin = new Vector2(0f, 1f);
			((RectTransform)obj).anchorMax = new Vector2(1f, 1f);
			((RectTransform)obj).pivot = new Vector2(0.5f, 1f);
			((RectTransform)obj).offsetMin = new Vector2(0f, 0f);
			float itemHeight = GetItemHeight(i);
			((RectTransform)obj).offsetMax = new Vector2(0f, itemHeight);
			((RectTransform)obj).sizeDelta = new Vector2((float)((Padding.left + Padding.right) * -1), itemHeight);
			((RectTransform)obj).anchoredPosition = new Vector2((float)(Padding.left - Padding.right) * 0.5f, (float)(-1 * (i * BlockHeight + Padding.top)));
			ActivePool[i] = item;
		}
	}

	private GameObject GetItem()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (InactivePool.Count == 0)
		{
			GameObject val = Object.Instantiate<GameObject>(SourceObject);
			val.transform.SetParent((Transform)(((Object)(object)OverrideContentRoot != (Object)null) ? ((object)OverrideContentRoot) : ((object)((Transform)ScrollRect.viewport).GetChild(0))), false);
			val.transform.localScale = Vector3.one;
			val.SetActive(false);
			InactivePool.Push(val);
		}
		return InactivePool.Pop();
	}
}
