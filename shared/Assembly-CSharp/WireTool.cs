using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class WireTool : HeldEntity
{
	public enum WireColour
	{
		Gray,
		Red,
		Green,
		Blue,
		Yellow,
		Pink,
		Purple,
		Orange,
		White,
		LightBlue,
		Invisible,
		Count
	}

	public struct PendingPlug
	{
		public IOEntity ent;

		public bool isInput;

		public int index;
	}

	private const int maxLineNodes = 16;

	private const float industrialWallOffset = 0.04f;

	public IOEntity.IOType wireType;

	public WireColour DefaultColor;

	public float radialMenuHoldTime = 0.25f;

	public float disconnectDelay = 0.15f;

	public float clearDelay = 0.65f;

	private bool justCleared;

	public GameObjectRef plugEffect;

	public SoundDefinition clearStartSoundDef;

	public SoundDefinition clearSoundDef;

	public PendingPlug pendingPlug;

	public const float MIN_SLACK = 0f;

	public const float MAX_SLACK = 2f;

	[CompilerGenerated]
	private NetworkableId _003CvalidatedWireEntity_003Ek__BackingField;

	private const float wireValidationDist = 5f;

	private const float wireValidationDistSqr = 25f;

	private const float IndustrialThickness = 0.01f;

	private bool CanChangeColours
	{
		get
		{
			IOEntity.IOType iOType = wireType;
			return iOType == IOEntity.IOType.Electric || iOType == IOEntity.IOType.Fluidic || iOType == IOEntity.IOType.Industrial;
		}
	}

	public NetworkableId validatedWireEntity
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CvalidatedWireEntity_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CvalidatedWireEntity_003Ek__BackingField = value;
		}
	}

	public int validatedWireSlot { get; private set; } = -1;

	public bool validatedWireIsInput { get; private set; }

	public unsafe override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("WireTool.OnRpcMessage"))
		{
			if (rpc == 2640128661u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_CancelPendingWire"));
				}
				using (TimeWarning.New("RPC_CancelPendingWire"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2640128661u, "RPC_CancelPendingWire", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2640128661u, "RPC_CancelPendingWire", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(2640128661u, "RPC_CancelPendingWire", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_CancelPendingWire(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_CancelPendingWire");
					}
				}
				return true;
			}
			if (rpc == 2571821359u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_MakeConnection"));
				}
				using (TimeWarning.New("RPC_MakeConnection"))
				{
					bool flag = HasUnlimitedIo(player);
					using (msg.read.UseRepeatedElementLimit(flag ? (-1) : 54))
					{
						FieldOperationLimitSuspensionScope val = msg.read.SuspendProtoFieldOperationLimit(flag);
						try
						{
							using (TimeWarning.New("Conditions"))
							{
								if (!RPC_Server.CallsPerSecond.Test(2571821359u, "RPC_MakeConnection", this, player, 5uL))
								{
									return true;
								}
								if (!RPC_Server.FromOwner.Test(2571821359u, "RPC_MakeConnection", this, player))
								{
									return true;
								}
								long position = msg.read.Position;
								WireConnectionMessage val2 = msg.read.Proto<WireConnectionMessage>((WireConnectionMessage)null);
								try
								{
									foreach (Vector3 linePoint in val2.linePoints)
									{
										if (!RPC_Server.InputValidation.Test(linePoint))
										{
											return true;
										}
									}
									foreach (WireLineAnchorInfo lineAnchor in val2.lineAnchors)
									{
										if (!RPC_Server.InputValidation.Test(lineAnchor.position))
										{
											return true;
										}
									}
									foreach (float slackLevel in val2.slackLevels)
									{
										if (!RPC_Server.InputValidation.Test(slackLevel))
										{
											return true;
										}
									}
									msg.read.Position = position;
									if (!RPC_Server.IsActiveItem.Test(2571821359u, "RPC_MakeConnection", this, player))
									{
										return true;
									}
								}
								finally
								{
									((IDisposable)val2)?.Dispose();
								}
							}
							try
							{
								using (TimeWarning.New("Call"))
								{
									RPCMessage rpc2 = new RPCMessage
									{
										connection = msg.connection,
										player = player,
										read = msg.read
									};
									RPC_MakeConnection(rpc2);
								}
							}
							catch (Exception ex2)
							{
								Debug.LogException(ex2);
								player.Kick("RPC Error in RPC_MakeConnection");
							}
						}
						finally
						{
							((IDisposable)(*(FieldOperationLimitSuspensionScope*)(&val))/*cast due to constrained. prefix*/).Dispose();
						}
					}
				}
				return true;
			}
			if (rpc == 986119119 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestChangeColor"));
				}
				using (TimeWarning.New("RPC_RequestChangeColor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(986119119u, "RPC_RequestChangeColor", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(986119119u, "RPC_RequestChangeColor", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(986119119u, "RPC_RequestChangeColor", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestChangeColor(msg3);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_RequestChangeColor");
					}
				}
				return true;
			}
			if (rpc == 1514179840 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestClear"));
				}
				using (TimeWarning.New("RPC_RequestClear"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1514179840u, "RPC_RequestClear", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1514179840u, "RPC_RequestClear", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(1514179840u, "RPC_RequestClear", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_RequestClear(msg4);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_RequestClear");
					}
				}
				return true;
			}
			if (rpc == 4283846014u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_WireStarted"));
				}
				using (TimeWarning.New("RPC_WireStarted"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4283846014u, "RPC_WireStarted", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(4283846014u, "RPC_WireStarted", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(4283846014u, "RPC_WireStarted", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_WireStarted(msg5);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_WireStarted");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public float GetMaxWireLength(BasePlayer forPlayer)
	{
		if ((Object)(object)forPlayer == (Object)null || !forPlayer.IsInCreativeMode || !Creative.unlimitedIo)
		{
			return 30f;
		}
		return 200f;
	}

	private static float CombineMinSlack(float a, float b)
	{
		float num = ((!(a > 0f) || !(b > 0f)) ? Mathf.Max(a, b) : Mathf.Min(a, b));
		return Mathf.Clamp(num, 0f, 2f);
	}

	private static float CombineMaxSlack(float a, float b)
	{
		float num = ((!(a < 2f) || !(b < 2f)) ? Mathf.Min(a, b) : Mathf.Max(a, b));
		return Mathf.Clamp(num, 0f, 2f);
	}

	private bool HasUnlimitedIo(BasePlayer player)
	{
		if (player.IsInCreativeMode)
		{
			return Creative.unlimitedIo;
		}
		return false;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	public void RPC_WireStarted(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if ((Object)(object)iOEntity == (Object)null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity))
		{
			return;
		}
		IOEntity.IOSlot[] array = (flag ? iOEntity.inputs : iOEntity.outputs);
		if (num >= 0 && num < array.Length)
		{
			Vector3 val = ((Component)iOEntity).transform.TransformPoint(array[num].handlePosition);
			if (!(Vector3.SqrMagnitude(((Component)player).transform.position - val) > 25f) && array[num].type == wireType && CanModifyEntity(player, iOEntity))
			{
				validatedWireEntity = uid;
				validatedWireSlot = num;
				validatedWireIsInput = flag;
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsActiveItem]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.InputValidation(new Type[] { typeof(WireConnectionMessage) })]
	[RPC_Server.MaxRepeatedElements(54)]
	[RPC_Server.IgnoreConditional("HasUnlimitedIo", new Type[]
	{
		typeof(RPC_Server.MaxRepeatedElements),
		typeof(RPC_Server.IgnoreProtoFieldOperationLimit)
	})]
	public void RPC_MakeConnection(RPCMessage rpc)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = rpc.player;
		WireConnectionMessage val = rpc.read.Proto<WireConnectionMessage>((WireConnectionMessage)null);
		try
		{
			List<Vector3> linePoints = val.linePoints;
			int inputIndex = val.inputIndex;
			int outputIndex = val.outputIndex;
			IOEntity iOEntity = new EntityRef<IOEntity>(val.inputID).Get(serverside: true);
			IOEntity iOEntity2 = new EntityRef<IOEntity>(val.outputID).Get(serverside: true);
			if ((Object)(object)iOEntity == (Object)null || (Object)(object)iOEntity2 == (Object)null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity.CanSkipWireToolBuildAuthorisation() ? iOEntity : iOEntity2) || !SharesRootParent(iOEntity, iOEntity2) || !ValidateLine(linePoints, iOEntity, iOEntity2, player, outputIndex) || inputIndex >= iOEntity.inputs.Length || outputIndex >= iOEntity2.outputs.Length || (Object)(object)iOEntity.inputs[inputIndex].connectedTo.Get() != (Object)null || (Object)(object)iOEntity2.outputs[outputIndex].connectedTo.Get() != (Object)null || (iOEntity.inputs[inputIndex].rootConnectionsOnly && !iOEntity2.IsRootEntity()) || !CanModifyEntity(player, iOEntity) || !CanModifyEntity(player, iOEntity2))
			{
				return;
			}
			NetworkableId val2 = (validatedWireIsInput ? val.inputID : val.outputID);
			int num = (validatedWireIsInput ? inputIndex : outputIndex);
			if (val2 != validatedWireEntity || num != validatedWireSlot)
			{
				return;
			}
			validatedWireEntity = default(NetworkableId);
			validatedWireSlot = -1;
			List<float> slackLevels = val.slackLevels;
			if (slackLevels.Count != linePoints.Count)
			{
				return;
			}
			for (int i = 0; i < slackLevels.Count; i++)
			{
				if (slackLevels[i] < 0f || slackLevels[i] > 2f)
				{
					return;
				}
			}
			float num2 = CombineMinSlack(Mathf.Clamp(iOEntity.GetMinWireSlack(isInput: true, inputIndex), 0f, 2f), Mathf.Clamp(iOEntity2.GetMinWireSlack(isInput: false, outputIndex), 0f, 2f));
			float num3 = CombineMaxSlack(Mathf.Clamp(iOEntity.GetMaxWireSlack(isInput: true, inputIndex), 0f, 2f), Mathf.Clamp(iOEntity2.GetMaxWireSlack(isInput: false, outputIndex), 0f, 2f));
			num3 = Mathf.Max(num3, num2);
			if (num2 > 0f || num3 < 2f)
			{
				if (slackLevels.Count < 2)
				{
					return;
				}
				int index = (validatedWireIsInput ? (slackLevels.Count - 2) : 0);
				float num4 = slackLevels[index];
				if (num4 < num2 - 0.001f || num4 > num3 + 0.001f)
				{
					return;
				}
			}
			IOEntity.LineAnchor[] array = new IOEntity.LineAnchor[val.lineAnchors.Count];
			if (!ValidateLineAnchors(iOEntity, val.lineAnchors, array, linePoints, player))
			{
				return;
			}
			WireColour wireColour = IntToColour(val.wireColor);
			if (Interface.CallHook("OnWireConnect", player, iOEntity, inputIndex, iOEntity2, outputIndex, val.linePoints, slackLevels) == null)
			{
				if (wireColour == WireColour.Invisible && !player.IsInCreativeMode)
				{
					wireColour = DefaultColor;
				}
				iOEntity2.ConnectTo(iOEntity, outputIndex, inputIndex, linePoints, slackLevels, array, wireColour);
				if (wireType == IOEntity.IOType.Industrial)
				{
					iOEntity.NotifyIndustrialNetworkChanged();
					iOEntity2.NotifyIndustrialNetworkChanged();
				}
				Facepunch.Rust.Analytics.Azure.OnIOEntityConnected(player, iOEntity, iOEntity2, wireColour);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool ValidateLineAnchors(IOEntity ioEnt, List<WireLineAnchorInfo> lineAnchors, IOEntity.LineAnchor[] receivedAnchors, List<Vector3> linePoints, BasePlayer ply)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < lineAnchors.Count; i++)
		{
			WireLineAnchorInfo val = lineAnchors[i];
			if (val.index < 0 || val.index >= linePoints.Count)
			{
				return false;
			}
			EntityRef<Door> entityRef = new EntityRef<Door>(val.parentID);
			Door door = entityRef.Get(serverside: true);
			if ((Object)(object)door == (Object)null || (Object)(object)door.model == (Object)null)
			{
				return false;
			}
			float num = 35f;
			if (Vector3.Distance(((Component)door).transform.position, ((Component)ioEnt).transform.position) > num)
			{
				return false;
			}
			if (string.IsNullOrEmpty(val.boneName) || !door.model.HasBone(val.boneName, out var _))
			{
				return false;
			}
			Door door2 = door.LookupPrefab<Door>();
			if ((Object)(object)door2 == (Object)null || (Object)(object)door2.model == (Object)null || !door2.model.HasBone(val.boneName, out var bone2))
			{
				return false;
			}
			Matrix4x4 val2 = ((Component)door2).transform.worldToLocalMatrix * bone2.localToWorldMatrix;
			Vector3 val3 = ((Matrix4x4)(ref val2)).MultiplyPoint3x4(val.position);
			Bounds val4 = door.bounds;
			((Bounds)(ref val4)).Expand(0.25f);
			if (!((Bounds)(ref val4)).Contains(val3))
			{
				return false;
			}
			receivedAnchors[i].entityRef = entityRef;
			receivedAnchors[i].boneName = val.boneName;
			receivedAnchors[i].index = (int)val.index;
			receivedAnchors[i].position = val.position;
		}
		return true;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void RPC_RequestClear(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		int num = msg.read.Int32();
		bool flag = msg.read.Bit();
		bool flag2 = msg.read.Bit();
		bool flag3 = msg.read.Bit();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if ((Object)(object)iOEntity == (Object)null || !CanPlayerUseWires(player, cached: false, 1f, iOEntity))
		{
			return;
		}
		IOEntity.IOSlot[] array = (flag ? iOEntity.inputs : iOEntity.outputs);
		if (num >= 0 && num < array.Length)
		{
			Vector3 val = ((Component)iOEntity).transform.TransformPoint(array[num].handlePosition);
			if (Vector3.SqrMagnitude(((Component)player).transform.position - val) > 25f)
			{
				return;
			}
		}
		else if (Vector3.SqrMagnitude(((Component)player).transform.position - ((Component)iOEntity).transform.position) > 25f)
		{
			return;
		}
		if (!GamePhysics.LineOfSight(player.eyes.center, player.eyes.position, 1218519041) || (!iOEntity.IsVisible(player.eyes.HeadRay(), 1218519041, 5f) && !iOEntity.IsVisible(player.eyes.position, 5f)))
		{
			return;
		}
		WireReconnectMessage val2 = Pool.Get<WireReconnectMessage>();
		if (flag2)
		{
			if (num < 0 || num >= array.Length)
			{
				val2.Dispose();
				return;
			}
			IOEntity.IOSlot iOSlot = array[num];
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if ((Object)(object)iOEntity2 == (Object)null)
			{
				val2.Dispose();
				return;
			}
			IOEntity.IOSlot iOSlot2 = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
			val2.isInput = !flag;
			val2.slotIndex = iOSlot.connectedToSlot;
			val2.otherEntityId = iOEntity2.net.ID;
			val2.wireColor = (int)iOSlot.wireColour;
			val2.linePoints = Pool.Get<List<Vector3>>();
			val2.slackLevels = Pool.Get<List<float>>();
			val2.lineAnchors = Pool.Get<List<WireLineAnchorInfo>>();
			val2.clearedEntityId = iOEntity.net.ID;
			IOEntity iOEntity3 = iOEntity;
			Vector3[] array2 = iOSlot.linePoints;
			if (array2 == null || array2.Length == 0)
			{
				iOEntity3 = iOEntity2;
				array2 = iOSlot2.linePoints;
			}
			if (array2 == null)
			{
				array2 = Array.Empty<Vector3>();
			}
			bool flag4 = (Object)(object)iOEntity3 != (Object)(object)iOEntity;
			if (((Object)(object)iOEntity == (Object)(object)iOEntity3) & flag)
			{
				flag4 = true;
			}
			val2.linePoints.AddRange(array2);
			float[] slackLevels = iOSlot.slackLevels;
			if (slackLevels == null || slackLevels.Length == 0)
			{
				slackLevels = iOSlot2.slackLevels;
			}
			float[] array3 = slackLevels;
			foreach (float item in array3)
			{
				val2.slackLevels.Add(item);
			}
			IOEntity.LineAnchor[] lineAnchors = iOSlot.lineAnchors;
			if (lineAnchors == null || lineAnchors.Length == 0)
			{
				lineAnchors = iOSlot2.lineAnchors;
			}
			if (lineAnchors != null)
			{
				IOEntity.LineAnchor[] array4 = lineAnchors;
				for (int i = 0; i < array4.Length; i++)
				{
					IOEntity.LineAnchor lineAnchor = array4[i];
					EntityRef<Door> entityRef = lineAnchor.entityRef;
					if (entityRef.Get(serverside: true).IsValid())
					{
						val2.lineAnchors.Add(lineAnchor.ToInfo());
					}
				}
			}
			val2.slackLevels.RemoveAt(val2.slackLevels.Count - 1);
			if (flag4)
			{
				val2.linePoints.Reverse();
				val2.slackLevels.Reverse();
				int num2 = val2.linePoints.Count - 1;
				foreach (WireLineAnchorInfo lineAnchor2 in val2.lineAnchors)
				{
					lineAnchor2.index = num2 - lineAnchor2.index;
				}
			}
			if (val2.lineAnchors.Count >= 0)
			{
				List<WireLineAnchorInfo> list = Pool.Get<List<WireLineAnchorInfo>>();
				foreach (WireLineAnchorInfo lineAnchor3 in val2.lineAnchors)
				{
					if (lineAnchor3.index == 0L || lineAnchor3.index == val2.linePoints.Count - 1)
					{
						list.Add(lineAnchor3);
					}
				}
				foreach (WireLineAnchorInfo item2 in list)
				{
					val2.lineAnchors.Remove(item2);
				}
				Pool.Free<WireLineAnchorInfo>(ref list, false);
			}
			if (val2.linePoints.Count >= 0)
			{
				val2.linePoints.RemoveAt(0);
				val2.linePoints.RemoveAt(val2.linePoints.Count - 1);
			}
			if (val2.slackLevels.Count >= 0)
			{
				val2.slackLevels.RemoveAt(val2.slackLevels.Count - 1);
			}
		}
		if (AttemptClearSlot(iOEntity, player, num, flag))
		{
			if (flag2)
			{
				if (validatedWireEntity == default(NetworkableId))
				{
					validatedWireEntity = val2.otherEntityId;
					validatedWireSlot = val2.slotIndex;
					validatedWireIsInput = val2.isInput;
				}
				ClientRPC(RpcTarget.Player("RPC_OnWireDisconnected", player), val2);
			}
			else if (!flag3)
			{
				validatedWireEntity = default(NetworkableId);
				validatedWireSlot = -1;
			}
		}
		val2.Dispose();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void RPC_CancelPendingWire(RPCMessage msg)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		validatedWireEntity = default(NetworkableId);
		validatedWireSlot = -1;
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void RPC_RequestChangeColor(RPCMessage msg)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		NetworkableId uid = msg.read.EntityID();
		IOEntity iOEntity = BaseNetworkable.serverEntities.Find(uid) as IOEntity;
		if ((Object)(object)iOEntity == (Object)null || Vector3.SqrMagnitude(((Component)player).transform.position - ((Component)iOEntity).transform.position) > 25f || !CanModifyEntity(player, iOEntity))
		{
			return;
		}
		int index = msg.read.Int32();
		bool flag = msg.read.Bit();
		WireColour wireColour = IntToColour(msg.read.Int32());
		if (wireColour == WireColour.Invisible && !player.IsInCreativeMode)
		{
			return;
		}
		IOEntity.IOSlot iOSlot = (flag ? iOEntity.inputs.ElementAtOrDefault(index) : iOEntity.outputs.ElementAtOrDefault(index));
		if (iOSlot != null)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			if (!((Object)(object)iOEntity2 == (Object)null))
			{
				IOEntity.IOSlot obj = (flag ? iOEntity2.outputs : iOEntity2.inputs)[iOSlot.connectedToSlot];
				iOSlot.wireColour = wireColour;
				iOEntity.SendNetworkUpdate();
				obj.wireColour = wireColour;
				iOEntity2.SendNetworkUpdate();
			}
		}
	}

	public static bool AttemptClearSlot(BaseNetworkable clearEnt, BasePlayer ply, int clearIndex, bool isInput)
	{
		IOEntity iOEntity = (((Object)(object)clearEnt != (Object)null) ? ((Component)clearEnt).GetComponent<IOEntity>() : null);
		IOEntity iOEntity2 = (IOEntity)(object)(isInput ? iOEntity.inputs[clearIndex] : iOEntity.outputs[clearIndex]);
		if ((Object)(object)((IOEntity.IOSlot)(object)iOEntity2).connectedTo.Get() == (Object)null)
		{
			return false;
		}
		iOEntity2 = ((IOEntity.IOSlot)(object)iOEntity2).connectedTo.Get();
		object obj = Interface.CallHook("OnWireClear", ply, iOEntity, clearIndex, iOEntity2, isInput);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)iOEntity == (Object)null)
		{
			return false;
		}
		if ((Object)(object)ply != (Object)null && !CanClearEntity(ply, iOEntity, clearIndex, isInput))
		{
			return false;
		}
		return iOEntity.Disconnect(clearIndex, isInput);
	}

	public WireColour IntToColour(int i)
	{
		if (i < 0)
		{
			i = 0;
		}
		if (i > 11)
		{
			i = 10;
		}
		i %= 11;
		return (WireColour)i;
	}

	public bool ValidateLine(List<Vector3> lineList, IOEntity inputEntity, IOEntity outputEntity, BasePlayer byPlayer, int outputIndex)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)byPlayer != (Object)null && byPlayer.IsInCreativeMode && Creative.unlimitedIo)
		{
			return true;
		}
		if (lineList.Count < 2 || lineList.Count > 18)
		{
			return false;
		}
		if ((Object)(object)inputEntity == (Object)null || (Object)(object)outputEntity == (Object)null)
		{
			return false;
		}
		Vector3 val = lineList[0];
		float num = 0f;
		int count = lineList.Count;
		float maxWireLength = GetMaxWireLength(byPlayer);
		for (int i = 1; i < count; i++)
		{
			Vector3 val2 = lineList[i];
			num += Vector3.Distance(val, val2);
			if (num > maxWireLength)
			{
				return false;
			}
			val = val2;
		}
		Vector3 val3 = lineList[count - 1];
		Bounds val4 = outputEntity.bounds;
		((Bounds)(ref val4)).Expand(0.5f);
		if (!((Bounds)(ref val4)).Contains(val3))
		{
			return false;
		}
		Vector3 val5 = ((Component)outputEntity).transform.TransformPoint(lineList[0]);
		val3 = ((Component)inputEntity).transform.InverseTransformPoint(val5);
		Bounds val6 = inputEntity.bounds;
		((Bounds)(ref val6)).Expand(0.5f);
		if (!((Bounds)(ref val6)).Contains(val3))
		{
			return false;
		}
		if ((Object)(object)byPlayer == (Object)null)
		{
			return false;
		}
		Vector3 position = ((Component)outputEntity).transform.TransformPoint(lineList[lineList.Count - 1]);
		if (byPlayer.Distance(position) > 5f && byPlayer.Distance(val5) > 5f)
		{
			return false;
		}
		if (outputIndex >= 0 && outputIndex < outputEntity.outputs.Length && outputEntity.outputs[outputIndex].type == IOEntity.IOType.Industrial && !VerifyLineOfSight(lineList, ((Component)outputEntity).transform.localToWorldMatrix))
		{
			return false;
		}
		return true;
	}

	public bool VerifyLineOfSight(List<Vector3> positions, Matrix4x4 localToWorldSpace)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldSpaceA = ((Matrix4x4)(ref localToWorldSpace)).MultiplyPoint3x4(positions[0]);
		for (int i = 1; i < positions.Count; i++)
		{
			Vector3 val = ((Matrix4x4)(ref localToWorldSpace)).MultiplyPoint3x4(positions[i]);
			if (!VerifyLineOfSight(worldSpaceA, val))
			{
				return false;
			}
			worldSpaceA = val;
		}
		return true;
	}

	public bool VerifyLineOfSight(Vector3 worldSpaceA, Vector3 worldSpaceB)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float maxDistance = Vector3.Distance(worldSpaceA, worldSpaceB);
		Vector3 val = worldSpaceA - worldSpaceB;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(worldSpaceB, normalized), 0.01f, list, maxDistance, 136380672, (QueryTriggerInteraction)0);
		bool result = true;
		foreach (RaycastHit item in list)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((Object)(object)entity != (Object)null && RaycastHitEx.IsOnLayer(item, (Layer)8))
			{
				if (entity is VendingMachine)
				{
					result = false;
					break;
				}
			}
			else if (!((Object)(object)entity != (Object)null) || !(entity is Door))
			{
				result = false;
				break;
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		return result;
	}

	public bool HasPendingPlug()
	{
		if ((Object)(object)pendingPlug.ent != (Object)null)
		{
			return pendingPlug.index != -1;
		}
		return false;
	}

	public bool PendingPlugIsInput()
	{
		if ((Object)(object)pendingPlug.ent != (Object)null && pendingPlug.index != -1)
		{
			return pendingPlug.isInput;
		}
		return false;
	}

	public bool PendingPlugIsType(IOEntity.IOType type)
	{
		if ((Object)(object)pendingPlug.ent == (Object)null || pendingPlug.index == -1)
		{
			return false;
		}
		IOEntity.IOSlot[] array = (pendingPlug.isInput ? pendingPlug.ent.inputs : pendingPlug.ent.outputs);
		if (pendingPlug.index < 0 || pendingPlug.index >= array.Length)
		{
			return false;
		}
		return array[pendingPlug.index].type == type;
	}

	public bool PendingPlugIsOutput()
	{
		if ((Object)(object)pendingPlug.ent != (Object)null && pendingPlug.index != -1)
		{
			return !pendingPlug.isInput;
		}
		return false;
	}

	public bool PendingPlugIsRoot()
	{
		if ((Object)(object)pendingPlug.ent != (Object)null)
		{
			return pendingPlug.ent.IsRootEntity();
		}
		return false;
	}

	private void ResetPendingPlug()
	{
		pendingPlug.ent = null;
		pendingPlug.index = -1;
	}

	public static bool CanPlayerUseWires(BasePlayer player, bool cached = false, float cacheDuration = 1f, IOEntity targetIoEnt = null)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("CanUseWires", player, cached, cacheDuration, targetIoEnt);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if ((Object)(object)player != (Object)null && player.IsInCreativeMode && Creative.unlimitedIo)
		{
			return true;
		}
		if (!player.TryGetHeldEntity(out WireTool heldEntity))
		{
			return false;
		}
		if (!heldEntity.CanBeUsedInWater() && player.IsSwimming())
		{
			return false;
		}
		if (!player.CanBuild(cached, cacheDuration) && ((Object)(object)targetIoEnt == (Object)null || !targetIoEnt.CanSkipWireToolBuildAuthorisation()))
		{
			return false;
		}
		if (player.FindTrigger<TriggerMonumentIOArea>(out var _))
		{
			return true;
		}
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(player.eyes.position, 0.1f, list, 536870912, (QueryTriggerInteraction)2);
		bool result2 = true;
		foreach (Collider item in list)
		{
			if (!((Component)item).gameObject.CompareTag("IgnoreWireCheck"))
			{
				result2 = false;
				break;
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return result2;
	}

	private static bool CanModifyEntity(BasePlayer player, IOEntity ent)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (ent.AllowWireConnections())
		{
			if (!player.CanBuild(((Component)ent).transform.position, ((Component)ent).transform.rotation, ent.bounds))
			{
				if (player.IsInCreativeMode)
				{
					return Creative.unlimitedIo;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool CanClearEntity(BasePlayer player, IOEntity ent, int slotIndex, bool isInput)
	{
		if (ent.AllowWireConnections())
		{
			if (!ent.CanBreakConnection(player, slotIndex, isInput))
			{
				if (player.IsInCreativeMode)
				{
					return Creative.unlimitedIo;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool SharesRootParent(BaseEntity a, BaseEntity b)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		bool flag = a.IsOnMovingObject();
		bool flag2 = b.IsOnMovingObject();
		if (flag & flag2)
		{
			if (IsCarFuelTank(a) && IsCarFuelTank(b))
			{
				return true;
			}
			return (Object)(object)a.GetRootParentEntity() == (Object)(object)b.GetRootParentEntity();
		}
		if ((flag | flag2) && !AllowDifferentParentConnections(a) && !AllowDifferentParentConnections(b))
		{
			return false;
		}
		BoatBuildingStation stationOverlappingPosition = BoatBuildingStation.GetStationOverlappingPosition(((Component)a).transform.position, a.isServer, 1.5f);
		BoatBuildingStation stationOverlappingPosition2 = BoatBuildingStation.GetStationOverlappingPosition(((Component)b).transform.position, b.isServer, 1.5f);
		return (Object)(object)stationOverlappingPosition == (Object)(object)stationOverlappingPosition2;
	}

	private static bool AllowDifferentParentConnections(BaseEntity ent)
	{
		if (IsCarFuelTank(ent))
		{
			return true;
		}
		return false;
	}

	private static bool IsCarFuelTank(BaseEntity ent)
	{
		if (ent is LiquidContainer)
		{
			return ent.GetParentEntity() is VehicleModuleStorage;
		}
		return false;
	}
}
