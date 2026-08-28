using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class NeonSign : Signage
{
	private const float FastSpeed = 0.5f;

	private const float MediumSpeed = 1f;

	private const float SlowSpeed = 2f;

	private const float MinSpeed = 0.5f;

	private const float MaxSpeed = 5f;

	[Header("Neon Sign")]
	public Light topLeft;

	public Light topRight;

	public Light bottomLeft;

	public Light bottomRight;

	public float lightIntensity = 2f;

	[Range(1f, 100f)]
	public int powerConsumption = 10;

	public Material activeMaterial;

	public Material inactiveMaterial;

	public float animationSpeed = 1f;

	public int currentFrame;

	public List<Lights> frameLighting;

	public const Flags Flag_HasAuxPower = Flags.Reserved9;

	public bool isAnimating;

	public Action animationLoopAction;

	private int[] cachedInputs;

	private readonly List<int> inputFrameHistory = new List<int>();

	public AmbienceEmitter ambientSoundEmitter;

	public SoundDefinition switchSoundDef;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("NeonSign.OnRpcMessage"))
		{
			if (rpc == 2433901419u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetAnimationSpeed"));
				}
				using (TimeWarning.New("SetAnimationSpeed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2433901419u, "SetAnimationSpeed", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2433901419u, "SetAnimationSpeed", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetAnimationSpeed(rPCMessage);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SetAnimationSpeed");
					}
				}
				return true;
			}
			if (rpc == 1919786296 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdateNeonColors"));
				}
				using (TimeWarning.New("UpdateNeonColors"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1919786296u, "UpdateNeonColors", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1919786296u, "UpdateNeonColors", this, player, 3f))
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
							UpdateNeonColors(msg2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in UpdateNeonColors");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return powerConsumption;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.neonSign == null)
		{
			return;
		}
		if (frameLighting != null)
		{
			foreach (Lights item in frameLighting)
			{
				item.Dispose();
			}
			Pool.Free<Lights>(ref frameLighting, false);
		}
		frameLighting = info.msg.neonSign.frameLighting;
		info.msg.neonSign.frameLighting = null;
		currentFrame = Mathf.Clamp(info.msg.neonSign.currentFrame, 0, paintableSources.Length);
		animationSpeed = Mathf.Clamp(info.msg.neonSign.animationSpeed, 0.5f, 5f);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		animationLoopAction = SwitchToNextFrame;
		cachedInputs = new int[inputs.Length];
	}

	public override void ResetState()
	{
		base.ResetState();
		CancelInvoke(animationLoopAction);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
		else
		{
			if (inputSlot <= 0)
			{
				return;
			}
			bool b = false;
			for (int i = 1; i < cachedInputs.Length; i++)
			{
				if (cachedInputs[i] >= powerConsumption)
				{
					b = true;
					break;
				}
			}
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved9, b);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (inputSlot >= 0 && inputSlot < cachedInputs.Length)
		{
			cachedInputs[inputSlot] = inputAmount;
		}
		base.UpdateFromInput(inputAmount, inputSlot);
		if (inputSlot > 0)
		{
			if (inputAmount > 0)
			{
				inputFrameHistory.Remove(inputSlot);
				inputFrameHistory.Add(inputSlot);
			}
			else
			{
				inputFrameHistory.Remove(inputSlot);
			}
		}
		int num = 0;
		int num2 = -1;
		if (HasFlag(Flags.Reserved9))
		{
			for (int i = 1; i < cachedInputs.Length; i++)
			{
				int num3 = cachedInputs[i];
				if (num3 > num)
				{
					num = num3;
					num2 = i - 1;
				}
				else if (num3 == num && num3 > 0)
				{
					int item = num2 + 1;
					if (inputFrameHistory.IndexOf(i) > inputFrameHistory.IndexOf(item))
					{
						num2 = i - 1;
					}
				}
			}
		}
		if (inputSlot == 0 && cachedInputs[0] != inputAmount)
		{
			cachedInputs[0] = inputAmount;
		}
		bool flag = HasFlag(Flags.Reserved8);
		if (flag && inputSlot == 0)
		{
			cachedInputs[0] = inputAmount;
			if (!isAnimating)
			{
				InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
				isAnimating = true;
			}
			if (num2 >= 0)
			{
				currentFrame = num2;
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
			}
		}
		if (HasFlag(Flags.Reserved9))
		{
			bool num4 = !flag && inputSlot == 0;
			bool flag2 = inputSlot > 0 && inputSlot - 1 == num2;
			bool flag3 = inputAmount == 0 && num2 != currentFrame && !flag;
			if (num4 | flag2 | flag3)
			{
				currentFrame = num2;
				if (isAnimating & flag)
				{
					CancelInvoke(animationLoopAction);
					InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
				}
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
				if (isAnimating && !flag)
				{
					CancelInvoke(animationLoopAction);
					isAnimating = false;
				}
			}
		}
		if (isAnimating && !flag)
		{
			CancelInvoke(animationLoopAction);
			isAnimating = false;
			MarkDirty();
		}
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		if (inputIndex == 0)
		{
			return powerConsumption;
		}
		if (!HasFlag(Flags.Reserved8))
		{
			return powerConsumption;
		}
		return 0;
	}

	private void SwitchToFrame(int index)
	{
		if (index >= 0 && index < paintableSources.Length)
		{
			int num = currentFrame;
			currentFrame = index;
			if (currentFrame != num && textureIDs[currentFrame] != 0)
			{
				ClientRPC(RpcTarget.NetworkGroup("SetFrame"), currentFrame);
			}
		}
	}

	private void SwitchToNextFrame()
	{
		int index = (currentFrame + 1) % paintableSources.Length;
		SwitchToFrame(index);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		List<Lights> list = Pool.Get<List<Lights>>();
		if (frameLighting != null)
		{
			foreach (Lights item in frameLighting)
			{
				list.Add(item.Copy());
			}
		}
		info.msg.neonSign = Pool.Get<NeonSign>();
		info.msg.neonSign.frameLighting = list;
		info.msg.neonSign.currentFrame = currentFrame;
		info.msg.neonSign.animationSpeed = animationSpeed;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	public void SetAnimationSpeed(RPCMessage msg)
	{
		float num = Mathf.Clamp(msg.read.Float(), 0.5f, 5f);
		if (!FloatEx.IsNaN(num))
		{
			animationSpeed = num;
			if (isAnimating)
			{
				CancelInvoke(animationLoopAction);
				InvokeRepeating(animationLoopAction, animationSpeed, animationSpeed);
			}
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void UpdateNeonColors(RPCMessage msg)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (CanUpdateSign(msg.player))
		{
			int num = msg.read.Int32();
			if (num >= 0 && num < paintableSources.Length)
			{
				EnsureInitialized();
				frameLighting[num].topLeft = ClampColor(msg.read.Color());
				frameLighting[num].topRight = ClampColor(msg.read.Color());
				frameLighting[num].bottomLeft = ClampColor(msg.read.Color());
				frameLighting[num].bottomRight = ClampColor(msg.read.Color());
				SendNetworkUpdate();
			}
		}
	}

	public new void EnsureInitialized()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (frameLighting == null)
		{
			frameLighting = Pool.Get<List<Lights>>();
		}
		while (frameLighting.Count < paintableSources.Length)
		{
			Lights val = Pool.Get<Lights>();
			val.topLeft = Color.clear;
			val.topRight = Color.clear;
			val.bottomLeft = Color.clear;
			val.bottomRight = Color.clear;
			frameLighting.Add(val);
		}
	}

	private static Color ClampColor(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		return new Color(FloatEx.IsNaN(color.r) ? 0f : Mathf.Clamp01(color.r), FloatEx.IsNaN(color.g) ? 0f : Mathf.Clamp01(color.g), FloatEx.IsNaN(color.b) ? 0f : Mathf.Clamp01(color.b), FloatEx.IsNaN(color.a) ? 0f : Mathf.Clamp01(color.a));
	}
}
