using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class PooltableMountable : BaseMountable
{
	[HideInInspector]
	public float SplineDistance;

	[SerializeField]
	[Space]
	private ViewModel poolCueViewmodel;

	[Header("3p Cue")]
	[SerializeField]
	[Tooltip("World-space 3p cue prop, locked to the right-hand prop bone each frame (like darts' held prop).")]
	private Transform cueHeldProp;

	[Tooltip("Grip offset in prop bone space. Mostly z: how far up the shaft the hand holds the cue, which is what the stroke rotates around.")]
	[SerializeField]
	private Vector3 cueHeldPropPositionOffset;

	[SerializeField]
	[Tooltip("Cue tilt relative to the prop bone.")]
	private Vector3 cueHeldPropEulerOffset;

	[SerializeField]
	[Tooltip("Cue material for the player in seat 0 (whoever started the game). Applies to the 3p prop and the viewmodel. Leave empty to keep whatever the prefabs ship with.")]
	[Header("Cue Material")]
	private Material player1CueMaterial;

	[Tooltip("Cue material for the player in seat 1 (the joiner).")]
	[SerializeField]
	private Material player2CueMaterial;

	[SerializeField]
	private float movementSpeed;

	[Tooltip("Max spline travel per second from mouse aim. Well above movementSpeed - the mouse is the fine aim and has to feel 1:1 - but bounded so a violent flick can't spin you round the table.")]
	[SerializeField]
	private float mouseAimMaxSpeed = 8f;

	private static readonly int RightHash = Animator.StringToHash("right");

	private static readonly int StrengthHash = Animator.StringToHash("strength");

	private const float RightLerpSpeed = 8f;

	private float currentRight;

	[Tooltip("Metres of spline travel per unit of mouse movement. Deliberately tiny - the mouse is the fine aim, A/D is for walking round the table.")]
	[SerializeField]
	private float mouseAimSensitivity = 0.002f;

	[SerializeField]
	private float pullbackSensitivity = 0.01f;

	[Tooltip("Normalized cue travel per second required to turn forward movement into a shot.")]
	[SerializeField]
	private float strikeSpeedThreshold = 1f;

	[Tooltip("Minimum pullback required before a fast forward movement can strike.")]
	[SerializeField]
	private float minimumStrikePower = 0.05f;

	private float cuePullback = 0.5f;

	private float strokePower;

	private bool cueStrokeActive;

	private Pooltable _poolTable;

	private float lastSplineUpdateTime;

	private TimeSince timeSinceClipCheck;

	private const float ClipCheckInterval = 0.25f;

	private float MaxSplineSpeed => Mathf.Max(movementSpeed, mouseAimMaxSpeed);

	private Pooltable poolTable
	{
		get
		{
			if ((Object)(object)_poolTable == (Object)null)
			{
				_poolTable = GetParentEntity() as Pooltable;
			}
			return _poolTable;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PooltableMountable.OnRpcMessage"))
		{
			if (rpc == 1427800463 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UpdateSplineDistance"));
				}
				using (TimeWarning.New("RPC_UpdateSplineDistance"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1427800463u, "RPC_UpdateSplineDistance", this, player, 30uL))
						{
							return true;
						}
						if (!RPC_Server.FromMounted.Test(1427800463u, "RPC_UpdateSplineDistance", this, player))
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
							RPC_UpdateSplineDistance(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_UpdateSplineDistance");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		lastSplineUpdateTime = Time.time;
		timeSinceClipCheck = TimeSince.op_Implicit(0f);
	}

	public override bool IsSeatClipping(BaseMountable mountable)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)poolTable == (Object)null || (Object)(object)mountable == (Object)null)
		{
			return false;
		}
		return poolTable.IsWalkPoseBlocked(((Component)mountable).transform.position, ((Component)mountable).transform.rotation);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if ((Object)(object)poolTable != (Object)null)
		{
			poolTable.OnMountablePlayerLeft(player.userID);
		}
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res, bool silent = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (base.GetDismountPosition(player, out res, silent))
		{
			return true;
		}
		res = GetMountedPosition();
		return true;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerServerInput(inputState, player);
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		if (!player.IsOnGround() || WaterFactorForPlayer(player, out var _) > 0.25f || inputState.WasJustPressed(BUTTON.JUMP))
		{
			DismountAllPlayers();
		}
		else
		{
			if (TimeSince.op_Implicit(timeSinceClipCheck) < 0.25f)
			{
				return;
			}
			timeSinceClipCheck = TimeSince.op_Implicit(0f);
			if (IsSeatClipping(this))
			{
				if ((Object)(object)poolTable != (Object)null && poolTable.TryFindFreeSplineDistance(SplineDistance, out var result))
				{
					poolTable.MoveMountableTowardSplineDistance(this, result, float.MaxValue);
				}
				DismountAllPlayers();
			}
		}
	}

	[RPC_Server.FromMounted]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(30uL)]
	public void RPC_UpdateSplineDistance(RPCMessage msg)
	{
		if (!((Object)(object)poolTable == (Object)null) && !((Object)(object)msg.player == (Object)null) && poolTable.CanPlayerMove(msg.player.userID))
		{
			float num = msg.read.Float();
			if (!float.IsNaN(num) && !float.IsInfinity(num))
			{
				float num2 = Mathf.Clamp(Time.time - lastSplineUpdateTime, 0f, 0.35f);
				lastSplineUpdateTime = Time.time;
				float maxWalkDistance = MaxSplineSpeed * num2 * 1.25f;
				poolTable.MoveMountableTowardSplineDistance(this, num, maxWalkDistance);
			}
		}
	}
}
