using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Carbon.Base;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Pooling;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using Oxide.Plugins;
using UnityEngine;
using UnityEngine.UI;

namespace Carbon.Modules;

public class HammerModule : CarbonModule<HammerModule.HammerConfig, HammerModule.HammerData>
{
	public class HammerEditor
	{
		[JsonIgnore]
		public ulong playerId;

		public float x = ins.DefaultX;

		public float y = ins.DefaultY;

		public float uiDistance = ins.UIDefaultDistance;

		public float moveDistance = ins.DefaultMoveDistance;

		public bool bypassCreativeMode;

		public bool waterLayer = true;

		public bool bypassImmovableEntityDestroyConfirmations;

		public bool bypassHammer;

		[JsonIgnore]
		public bool showExtra;

		[JsonIgnore]
		public bool destructionMode;

		[JsonIgnore]
		public bool isMovingEntity;

		[JsonIgnore]
		public bool isRepairingOrDestroyingBuilding;

		private BasePlayer player;

		public BasePlayer GetPlayer()
		{
			if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)player))
			{
				player = BasePlayer.FindAwakeOrSleepingByID(playerId);
			}
			return player;
		}

		public void Reset()
		{
			showExtra = false;
			isMovingEntity = false;
			isRepairingOrDestroyingBuilding = false;
			destructionMode = false;
			lastCreativeModePlayers.Remove(playerId);
		}

		public Vector2 GetCoordinates()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(x, y);
		}

		public void SetCoordinates(Vector2 value)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			x = value.x;
			y = value.y;
			ins.Save();
		}
	}

	public class HammerConfig
	{
		public int MinimumAuthLevel = 1;

		public float UIRefreshRate = 0.1f;

		public float UIDefaultDistance = 10f;

		public float UIDistanceFlyMultiplier = 2.5f;

		public float UIDefaultX = 0.75f;

		public float UIDefaultY = 0.25f;

		public float DefaultMoveDistance = 5f;

		public float MoveLerp = 10f;

		public bool MoveEverything;

		public float BuildingBatchRefreshRate = 0.25f;

		public int BuildingRepairBatchCount = 50;

		public int BuildingDestroyBatchCount = 15;
	}

	public class HammerData
	{
		public Dictionary<ulong, HammerEditor> Hammers = new Dictionary<ulong, HammerEditor>();

		public HammerEditor GetOrCreateEditor(ulong playerId)
		{
			if (!Hammers.TryGetValue(playerId, out var value))
			{
				value = (Hammers[playerId] = new HammerEditor());
			}
			if (value.playerId == 0L)
			{
				value.playerId = playerId;
			}
			return value;
		}
	}

	public const string cuiName = "hammereditor.cui";

	public ListHashSet<Func<BaseEntity, bool, (string name, object value, bool shouldShow)>> CustomFields = new ListHashSet<Func<BaseEntity, bool, (string, object, bool)>>();

	public ListHashSet<Func<BaseEntity, bool, (string name, string color, string command, bool shouldShow)>> CustomButons = new ListHashSet<Func<BaseEntity, bool, (string, string, string, bool)>>();

	private static readonly Phrase destroyingBuildingCancelledPhrase;

	private static readonly Phrase destroyingBuildingPhrase;

	private static readonly Phrase repairedCancelledPhrase;

	private static readonly Phrase repairedPhrase;

	private static readonly Dictionary<ulong, BaseEntity> lastCreativeModePlayers;

	private static readonly Dictionary<ulong, BaseEntity> lastLastCreativeModePlayers;

	private static readonly Dictionary<string, ModalModule.Modal.Field> temp;

	private static CuiDraggableComponent cachedDraggable;

	private static HammerModule ins;

	private static bool isSubscribedToOnPlayerInput;

	private static bool forcefullySubscribeToOnPlayerInput;

	private static readonly string[] blacklistedMovingPrefabs;

	public ModalModule Modal;

	private Timer timer;

	public override string Name => "Hammer";

	public override VersionNumber Version => new VersionNumber(1, 0, 0);

	public override bool EnabledByDefault => false;

	public override Type Type => typeof(HammerModule);

	[CommandVar("hammer.uidistanceflymultiplier", "The multiplication value of the distance needed for an entity to be picked up by the Hammer UI when flying")]
	[AuthLevel(1)]
	public float UIDistanceFlyMultiplier
	{
		get
		{
			return base.ConfigInstance.UIDistanceFlyMultiplier;
		}
		set
		{
			base.ConfigInstance.UIDistanceFlyMultiplier = value.Clamp(1f, 10f);
			Save();
		}
	}

	[CommandVar("hammer.uidistance", "The minimum distance from an entity you're looking at to be picked up by the Hammer UI")]
	[AuthLevel(1)]
	public float UIDefaultDistance
	{
		get
		{
			return base.ConfigInstance.UIDefaultDistance;
		}
		set
		{
			base.ConfigInstance.UIDefaultDistance = value.Clamp(0.5f, 50f);
			Save();
		}
	}

	[CommandVar("hammer.uirefreshrate", "The responsiveness of how fast the Hammer UI updates (lower is more accurate, but could be affecting performance)")]
	[AuthLevel(1)]
	public float UIRefreshRate
	{
		get
		{
			return base.ConfigInstance.UIRefreshRate;
		}
		set
		{
			base.ConfigInstance.UIRefreshRate = value.Clamp(0f, 2.5f);
			timer?.Destroy();
			timer = Community.Runtime.Core.timer.Every(base.ConfigInstance.UIRefreshRate, TickCheck);
			Save();
		}
	}

	[CommandVar("hammer.movedistance", "The maximum distance away of the moved entity from the player's face")]
	[AuthLevel(1)]
	public float DefaultMoveDistance
	{
		get
		{
			return base.ConfigInstance.DefaultMoveDistance;
		}
		set
		{
			base.ConfigInstance.DefaultMoveDistance = value.Clamp(0.5f, 50f);
			Save();
		}
	}

	[CommandVar("hammer.movelerp", "Smoothing value of the moved entity (lesser is smoother)")]
	[AuthLevel(1)]
	public float MoveLerp
	{
		get
		{
			return base.ConfigInstance.MoveLerp;
		}
		set
		{
			base.ConfigInstance.MoveLerp = value.Clamp(1f, 20f);
			Save();
		}
	}

	[CommandVar("hammer.moveeverything", "Bypass all logical checks for important entities when moving entities (use cautiously!)")]
	[AuthLevel(2)]
	public bool MoveEverything
	{
		get
		{
			return base.ConfigInstance.MoveEverything;
		}
		set
		{
			base.ConfigInstance.MoveEverything = value;
			Save();
		}
	}

	[CommandVar("hammer.uix", "Default UI X-axis position")]
	[AuthLevel(1)]
	public float DefaultX
	{
		get
		{
			return base.ConfigInstance.UIDefaultX;
		}
		set
		{
			base.ConfigInstance.UIDefaultX = value.Clamp(0f, 1f);
			Save();
		}
	}

	[CommandVar("hammer.uiy", "Default UI Y-axis position")]
	[AuthLevel(1)]
	public float DefaultY
	{
		get
		{
			return base.ConfigInstance.UIDefaultY;
		}
		set
		{
			base.ConfigInstance.UIDefaultY = value.Clamp(0f, 1f);
			Save();
		}
	}

	[CommandVar("hammer.brepairbatch", "Building entity repair count per batch")]
	[AuthLevel(1)]
	public int BuildingRepairBatch
	{
		get
		{
			return base.ConfigInstance.BuildingRepairBatchCount;
		}
		set
		{
			base.ConfigInstance.BuildingRepairBatchCount = value.Clamp(1, 100);
			Save();
		}
	}

	[CommandVar("hammer.bdestroybatch", "Building entity destruction count per batch")]
	[AuthLevel(1)]
	public int BuildingDestroyBatch
	{
		get
		{
			return base.ConfigInstance.BuildingDestroyBatchCount;
		}
		set
		{
			base.ConfigInstance.BuildingDestroyBatchCount = value.Clamp(1, 100);
			Save();
		}
	}

	[CommandVar("hammer.bbatchrefreshrate", "Speed of how fast batch iterations happen for building repairing and destroying")]
	[AuthLevel(1)]
	public float BuildingBatchRefreshRate
	{
		get
		{
			return base.ConfigInstance.BuildingBatchRefreshRate;
		}
		set
		{
			base.ConfigInstance.BuildingBatchRefreshRate = value.Clamp(0f, 2f);
			Save();
		}
	}

	[CommandVar("hammer.minauthlevel", "Minimum auth level for certain Hammer UI checks")]
	[AuthLevel(2)]
	public int MinimumAuthLevel
	{
		get
		{
			return base.ConfigInstance.MinimumAuthLevel;
		}
		set
		{
			base.ConfigInstance.MinimumAuthLevel = value.Clamp(0, 4);
			Save();
		}
	}

	public override void Init()
	{
		base.Init();
		ins = this;
	}

	public override void OnPostServerInit(bool initial)
	{
		base.OnPostServerInit(initial);
		Modal = BaseModule.GetModule<ModalModule>();
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		timer?.Destroy();
		timer = Community.Runtime.Core.timer.Every(UIRefreshRate, TickCheck);
		ValidatePermanentPlayerInputHook();
	}

	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);
		timer?.Destroy();
		timer = null;
		lastCreativeModePlayers.Clear();
		lastLastCreativeModePlayers.Clear();
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			BasePlayer player = BasePlayer.activePlayerList[i];
			ClearGUI(player);
		}
		foreach (KeyValuePair<ulong, HammerEditor> hammer in base.DataInstance.Hammers)
		{
			HammerEditor value = hammer.Value;
			value.Reset();
		}
	}

	public override void Load()
	{
		base.Load();
		ValidatePermanentPlayerInputHook();
	}

	public bool ShouldSubscribeToOnPlayerInput()
	{
		if (forcefullySubscribeToOnPlayerInput)
		{
			return true;
		}
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			BasePlayer val = BasePlayer.activePlayerList[i];
			if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val) && val.IsInCreativeMode)
			{
				return true;
			}
		}
		return false;
	}

	public void ValidatePermanentPlayerInputHook()
	{
		foreach (KeyValuePair<ulong, HammerEditor> hammer in base.DataInstance.Hammers)
		{
			if (hammer.Value.bypassHammer || hammer.Value.bypassCreativeMode)
			{
				forcefullySubscribeToOnPlayerInput = true;
				return;
			}
		}
		forcefullySubscribeToOnPlayerInput = false;
	}

	public bool CanBeMoved(BasePlayer player, BaseEntity entity)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity))
		{
			return false;
		}
		if (((NetworkableId)(ref ((BaseNetworkable)player).net.ID)).Equals(((BaseNetworkable)entity).net.ID))
		{
			return false;
		}
		if (!(entity is SimpleBuildingBlock) && !(entity is BuildingBlock))
		{
			IOEntity val = (IOEntity)(object)((entity is IOEntity) ? entity : null);
			if (val != null && (val.GetConnectedInputCount() > 0 || val.GetConnectedOutputCount() > 0))
			{
				return false;
			}
			if (((BaseNetworkable)entity).GetRootParentEntity() is PlayerBoat || StringEx.Contains(((BaseNetworkable)entity).PrefabName, "boatbuilding", CompareOptions.OrdinalIgnoreCase))
			{
				return false;
			}
			if (MoveEverything)
			{
				return true;
			}
			BasePlayer val2 = (BasePlayer)(object)((entity is BasePlayer) ? entity : null);
			if (val2 != null && val2.IsSleeping())
			{
				return true;
			}
			for (int i = 0; i < blacklistedMovingPrefabs.Length; i++)
			{
				if (!(entity is MiningQuarry) && ((BaseNetworkable)entity).ShortPrefabName.Contains(blacklistedMovingPrefabs[i], StringComparison.CurrentCultureIgnoreCase))
				{
					return false;
				}
			}
			if (!(entity is NPCVendingMachine))
			{
				if (!(entity is BigWheelBettingTerminal))
				{
					if (!(entity is SlotMachine))
					{
						if (!(entity is MarketTerminal))
						{
							if (!(entity is FuseBox))
							{
								if (!(entity is ANDSwitch) && !(entity is ORSwitch) && !(entity is XORSwitch) && !(entity is RANDSwitch) && !(entity is SmartSwitch) && !(entity is DummySwitch) && !(entity is ElectricSwitch) && !(entity is TimerSwitch) && !(entity is PressButton) && !(entity is RFBroadcaster) && !(entity is CardReader) && !(entity is DoorManipulator))
								{
									if (!(entity is Recycler))
									{
										if (!(entity is WheelSwitch))
										{
											if (!(entity is ProgressDoor))
											{
												if (!(entity is HackableLockedCrate))
												{
													if (!(entity is Door))
													{
														if (!(entity is Lift))
														{
															if (!(entity is ComputerStation))
															{
																if (!(entity is HarborCraneContainerPickup) && !(entity is HarborCraneStatic) && !(entity is MagnetCrane))
																{
																	if (!(entity is Barricade))
																	{
																		if (!(entity is BaseSubmarine))
																		{
																			if (!(entity is Candle))
																			{
																				if (!(entity is DroppedItemContainer))
																				{
																					if (!(entity is CinematicEntity))
																					{
																						if (!(entity is HotAirBalloon))
																						{
																							if (!(entity is BaseHelicopter))
																							{
																								if (!(entity is BaseChair))
																								{
																									if (!(entity is BaseBoat))
																									{
																										if (!(entity is BaseCorpse))
																										{
																											if (!(entity is BaseLadder))
																											{
																												if (!(entity is RidableHorse))
																												{
																													if (!(entity is MiningQuarry))
																													{
																														if (!(entity is TreeEntity))
																														{
																															if (!(entity is Snowmobile) && !(entity is Bike))
																															{
																																if (!(entity is ModularCar) && !(entity is BasicCar))
																																{
																																	if (entity is DecayEntity)
																																	{
																																		return true;
																																	}
																																	string shortPrefabName = ((BaseNetworkable)entity).ShortPrefabName;
																																	return StringEx.Contains(((BaseNetworkable)entity).ShortPrefabName, "deploy", CompareOptions.IgnoreCase) || StringEx.Contains(((BaseNetworkable)entity).ShortPrefabName, "cliff", CompareOptions.IgnoreCase) || StringEx.Contains(((BaseNetworkable)entity).ShortPrefabName, "rock", CompareOptions.IgnoreCase) || StringEx.Contains(((BaseNetworkable)entity).ShortPrefabName, "admin_invis", CompareOptions.IgnoreCase) || (StringEx.Contains(((BaseNetworkable)entity).ShortPrefabName, "grass_displace", CompareOptions.IgnoreCase) ? true : false);
																																}
																																return true;
																															}
																															return true;
																														}
																														return true;
																													}
																													return true;
																												}
																												return true;
																											}
																											return true;
																										}
																										return true;
																									}
																									return true;
																								}
																								return true;
																							}
																							return true;
																						}
																						return true;
																					}
																					return true;
																				}
																				return true;
																			}
																			return true;
																		}
																		return true;
																	}
																	return false;
																}
																return false;
															}
															return false;
														}
														return false;
													}
													return false;
												}
												return false;
											}
											return false;
										}
										return false;
									}
									return false;
								}
								return false;
							}
							return false;
						}
						return false;
					}
					return false;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public bool CanBeToggled(BaseEntity entity)
	{
		if (!(entity is Door))
		{
			IOEntity val = (IOEntity)(object)((entity is IOEntity) ? entity : null);
			if (val != null)
			{
				if (val.inputs.Length != 0)
				{
					return true;
				}
				if (entity is IAlwaysOn)
				{
					goto IL_006f;
				}
				if (entity is VendingMachine)
				{
					return true;
				}
			}
			else
			{
				if (entity is StorageContainer)
				{
					return true;
				}
				if (entity is IAlwaysOn)
				{
					goto IL_006f;
				}
				if (entity is MiningQuarry || entity is EngineSwitch)
				{
					return true;
				}
				if (entity is BuildingBlock)
				{
					return true;
				}
				if (entity is SteeringWheel)
				{
					return true;
				}
			}
			return false;
		}
		return true;
		IL_006f:
		return true;
	}

	public void TickCheck()
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		using (TimeMeasure.New("Hammer.TickCheck"))
		{
			bool flag = ShouldSubscribeToOnPlayerInput();
			if (flag && !isSubscribedToOnPlayerInput)
			{
				Subscribe("OnPlayerInput");
				isSubscribedToOnPlayerInput = true;
			}
			else if (!flag && isSubscribedToOnPlayerInput)
			{
				Unsubscribe("OnPlayerInput");
				isSubscribedToOnPlayerInput = false;
			}
			PooledList<HammerEditor> val = Pool.Get<PooledList<HammerEditor>>();
			try
			{
				foreach (KeyValuePair<ulong, BaseEntity> lastCreativeModePlayer in lastCreativeModePlayers)
				{
					HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(lastCreativeModePlayer.Key);
					if (!ShouldShowUI(orCreateEditor, out var _))
					{
						((List<HammerEditor>)(object)val).Add(orCreateEditor);
					}
				}
				lastLastCreativeModePlayers.Clear();
				foreach (KeyValuePair<ulong, BaseEntity> lastCreativeModePlayer2 in lastCreativeModePlayers)
				{
					lastLastCreativeModePlayers[lastCreativeModePlayer2.Key] = lastCreativeModePlayer2.Value;
				}
				lastCreativeModePlayers.Clear();
				for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
				{
					BasePlayer val2 = BasePlayer.activePlayerList[i];
					HammerEditor orCreateEditor2 = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(val2.userID));
					if (ShouldShowUI(orCreateEditor2, out var entity2) && !orCreateEditor2.showExtra)
					{
						if (!lastLastCreativeModePlayers.TryGetValue(EncryptedValue<ulong>.op_Implicit(val2.userID), out var value) || (Object)(object)value != (Object)(object)entity2)
						{
							ApplyGUI(val2, entity2, showExtra: false);
						}
						lastCreativeModePlayers[EncryptedValue<ulong>.op_Implicit(val2.userID)] = entity2;
					}
				}
				for (int j = 0; j < ((List<HammerEditor>)(object)val).Count; j++)
				{
					HammerEditor hammerEditor = ((List<HammerEditor>)(object)val)[j];
					if (!hammerEditor.showExtra)
					{
						ClearGUI(hammerEditor.GetPlayer());
					}
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public bool ShouldShowUI(HammerEditor editor, out BaseEntity entity)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		entity = null;
		BasePlayer player = editor.GetPlayer();
		float num = editor.uiDistance;
		if (player.IsFlying)
		{
			num *= UIDistanceFlyMultiplier;
		}
		Item activeItem = player.GetActiveItem();
		bool bypassHammer = editor.bypassHammer;
		bool flag = bypassHammer;
		if (!flag)
		{
			bool flag2 = activeItem != null;
			bool flag3 = flag2;
			if (flag3)
			{
				int itemid = activeItem.info.itemid;
				bool flag4 = ((itemid == 200773292 || itemid == 1803831286) ? true : false);
				flag3 = flag4;
			}
			flag = flag3;
		}
		bool flag5 = flag;
		RaycastHit val = default(RaycastHit);
		if ((!player.IsInCreativeMode && !editor.bypassCreativeMode) || !flag5 || !Physics.Raycast(player.eyes.HeadRay(), ref val, num, -1, (QueryTriggerInteraction)1))
		{
			return false;
		}
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)(entity = RaycastHitEx.GetEntity(val))))
		{
			return !editor.isMovingEntity;
		}
		return false;
	}

	public void ApplyGUI(BasePlayer player, BaseEntity entity, bool showExtra)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Invalid comparison between Unknown and I4
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0520: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_056c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_059e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		//IL_0619: Unknown result type (might be due to invalid IL or missing references)
		//IL_0671: Unknown result type (might be due to invalid IL or missing references)
		//IL_0676: Unknown result type (might be due to invalid IL or missing references)
		//IL_067a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_069a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_074e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0779: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0723: Unknown result type (might be due to invalid IL or missing references)
		//IL_083f: Unknown result type (might be due to invalid IL or missing references)
		//IL_081f: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity))
		{
			ClearGUI(player);
			return;
		}
		using (CUI cUI = new CUI(Community.Runtime.Core.CuiHandler))
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			float offset = 0f;
			Vector2 coordinates = orCreateEditor.GetCoordinates();
			CuiElementContainer cuiElementContainer = cUI.CreateContainer("hammereditor.cui", Cache.CUI.BlackColor, coordinates.x, coordinates.x, coordinates.y, coordinates.y, -150f, 150f, 0f, 0f, 0f, 0f, showExtra, showExtra, CUI.ClientPanels.Hud, "hammereditor.cui");
			CuiElement cuiElement = cuiElementContainer[0];
			cuiElement.Components.Add(cachedDraggable);
			cachedDraggable.LimitToParent = true;
			cachedDraggable.ParentLimitIndex = 1;
			cachedDraggable.DragAlpha = 0.5f;
			cachedDraggable.PositionRPC = (DraggablePositionSendType)0;
			NetworkableId val = (NetworkableId)(BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) ? ((BaseNetworkable)entity).net.ID : default(NetworkableId));
			CreateText(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, (CanBeMoved(player, entity) ? "<color=green><b>✓</b></color>" : "<color=red><b>✘</b></color>") + " Use <color=white>RIGHT-CLICK</color> to move the entity (hold <color=white>SPRINT</color> to skip auto-snapping)\n" + (CanBeToggled(entity) ? "<color=green><b>✓</b></color>" : "<color=red><b>✘</b></color>") + " Use <color=white>MIDDLE-CLICK</color> to toggle the entity (hold <color=white>SPRINT</color> to lock/unlock)");
			if (showExtra || orCreateEditor.destructionMode)
			{
				CreateButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Destruction Mode", 3, orCreateEditor.destructionMode ? "#8bb52a" : ".9 .2 .3 .4");
			}
			BasePlayer val2 = (BasePlayer)(object)((entity is BasePlayer) ? entity : null);
			if (val2 == null || !val2.userID.IsSteamId())
			{
				CreateButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Destroy Entity", 1);
			}
			for (int i = 0; i < CustomButons.Count; i++)
			{
				(string, string, string, bool) tuple = CustomButons[i](entity, showExtra);
				if (tuple.Item4)
				{
					CreateCustomButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, tuple.Item1, tuple.Item3, tuple.Item2 ?? ".9 .2 .3 .9");
				}
			}
			for (int j = 0; j < CustomFields.Count; j++)
			{
				(string, object, bool) tuple2 = CustomFields[j](entity, showExtra);
				if (tuple2.Item3)
				{
					CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, tuple2.Item1, tuple2.Item2);
				}
			}
			if (showExtra)
			{
				BaseLock obj = entity.GetLock();
				CodeLock val3 = (CodeLock)(object)((obj is CodeLock) ? obj : null);
				if (val3 != null)
				{
					if (val3.hasGuestCode)
					{
						CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Guest Code", val3.guestCode);
					}
					CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Code", val3.code);
				}
			}
			ModularCar val4 = null;
			SleepingBag val5 = (SleepingBag)(object)((entity is SleepingBag) ? entity : null);
			if (val5 == null)
			{
				MiningQuarry val6 = (MiningQuarry)(object)((entity is MiningQuarry) ? entity : null);
				if (val6 == null)
				{
					IOEntity val7 = (IOEntity)(object)((entity is IOEntity) ? entity : null);
					if (val7 == null)
					{
						SteeringWheel val8 = (SteeringWheel)(object)((entity is SteeringWheel) ? entity : null);
						if (val8 == null)
						{
							if (!(entity is PlanterBox))
							{
								VehicleModuleEngine val9 = (VehicleModuleEngine)(object)((entity is VehicleModuleEngine) ? entity : null);
								if (val9 == null)
								{
									ModularCar val10 = (ModularCar)(object)((entity is ModularCar) ? entity : null);
									if (val10 != null)
									{
										val4 = val10;
									}
								}
								else
								{
									val4 = ((VehicleModuleSeating)val9).Car;
								}
							}
							else
							{
								CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Temperature", $"{entity.currentTemperature:0}°C / {CelsiusToFahrenheit(entity.currentTemperature):0}°F");
							}
						}
						else if (showExtra)
						{
							CUI cui = cUI;
							string name = cuiElementContainer.Name;
							PlayerBoatLock boatLock = val8.BoatLock;
							CreateOption(cui, cuiElementContainer, name, ref offset, val, "Code", (boatLock != null) ? boatLock.Code : null);
						}
					}
					else
					{
						CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Power", val7.currentEnergy.ToString("0"));
					}
				}
				else
				{
					CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Static Type", val6.staticType);
				}
			}
			else
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Assigned To", ((object)BasePlayer.FindAwakeOrSleepingByID(val5.deployerUserID))?.ToString() ?? val5.deployerUserID.ToString());
			}
			if (showExtra && BaseNetworkableEx.IsValid((BaseNetworkable)(object)val4) && val4.CarLock.HasALock)
			{
				if (val4.CarLock.whitelistPlayers.Count > 0)
				{
					ulong num = val4.CarLock.whitelistPlayers[0];
					CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Lock Owner ID", num);
					BasePlayer val11 = BasePlayer.FindAwakeOrSleepingByID(num);
					if (val11 != null && BaseNetworkableEx.IsValid((BaseNetworkable)(object)val11))
					{
						CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Lock Owner", val11.displayName);
					}
				}
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Code", val4.CarLock.Code);
			}
			if (entity == null || (int)entity.flags > 0)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Flags", entity?.flags);
			}
			if (entity == null || entity.skinID != 0)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Skin ID", entity?.skinID);
			}
			Vector3? val12 = ((entity != null) ? new Vector3?(((Component)entity).transform.localScale) : ((Vector3?)null));
			Vector3 one = Vector3.one;
			if (!val12.HasValue || val12.GetValueOrDefault() != one)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Scale", (entity != null) ? new Vector3?(((Component)entity).transform.localScale) : ((Vector3?)null));
			}
			CUI cui2 = cUI;
			string name2 = cuiElementContainer.Name;
			Vector3? val13;
			if (entity == null)
			{
				val13 = null;
			}
			else
			{
				Quaternion rotation = ((Component)entity).transform.rotation;
				val13 = ((Quaternion)(ref rotation)).eulerAngles;
			}
			CreateOption(cui2, cuiElementContainer, name2, ref offset, val, "Rotation", val13);
			CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Position", (entity != null) ? new Vector3?(((Component)entity).transform.position) : ((Vector3?)null));
			if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && entity.OwnerID != 0L)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Owner ID", entity.OwnerID);
				BasePlayer val14 = BasePlayer.FindAwakeOrSleepingByID(entity.OwnerID);
				if (val14 != null && BaseNetworkableEx.IsValid((BaseNetworkable)(object)val14))
				{
					CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Owner", val14.displayName);
				}
			}
			BasePlayer val15 = (BasePlayer)(object)((entity is BasePlayer) ? entity : null);
			if (val15 != null)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Display Name", val15.displayName);
			}
			BuildingBlock val16 = (BuildingBlock)(object)((entity is BuildingBlock) ? entity : null);
			if (val16 != null)
			{
				CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Building ID", ((DecayEntity)val16).buildingID);
				if (showExtra)
				{
					CreateButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, $"Destroy Building ({((DecayEntity)val16).GetBuilding().decayEntities.Count:n0} entities)", 2);
				}
			}
			CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "NetID", val);
			CreateOption(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Target", (entity != null) ? ((BaseNetworkable)entity).ShortPrefabName : null);
			if (!showExtra)
			{
				CreateButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Show extra settings", 0, "#8cbf1d");
			}
			else
			{
				CreateButton(cUI, cuiElementContainer, cuiElementContainer.Name, ref offset, val, "Show fewer settings", 0);
			}
			cUI.Send(cuiElementContainer, player);
		}
		static void CreateButton(CUI cUI2, CuiElementContainer container, string panel, ref float reference, NetworkableId id, string text, int optionId, string color = ".9 .2 .3 .9")
		{
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			CUI.Pair<string, CuiElement> pair = cUI2.CreatePanel(container, panel, ".1 .1 .1 .3", null, 0f, 1f, 0f, 1f, 0f, 0f, -10f + reference, 10f + reference, blur: true);
			cUI2.CreateProtectedButton(container, pair, color, Cache.CUI.WhiteColor, text.ToUpperInvariant(), 9, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, $"ezeditor.editoption {optionId} {id}", (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold);
			reference += 22.5f;
		}
		static void CreateCustomButton(CUI cUI2, CuiElementContainer container, string panel, ref float reference, string text, string command, string color = ".9 .2 .3 .9")
		{
			CUI.Pair<string, CuiElement> pair = cUI2.CreatePanel(container, panel, ".1 .1 .1 .3", null, 0f, 1f, 0f, 1f, 0f, 0f, -10f + reference, 10f + reference, blur: true);
			cUI2.CreateProtectedButton(container, pair, color, Cache.CUI.WhiteColor, text.ToUpperInvariant(), 8, null, 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, command, (TextAnchor)4, CUI.Handler.FontTypes.RobotoCondensedBold);
			reference += 22.5f;
		}
		static void CreateOption(CUI cUI2, CuiElementContainer container, string panel, ref float reference, NetworkableId id, string text, object value)
		{
			CUI.Pair<string, CuiElement> pair = cUI2.CreatePanel(container, panel, ".1 .1 .1 .3", null, 0f, 1f, 0f, 1f, 0f, 0f, -10f + reference, 10f + reference, blur: true);
			cUI2.CreateText(container, pair, "1 1 1 .5", text, 10, 0f, 0.25f, 0f, 1f, 0f, 0f, 0f, 0f, (TextAnchor)5, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			CUI.Pair<string, CuiElement> pair2 = cUI2.CreatePanel(container, pair, "0 0 0 .5", null, 0.28f);
			cUI2.CreateProtectedInputField(container, pair2, Cache.CUI.WhiteColor, value?.ToString() ?? "undefined", 10, 0, readOnly: true, 0f, 1f, 0f, 1f, 7.5f, 0f, 0f, 0f, null, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, autoFocus: false, hudMenuInput: false, (LineType)0);
			reference += 22.5f;
		}
		static void CreateText(CUI cUI2, CuiElementContainer container, string panel, ref float reference, string text)
		{
			CUI.Pair<string, CuiElement> pair = cUI2.CreatePanel(container, panel, ".1 .1 .1 .3", null, 0f, 1f, 0f, 1f, 0f, 0f, -12.5f + reference, 12.5f + reference, blur: true);
			cUI2.CreateText(container, pair, "1 1 1 .4", text, 8, 0f, 1f, 0f, 1f, 10f, 0f, 0f, 0f, (TextAnchor)3, CUI.Handler.FontTypes.RobotoCondensedRegular, (VerticalWrapMode)1);
			reference += 25f;
		}
	}

	public void ClearGUI(BasePlayer player)
	{
		using CUI cUI = new CUI(Community.Runtime.Core.CuiHandler);
		cUI.Destroy("hammereditor.cui", player);
	}

	private unsafe void OnPlayerInput(BasePlayer player, InputState state)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Invalid comparison between Unknown and I4
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		if (lastCreativeModePlayers.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), out var value) && state.WasJustPressed((BUTTON)134217728) && CanBeToggled(value))
		{
			bool flag = state.IsDown((BUTTON)128);
			Flags val = (Flags)(flag ? 16 : 8);
			Flags val2 = (Flags)(flag ? 16 : 2);
			if (flag)
			{
				BaseLock val3 = value.GetLock();
				if (val3 != null)
				{
					FlagsUpdateScope val4 = ((BaseEntity)val3).StartSetFlags((FlagsUpdateMode)2);
					try
					{
						((FlagsUpdateScope)(ref val4)).Set((Flags)16, !((BaseEntity)val3).IsLocked(), false);
						return;
					}
					finally
					{
						((IDisposable)(*(FlagsUpdateScope*)(&val4))/*cast due to constrained. prefix*/).Dispose();
					}
				}
			}
			VendingMachine val5 = (VendingMachine)(object)((value is VendingMachine) ? value : null);
			if (val5 == null)
			{
				BuildingBlock val6 = (BuildingBlock)(object)((value is BuildingBlock) ? value : null);
				if (val6 == null)
				{
					if (!(value is Candle) && !(value is SteeringWheel) && !(value is Door))
					{
						if (!(value is IOEntity))
						{
							if (!(value is StorageContainer))
							{
								if (!(value is EngineSwitch))
								{
									MiningQuarry val7 = (MiningQuarry)(object)((value is MiningQuarry) ? value : null);
									if (val7 != null)
									{
										val7.staticType = (QuarryType)(val7.staticType + 1);
										if ((int)val7.staticType > 3)
										{
											val7.staticType = (QuarryType)0;
										}
										val7.UpdateStaticDeposit();
									}
								}
								else
								{
									BaseEntity parentEntity = ((BaseNetworkable)value).GetParentEntity();
									MiningQuarry val8 = (MiningQuarry)(object)((parentEntity is MiningQuarry) ? parentEntity : null);
									if (val8 != null)
									{
										val8.EngineSwitch(!((BaseEntity)val8).IsOn());
									}
								}
							}
							else
							{
								FlagsUpdateScope val9 = value.StartSetFlags((FlagsUpdateMode)2);
								try
								{
									((FlagsUpdateScope)(ref val9)).Set(val2, !value.HasFlag(val2), false);
								}
								finally
								{
									((IDisposable)(*(FlagsUpdateScope*)(&val9))/*cast due to constrained. prefix*/).Dispose();
								}
							}
						}
						else
						{
							bool flag2 = value.HasFlag(val2);
							FlagsUpdateScope val10 = value.StartSetFlags((FlagsUpdateMode)2);
							try
							{
								((FlagsUpdateScope)(ref val10)).Set(val2, !flag2, false);
								((FlagsUpdateScope)(ref val10)).Set((Flags)65536, !flag2, false);
							}
							finally
							{
								((IDisposable)(*(FlagsUpdateScope*)(&val10))/*cast due to constrained. prefix*/).Dispose();
							}
						}
					}
					else
					{
						FlagsUpdateScope val11 = value.StartSetFlags((FlagsUpdateMode)2);
						try
						{
							((FlagsUpdateScope)(ref val11)).Set(val, !value.HasFlag(val), false);
						}
						finally
						{
							((IDisposable)(*(FlagsUpdateScope*)(&val11))/*cast due to constrained. prefix*/).Dispose();
						}
					}
				}
				else if ((PrefabAttribute)(object)val6.blockDefinition != (PrefabAttribute)null && val6.blockDefinition.canRotateAfterPlacement)
				{
					Transform transform = ((Component)val6).transform;
					transform.localRotation *= Quaternion.Euler(val6.blockDefinition.rotationAmount);
					((BaseEntity)val6).RefreshEntityLinks();
					((StabilityEntity)val6).UpdateSurroundingEntities();
					val6.UpdateSkin(true);
					val6.RefreshNeighbours(false);
					((BaseNetworkable)val6).SendNetworkUpdateImmediate();
					((BaseEntity)val6).ClientRPC(RpcTarget.NetworkGroup("RefreshSkin"));
					if (!val6.globalNetworkCooldown)
					{
						val6.globalNetworkCooldown = true;
						GlobalNetworkHandler.server.TrySendNetworkUpdate((BaseNetworkable)(object)val6);
						((FacepunchBehaviour)val6).CancelInvoke((Action)val6.ResetGlobalNetworkCooldown);
						((FacepunchBehaviour)val6).Invoke((Action)val6.ResetGlobalNetworkCooldown, 15f);
					}
				}
			}
			else if (val5.CanRotate())
			{
				((Component)value).transform.rotation = Quaternion.LookRotation(-((Component)value).transform.forward, ((Component)value).transform.up);
				((BaseNetworkable)value).SendNetworkUpdate((NetworkQueue)0);
			}
			lastCreativeModePlayers.Remove(EncryptedValue<ulong>.op_Implicit(player.userID));
		}
		if (state.WasJustPressed((BUTTON)2048) && player.Connection.authLevel >= MinimumAuthLevel)
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			if (orCreateEditor.isMovingEntity)
			{
				orCreateEditor.isMovingEntity = false;
				lastCreativeModePlayers.Remove(EncryptedValue<ulong>.op_Implicit(player.userID));
			}
			else if (CanBeMoved(player, value))
			{
				orCreateEditor.isMovingEntity = true;
				lastCreativeModePlayers.Remove(EncryptedValue<ulong>.op_Implicit(player.userID));
				((MonoBehaviour)player).StartCoroutine(MoveEntityRoutine(base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID)), value));
			}
		}
	}

	private object OnHammerHit(BasePlayer player, HitInfo info)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (player.Connection.authLevel < MinimumAuthLevel)
		{
			return null;
		}
		HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
		if ((!player.IsInCreativeMode && !orCreateEditor.bypassCreativeMode) || info == null)
		{
			return null;
		}
		if (orCreateEditor.destructionMode)
		{
			BaseEntity hitEntity = info.HitEntity;
			if (hitEntity != null && (CanBeMoved(player, hitEntity) || CanBeToggled(hitEntity)))
			{
				((BaseNetworkable)hitEntity).Kill((DestroyMode)1, true);
				return Cache.False;
			}
		}
		BaseEntity hitEntity2 = info.HitEntity;
		BaseEntity obj = ((hitEntity2 != null) ? ((BaseNetworkable)hitEntity2).GetParentEntity() : null);
		PlayerBoat val = (PlayerBoat)(object)((obj is PlayerBoat) ? obj : null);
		if (val != null)
		{
			((BaseCombatEntity)val).Heal(float.MaxValue);
			return Cache.False;
		}
		BaseEntity hitEntity3 = info.HitEntity;
		BuildingBlock val2 = (BuildingBlock)(object)((hitEntity3 is BuildingBlock) ? hitEntity3 : null);
		if (val2 != null)
		{
			Building building = ((DecayEntity)val2).GetBuilding();
			if (building != null && !orCreateEditor.isRepairingOrDestroyingBuilding)
			{
				PooledList<BaseCombatEntity> val3 = Pool.Get<PooledList<BaseCombatEntity>>();
				((List<BaseCombatEntity>)(object)val3).AddRange((IEnumerable<BaseCombatEntity>)building.decayEntities);
				((MonoBehaviour)player).StartCoroutine(RepairEntitiesOverTime(orCreateEditor, (List<BaseCombatEntity>)(object)val3));
				return Cache.False;
			}
		}
		return null;
	}

	private void OnActiveItemChanged(BasePlayer player, Item oldItem)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) || !player.IsConnected || player.Connection.authLevel < MinimumAuthLevel)
		{
			return;
		}
		HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
		bool bypassHammer = orCreateEditor.bypassHammer;
		bool flag = bypassHammer;
		if (!flag)
		{
			bool flag2 = oldItem != null;
			bool flag3 = flag2;
			if (flag3)
			{
				int itemid = oldItem.info.itemid;
				bool flag4 = ((itemid == 200773292 || itemid == 1803831286) ? true : false);
				flag3 = flag4;
			}
			flag = flag3;
		}
		if (flag)
		{
			orCreateEditor.Reset();
			ClearGUI(player);
		}
	}

	private void OnPlayerSleep(BasePlayer player)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && player.IsConnected && player.Connection.authLevel >= MinimumAuthLevel)
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			orCreateEditor.Reset();
			ClearGUI(player);
		}
	}

	private void OnPlayerDeath(BasePlayer player)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (player.IsConnected && player.Connection.authLevel >= MinimumAuthLevel)
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			orCreateEditor.Reset();
			ClearGUI(player);
		}
	}

	private void OnPlayerConnected(BasePlayer player)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && player.Connection != null && player.Connection.authLevel < MinimumAuthLevel)
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			orCreateEditor.bypassCreativeMode = false;
			orCreateEditor.bypassHammer = false;
			orCreateEditor.bypassImmovableEntityDestroyConfirmations = false;
		}
	}

	private void OnPlayerDisconnected(BasePlayer player)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && player.Connection.authLevel >= MinimumAuthLevel)
		{
			HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID));
			orCreateEditor.Reset();
		}
	}

	private void OnCuiDraggableDrag(BasePlayer player, string name, Vector3 position, DraggablePositionSendType type)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (name.Equals("hammereditor.cui") && (int)type == 1)
		{
			base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(player.userID)).SetCoordinates(Vector2.op_Implicit(position));
		}
	}

	public static float CelsiusToFahrenheit(float celsius)
	{
		return celsius * 9f / 5f + 32f;
	}

	[ProtectedCommand("ezeditor.editoption")]
	private void EditOption(Arg arg)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer val = ArgEx.Player(arg);
		HammerEditor editor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(val.userID));
		int num = arg.GetInt(0, 0);
		BaseNetworkable obj = BaseNetworkable.serverEntities.Find(ArgEx.GetEntityID(arg, 1, default(NetworkableId)));
		BaseEntity entity = (BaseEntity)(object)((obj is BaseEntity) ? obj : null);
		if (num != 0 && !BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity))
		{
			val.ChatMessage("Entity is now invalid");
			return;
		}
		switch (num)
		{
		case 0:
		{
			editor.showExtra = !editor.showExtra;
			if (ShouldShowUI(editor, out var _))
			{
				ApplyGUI(val, entity, showExtra: true);
			}
			else
			{
				ClearGUI(val);
			}
			break;
		}
		case 1:
			if (CanBeMoved(val, entity) || entity is BuildingBlock)
			{
				((BaseNetworkable)entity).Kill((DestroyMode)1, true);
				editor.showExtra = false;
				ClearGUI(val);
				break;
			}
			if (editor.bypassImmovableEntityDestroyConfirmations)
			{
				((BaseNetworkable)entity).Kill((DestroyMode)1, true);
				editor.showExtra = false;
				ClearGUI(val);
				break;
			}
			Modal.Open(val, "Are you sure you wanna destroy that entity?", temp, delegate(BasePlayer player, ModalModule.Modal modal)
			{
				((BaseNetworkable)entity).Kill((DestroyMode)1, true);
				editor.showExtra = false;
				ClearGUI(player);
			});
			break;
		case 2:
		{
			BaseEntity obj2 = entity;
			BuildingBlock block = (BuildingBlock)(object)((obj2 is BuildingBlock) ? obj2 : null);
			if (block != null && !editor.isRepairingOrDestroyingBuilding)
			{
				Modal.Open(val, "Are you sure you wanna destroy that building?", temp, delegate(BasePlayer player, ModalModule.Modal modal)
				{
					PooledList<BaseEntity> val2 = Pool.Get<PooledList<BaseEntity>>();
					((List<BaseEntity>)(object)val2).AddRange((IEnumerable<BaseEntity>)((DecayEntity)block).GetBuilding().decayEntities);
					((MonoBehaviour)player).StartCoroutine(DestroyEntitiesOverTime(editor, (List<BaseEntity>)(object)val2));
					editor.showExtra = false;
					ClearGUI(player);
				});
			}
			break;
		}
		case 3:
			editor.destructionMode = !editor.destructionMode;
			editor.showExtra = false;
			ClearGUI(val);
			break;
		}
	}

	private IEnumerator DestroyEntitiesOverTime(HammerEditor editor, List<BaseEntity> entities)
	{
		BasePlayer player = editor.GetPlayer();
		editor.isRepairingOrDestroyingBuilding = true;
		int num = 0;
		int completedEntities = 0;
		int completedEntitiesDead = 0;
		bool wasCancelled = false;
		player.ShowToast((Styles)1, destroyingBuildingPhrase, false, new string[3]
		{
			"1",
			entities.Count.ToString("n0"),
			completedEntitiesDead.ToString("n0")
		});
		for (int i = 0; i < entities.Count; i++)
		{
			if (!editor.isRepairingOrDestroyingBuilding)
			{
				wasCancelled = true;
				break;
			}
			if (num > BuildingDestroyBatch)
			{
				yield return null;
				yield return null;
				yield return CoroutineEx.waitForSeconds(BuildingBatchRefreshRate);
				player.ShowToast((Styles)1, destroyingBuildingPhrase, false, new string[3]
				{
					(i + 1).ToString("n0"),
					entities.Count.ToString("n0"),
					completedEntitiesDead.ToString("n0")
				});
				num = 0;
			}
			BaseEntity val = entities[i];
			if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
			{
				((BaseNetworkable)val).Kill((DestroyMode)0, true);
				num++;
				completedEntities++;
			}
			else
			{
				completedEntitiesDead++;
			}
		}
		if (wasCancelled)
		{
			player.ShowToast((Styles)1, destroyingBuildingCancelledPhrase, false, Array.Empty<string>());
		}
		else
		{
			player.ShowToast((Styles)1, destroyingBuildingPhrase, false, new string[3]
			{
				completedEntities.ToString("n0"),
				entities.Count.ToString("n0"),
				completedEntitiesDead.ToString("n0")
			});
		}
		Pool.FreeUnmanaged<BaseEntity>(ref entities);
		editor.isRepairingOrDestroyingBuilding = false;
	}

	private IEnumerator RepairEntitiesOverTime(HammerEditor editor, List<BaseCombatEntity> entities)
	{
		BasePlayer player = editor.GetPlayer();
		editor.isRepairingOrDestroyingBuilding = true;
		int currentBatch = 0;
		int completedEntities = 0;
		int completedEntitiesDead = 0;
		int completedEntitiesNeededRepair = 0;
		bool wasCancelled = false;
		player.ShowToast((Styles)0, repairedPhrase, false, new string[4]
		{
			"1",
			entities.Count.ToString("n0"),
			completedEntitiesNeededRepair.ToString("n0"),
			completedEntitiesDead.ToString("n0")
		});
		for (int i = 0; i < entities.Count; i++)
		{
			if (!editor.isRepairingOrDestroyingBuilding)
			{
				wasCancelled = true;
				break;
			}
			if (currentBatch > BuildingRepairBatch)
			{
				currentBatch = 0;
				player.ShowToast((Styles)0, repairedPhrase, false, new string[4]
				{
					(i + 1).ToString("n0"),
					entities.Count.ToString("n0"),
					completedEntitiesNeededRepair.ToString("n0"),
					completedEntitiesDead.ToString("n0")
				});
				yield return CoroutineEx.waitForSeconds(BuildingBatchRefreshRate);
			}
			BaseCombatEntity val = entities[i];
			if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
			{
				if (!Mathf.Approximately(val.healthFraction, 1f))
				{
					completedEntitiesNeededRepair++;
					val.Heal(float.MaxValue);
				}
				currentBatch++;
				completedEntities++;
				yield return null;
			}
			else
			{
				completedEntitiesDead++;
			}
		}
		if (wasCancelled)
		{
			player.ShowToast((Styles)1, repairedCancelledPhrase, false, Array.Empty<string>());
		}
		else
		{
			player.ShowToast((Styles)0, repairedPhrase, false, new string[4]
			{
				entities.Count.ToString("n0"),
				entities.Count.ToString("n0"),
				completedEntitiesNeededRepair.ToString("n0"),
				completedEntitiesDead.ToString("n0")
			});
		}
		Pool.FreeUnmanaged<BaseCombatEntity>(ref entities);
		editor.isRepairingOrDestroyingBuilding = false;
	}

	private unsafe IEnumerator MoveEntityRoutine(HammerEditor editor, BaseEntity entity)
	{
		int layer = 1218652417;
		if (editor.waterLayer)
		{
			layer += 16;
		}
		BasePlayer player = editor.GetPlayer();
		Vector3 rotation = Vector3.up * 180f;
		List<RaycastHit> hits = Pool.Get<List<RaycastHit>>();
		bool hasContact = true;
		Rigidbody rigidbody = ((Component)entity).GetComponent<Rigidbody>() ?? ((Component)entity).GetComponentInChildren<Rigidbody>() ?? ((Component)entity).GetComponentInParent<Rigidbody>();
		bool? wasKinematic = ((rigidbody != null) ? new bool?(rigidbody.isKinematic) : ((bool?)null));
		if (rigidbody != null)
		{
			rigidbody.isKinematic = true;
		}
		if (entity is BaseHelicopter)
		{
			FlagsUpdateScope val = entity.StartSetFlags((FlagsUpdateMode)2);
			try
			{
				((FlagsUpdateScope)(ref val)).Set((Flags)16777216, true, false);
			}
			finally
			{
				((IDisposable)(*(FlagsUpdateScope*)(&val))/*cast due to constrained. prefix*/).Dispose();
			}
		}
		if (entity is RidableHorse)
		{
			FlagsUpdateScope val2 = entity.StartSetFlags((FlagsUpdateMode)2);
			try
			{
				((FlagsUpdateScope)(ref val2)).Set((Flags)2097152, true, false);
			}
			finally
			{
				((IDisposable)(*(FlagsUpdateScope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
			}
		}
		ClearGUI(player);
		RaycastHit hit = default(RaycastHit);
		if (entity != null)
		{
			entity.SetParent((BaseEntity)null, true, false);
		}
		Transform transform = ((entity != null) ? ((Component)entity).transform : null);
		Quaternion lookRotation;
		while (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && editor.isMovingEntity && !player.IsSleeping())
		{
			hits.Clear();
			hit = default(RaycastHit);
			GamePhysics.TraceAll(player.eyes.HeadRay(), 0f, hits, editor.moveDistance, layer, (QueryTriggerInteraction)1, (BaseEntity)null);
			for (int i = 0; i < hits.Count; i++)
			{
				RaycastHit val3 = hits[i];
				BaseEntity entity2 = RaycastHitEx.GetEntity(val3);
				if (!((Object)(object)entity2 == (Object)(object)entity) && !BaseEntityEx.HasEntityInParents(entity2, entity))
				{
					hit = val3;
					break;
				}
			}
			hasContact = ((RaycastHit)(ref hit)).point != Vector3.zero;
			if (!hasContact)
			{
				((RaycastHit)(ref hit)).point = player.eyes.position + player.eyes.HeadForward() * editor.moveDistance;
			}
			if (player.serverInput.WasJustPressed((BUTTON)8192))
			{
				rotation += Vector3.up * 90f;
				player.serverInput.SwallowButton((BUTTON)8192);
			}
			float num = Time.deltaTime * MoveLerp;
			transform.position = Vector3.Lerp(transform.position, ((RaycastHit)(ref hit)).point, num);
			Quaternion localRotation = transform.localRotation;
			Quaternion val4 = Quaternion.FromToRotation(Vector3.up, ((RaycastHit)(ref hit)).normal) * Quaternion.Euler(rotation);
			lookRotation = player.eyes.GetLookRotation();
			transform.localRotation = Quaternion.Slerp(localRotation, val4 * Quaternion.Euler(Vector3Ex.WithX(((Quaternion)(ref lookRotation)).eulerAngles, 0f)), num);
			((BaseNetworkable)entity).SendNetworkUpdate_Position();
			yield return null;
		}
		if (!hasContact && BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && !player.serverInput.IsDown((BUTTON)128))
		{
			RaycastHit hit2 = default(RaycastHit);
			GamePhysics.Trace(new Ray(transform.position, Vector3.down), 0f, ref hit2, float.MaxValue, layer, (QueryTriggerInteraction)1, (BaseEntity)null);
			Vector3 targetPosition = ((RaycastHit)(ref hit2)).point;
			Quaternion val5 = Quaternion.FromToRotation(Vector3.up, ((RaycastHit)(ref hit2)).normal) * Quaternion.Euler(rotation);
			lookRotation = player.eyes.GetLookRotation();
			Quaternion targetRotation = val5 * Quaternion.Euler(Vector3Ex.WithX(((Quaternion)(ref lookRotation)).eulerAngles, 0f));
			if (targetPosition != Vector3.zero)
			{
				float currentTime = 0f;
				while (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && currentTime <= 0.75f)
				{
					currentTime += Time.deltaTime;
					float num2 = currentTime.Scale(0f, 0.75f, 0f, 1f);
					transform.position = Vector3.Lerp(transform.position, targetPosition, num2);
					transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, num2);
					((BaseNetworkable)entity).SendNetworkUpdate_Position();
					yield return null;
				}
				BaseEntity entity3 = RaycastHitEx.GetEntity(hit2);
				if (entity3 != null && (Object)(object)entity3 != (Object)(object)entity && !(entity3 is BasePlayer) && !(entity is BasePlayer))
				{
					if (entity != null)
					{
						entity.SetParent(entity3, true, false);
					}
				}
			}
		}
		else if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity))
		{
			BaseEntity entity4 = RaycastHitEx.GetEntity(hit);
			if (entity4 != null && (Object)(object)entity4 != (Object)(object)entity && !(entity is BasePlayer) && !(entity4 is BasePlayer))
			{
				entity.SetParent(entity4, true, false);
			}
		}
		ClearGUI(player);
		editor.isMovingEntity = false;
		if ((Object)(object)rigidbody != (Object)null)
		{
			rigidbody.isKinematic = wasKinematic == true;
			((Component)rigidbody).transform.hasChanged = true;
			rigidbody.WakeUp();
		}
		BaseBoat val6 = (BaseBoat)(object)((entity is BaseBoat) ? entity : null);
		if (val6 != null)
		{
			((BaseVehicle)val6).OnServerWake();
		}
		Pool.FreeUnmanaged<RaycastHit>(ref hits);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && entity is BaseHelicopter)
		{
			FlagsUpdateScope val7 = entity.StartSetFlags((FlagsUpdateMode)2);
			try
			{
				((FlagsUpdateScope)(ref val7)).Set((Flags)16777216, false, false);
			}
			finally
			{
				((IDisposable)(*(FlagsUpdateScope*)(&val7))/*cast due to constrained. prefix*/).Dispose();
			}
		}
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && entity is RidableHorse)
		{
			FlagsUpdateScope val8 = entity.StartSetFlags((FlagsUpdateMode)2);
			try
			{
				((FlagsUpdateScope)(ref val8)).Set((Flags)2097152, false, false);
			}
			finally
			{
				((IDisposable)(*(FlagsUpdateScope*)(&val8))/*cast due to constrained. prefix*/).Dispose();
			}
		}
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity) && !(entity is BaseCorpse) && !BaseEntityEx.HasEntityInParents(entity, (BaseEntity)(object)player) && !BaseEntityEx.HasEntityInParents((BaseEntity)(object)player, entity))
		{
			ReconstructEntity(entity);
		}
	}

	private void ReconstructEntity(BaseEntity entity)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)entity))
		{
			return;
		}
		entity.networkEntityScale = ((Component)entity).transform.localScale != Vector3.one;
		for (int i = 0; i < ((BaseNetworkable)entity).net.group.subscribers.Count; i++)
		{
			entity.DestroyOnClient(((BaseNetworkable)entity).net.group.subscribers[i]);
		}
		if (((BaseNetworkable)entity).children != null)
		{
			for (int j = 0; j < ((BaseNetworkable)entity).children.Count; j++)
			{
				ReconstructEntity(((BaseNetworkable)entity).children[j]);
			}
		}
		((BaseNetworkable)entity).SendNetworkUpdateImmediate();
	}

	[ConsoleCommand("hammer", "Player-specific configuration editing for the Hammer UI and its behaviour")]
	public void Hammer(Arg arg)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer val = ArgEx.Player(arg);
		if ((Object)(object)val == (Object)null)
		{
			arg.ReplyWith("Command must be called from a client");
			return;
		}
		if (val.Connection.authLevel < MinimumAuthLevel)
		{
			arg.ReplyWith("Low auth level");
			return;
		}
		HammerEditor orCreateEditor = base.DataInstance.GetOrCreateEditor(EncryptedValue<ulong>.op_Implicit(val.userID));
		string text = arg.GetString(0, "");
		bool flag = true;
		object arg2 = null;
		switch (text)
		{
		case "uidistance":
			arg2 = (orCreateEditor.uiDistance = arg.GetFloat(1, orCreateEditor.uiDistance));
			break;
		case "movedistance":
			arg2 = (orCreateEditor.moveDistance = arg.GetFloat(1, orCreateEditor.moveDistance));
			break;
		case "waterlayer":
			arg2 = (orCreateEditor.waterLayer = arg.GetBool(1, orCreateEditor.waterLayer));
			break;
		case "creativebypass":
			arg2 = (orCreateEditor.bypassCreativeMode = arg.GetBool(1, orCreateEditor.bypassCreativeMode));
			break;
		case "hammerbypass":
			arg2 = (orCreateEditor.bypassHammer = arg.GetBool(1, orCreateEditor.bypassHammer));
			ValidatePermanentPlayerInputHook();
			break;
		case "bypassimmovableentitydestroyconfirmations":
			arg2 = (orCreateEditor.bypassImmovableEntityDestroyConfirmations = arg.GetBool(1, orCreateEditor.bypassImmovableEntityDestroyConfirmations));
			break;
		case "resetui":
			orCreateEditor.x = base.ConfigInstance.UIDefaultX;
			orCreateEditor.y = base.ConfigInstance.UIDefaultY;
			orCreateEditor.Reset();
			arg2 = "Hammer UI has been reset";
			break;
		default:
		{
			StringTable stringTable = new StringTable("option", "value", "help");
			try
			{
				stringTable.AddRow("resetui", null, "Resets the position of the UI, in case it's stuck somehow, even though it shouldn't");
				stringTable.AddRow("uidistance", orCreateEditor.uiDistance, "Minimum distance from the player to the entity to show the Hammer UI");
				stringTable.AddRow("movedistance", orCreateEditor.moveDistance, "Distance the entity will float in front of the player if not connecting to a surface");
				stringTable.AddRow("waterlayer", orCreateEditor.waterLayer, "Should the water layer of the ocean be considered?");
				stringTable.AddRow("creativebypass", orCreateEditor.bypassCreativeMode, "Allow players to use the Hammer UI regardless if they're in creative mode or not");
				stringTable.AddRow("hammerbypass", orCreateEditor.bypassHammer, "Allow players to use the Hammer UI regardless if they're holding a hammer item");
				stringTable.AddRow("bypassimmovableentitydestroyconfirmations", orCreateEditor.waterLayer, "Should the confirmation popup happen when attempting to destroy an entity that can't be moved?");
				arg.ReplyWith("Invalid syntax!\n" + stringTable.Write(StringTable.FormatTypes.None));
				flag = false;
			}
			finally
			{
				((IDisposable)stringTable/*cast due to constrained. prefix*/).Dispose();
			}
			break;
		}
		}
		if (flag)
		{
			arg.ReplyWith($"Hammer config - {text}: {arg2}");
			Save();
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0573: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		object obj4 = ((num > 3) ? args[3] : null);
		try
		{
			switch (hook)
			{
			case 3176354530u:
			{
				bool flag = ((obj is HammerEditor || obj == null) ? true : false);
				bool flag4 = flag;
				HammerEditor editor2 = (flag4 ? ((HammerEditor)(obj ?? null)) : null);
				flag = ((obj2 is List<BaseEntity> || obj2 == null) ? true : false);
				bool flag5 = flag;
				List<BaseEntity> entities2 = (flag5 ? ((List<BaseEntity>)(obj2 ?? null)) : null);
				if (flag4 & flag5)
				{
					return DestroyEntitiesOverTime(editor2, entities2);
				}
				break;
			}
			case 3750013587u:
			{
				bool flag = ((obj is Arg || obj == null) ? true : false);
				bool flag10 = flag;
				Arg arg = ((!flag10) ? ((Arg)null) : ((Arg)(obj ?? null)));
				if (flag10)
				{
					EditOption(arg);
					return null;
				}
				break;
			}
			case 1994070204u:
			{
				bool flag = ((obj is HammerEditor || obj == null) ? true : false);
				bool flag16 = flag;
				HammerEditor editor3 = (flag16 ? ((HammerEditor)(obj ?? null)) : null);
				flag = ((obj2 is BaseEntity || obj2 == null) ? true : false);
				bool flag17 = flag;
				BaseEntity entity2 = ((!flag17) ? ((BaseEntity)null) : ((BaseEntity)(obj2 ?? null)));
				if (flag16 & flag17)
				{
					return MoveEntityRoutine(editor3, entity2);
				}
				break;
			}
			case 2268037981u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag22 = flag;
				BasePlayer player8 = ((!flag22) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is Item || obj2 == null) ? true : false);
				bool flag23 = flag;
				Item oldItem = ((!flag23) ? ((Item)null) : ((Item)(obj2 ?? null)));
				if (flag22 & flag23)
				{
					OnActiveItemChanged(player8, oldItem);
					return null;
				}
				break;
			}
			case 1614693435u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag18 = flag;
				BasePlayer player7 = ((!flag18) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag19 = flag;
				string name = (flag19 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is Vector3 || obj3 == null) ? true : false);
				bool flag20 = flag;
				Vector3 position = (flag20 ? ((Vector3)(obj3 ?? ((object)default(Vector3)))) : default(Vector3));
				flag = ((obj4 is DraggablePositionSendType || obj4 == null) ? true : false);
				bool flag21 = flag;
				DraggablePositionSendType type = (DraggablePositionSendType)(flag21 ? ((int)(DraggablePositionSendType)(obj4 ?? ((object)(DraggablePositionSendType)0))) : 0);
				if (flag18 & flag19 & flag20 & flag21)
				{
					OnCuiDraggableDrag(player7, name, position, type);
					return null;
				}
				break;
			}
			case 4229965862u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag14 = flag;
				BasePlayer player6 = ((!flag14) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is HitInfo || obj2 == null) ? true : false);
				bool flag15 = flag;
				HitInfo info = ((!flag15) ? ((HitInfo)null) : ((HitInfo)(obj2 ?? null)));
				if (flag14 & flag15)
				{
					return OnHammerHit(player6, info);
				}
				break;
			}
			case 2848347654u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag6 = flag;
				BasePlayer player = ((!flag6) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag6)
				{
					OnPlayerConnected(player);
					return null;
				}
				break;
			}
			case 3560982762u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag7 = flag;
				BasePlayer player2 = ((!flag7) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag7)
				{
					OnPlayerDeath(player2);
					return null;
				}
				break;
			}
			case 72085565u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag12 = flag;
				BasePlayer player5 = ((!flag12) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag12)
				{
					OnPlayerDisconnected(player5);
					return null;
				}
				break;
			}
			case 3411611961u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag8 = flag;
				BasePlayer player3 = ((!flag8) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is InputState || obj2 == null) ? true : false);
				bool flag9 = flag;
				InputState state = ((!flag9) ? ((InputState)null) : ((InputState)(obj2 ?? null)));
				if (flag8 & flag9)
				{
					OnPlayerInput(player3, state);
					return null;
				}
				break;
			}
			case 4058415132u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag11 = flag;
				BasePlayer player4 = ((!flag11) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag11)
				{
					OnPlayerSleep(player4);
					return null;
				}
				break;
			}
			case 3047062756u:
			{
				bool flag = ((obj is BaseEntity || obj == null) ? true : false);
				bool flag13 = flag;
				BaseEntity entity = ((!flag13) ? ((BaseEntity)null) : ((BaseEntity)(obj ?? null)));
				if (flag13)
				{
					ReconstructEntity(entity);
					return null;
				}
				break;
			}
			case 1897255792u:
			{
				bool flag = ((obj is HammerEditor || obj == null) ? true : false);
				bool flag2 = flag;
				HammerEditor editor = (flag2 ? ((HammerEditor)(obj ?? null)) : null);
				flag = ((obj2 is List<BaseCombatEntity> || obj2 == null) ? true : false);
				bool flag3 = flag;
				List<BaseCombatEntity> entities = (flag3 ? ((List<BaseCombatEntity>)(obj2 ?? null)) : null);
				if (flag2 & flag3)
				{
					return RepairEntitiesOverTime(editor, entities);
				}
				break;
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

	static HammerModule()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		destroyingBuildingCancelledPhrase = new Phrase("destroyedbuildingCancelled", "Destroying building: <color=white>cancelled</color>");
		destroyingBuildingPhrase = new Phrase("destroyedbuilding", "Destroying building: <color=white>{0}</color>/{1} entities ({2} dead)");
		repairedCancelledPhrase = new Phrase("repairedCancelled", "Repairing: <color=white>cancelled</color>");
		repairedPhrase = new Phrase("repaired", "Repairing: <color=white>{0}</color>/{1} entities ({2} needed repair, {3} dead)");
		lastCreativeModePlayers = new Dictionary<ulong, BaseEntity>();
		lastLastCreativeModePlayers = new Dictionary<ulong, BaseEntity>();
		temp = new Dictionary<string, ModalModule.Modal.Field>();
		cachedDraggable = new CuiDraggableComponent();
		isSubscribedToOnPlayerInput = true;
		blacklistedMovingPrefabs = new string[8] { "crudeoutput", "hopperoutput", "fuelstorage", "excavator_output_pile", "static", "caboose", "elevator", "mission" };
	}
}
