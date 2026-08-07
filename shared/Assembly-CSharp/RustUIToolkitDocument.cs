using System;
using Rust.UI.Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class RustUIToolkitDocument : IDisposable
{
	public GameObject GameObject { get; private set; }

	public UIDocument Document { get; private set; }

	public NeedsCursor Cursor { get; private set; }

	public PanelSettings PanelSettings { get; private set; }

	public VisualElement Root
	{
		get
		{
			if (!((Object)(object)Document != (Object)null))
			{
				return null;
			}
			return Document.rootVisualElement;
		}
	}

	private RustUIToolkitDocument()
	{
	}

	public static RustUIToolkitDocument Create(Transform parent, string name, float sortingOrder, bool withCursor = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		RustUIToolkitDocument rustUIToolkitDocument = new RustUIToolkitDocument
		{
			GameObject = new GameObject(name)
		};
		rustUIToolkitDocument.GameObject.transform.SetParent(parent, false);
		if (withCursor)
		{
			rustUIToolkitDocument.Cursor = rustUIToolkitDocument.GameObject.AddComponent<NeedsCursor>();
		}
		rustUIToolkitDocument.PanelSettings = CreatePanelSettings(name, sortingOrder);
		rustUIToolkitDocument.Document = rustUIToolkitDocument.GameObject.AddComponent<UIDocument>();
		rustUIToolkitDocument.Document.panelSettings = rustUIToolkitDocument.PanelSettings;
		rustUIToolkitDocument.Document.sortingOrder = sortingOrder;
		VisualElement rootVisualElement = rustUIToolkitDocument.Document.rootVisualElement;
		if (rootVisualElement != null)
		{
			RustUI.Attach<VisualElement>(rootVisualElement);
			rootVisualElement.pickingMode = (PickingMode)1;
		}
		return rustUIToolkitDocument;
	}

	public void SetContent(VisualElement content)
	{
		VisualElement root = Root;
		if (root != null && content != null)
		{
			RustUI.Attach<VisualElement>(root);
			root.pickingMode = (PickingMode)1;
			if (content.parent != root)
			{
				root.Clear();
				root.Add(content);
			}
		}
	}

	public void SetVisible(bool visible)
	{
		if (!((Object)(object)Document == (Object)null))
		{
			((Behaviour)Document).enabled = true;
			VisualElement rootVisualElement = Document.rootVisualElement;
			if (rootVisualElement != null)
			{
				RustUI.SetVisible(rootVisualElement, visible);
			}
		}
	}

	public void SetCursorEnabled(bool enabled)
	{
		if ((Object)(object)Cursor != (Object)null)
		{
			((Behaviour)Cursor).enabled = enabled;
		}
	}

	public void Dispose()
	{
		if ((Object)(object)Document != (Object)null && Document.rootVisualElement != null)
		{
			Document.rootVisualElement.Clear();
		}
		if ((Object)(object)GameObject != (Object)null)
		{
			Object.Destroy((Object)(object)GameObject);
			GameObject = null;
			Document = null;
			Cursor = null;
		}
		if ((Object)(object)PanelSettings != (Object)null)
		{
			Object.Destroy((Object)(object)PanelSettings);
			PanelSettings = null;
		}
	}

	private static PanelSettings CreatePanelSettings(string name, float sortingOrder)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		PanelSettings panelSettings = RustUIHost.PanelSettings;
		PanelSettings obj = ScriptableObject.CreateInstance<PanelSettings>();
		((Object)obj).name = name + " PanelSettings";
		obj.themeStyleSheet = panelSettings.themeStyleSheet;
		obj.scaleMode = panelSettings.scaleMode;
		obj.referenceResolution = panelSettings.referenceResolution;
		obj.match = panelSettings.match;
		obj.sortingOrder = sortingOrder;
		return obj;
	}
}
