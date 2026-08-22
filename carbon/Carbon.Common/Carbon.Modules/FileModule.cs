using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class FileModule : CarbonModule<EmptyModuleConfig, EmptyModuleData>
{
	public class FileBrowser
	{
		public struct Item
		{
			public string Name;

			public string Path;

			public bool IsDirectory;

			public FileInfo Info;

			public static Item Make(string path)
			{
				Item result = new Item
				{
					Name = System.IO.Path.GetFileName(path),
					Path = path,
					IsDirectory = OsEx.Folder.Exists(path)
				};
				if (!result.IsDirectory)
				{
					result.Info = new FileInfo(path);
				}
				return result;
			}
		}

		public string Title;

		public string DirectoryLimit;

		public string Extension;

		public bool AllowBackFromParent;

		public Action<BasePlayer, FileBrowser> OnConfirm;

		public Action<BasePlayer, FileBrowser> OnCancel;

		public Func<Item, string> OnExtraInfo;

		public string BackgroundColor = "0 0 0 0.99";

		public string SelectedFile;

		public string DeletingFile;

		internal string CurrentDirectory;

		internal CUI.Handler Handler;

		internal const string PanelId = "carbonfileui";

		internal List<Item> Items = new List<Item>();

		public void ChangeDirectory(string directory)
		{
			Items.Clear();
			if (string.IsNullOrEmpty(DirectoryLimit) || directory.Contains(DirectoryLimit))
			{
				CurrentDirectory = directory;
			}
			if (OsEx.Folder.Exists(directory))
			{
				Items.AddRange(from x in Directory.EnumerateDirectories(CurrentDirectory, "*")
					select Item.Make(x) into x
					orderby x.Name
					select x);
				Items.AddRange(from x in Directory.EnumerateFiles(CurrentDirectory, "*" + Extension)
					select Item.Make(x) into x
					orderby x.Info.LastWriteTime descending
					select x);
			}
		}

		public void Draw(BasePlayer player)
		{
			AdminModule.PlayerSession playerSession = Instance.Admin.GetPlayerSession(player);
			using (CUI cui = new CUI(Handler))
			{
				CuiElementContainer container = cui.CreateContainer("carbonfileui", BackgroundColor, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, needsCursor: true, needsKeyboard: false, CUI.ClientPanels.Overlay, "carbonfileui");
				CUI.Pair<string, CuiElement> pair = cui.CreatePanel(container, "carbonfileui", "0.05 0.05 0.05 0.9", null, 0.5f, 0.5f, 0.5f, 0.5f, -300f, 300f, -250f, 250f);
				cui.CreateText(container, pair, Cache.CUI.WhiteColor, string.Format("{0} ({1}) ({2:n0})", Title.ToUpper(), string.IsNullOrEmpty(Extension) ? "*" : Extension, Items.Count((Item x) => !x.IsDirectory)), 20, 0.03f, 1f, 0f, 0.97f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedBold, (VerticalWrapMode)1);
				CUI.Pair<string, CuiElement> pair2 = cui.CreatePanel(container, pair, Cache.CUI.BlankColor, null, 0f, 1f, 0f, 0.9f);
				cui.CreateText(container, pair2, "1 1 1 0.2", "NAME", 12, 0.03f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, pair2, "1 1 1 0.2", "INFO", 12, 0.33f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, pair2, "1 1 1 0.2", "DATE", 12, 0.63f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cui.CreateText(container, pair2, "1 1 1 0.2", "SIZE", 12, 0.8f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)0, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				CUI.Pair<string, CuiElement, CuiElement> pair3 = cui.CreateProtectedButton(container, pair, "0.5 0 0 0.4", Cache.CUI.BlankColor, string.Empty, 0, null, 0.955f, 0.99f, 0.95f, 0.99f, 0f, 0f, 0f, 0f, "file.action cancel", (TextAnchor)4);
				cui.CreateImage(container, pair3, "close", "1 0.5 0.5 0.3", null, 0.2f, 0.8f, 0.2f, 0.8f);
				CUI.Pair<string, CuiElement> pair4 = cui.CreateScrollView(container, pair, vertical: true, horizontal: false, (MovementType)1, 0.1f, inertia: true, 0.1f, 30f, out var contentTransformComponent, out var _, out var verticalScrollBar, 0.01f, 1f, 0f, 0.85f, 0f, 0f, 0.04f);
				cui.CreatePanel(container, pair4, Cache.CUI.BlankColor);
				verticalScrollBar.Size = 3f;
				verticalScrollBar.Invert = true;
				float offset = 0f;
				float value = (float)(Items.Count + 1) * 28f;
				contentTransformComponent.AnchorMin = "0 0";
				contentTransformComponent.AnchorMax = "0.985 0";
				contentTransformComponent.OffsetMin = $"0 {0f - value.Clamp(425f, float.MaxValue)}";
				contentTransformComponent.OffsetMax = "0 0";
				DrawItem(this, new Item
				{
					IsDirectory = true,
					Path = OsEx.Folder.GetPreviousFolder(CurrentDirectory)
				}, cui, container, -1, pair4, ref offset);
				for (int num = 0; num < Items.Count; num++)
				{
					DrawItem(this, Items[num], cui, container, num, pair4, ref offset);
				}
				playerSession.SetStorage(null, "file", this);
				cui.Send(container, player);
			}
			static void DrawItem(FileBrowser browser, Item item, CUI cUI, CuiElementContainer container2, int i, CUI.Pair<string, CuiElement> scroll, ref float reference)
			{
				CUI.Pair<string, CuiElement, CuiElement> pair5 = cUI.CreateProtectedButton(container2, scroll, "0.3 0.3 0.3 0.5", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 1f, 1f, 0f, 0f, reference - 25f, reference, $"file.action select {i}", (TextAnchor)3);
				cUI.CreateText(container2, pair5, "1 1 1 0.7", (i == -1) ? "..." : Path.GetFileName(item.Path), 10, 0.045f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				cUI.CreateImage(container2, pair5, item.IsDirectory ? "folder" : "file", "0.7 0.7 0.7 0.5", null, 0.01f, 0.04f, 0.15f, 0.85f);
				cUI.CreateText(container2, pair5, "1 1 1 0.4", browser.OnExtraInfo?.Invoke(item), 10, 0.33f, 0.65f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
				if (!item.IsDirectory)
				{
					DateTime lastWriteTime = item.Info.LastWriteTime;
					cUI.CreateText(container2, pair5, "1 1 1 0.4", string.Format("{0:00}.{1:00}.{2:0000} {3:00}:{4:00}", new object[5] { lastWriteTime.Month, lastWriteTime.Day, lastWriteTime.Year, lastWriteTime.Hour, lastWriteTime.Minute }), 10, 0.63f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cUI.CreateText(container2, pair5, "1 1 1 0.4", item.Info.Length.Format().ToUpper() ?? "", 10, 0.8f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
					cUI.CreateProtectedButton(container2, pair5, "0.5 0 0 0.4", "1 0.5 0.5 0.3", "DELETE", 8, null, 0.9f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, $"file.action delete {i}", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, 0f, 0f, needsCursor: false, needsKeyboard: false, null, null, outlineUseGraphicAlpha: false, $"filedelete{i}").Element2.Name = $"filedeletetext{i}";
				}
				reference -= 28f;
			}
		}
	}

	public readonly CUI.Handler Handler = new CUI.Handler();

	public static FileModule Instance { get; internal set; }

	public AdminModule Admin { get; internal set; }

	public override string Name => "File";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override Type Type => typeof(FileModule);

	public override bool EnabledByDefault => true;

	public override bool ForceEnabled => true;

	public override bool InitEnd()
	{
		Instance = this;
		Admin = BaseModule.GetModule<AdminModule>();
		return base.InitEnd();
	}

	public FileBrowser Open(BasePlayer player, string title, string directory, string directoryLimit, string extension, Action<BasePlayer, FileBrowser> onConfirm = null, Action<BasePlayer, FileBrowser> onCancel = null, Func<FileBrowser.Item, string> onExtraInfo = null)
	{
		FileBrowser file = new FileBrowser
		{
			Title = title,
			DirectoryLimit = directoryLimit,
			Extension = (string.IsNullOrEmpty(extension) ? string.Empty : ("." + extension)),
			OnCancel = onCancel,
			OnConfirm = onConfirm,
			OnExtraInfo = onExtraInfo,
			Handler = new CUI.Handler()
		};
		NextFrame(delegate
		{
			file.ChangeDirectory(directory);
			file.Draw(player);
		});
		return file;
	}

	public void Close(BasePlayer player)
	{
		using CUI cUI = new CUI(Handler);
		cUI.Destroy("carbonfileui", player);
	}

	[ProtectedCommand("file.action")]
	private void FileAction(Arg arg)
	{
		AdminModule.PlayerSession ap = Admin.GetPlayerSession(ArgEx.Player(arg));
		FileBrowser file = ap.GetStorage<FileBrowser>(null, "file");
		switch (arg.GetString(0, ""))
		{
		case "select":
		{
			int num = arg.GetInt(1, 0);
			if (num == -1)
			{
				file.ChangeDirectory(OsEx.Folder.GetPreviousFolder(file.CurrentDirectory));
				file.Draw(ap.Player);
				break;
			}
			FileBrowser.Item item = file.Items[num];
			if (item.IsDirectory)
			{
				file.ChangeDirectory(item.Path);
				file.Draw(ap.Player);
			}
			else
			{
				Instance.Close(ap.Player);
				file.SelectedFile = item.Path;
				file.OnConfirm?.Invoke(ap.Player, file);
			}
			break;
		}
		case "cancel":
			Instance.Close(ap.Player);
			file.OnCancel?.Invoke(ap.Player, file);
			break;
		case "delete":
		{
			int index = arg.GetInt(1, 0);
			string path = file.Items[index].Path;
			if (file.DeletingFile == path)
			{
				OsEx.File.Delete(path);
				file.Items.RemoveAll((FileBrowser.Item x) => x.Path == path);
				file.DeletingFile = null;
				file.Draw(ap.Player);
			}
			if (!string.IsNullOrEmpty(file.DeletingFile))
			{
				break;
			}
			using CUI cui = new CUI(Handler);
			using CUI.Handler.UpdatePool updatePool = cui.UpdatePool();
			updatePool.Add(cui.UpdateProtectedButton($"filedelete{index}", "0 0.5 0 0.4", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, $"file.action delete {index}", (TextAnchor)4));
			updatePool.Add(cui.UpdateText($"filedeletetext{index}", "0 1 0 0.4", "CONFIRM", 8, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
			updatePool.Send(ap.Player);
			file.DeletingFile = path;
			Community.Runtime.Core.timer.In(0.75f, delegate
			{
				using CUI cui2 = new CUI(Handler);
				using CUI.Handler.UpdatePool updatePool2 = cui2.UpdatePool();
				updatePool2.Add(cui2.UpdateProtectedButton($"filedelete{index}", "0.5 0 0 0.4", Cache.CUI.BlankColor, string.Empty, 0, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, $"file.action delete {index}", (TextAnchor)4));
				updatePool2.Add(cui2.UpdateText($"filedeletetext{index}", "1 0.5 0.5 0.3", "DELETE", 8, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1));
				updatePool2.Send(ap.Player);
				file.DeletingFile = null;
			});
			break;
		}
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		object obj = ((args?.Length > 0) ? args[0] : null);
		try
		{
			if (hook == 2520824062u)
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag2 = flag;
				Arg arg = ((!flag2) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag2)
				{
					FileAction(arg);
					return null;
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error(string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				Name,
				Version,
				hook
			}), ex);
			OnException(hook);
		}
		return null;
	}
}
