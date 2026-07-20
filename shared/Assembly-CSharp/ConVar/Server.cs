using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Development.Attributes;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Reports;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using Network.Relay;
using Rust;
using UnityEngine;

namespace ConVar;

[ResetStaticFields]
[Factory("server")]
public class Server : ConsoleSystem
{
	[ServerVar(Help = "(Generated) IP address the server binds to; leave empty to bind to all interfaces")]
	public static string ip = "";

	[ServerVar(Help = "(Generated) UDP port the server listens on for player connections")]
	public static int port = 28015;

	[ServerVar(Help = "(Generated) UDP port used for Steam server browser queries; uses the game port if set to 0")]
	public static int queryport = 0;

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Maximum number of players allowed on the server at the same time")]
	public static int maxplayers = 500;

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Server name displayed in the server browser")]
	public static string hostname = "My Untitled Rust Server";

	[ServerVar(Help = "(Generated) Unique identifier for this server instance; determines the subfolder used for saves, configs and map data")]
	public static string identity = "my_server_identity";

	[ServerVar(Help = "(Generated) Override the root storage folder for server files; leave empty to use the default server/identity path")]
	public static string filefolderoverride = "";

	[ServerVar(Help = "(Generated) Map level to load on startup, e.g. 'Procedural Map', 'Barren', or a custom map name")]
	public static string level = "Procedural Map";

	[ServerVar(Help = "(Generated) URL to download a custom map file from; if set the server will fetch and load this map instead of generating one")]
	public static string levelurl = "";

	[ServerVar(Help = "(Generated) When true the server sends the map file to clients so they can load it without re-generating it locally")]
	public static bool leveltransfer = true;

	[ServerVar(Help = "(Generated) Seed value used for procedural world generation; changing this produces a completely different map layout")]
	public static int seed = 1337;

	[ServerVar(Help = "(Generated) Secondary salt value mixed into procedural world generation; used to vary monument and road placement")]
	public static int salt = 1;

	[ServerVar(Help = "(Generated) Width and height of the procedurally generated world in metres; larger values create a bigger map with more resources")]
	public static int worldsize = 4500;

	[ServerVar(Help = "(Generated) How often (in seconds) the server automatically saves the game world to disk; default is every 600 seconds (10 minutes)")]
	public static int saveinterval = 600;

	[ServerVar(Help = "(Generated) Network encryption mode; 2 = enabled (recommended), 0 = disabled")]
	public static int encryption = 2;

	[ServerVar(Help = "(Generated) Easy Anti-Cheat product ID used to authenticate this server with EAC services")]
	public static string anticheatid = "xyza7891h6UjNfd0eb2HQGtaul0WhfvS";

	[ServerVar(Help = "(Generated) Easy Anti-Cheat product key used alongside anticheatid for EAC server authentication")]
	public static string anticheatkey = "OWUDFZmi9VNL/7VhGVSSmCWALKTltKw8ISepa0VXs60";

	[ServerVar(Help = "(Generated) When true clients must provide a valid EAC token to connect; disable only for testing or modded environments")]
	public static bool anticheattoken = true;

	[ServerVar(Help = "Whether or not to send additional analytics to EAC")]
	public static bool eac_gameplay_data = true;

	[ServerVar(Help = "(Generated) When true players are kicked if EAC authentication fails; disable to allow players through even when EAC is unavailable")]
	public static bool strictauth_eac = false;

	[ServerVar(Help = "(Generated) When true players are kicked if Steam authentication fails; disable to allow connections when Steam auth servers are unreachable")]
	public static bool strictauth_steam = false;

	[ServerVar(Help = "(Generated) Number of server simulation ticks per second; higher values improve responsiveness but increase CPU usage")]
	public static int tickrate = 10;

	[ServerVar(Help = "(Generated) How many times per second entity network state is sent to clients; higher values reduce perceived lag for moving objects")]
	public static int entityrate = 16;

	[ServerVar(Help = "(Generated) How often (in seconds) the full entity schema is re-broadcast to all clients; default is every 1800 seconds")]
	public static float schematime = 1800f;

	[ServerVar(Help = "(Generated) Duration of a full day/night cycle in seconds; default 500s means roughly one cycle every 8 real-world minutes")]
	public static float cycletime = 500f;

	[ServerVar(Help = "(Generated) Marks this as an official Facepunch server; only set by Facepunch — do not enable on community servers")]
	public static bool official = false;

	[ServerVar(Help = "(Generated) Enables collection and reporting of gameplay statistics such as kill counts, damage dealt and resource gathered")]
	public static bool stats = false;

	[ServerVar(Help = "(Generated) Enables structural stability simulation; when disabled buildings will not collapse even if their supports are destroyed")]
	public static bool stability = true;

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Enables radiation zones at monuments; disabling removes all radiation hazards from the map")]
	public static bool radiation = true;

	[ReplicatedVar]
	public static float max_explosive_protection = 0.75f;

	[ServerVar(Help = "(Generated) Seconds before items dropped on the ground despawn; default is 300 seconds (5 minutes)")]
	public static float itemdespawn = 300f;

	[ServerVar(Help = "(Generated) Multiplier applied to the base item despawn time for items sitting inside loot containers; default 2x extends their lifetime")]
	public static float itemdespawn_container_scale = 2f;

	[ServerVar(Help = "(Generated) Upper cap on the loot container despawn multiplier; at default 24 the maximum lifetime is 24 x 5 min = 2 hours")]
	public static int itemdespawn_container_max_multiplier = 24;

	[ServerVar(Help = "(Generated) Fast-despawn time in seconds used for short-lived dropped items such as empty casings or small debris; default 30 seconds")]
	public static float itemdespawn_quick = 30f;

	[ServerVar(Help = "(Generated) Seconds before a player corpse is removed from the world; default is 300 seconds (5 minutes)")]
	public static float corpsedespawn = 300f;

	[ServerVar(Help = "(Generated) Seconds before environmental debris entities (e.g. broken barrel remnants) are removed from the world; default 30 seconds")]
	public static float debrisdespawn = 30f;

	[ServerVar(Help = "(Generated) Enables PvE mode — players cannot damage other players; they can still be killed by NPCs and the environment")]
	public static bool pve = false;

	[ReplicatedVar]
	public static bool cinematic = false;

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Short description of the server shown to players in the server browser")]
	public static string description = "No server description has been provided.";

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Server website URL displayed in the server browser; leave empty to show no link")]
	public static string url = "";

	[ServerVar(Help = "(Generated) Server branch tag used by the server browser to identify modded or experimental variants; leave empty for vanilla")]
	public static string branch = "";

	[ServerVar(Help = "(Generated) Maximum number of Steam server browser queries the server will respond to per second before rate-limiting")]
	public static int queriesPerSecond = 2000;

	[ServerVar(Help = "(Generated) Maximum number of Steam server browser queries allowed per minute from a single IP address")]
	public static int ipQueriesPerMin = 30;

	[ServerVar(Help = "(Generated) Enables automatic backups of server statistics data")]
	public static bool statBackup = false;

	[ServerVar(Help = "(Generated) Seconds a disconnected player must wait before they are allowed to rejoin the server; default is 300 seconds (5 minutes)")]
	public static int rejoin_delay = 300;

	[ServerVar(Help = "(Generated) Override the geographic region code used for ping estimation in the server browser; leave empty to use automatic detection")]
	public static string ping_region_code_override = "";

	private static string _favoritesEndpoint = "";

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) URL of the banner/header image shown at the top of this server's page in the server browser")]
	public static string headerimage = "";

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) URL of the logo image shown for this server in the server browser")]
	public static string logoimage = "";

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "(Generated) Number of rolling save-file backups to keep; each autosave rotates the oldest backup out")]
	public static int saveBackupCount = 2;

	[ReplicatedVar(Saved = true, ShowInAdminUI = true)]
	public static string motd = "";

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for all melee weapon damage dealt; 1.0 = default, 2.0 = double damage, 0.5 = half damage")]
	public static float meleedamage = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for all arrow and bow damage dealt; 1.0 = default, 2.0 = double damage")]
	public static float arrowdamage = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for all bullet damage dealt by firearms; 1.0 = default, 2.0 = double damage")]
	public static float bulletdamage = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for all bleeding damage over time; 1.0 = default")]
	public static float bleedingdamage = 1f;

	[ServerVar(Saved = true, Help = "How much to scale standard paintball damage (see paintballoverallsdamage for scaling damage for when players have overalls on)")]
	public static float paintballstandarddamage = 1f;

	[ServerVar(Saved = true, Help = "How much to scale paintball damage when both the hit player and initiator player have paintball overalls on (see paintballstandarddamage for scaling standard paintball damage)")]
	public static float paintballoverallsdamage = 1f;

	[ServerVar(Saved = true, Help = "How much to increase time to kill in pvp globally, 2.0 = twice as long, 0.5 = half as long")]
	public static float pvp_ttk_global = 1f;

	[ServerVar(Saved = true, Help = "How much to increase time to kill with melee in pvp globally, 2.0 = twice as long, 0.5 = half as long")]
	public static float pvp_ttk_melee = 1f;

	[ServerVar(Saved = true, Help = "How much to increase time to kill bullets in pvp globally, 2.0 = twice as long, 0.5 = half as long")]
	public static float pvp_ttk_bullet = 1f;

	[ServerVar(Help = "Lower damage of explosives to 1 and allow them to be triggered multiple times")]
	public static bool explosive_testing_mode = false;

	[ServerVar(Saved = true, Help = "(Generated) Multiplier for radiation intensity inside oil rig monuments; higher values increase radiation damage per second")]
	public static float oilrig_radiation_amount_scale = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Multiplier for how long radiation lingers after an event at oil rig monuments; higher values extend the radiation duration")]
	public static float oilrig_radiation_time_scale = 1f;

	[ServerVar(Help = "(Generated) Radiation level at which the oil rig alarm triggers; 0 means the alarm activates immediately when any radiation is present")]
	public static float oilrig_radiation_alarm_threshold = 0f;

	[ReplicatedVar(Saved = true)]
	public static float funWaterDamageThreshold = 0.8f;

	[ReplicatedVar(Saved = true)]
	public static float funWaterWetnessGain = 0.05f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for armour effectiveness against melee damage; 1.0 = default, values above 1.0 make armour stronger against melee")]
	public static float meleearmor = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for armour effectiveness against arrow and bow damage; 1.0 = default")]
	public static float arrowarmor = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for armour effectiveness against bullet damage; 1.0 = default")]
	public static float bulletarmor = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Global multiplier for armour effectiveness against bleeding damage; 1.0 = default")]
	public static float bleedingarmor = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Additional bullet damage multiplier applied only in player-vs-player combat, stacks with bulletdamage")]
	public static float pvpBulletDamageMultiplier = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Additional bullet damage multiplier applied only when players shoot NPCs or animals, stacks with bulletdamage")]
	public static float pveBulletDamageMultiplier = 1f;

	[ServerVar(Help = "(Generated) Number of entities processed per network update batch; lower values spread the load across more frames but increase total overhead")]
	public static int updatebatch = 512;

	[ServerVar(Help = "(Generated) Number of entities processed per batch during the initial spawn network update; higher values send more entities per frame during map load")]
	public static int updatebatchspawn = 1024;

	[ServerVar(Help = "(Generated) Number of entities included in each entity-update batch per frame; tune to balance CPU time spent on entity processing")]
	public static int entitybatchsize = 100;

	[ServerVar(Help = "(Generated) Time in seconds allocated to each entity batch update pass; the server will not start a new batch until this interval has elapsed")]
	public static float entitybatchtime = 1f;

	[ServerVar(Help = "(Generated) How often (in seconds) composters advance their composting progress; default is every 300 seconds (5 minutes)")]
	public static float composterUpdateInterval = 300f;

	[ReplicatedVar]
	public static float planttick = 60f;

	[ServerVar(Help = "(Generated) Multiplier for plant growth tick speed; values above 1.0 make plants grow faster, values below 1.0 slow growth")]
	public static float planttickscale = 1f;

	private static int _maxHttp = 32;

	[ServerVar(Help = "(Generated) When enabled, plants that would normally be in a critically poor condition are given a minimum viable condition score instead of immediately dying")]
	public static bool useMinimumPlantCondition = true;

	[ServerVar(Saved = true, Help = "(Generated) Probability (0–1) per growth tick that a plant growing outside a planter box will die; default 0.005 means a 0.5% chance each tick")]
	public static float nonPlanterDeathChancePerTick = 0.005f;

	[ServerVar(Saved = true, Help = "(Generated) Radius in metres within which a ceiling light provides artificial light that counts toward a growable plant's light requirement")]
	public static float ceilingLightGrowableRange = 3f;

	[ReplicatedVar(Saved = true)]
	public static float artificialTemperatureGrowableRange = 4f;

	[ServerVar(Saved = true, Help = "(Generated) Vertical offset in metres added when checking whether a ceiling light illuminates a plant directly below it")]
	public static float ceilingLightHeightOffset = 3f;

	[ReplicatedVar(Saved = true)]
	public static float sprinklerRadius = 3f;

	[ServerVar(Saved = true, Help = "(Generated) Vertical eye-height offset in metres used when raycasting to determine whether a sprinkler can water a given plant")]
	public static float sprinklerEyeHeightOffset = 3f;

	[ServerVar(Saved = true, Help = "(Generated) When true, uses the old sprinkler initialisation process when loading a save; enable if upgrading from an older server version to avoid sprinkler layout issues")]
	public static bool useLegacySprinklerLoadProcess = false;

	[ServerVar(Saved = true, Help = "(Generated) Soil saturation level (0–1) at which a planter box is considered perfectly watered for quality bonuses; default 0.6")]
	public static float optimalPlanterQualitySaturation = 0.6f;

	[ServerVar(Help = "(Generated) Multiplier for player metabolism tick frequency; lower values slow down hunger, thirst and calorie consumption rates")]
	public static float metabolismtick = 1f;

	[ServerVar(Help = "(Generated) Rate multiplier for status effect (buff/debuff) ticks; lower values slow all active modifiers such as poison, radiation sickness and warmth")]
	public static float modifierTickRate = 1f;

	[ServerVar(Saved = true, Help = "(Generated) Minimum seconds that must pass after a player recovers from being wounded before they can be put into the wounded state again")]
	public static float rewounddelay = 60f;

	[ServerVar(Saved = true, Help = "Can players be wounded after receiving fatal damage")]
	public static bool woundingenabled = true;

	[ServerVar(Saved = true, Help = "Do players go into the crawling wounded state")]
	public static bool crawlingenabled = true;

	[ServerVar(Help = "Base chance of recovery after crawling wounded state", Saved = true)]
	public static float woundedrecoverchance = 0.2f;

	[ServerVar(Help = "Base chance of recovery after incapacitated wounded state", Saved = true)]
	public static float incapacitatedrecoverchance = 0.1f;

	[ServerVar(Help = "Maximum percent chance added to base wounded/incapacitated recovery chance, based on the player's food and water level", Saved = true)]
	public static float woundedmaxfoodandwaterbonus = 0.25f;

	[ServerVar(Help = "Minimum initial health given when a player dies and moves to crawling wounded state", Saved = false)]
	public static int crawlingminimumhealth = 7;

	[ServerVar(Help = "Maximum initial health given when a player dies and moves to crawling wounded state", Saved = false)]
	public static int crawlingmaximumhealth = 12;

	[ServerVar(Saved = true, Help = "(Generated) When true, fall damage is calculated server-side for improved anti-cheat security; disabling may reduce server load but allows clients to manipulate fall damage")]
	public static bool playerserverfall = true;

	[ServerVar(Help = "(Generated) Enables plant light detection — growable plants will check nearby light sources each tick and adjust growth speed and quality accordingly")]
	public static bool plantlightdetection = true;

	[ServerVar(Help = "(Generated) Radius in metres around a player's death point — any sleeping bag or bed within this range is put on a respawn cooldown to prevent spawn-camping")]
	public static float respawnresetrange = 50f;

	[ReplicatedVar]
	public static int max_sleeping_bags = 15;

	[ReplicatedVar]
	public static bool bag_quota_item_amount = true;

	[ServerVar(Help = "(Generated) Maximum number of unacknowledged network messages per connection before the server starts applying backpressure to that client")]
	public static int maxunack = 4;

	[ServerVar(Help = "(Generated) Enables server-side network caching of entity state; when enabled only deltas are sent per update rather than the full entity data, significantly reducing bandwidth")]
	public static bool netcache = true;

	[ServerVar(Help = "(Generated) Whether player corpses are spawned when players die; disabling removes corpses entirely and items are dropped directly")]
	public static bool corpses = true;

	[ServerVar(Help = "(Generated) Enables automatic server-side game events such as helicopter patrols, airdrops and cargo ship visits")]
	public static bool events = true;

	[ServerVar(Help = "(Generated) Whether items drop to the ground from a player's inventory when they die; disable to prevent item drops on death")]
	public static bool dropitems = true;

	[ServerVar(Help = "(Generated) Maximum byte size of the network entity cache; 0 means no hard limit")]
	public static int netcachesize = 0;

	[ServerVar(Help = "(Generated) Maximum byte size of the entity save cache used to accelerate autosaves; 0 means no hard limit")]
	public static int savecachesize = 0;

	[ServerVar(Help = "(Generated) Number of recent combat events retained in each player's combat log, viewable with combatlog")]
	public static int combatlogsize = 30;

	[ServerVar(Help = "(Generated) Seconds of delay before a combat event appears in the player's own combat log, preventing real-time tracking during a fight")]
	public static int combatlogdelay = 10;

	[ServerVar(Help = "(Generated) Seconds an incoming connection has to complete authentication (Steam + EAC) before being forcibly disconnected")]
	public static int authtimeout = 60;

	[ServerVar(Help = "(Generated) Seconds of network inactivity before a connected player is timed out and disconnected")]
	public static int playertimeout = 60;

	[ServerVar(ShowInAdminUI = true, Help = "(Generated) Minutes of in-game inactivity before a player is automatically kicked; set to 0 to disable idle kick")]
	public static int idlekick = 30;

	[ServerVar(Help = "(Generated) Controls who is subject to the idle kick: 0 = nobody, 1 = non-admin players only, 2 = all players including admins")]
	public static int idlekickmode = 1;

	[ServerVar(Help = "(Generated) Minutes of inactivity before admin players are idle-kicked; 0 disables idle kick specifically for admins regardless of idlekickmode")]
	public static int idlekickadmins = 0;

	[ServerVar(Help = "(Generated) When enabled, long-distance ambient sounds (e.g. distant gunfire) are networked to clients; disabling may reduce bandwidth on busy servers")]
	public static bool long_distance_sounds = true;

	private static string _gamemode;

	private static string _tags = "";

	[ServerVar(Help = "Censors the Steam player list to make player tracking more difficult")]
	public static bool censorplayerlist = true;

	[ServerVar(Help = "HTTP API endpoint for centralized banning (see wiki)")]
	public static string bansServerEndpoint = "";

	[ServerVar(Help = "Failure mode for centralized banning, set to 1 to reject players from joining if it's down (see wiki)")]
	public static int bansServerFailureMode = 0;

	[ServerVar(Help = "Timeout (in seconds) for centralized banning web server requests")]
	public static int bansServerTimeout = 5;

	[ServerVar(Help = "HTTP API endpoint for receiving F7 reports", Saved = true)]
	public static string reportsServerEndpoint = "";

	[ServerVar(Help = "If set, this key will be included with any reports sent via reportsServerEndpoint (for validation)", Saved = true)]
	public static string reportsServerEndpointKey = "";

	[ServerVar(Help = "Should F7 reports from players be printed to console", Saved = true)]
	public static bool printReportsToConsole = false;

	[ServerVar(Help = "If a player presses the respawn button, respawn at their death location (for trailer filming)")]
	public static bool respawnAtDeathPosition = false;

	[ServerVar(Help = "When a player respawns give them the loadout assigned to client.RespawnLoadout (created with inventory.saveloadout)")]
	public static bool respawnWithLoadout = false;

	[ServerVar(Help = "When transferring water, should containers keep 1 water behind. Enabling this should help performance if water IO is causing performance loss", Saved = true)]
	public static bool waterContainersLeaveWaterBehind = false;

	[ServerVar(Help = "How often industrial conveyors attempt to move items (value is an interval measured in seconds). Setting to 0 will disable all movement", Saved = true, ShowInAdminUI = true)]
	public static float conveyorMoveFrequency = 5f;

	[ServerVar(Help = "How often industrial crafters attempt to craft items (value is an interval measured in seconds). Setting to 0 will disable all crafting", Saved = true, ShowInAdminUI = true)]
	public static float industrialCrafterFrequency = 5f;

	[ReplicatedVar(Help = "How much scrap is required to research default blueprints", Saved = true, ShowInAdminUI = true)]
	public static int defaultBlueprintResearchCost = 10;

	[ServerVar(Help = "Whether to check for illegal industrial pipes when changing building block states (roof bunkers)", Saved = true, ShowInAdminUI = true)]
	public static bool enforcePipeChecksOnBuildingBlockChanges = true;

	[ServerVar(Help = "How many stacks a single conveyor can move in a single tick", Saved = true, ShowInAdminUI = true)]
	public static int maxItemStacksMovedPerTickIndustrial = 12;

	[ServerVar(Help = "How long per frame to spend on industrial jobs", Saved = true, ShowInAdminUI = true)]
	public static float industrialFrameBudgetMs = 0.5f;

	[ServerVar(Help = "Should industrial be paused during autosaves")]
	public static bool pauseindustrialduringsave = true;

	[ServerVar(Help = "When enabled industrial transfers will abort if they start to take too long. Will lead to inconsistent splitting but should retain performance", Saved = true)]
	public static bool industrialTransferStrictTimeLimits = true;

	[ServerVar(Help = "Enables a faster way to move items around during conveyor transfers. Should be on unless there's a issue")]
	public static bool industrialAllowQuickMove = true;

	[ServerVar(Help = "How long per frame to spend animating items moving into the hopper (will be instant if <= 0)", Saved = true, ShowInAdminUI = true)]
	public static float hopperAnimationBudgetMs = 0.1f;

	[ServerVar(Help = "Set to false to disable the storage adaptor sorting functionality")]
	public static bool allowSorting = true;

	[ServerVar(Help = "How long per frame to spend on updating water wheel power generation and water info", Saved = true, ShowInAdminUI = true)]
	public static float waterWheelWorkBudgetMs = 0.1f;

	[ServerVar(Help = "Reposition attachments like storage adaptors if they have moved on reskins")]
	public static bool repositionAttachmentsOnReskin = true;

	[ReplicatedVar(Help = "How many markers each player can place", Saved = true, ShowInAdminUI = true)]
	public static int maximumMapMarkers = 5;

	[ServerVar(Help = "How many pings can be placed by each player", Saved = true, ShowInAdminUI = true)]
	public static int maximumPings = 5;

	[ServerVar(Help = "How long a ping should last", Saved = true, ShowInAdminUI = true)]
	public static float pingDuration = 10f;

	[ServerVar(Help = "Allows backpack equipping while not grounded", Saved = true, ShowInAdminUI = true)]
	public static bool canEquipBackpacksInAir = false;

	[ReplicatedVar(Help = "How long it takes to pick up a used parachute in seconds", Saved = true, ShowInAdminUI = true)]
	public static float parachuteRepackTime = 8f;

	[ServerVar(Help = "Whether emoji ownership is checked server side. Could be performance draining in high chat volumes")]
	public static bool emojiOwnershipCheck = true;

	[ReplicatedVar(Help = "Skip death screen fade", Saved = false, ShowInAdminUI = false)]
	public static bool skipDeathScreenFade = false;

	[ReplicatedVar(Help = "Controls whether the tutorial is enabled on this server", Saved = true, ShowInAdminUI = true, Default = "false")]
	public static bool tutorialEnabled = false;

	[ReplicatedVar(Help = "How much of a tax to apply to tech unlocks at a level 1 workbench. 10 = additional 10% scrap cost", Saved = true)]
	public static float workbenchTaxRate1 = 0f;

	[ReplicatedVar(Help = "How much of a tax to apply to tech unlocks at a level 2 workbench. 10 = additional 10% scrap cost", Saved = true)]
	public static float workbenchTaxRate2 = 0f;

	[ReplicatedVar(Help = "How much of a tax to apply to tech unlocks at a level 3 workbench. 10 = additional 10% scrap cost", Saved = true)]
	public static float workbenchTaxRate3 = 0f;

	[ServerVar(Help = "Automatically upload procedurally generated maps so that players download them (faster) instead of re-generating them", Saved = true, ShowInAdminUI = true)]
	public static bool autoUploadMap = true;

	[ReplicatedVar(Help = "Can players use the in-game map")]
	public static bool mapenabled = true;

	[ReplicatedVar(Help = "Should the in-game map be covered by a fog of war")]
	public static bool fogofwar = false;

	[ReplicatedVar(Help = "Should the deep sea map be covered by fog of war")]
	public static bool deepSeaFogofwar = true;

	[ReplicatedVar(Help = "How much area around the player is revealed when using fog of war. Must be a multiple of 32")]
	public static int fogofwarrevealsize = 256;

	[ReplicatedVar(Help = "Will the in-game compass show at the top of the screen")]
	public static bool compassenabled = true;

	[ReplicatedVar(Help = "Should the player see their position on the map")]
	public static bool hideplayeronmap = false;

	[ReplicatedVar(Help = "Should hte player see their direction on the map")]
	public static bool hideplayermapdirection = false;

	[ServerVar(Help = "Automatically upload an image of the map, used to show the map in the server browser", Saved = true, ShowInAdminUI = true)]
	public static bool autoUploadMapImages = true;

	[ServerVar(Help = "How often (in hours) the water well NPC's update their sell orders")]
	public static float waterWellNpcSalesRefreshFrequency = 1f;

	[ReplicatedVar(Help = "Opens a loot panel when interacting with a workbench instead of going straight into the tech tree. Designed for backwards compatibility with mods.")]
	public static bool useLegacyWorkbenchInteraction = false;

	[ServerVar(Help = "If no players are in this range kayaks, boogie boards and inner tubes will switch to a cheaper buoyancy system")]
	public static float lowPriorityBuoyancyRange = 30f;

	[ServerVar(Help = "If true hot air balloons can be shot down with homing missiles")]
	public static bool homingMissileTargetsHab = false;

	[ServerVar(Help = "Require a premium status account to connect to this server")]
	public static bool premium = false;

	[ReplicatedVar(Help = "Whether to run the food spoiling system")]
	public static bool foodSpoiling = true;

	[ServerVar(Help = "(Generated) Maximum milliseconds per frame the server spends advancing food spoil timers; lower values reduce frame time impact at the cost of less frequent spoil updates")]
	public static float foodSpoilingBudgetMs = 0.05f;

	[ServerVar(Help = "Maximum difference (in seconds) that two items with spoil timers can have and still be stackable")]
	public static float maxFoodSpoilTimeDiffForItemStack = 180f;

	[ServerVar(Help = "If two spoiled food items are both above this threshold then we will allow them to be stacked")]
	public static float normalisedFoodSpoilTimeStackThreshold = 0.9f;

	[ServerVar(Help = "Whether to run local avoidance for chickens, disabling might get a slight performance improvement but chickens will clip", Saved = true, ShowInAdminUI = true)]
	public static bool farmChickenLocalAvoidance = true;

	[ServerVar(Help = "Endpoint to use to check if players have premium status")]
	public static string premiumVerifyEndpoint = "https://rust-api.facepunch.com/api/premium/verify";

	[ServerVar(Help = "Minimum time to recheck premium status for already connected players (in seconds)")]
	public static float premiumRecheckMinSeconds = 300f;

	[ServerVar(Help = "How often to do premium status rechecks")]
	public static float premiumRecheckInterval = 300f;

	[ServerVar(Help = "Maximum number of players to recheck at a time")]
	public static int premiumRecheckMaxBatchSize = 100;

	[ServerVar(Saved = true, Help = "(Generated) When true, vine tree variants are included during world generation; disable to remove all climbable vine trees from the map")]
	public static bool spawnVineTrees = true;

	[ServerVar(Saved = true, Help = "(Generated) When true, players can grab and swing on deployed vines; disable to prevent vine-swinging movement")]
	public static bool allowVineSwinging = true;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Bags will increase their respawn time by this much")]
	public static float respawnTimeAdditionBag = 0f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "Beds will increase their respawn time by this much")]
	public static float respawnTimeAdditionBed = 0f;

	[ServerVar(Saved = true, ShowInAdminUI = true, Help = "All ammo drops from NPC loot will be multiplied by this")]
	public static float npcAmmoLootMultiplier = 1f;

	[ReplicatedVar(Help = "Multiplies crafting cost of firearm ammunition", Saved = true, ShowInAdminUI = true)]
	public static float hardcoreFirearmAmmunitionCraftingMultiplier = 1f;

	[ServerVar(Help = "Allows radiation to flood monuments to force puzzles to reset")]
	public static bool monumentPuzzleResetRadiation = true;

	[ServerVar(Help = "(Generated) Multiplier applied to a monument's normal radiation radius when the puzzle-reset radiation cloud is active; default 1.5x expands the zone beyond its usual boundary")]
	public static float monumentPuzzleResetRadiationRadiusMultiplier = 1.5f;

	[ServerVar(Help = "Clamp radiation multiplier to this amount of meters, -1 = ignored")]
	public static float monumentPuzzleResetRadiationMaxRadiusIncrease = 20f;

	[ServerVar(Help = "How long before the reset happens do we start applying radiation")]
	public static float monumentPuzzleResetRadiationPreResetTime = 300f;

	[ServerVar(Help = "How long does a monument puzzle need to be empty with full rads before it can reset")]
	public static float monumentPuzzleResetRadiationPlayerEmptyTime = 120f;

	[ServerVar(Help = "(Generated) Radiation damage per second applied to players inside a monument during its puzzle-reset radiation phase")]
	public static float monumentPuzzleResetRadiationAmount = 3f;

	[ServerVar(Help = "Force enable radiation in monument puzzles to confirm they work")]
	public static bool monumentpuzzleresetradiationoverride = false;

	[ServerVar(Help = "(Generated) When enabled, debug spheres are drawn in the world showing the radiation zone boundaries during monument puzzle resets")]
	public static bool drawpuzzleresets = false;

	[ServerVar(Help = "(Generated) When enabled, the puzzle reset timer pauses for monuments that have not been looted yet, preventing resets of untouched areas")]
	public static bool pauseunlootedpuzzles = true;

	[ServerVar(Saved = true, Help = "(Generated) When enabled, chat warnings are broadcast to players inside a monument shortly before its puzzle-reset radiation begins")]
	public static bool monumentPuzzleResetWarnings = true;

	[ServerVar(Saved = true, Help = "(Generated) Maximum milliseconds per frame spent processing furnace and campfire cook ticks; lower values keep frames smoother on high-population servers")]
	public static float ovenCookBudgetMs = 0.25f;

	[ServerVar(Help = "(Generated) Enables a server-wide required system configuration that all connecting clients must satisfy; used to enforce minimum hardware or software requirements")]
	public static bool useServerWideRequiredSystemConfig = false;

	[ServerVar(Help = "(Generated) Enables per-player required system configuration checks on connect; allows different requirements to be enforced for individual players")]
	public static bool usePerPlayerRequiredSystemConfig = false;

	[ServerVar(Saved = true, Help = "(Generated) When enabled, weapons and tools holstered on a player's back are visible on their character model to other players")]
	public static bool showHolsteredItems = true;

	[ServerVar(Help = "(Generated) Maximum world-state update packets per second accepted from each individual client; prevents flooding the server with position spam")]
	public static int maxpacketspersecond_world = 1;

	[ServerVar(Help = "(Generated) Maximum RPC (Remote Procedure Call) packets per second accepted from each client; limits how fast clients can trigger server-side actions")]
	public static int maxpacketspersecond_rpc = 200;

	[ServerVar(Help = "(Generated) Maximum RPC signal packets per second accepted from each client; signal RPCs are lightweight event triggers used for interactions")]
	public static int maxpacketspersecond_rpc_signal = 30;

	[ServerVar(Help = "(Generated) Maximum console command packets per second accepted from each client; rate-limits how quickly clients can send commands to the server")]
	public static int maxpacketspersecond_command = 100;

	[ServerVar(Help = "(Generated) Maximum byte size of a single console command packet from a client; oversized packets are rejected")]
	public static int maxpacketsize_command = 100000;

	[ServerVar(Help = "(Generated) Maximum byte size of a single global-trees network packet; oversized packets are dropped")]
	public static int maxpacketsize_globaltrees = 100;

	[ServerVar(Help = "(Generated) Maximum byte size of a single global-entities network packet; oversized packets are dropped")]
	public static int maxpacketsize_globalentities = 1000;

	[ServerVar(Help = "Maximum number of bytes permitted in VoiceData packets, oversized packets will be dropped")]
	public static int maxpacketsize_voicedata = 8096;

	[ServerVar(Help = "(Generated) Maximum tick-update packets per second accepted from each client; these carry player inputs and must stay within this rate to be processed")]
	public static int maxpacketspersecond_tick = 300;

	[ServerVar(Help = "(Generated) Maximum voice chat packets per second accepted from each client; reducing this limits voice bandwidth usage per player")]
	public static int maxpacketspersecond_voice = 100;

	[ServerVar(Help = "(Generated) Maximum sync-var (replicated variable) update packets per second accepted from each client")]
	public static int maxpacketspersecond_syncvar = 200;

	[ServerVar(Help = "(Generated) Enables packet-type logging; must be true before the packetlog command will return data. Collects packet type call counts at runtime")]
	public static bool packetlog_enabled = false;

	[ServerVar(Help = "(Generated) Enables RPC call logging; must be true before the rpclog command will return data. Tracks how often each RPC is called")]
	public static bool rpclog_enabled = false;

	[ServerVar(Help = "MS per frame to spend warming up entity save caches")]
	public static int saveframebudget = 5;

	[ServerVar(Help = "Player Update parallelism mode: 2-4, Higher modes are faster but more experimental. 3 by default")]
	public static int UsePlayerUpdateJobs = 3;

	[ServerVar(Help = "UsePlayerUpdateJobs 4 related - how many players to gather occlusion pairs for per task")]
	public static int OcclusionGatherBatchPlayerCount = 64;

	[ServerVar(Help = "UsePlayerUpdateJobs 2 related - how many snapshot messages to batch into 1 task")]
	public static int SnapshotTaskBatchCount = 64;

	[ServerVar(Help = "UsePlayerUpdateJobs 2 related - how many destroy messages to batch into 1 task")]
	public static int DestroyTaskBatchCount = 128;

	[ServerVar(Help = "(Generated) Setting this to true assigns a new random value to the world generation seed; useful for wipe scripts that want a fresh random map each time")]
	public static bool randomize_seed
	{
		get
		{
			return false;
		}
		set
		{
			if (value)
			{
				seed = new Random().Next();
			}
		}
	}

	[ServerVar(Saved = true, Help = "(Generated) Persistent unique identifier for this server instance, used when recording demos and for analytics attribution")]
	public static string server_id { get; set; }

	[ServerVar(ShowInAdminUI = true, Saved = true, Help = "Domain name to save when players favorite your server. The port can be omitted if using the default port or a SRV DNS record is created.")]
	public static string favoritesEndpoint
	{
		get
		{
			return _favoritesEndpoint;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				_favoritesEndpoint = "";
				return;
			}
			value = value.Trim();
			if (value.StartsWith("https://"))
			{
				string text = value;
				int length = "https://".Length;
				value = text.Substring(length, text.Length - length);
			}
			if (value.StartsWith("http://"))
			{
				string text = value;
				int length = "http://".Length;
				value = text.Substring(length, text.Length - length);
			}
			_favoritesEndpoint = value.Trim().ToLowerInvariant();
		}
	}

	[ServerVar(Help = "(Generated) Sets the EOS (Epic Online Services) anti-cheat log verbosity level; higher values produce more detailed anti-cheat diagnostic output")]
	public static int anticheatlog
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected I4, but got Unknown
			return (int)EOS.LogLevel;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			EOS.LogLevel = (LogLevel)value;
		}
	}

	[ServerVar(Help = "(Generated) Maximum number of simultaneous outbound HTTP connections the server may have open at once; used for map uploads, ban checks, and other web requests")]
	public static int http_connection_limit
	{
		get
		{
			return _maxHttp;
		}
		set
		{
			_maxHttp = value;
			HttpManager.UpdateMaxConnections();
		}
	}

	[ServerVar(Help = "(Generated) Short name of the game mode to activate on this server (e.g. 'softcore', 'hardcore'); applies convar overrides defined by that game mode's prefab")]
	public static string gamemode
	{
		get
		{
			return _gamemode;
		}
		set
		{
			_gamemode = value;
			ApplyGamemode();
		}
	}

	[ServerVar(Help = "Comma-separated server browser tag values (see wiki)", Saved = true, ShowInAdminUI = true)]
	public static string tags
	{
		get
		{
			return _tags;
		}
		set
		{
			_tags = AutoCorrectTags(value);
		}
	}

	[ServerVar(Help = "(Generated) Maximum byte size of the client info blob sent during the connection handshake; clients sending a larger payload are rejected")]
	public static int maxclientinfosize
	{
		get
		{
			return Connection.MaxClientInfoSize;
		}
		set
		{
			Connection.MaxClientInfoSize = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum number of simultaneous connections allowed from the same IP address; helps mitigate connection-flooding attacks (clamped 1–1000)")]
	public static int maxconnectionsperip
	{
		get
		{
			return Network.Server.MaxConnectionsPerIP;
		}
		set
		{
			Network.Server.MaxConnectionsPerIP = Mathf.Clamp(value, 1, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum milliseconds the network receive thread is allowed to run per frame; increase if players report missed packets on high-population servers (clamped 10–1000)")]
	public static int maxreceivetime
	{
		get
		{
			return Network.Server.MaxReceiveTime;
		}
		set
		{
			Network.Server.MaxReceiveTime = Mathf.Clamp(value, 10, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum milliseconds the main game thread will wait for a network operation before timing out; increase to tolerate brief network stalls (clamped 1–1000)")]
	public static int maxmainthreadwait
	{
		get
		{
			return Network.Server.MaxMainThreadWait;
		}
		set
		{
			Network.Server.MaxMainThreadWait = Mathf.Clamp(value, 1, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum milliseconds the dedicated read thread will block waiting for incoming data before timing out (clamped 1–1000)")]
	public static int maxreadthreadwait
	{
		get
		{
			return Network.Server.MaxReadThreadWait;
		}
		set
		{
			Network.Server.MaxReadThreadWait = Mathf.Clamp(value, 1, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum milliseconds the dedicated write thread will block waiting to send data before timing out (clamped 1–1000)")]
	public static int maxwritethreadwait
	{
		get
		{
			return Network.Server.MaxWriteThreadWait;
		}
		set
		{
			Network.Server.MaxWriteThreadWait = Mathf.Clamp(value, 1, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum milliseconds the decryption thread will block before timing out; increase if CPU-heavy encryption causes dropped packets (clamped 1–1000)")]
	public static int maxdecryptthreadwait
	{
		get
		{
			return Network.Server.MaxDecryptThreadWait;
		}
		set
		{
			Network.Server.MaxDecryptThreadWait = Mathf.Clamp(value, 1, 1000);
		}
	}

	[ServerVar(Help = "(Generated) Maximum number of packets that can be queued in the incoming read queue; excess packets are dropped to prevent memory exhaustion")]
	public static int maxreadqueuelength
	{
		get
		{
			return Network.Server.MaxReadQueueLength;
		}
		set
		{
			Network.Server.MaxReadQueueLength = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum number of packets that can be queued in the outgoing write queue; excess packets are dropped when the queue is full")]
	public static int maxwritequeuelength
	{
		get
		{
			return Network.Server.MaxWriteQueueLength;
		}
		set
		{
			Network.Server.MaxWriteQueueLength = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum number of encrypted packets that can wait in the decryption queue before being dropped")]
	public static int maxdecryptqueuelength
	{
		get
		{
			return Network.Server.MaxDecryptQueueLength;
		}
		set
		{
			Network.Server.MaxDecryptQueueLength = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum total byte size of the incoming read queue; excess bytes are dropped to prevent memory exhaustion from a flood of large packets")]
	public static int maxreadqueuebytes
	{
		get
		{
			return Network.Server.MaxReadQueueBytes;
		}
		set
		{
			Network.Server.MaxReadQueueBytes = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum total byte size of the outgoing write queue; if the queue fills the oldest packets are dropped")]
	public static int maxwritequeuebytes
	{
		get
		{
			return Network.Server.MaxWriteQueueBytes;
		}
		set
		{
			Network.Server.MaxWriteQueueBytes = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Maximum total byte size of packets waiting for decryption; excess packets are dropped when the limit is reached")]
	public static int maxdecryptqueuebytes
	{
		get
		{
			return Network.Server.MaxDecryptQueueBytes;
		}
		set
		{
			Network.Server.MaxDecryptQueueBytes = Mathf.Max(value, 1);
		}
	}

	[ServerVar(Help = "(Generated) Size of the LRU player-state cache; higher values keep more player states in memory, reducing disk reads when reconnecting players")]
	public static int player_state_cache_size
	{
		get
		{
			return SingletonComponent<ServerMgr>.Instance?.playerStateManager.CacheSize ?? 0;
		}
		set
		{
			SingletonComponent<ServerMgr>.Instance.playerStateManager.CacheSize = value;
		}
	}

	[ServerVar(Help = "(Generated) Prints the current server decryption queue depth")]
	public static int rust_relay_send_queue
	{
		get
		{
			return RustRelay.SendQueueCount;
		}
		set
		{
			RustRelay.SendQueueCount = value;
		}
	}

	[ServerVar(Help = "(Generated) Global cap on total network packets per second the server will accept across all connected clients combined")]
	public static int maxpacketspersecond
	{
		get
		{
			return (int)Network.Server.MaxPacketsPerSecond;
		}
		set
		{
			Network.Server.MaxPacketsPerSecond = (ulong)Mathf.Clamp(value, 1, 1000000);
		}
	}

	public static string rootFolder => "server/" + identity;

	public static string filesStorageFolder
	{
		get
		{
			if (!string.IsNullOrEmpty(filefolderoverride))
			{
				return filefolderoverride;
			}
			return rootFolder;
		}
	}

	public static string backupFolder => "backup/0/" + identity;

	public static string backupFolder1 => "backup/1/" + identity;

	public static string backupFolder2 => "backup/2/" + identity;

	public static string backupFolder3 => "backup/3/" + identity;

	[ServerVar(Help = "(Generated) Enables or disables network packet compression on the server; compression reduces bandwidth at the cost of a small amount of CPU time")]
	public static bool compression
	{
		get
		{
			if (Net.sv == null)
			{
				return false;
			}
			return Net.sv.compressionEnabled;
		}
		set
		{
			Net.sv.compressionEnabled = value;
		}
	}

	[ServerVar(Help = "(Generated) Enables low-level network activity logging on the server; produces verbose output useful for diagnosing connection and packet issues")]
	public static bool netlog
	{
		get
		{
			if (Net.sv == null)
			{
				return false;
			}
			return Net.sv.logging;
		}
		set
		{
			Net.sv.logging = value;
		}
	}

	public static bool UseUniTasks => UsePlayerUpdateJobs >= 3;

	[ReplicatedVar(Name = "era", Help = "none,primitive,medieval,frontier,rust")]
	public static string era
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return ((object)Era/*cast due to constrained. prefix*/).ToString();
		}
		set
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(value) && (int)Era != 0)
			{
				Era = (Era)0;
				OnEraChanged();
				return;
			}
			Era val = Era;
			switch (value.ToLower())
			{
			case "unknown":
			case "none":
				Era = (Era)0;
				break;
			case "primitive":
				Era = (Era)10;
				break;
			case "siege":
			case "medieval":
				Era = (Era)20;
				break;
			case "frontier":
				Era = (Era)30;
				break;
			case "modern":
			case "rust":
				Era = (Era)1000;
				break;
			}
			if (val != Era)
			{
				OnEraChanged();
			}
		}
	}

	public static Era Era { get; private set; }

	private static void ApplyGamemode()
	{
		GameModeManifest gameModeManifest = GameModeManifest.Get();
		if ((Object)(object)gameModeManifest == (Object)null)
		{
			Debug.LogError((object)"No GameModeManifest found");
			return;
		}
		foreach (GameObjectRef gameModePrefab in gameModeManifest.gameModePrefabs)
		{
			object obj;
			if (gameModePrefab == null)
			{
				obj = null;
			}
			else
			{
				GameObject obj2 = gameModePrefab.Get();
				obj = ((obj2 != null) ? obj2.GetComponent<BaseGameMode>() : null);
			}
			BaseGameMode baseGameMode = (BaseGameMode)obj;
			if (baseGameMode.shortname == gamemode)
			{
				baseGameMode.ApplyConVars();
				return;
			}
		}
		Debug.LogWarning((object)("Couldn't find gamemode: " + gamemode));
	}

	public static float GetTaxRateForWorkbenchUnlock(int workbenchLevel)
	{
		float num = 0f;
		switch (workbenchLevel)
		{
		case 0:
			num = workbenchTaxRate1;
			break;
		case 1:
			num = workbenchTaxRate2;
			break;
		case 2:
			num = workbenchTaxRate3;
			break;
		}
		return Mathf.Clamp(num, 0f, 100f);
	}

	public static float TickDelta()
	{
		return 1f / (float)tickrate;
	}

	public static float TickTime(uint tick)
	{
		return (float)((double)TickDelta() * (double)tick);
	}

	[ServerVar(Help = "Show holstered items on player bodies")]
	public static void setshowholstereditems(Arg arg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		showHolsteredItems = arg.GetBool(0, showHolsteredItems);
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.inventory.UpdatedVisibleHolsteredItems();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		enumerator = BasePlayer.sleepingPlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.inventory.UpdatedVisibleHolsteredItems();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Prints the number of player states currently held in the in-memory player state cache")]
	public static void player_state_cache_count(Arg args)
	{
		args.ReplyWith(SingletonComponent<ServerMgr>.Instance.playerStateManager.CacheCount);
	}

	[ServerVar(Help = "(Generated) Prints the total number of player state cache entries evicted since server startup; high values suggest the cache size should be increased")]
	public static void player_state_cache_evictions(Arg args)
	{
		args.ReplyWith(SingletonComponent<ServerMgr>.Instance.playerStateManager.CacheEvictions);
	}

	[ServerVar(Help = "(Generated) Prints the current server incoming network read queue depth (packet count and total byte size)")]
	public static string printreadqueue(Arg arg)
	{
		return "Server read queue: " + Net.sv.ReadQueueLength + " items / " + NumberExtensions.FormatBytes<int>(Net.sv.ReadQueueBytes, false);
	}

	[ServerVar(Help = "(Generated) Prints the current server outgoing network write queue depth (packet count and total byte size)")]
	public static string printwritequeue(Arg arg)
	{
		return "Server write queue: " + Net.sv.WriteQueueLength + " items / " + NumberExtensions.FormatBytes<int>(Net.sv.WriteQueueBytes, false);
	}

	[ServerVar]
	public static string printdecryptqueue(Arg arg)
	{
		return "Server decrypt queue: " + Net.sv.DecryptQueueLength + " items / " + NumberExtensions.FormatBytes<int>(Net.sv.DecryptQueueBytes, false);
	}

	[ServerVar(Help = "(Generated) Prints a sorted table of network packet types and their cumulative call counts since logging was enabled; requires packetlog_enabled = true")]
	public static string packetlog(Arg arg)
	{
		if (!packetlog_enabled)
		{
			return "Packet log is not enabled.";
		}
		List<Tuple<Message.Type, ulong>> list = new List<Tuple<Message.Type, ulong>>();
		foreach (KeyValuePair<Message.Type, TimeAverageValue> item in SingletonComponent<ServerMgr>.Instance.packetHistory.dict)
		{
			list.Add(new Tuple<Message.Type, ulong>(item.Key, item.Value.Calculate()));
		}
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumn("type");
			val.AddColumn("calls");
			foreach (Tuple<Message.Type, ulong> item2 in list.OrderByDescending((Tuple<Message.Type, ulong> entry) => entry.Item2))
			{
				if (item2.Item2 == 0L)
				{
					break;
				}
				string text = item2.Item1.ToString();
				string text2 = item2.Item2.ToString();
				val.AddRow(new string[2] { text, text2 });
			}
			return flag ? val.ToJson(true) : ((object)val).ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Prints a sorted table of RPC identifiers, their string names, and cumulative call counts; requires rpclog_enabled = true")]
	public static string rpclog(Arg arg)
	{
		if (!rpclog_enabled)
		{
			return "RPC log is not enabled.";
		}
		List<Tuple<uint, ulong>> list = new List<Tuple<uint, ulong>>();
		foreach (KeyValuePair<uint, TimeAverageValue> item in SingletonComponent<ServerMgr>.Instance.rpcHistory.dict)
		{
			list.Add(new Tuple<uint, ulong>(item.Key, item.Value.Calculate()));
		}
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("calls");
			foreach (Tuple<uint, ulong> item2 in list.OrderByDescending((Tuple<uint, ulong> entry) => entry.Item2))
			{
				if (item2.Item2 == 0L)
				{
					break;
				}
				string text = item2.Item1.ToString();
				string text2 = StringPool.Get(item2.Item1);
				string text3 = item2.Item2.ToString();
				val.AddRow(new string[3] { text, text2, text3 });
			}
			return ((object)val).ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Starts a server")]
	public static void start(Arg arg)
	{
		if (Net.sv.IsConnected())
		{
			arg.ReplyWith("There is already a server running!");
			return;
		}
		string strLevelName = arg.GetString(0, level);
		if (!LevelManager.IsValid(strLevelName))
		{
			arg.ReplyWith("Level '" + strLevelName + "' isn't valid!");
			return;
		}
		if (Object.op_Implicit((Object)(object)Object.FindObjectOfType<ServerMgr>()))
		{
			arg.ReplyWith("There is already a server running!");
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)GameManager.server.CreatePrefab("assets/bundled/prefabs/system/shared.prefab"));
		Object.DontDestroyOnLoad((Object)(object)GameManager.server.CreatePrefab("assets/bundled/prefabs/system/server.prefab"));
		((MonoBehaviour)Global.Runner).StartCoroutine(LoadImpl());
		IEnumerator LoadImpl()
		{
			yield return LevelManager.LoadLevelAsync(strLevelName);
		}
	}

	[ServerVar(Help = "Stops a server")]
	public static void stop(Arg arg)
	{
		if (!Net.sv.IsConnected())
		{
			arg.ReplyWith("There isn't a server running!");
		}
		else
		{
			Net.sv.Stop(arg.GetString(0, "Stopping Server"));
		}
	}

	[ServerVar(Help = "Backup server folder")]
	public static void backup()
	{
		DirectoryEx.Backup(backupFolder, backupFolder1, backupFolder2, backupFolder3);
		DirectoryEx.CopyAll(rootFolder, backupFolder);
	}

	public static string GetServerFolder(string folder)
	{
		string text = rootFolder + "/" + folder;
		if (Directory.Exists(text))
		{
			return text;
		}
		Directory.CreateDirectory(text);
		return text;
	}

	[ServerVar(Help = "Writes config files")]
	public static void writecfg(Arg arg)
	{
		string contents = ConsoleSystem.SaveToConfigString(bServer: true);
		File.WriteAllText(GetServerFolder("cfg") + "/serverauto.cfg", contents);
		ServerUsers.Save();
		arg.ReplyWith("Config Saved");
	}

	[ServerVar(Help = "(Generated) Prints the current server frame rate in frames per second")]
	public static void fps(Arg arg)
	{
		arg.ReplyWith(Performance.report.frameRate + " FPS");
	}

	[ServerVar(Help = "Force save the current game")]
	public static void save(Arg arg)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		foreach (BaseEntity save in BaseEntity.saveList)
		{
			save.InvalidateNetworkCache();
		}
		Debug.Log((object)("Invalidate Network Cache took " + stopwatch.Elapsed.TotalSeconds.ToString("0.00") + " seconds"));
		SaveRestore.Save(AndWait: true);
	}

	[ServerVar(Help = "(Generated) Reads and executes serverauto.cfg then server.cfg from the server's cfg folder, applying all saved convar values")]
	public static string readcfg(Arg arg)
	{
		string serverFolder = GetServerFolder("cfg");
		if (File.Exists(serverFolder + "/serverauto.cfg"))
		{
			string strFile = File.ReadAllText(serverFolder + "/serverauto.cfg");
			ConsoleSystem.RunFile(Option.Server.Quiet(), strFile);
		}
		if (File.Exists(serverFolder + "/server.cfg"))
		{
			string strFile2 = File.ReadAllText(serverFolder + "/server.cfg");
			ConsoleSystem.RunFile(Option.Server.Quiet(), strFile2);
		}
		return "Server Config Loaded";
	}

	[ServerVar(Help = "(Generated) Returns the network protocol identifier string the server is currently using; clients must match this to connect")]
	public static string netprotocol(Arg arg)
	{
		if (Net.sv == null)
		{
			return string.Empty;
		}
		return Net.sv.ProtocolId;
	}

	[ServerUserVar]
	public static void cheatreport(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			string text = arg.GetUInt64(0, 0uL).ToString();
			string text2 = arg.GetString(1);
			Debug.LogWarning((object)(((object)basePlayer)?.ToString() + " reported " + text + ": " + StringEx.ToPrintable(text2, 140)));
			EACServer.SendPlayerBehaviorReport(basePlayer, (PlayerReportsCategory)1, text, text2);
		}
	}

	[ServerVar(Help = "Get info on player corpses on the server")]
	public static void corpseinfo(Arg arg)
	{
		PlayerCorpse[] array = BaseNetworkable.serverEntities.OfType<PlayerCorpse>().ToArray();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		PlayerCorpse[] array2 = array;
		foreach (PlayerCorpse playerCorpse in array2)
		{
			if (playerCorpse.isClient)
			{
				continue;
			}
			num++;
			if (playerCorpse.CorpseIsRagdoll)
			{
				num2++;
				if (playerCorpse.CorpseRagdollScript.IsKinematic)
				{
					num3++;
				}
				else if (playerCorpse.CorpseRagdollScript.IsFullySleeping())
				{
					num4++;
				}
			}
		}
		int num5 = num2 - num3 - num4;
		float num6 = ((num2 > 0) ? ((float)num5 / (float)num2) : 0f);
		string strValue = $"Found {num} player corpses in the world, " + $"of which {num2} are using server-side ragdolls. " + string.Format("{0} of those are active ({1:0%}), {2} are sleeping, and {3} are kinematic.", new object[4] { num5, num6, num4, num3 });
		arg.ReplyWith(strValue);
	}

	[ServerAllVar(Help = "Get the player combat log")]
	public static string combatlog(Arg arg)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs() && arg.IsAdmin)
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if ((Object)(object)basePlayer == (Object)null || basePlayer.net == null)
		{
			return "invalid player";
		}
		CombatLog combat = basePlayer.stats.combat;
		int count = combatlogsize;
		bool json = arg.HasArg("--json");
		bool isAdmin = arg.IsAdmin;
		ulong requestingUser = arg.Connection?.userid ?? 0;
		return combat.Get(count, default(NetworkableId), json, isAdmin, requestingUser);
	}

	[ServerAllVar(Help = "Get the player combat log, only showing outgoing damage")]
	public static string combatlog_outgoing(Arg arg)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs() && arg.IsAdmin)
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if ((Object)(object)basePlayer == (Object)null)
		{
			return "invalid player";
		}
		return basePlayer.stats.combat.Get(combatlogsize, basePlayer.net.ID, arg.HasArg("--json"), arg.IsAdmin, arg.Connection?.userid ?? 0);
	}

	[ServerVar(Help = "Print the current player position.")]
	public static string printpos(Arg arg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs())
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if (!((Object)(object)basePlayer == (Object)null))
		{
			return ((object)((Component)basePlayer).transform.position/*cast due to constrained. prefix*/).ToString();
		}
		return "invalid player";
	}

	[ServerVar(Help = "Print the current player center position.")]
	public static string printposcenter(Arg arg)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs())
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if (!((Object)(object)basePlayer == (Object)null))
		{
			return ((object)basePlayer.GetCenter(ducked: false)/*cast due to constrained. prefix*/).ToString();
		}
		return "invalid player";
	}

	[ServerVar(Help = "Print the current player rotation.")]
	public static string printrot(Arg arg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs())
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if (!((Object)(object)basePlayer == (Object)null))
		{
			Quaternion rotation = ((Component)basePlayer).transform.rotation;
			return ((object)((Quaternion)(ref rotation)).eulerAngles/*cast due to constrained. prefix*/).ToString();
		}
		return "invalid player";
	}

	[ServerVar(Help = "Print the current player eyes.")]
	public static string printeyes(Arg arg)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.HasArgs())
		{
			basePlayer = ArgEx.GetPlayerOrSleeper(arg, 0);
		}
		if (!((Object)(object)basePlayer == (Object)null))
		{
			Quaternion rotation = basePlayer.eyes.rotation;
			return ((object)((Quaternion)(ref rotation)).eulerAngles/*cast due to constrained. prefix*/).ToString();
		}
		return "invalid player";
	}

	[ServerVar(ServerAdmin = true, Help = "This sends a snapshot of all the entities in the client's pvs. This is mostly redundant, but we request this when the client starts recording a demo.. so they get all the information.")]
	public static void snapshot(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null))
		{
			Debug.Log((object)("Sending full snapshot to " + (object)basePlayer));
			basePlayer.SendCompleteSnapshot();
		}
	}

	[ServerVar(Help = "Send network update for all players")]
	public static void sendnetworkupdate(Arg arg)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.SendNetworkUpdate();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void GetPlayerListPosTable(TextTable table)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		table.ResizeColumns(4);
		table.AddColumn("SteamID");
		table.AddColumn("DisplayName");
		table.AddColumn("POS");
		table.AddColumn("ROT");
		table.ResizeRows(BasePlayer.activePlayerList.Count);
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				table.AddValue(current.userID.Get());
				table.AddValue(current.displayName);
				table.AddValue(((Component)current).transform.position);
				table.AddValue(current.eyes.BodyForward());
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Prints the position of all players on the server")]
	public static void playerlistpos(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			GetPlayerListPosTable(val);
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Prints all the vending machines on the server")]
	public static void listvendingmachines(Arg arg)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumns(new string[3] { "EntityId", "Position", "Name" });
			foreach (VendingMachine item in BaseNetworkable.serverEntities.OfType<VendingMachine>())
			{
				val.AddRow(new string[3]
				{
					((object)Unsafe.As<NetworkableId, NetworkableId>(ref item.net.ID)/*cast due to constrained. prefix*/).ToString(),
					((object)((Component)item).transform.position/*cast due to constrained. prefix*/).ToString(),
					StringExtensions.QuoteSafe(item.shopName)
				});
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Prints all the Tool Cupboards on the server")]
	public static void listtoolcupboards(Arg arg)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			val.AddColumns(new string[3] { "EntityId", "Position", "Authed" });
			foreach (BuildingPrivlidge item in BaseNetworkable.serverEntities.OfType<BuildingPrivlidge>())
			{
				val.AddRow(new string[3]
				{
					((object)Unsafe.As<NetworkableId, NetworkableId>(ref item.net.ID)/*cast due to constrained. prefix*/).ToString(),
					((object)((Component)item).transform.position/*cast due to constrained. prefix*/).ToString(),
					item.authorizedPlayers.Count.ToString()
				});
			}
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Sends a video URL to all connected players, causing the in-game video player to open and play the specified video on every client")]
	public static void BroadcastPlayVideo(Arg arg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		string text = arg.GetString(0);
		if (string.IsNullOrWhiteSpace(text))
		{
			arg.ReplyWith("Missing video URL");
			return;
		}
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Command("client.playvideo", text);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		arg.ReplyWith($"Sent video to {BasePlayer.activePlayerList.Count} players");
	}

	[ServerVar(Help = "Rescans the serveremoji folder, note that clients will need to reconnect to get the latest emoji")]
	public static void ResetServerEmoji()
	{
		RustEmojiLibrary.ResetServerEmoji();
	}

	[ServerVar(Help = "(Generated) Returns the current number of bot (AI-controlled player) entities active on the server")]
	public static string BotCount()
	{
		return BasePlayer.bots.Count.ToString();
	}

	[ServerVar(Help = "Prints the current wipe id of the sav")]
	public static void printwipeid(Arg arg)
	{
		if (string.IsNullOrEmpty(SaveRestore.WipeId))
		{
			arg.ReplyWith("ERROR: wipe ID is null or empty!");
		}
		else
		{
			arg.ReplyWith(SaveRestore.WipeId);
		}
	}

	[ServerVar(Help = "Clears the loot spawn cache used to restrict loot into each era")]
	public static void clear_loot_spawn_cache(Arg arg)
	{
		LootContainer[] source = (from x in GameManager.server.preProcessed.prefabList.Values
			select x.GetComponent<LootContainer>() into x
			where (Object)(object)x != (Object)null
			select x).ToArray();
		LootSpawn[] array = (from x in source.Select((LootContainer x) => x.lootDefinition).Concat(from x in source.SelectMany((LootContainer x) => x.LootSpawnSlots)
				select x.definition)
			where (Object)(object)x != (Object)null
			select x).ToArray();
		LootSpawn[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			array2[num].ClearCache();
		}
		arg.ReplyWith($"Cleared {array.Length} loot spawn caches");
	}

	[ServerVar(Help = "(Generated) Kills all server-side tree entities within a given radius of the calling player (or a specified world position); args: radius [x y z]")]
	public static void clear_trees_radius(Arg arg)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		float num = arg.GetFloat(0);
		Vector3 position = (((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero);
		if (arg.HasArgs(2))
		{
			position = arg.GetVector3(1);
		}
		int num2 = 0;
		if ((Object)(object)basePlayer != (Object)null)
		{
			List<TreeEntity> list = Pool.Get<List<TreeEntity>>();
			global::Vis.Entities(position, num, list, 1073741824, (QueryTriggerInteraction)2);
			foreach (TreeEntity item in list)
			{
				item.Kill();
				num2++;
			}
			Pool.FreeUnmanaged<TreeEntity>(ref list);
		}
		arg.ReplyWith($"Deleted {num2} server tree entities within {num}m");
	}

	[ServerVar(Help = "(Generated) Kills all server-side bush entities within a given radius of the calling player (or a specified world position); args: radius [x y z]")]
	public static void clear_bushes_radius(Arg arg)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		float num = arg.GetFloat(0);
		Vector3 position = (((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero);
		if (arg.HasArgs(2))
		{
			position = arg.GetVector3(1);
		}
		int num2 = 0;
		if ((Object)(object)basePlayer != (Object)null)
		{
			PooledList<BushEntity> val = Pool.Get<PooledList<BushEntity>>();
			try
			{
				global::Vis.Entities(position, num, (List<BushEntity>)(object)val, 67108864, (QueryTriggerInteraction)2);
				foreach (BushEntity item in (List<BushEntity>)(object)val)
				{
					item.Kill();
					num2++;
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		arg.ReplyWith($"Deleted {num2} server bush entities within {num}m");
	}

	[ServerVar(Help = "Deletes items on the server that are not allowed in the era")]
	public static void enforce_era_restrictions(Arg arg)
	{
		int num = 0;
		int num2 = 0;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Item allItem in ItemManager.GetAllItems())
		{
			num++;
			if (!allItem.info.IsAllowed((EraRestriction)0))
			{
				if (!dictionary.ContainsKey(allItem.info.shortname))
				{
					dictionary.Add(allItem.info.shortname, allItem.amount);
				}
				else
				{
					dictionary[allItem.info.shortname] += allItem.amount;
				}
				allItem.Remove();
				num2++;
			}
		}
		ItemManager.DoRemoves();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"Iterated '{num}' items and removed '{num2}' restricted items");
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			stringBuilder.AppendLine($"{item.Key}: {item.Value}");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Debug command: fills all chicken coops within 5 metres of the calling player to their maximum capacity")]
	public static void fillChickenCoop(Arg arg)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		PooledList<ChickenCoop> val = Pool.Get<PooledList<ChickenCoop>>();
		try
		{
			global::Vis.Entities(((Component)basePlayer).transform.position, 5f, (List<ChickenCoop>)(object)val, 256, (QueryTriggerInteraction)2);
			foreach (ChickenCoop item in (List<ChickenCoop>)(object)val)
			{
				if (item.isServer)
				{
					item.DebugFillCoop();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Unlock all static respawn points")]
	public static void unlockrespawns(Arg arg)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0);
		if ((Object)(object)basePlayer == (Object)null)
		{
			if (arg.HasArgs())
			{
				arg.ReplyWith("Can't find player");
				return;
			}
			basePlayer = ArgEx.Player(arg);
		}
		Enumerator<SleepingBag> enumerator = SleepingBag.sleepingBags.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is StaticRespawnArea staticRespawnArea && !staticRespawnArea.IsAuthed(basePlayer.userID))
				{
					staticRespawnArea.Authorize(basePlayer.userID);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	[ServerVar(Help = "Clear all static respawn points")]
	public static void resetrespawns(Arg arg)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.GetPlayer(arg, 0);
		if ((Object)(object)basePlayer == (Object)null)
		{
			if (arg.HasArgs())
			{
				arg.ReplyWith("Can't find player");
				return;
			}
			basePlayer = ArgEx.Player(arg);
		}
		Enumerator<SleepingBag> enumerator = SleepingBag.sleepingBags.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is StaticRespawnArea staticRespawnArea && staticRespawnArea.IsAuthed(basePlayer.userID))
				{
					staticRespawnArea.Deauthorize(basePlayer.userID);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void GetPlayerReportsListTable(TextTable table)
	{
		table.ResizeColumns(4);
		table.AddColumn("NumReports");
		table.AddColumn("UserID");
		table.AddColumn("DisplayName");
		table.AddColumn("IsConnected");
		foreach (BasePlayer item in BasePlayer.allPlayerList.OrderByDescending((BasePlayer x) => x.State.numberOfTimesReported))
		{
			if (item.State.numberOfTimesReported >= 1)
			{
				table.AddValue(item.State.numberOfTimesReported);
				table.AddValue((ulong)item.userID);
				table.AddValue(item.displayName);
				table.AddValue(item.IsConnected);
			}
		}
	}

	[ServerVar(Help = "List the amount of reports players on the server have received")]
	public static void listplayerreportcounts(Arg arg)
	{
		bool flag = arg.HasArg("--json");
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.ShouldPadColumns = !flag;
			GetPlayerReportsListTable(val);
			arg.ReplyWith(flag ? val.ToJson(true) : ((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Clear the player reports list")]
	public static void clearplayerreportcounts(Arg arg)
	{
		foreach (BasePlayer allPlayer in BasePlayer.allPlayerList)
		{
			allPlayer.State.numberOfTimesReported = 0;
		}
		arg.ReplyWith("Cleared report counts");
	}

	private static void OnEraChanged()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<BaseNetworkable> enumerator = BaseNetworkable.serverEntities.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current is LootContainer lootContainer)
				{
					lootContainer.PopulateLoot();
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		SingletonComponent<SpawnHandler>.Instance?.EnforceLimits();
	}

	private static string AutoCorrectTags(string value)
	{
		List<string> inputValues = (from s in value.Split(',', StringSplitOptions.RemoveEmptyEntries)
			select s.Trim().ToLowerInvariant()).ToList();
		List<string> outputValues = new List<string>();
		Add(new string[3] { "monthly", "biweekly", "weekly" });
		Add(new string[3] { "vanilla", "hardcore", "softcore" });
		Add(new string[1] { "roleplay" });
		Add(new string[1] { "creative" });
		Add(new string[1] { "minigame" });
		Add(new string[1] { "training" });
		Add(new string[1] { "battlefield" });
		Add(new string[1] { "broyale" });
		Add(new string[1] { "builds" });
		Add(new string[7] { "NA", "SA", "EU", "WA", "EA", "OC", "AF" });
		Add(new string[1] { "tut" });
		Add(new string[1] { "premium" });
		if (!pve)
		{
			Add(new string[1] { "pve" });
		}
		return string.Join(',', outputValues);
		void Add(string[] options)
		{
			if (outputValues.Count < 4)
			{
				foreach (string text in options)
				{
					if (inputValues.Contains<string>(text, StringComparer.InvariantCultureIgnoreCase))
					{
						outputValues.Add(text);
						break;
					}
				}
			}
		}
	}
}
