using Objects.Player.Constants;
using Objects.Player.Input;
using UnityEngine;
using VEngine.Entity;
using Animator = VEngine.Animation.Animator;
using VEngine.Camera;

namespace Objects.Player
{
	public class PlayerVars : Entity
	{
		private static PlayerObject _leader;

		public static PlayerObject Leader
		{
			get
			{
				if (_leader) return _leader;
				_leader = FindObjectOfType<PlayerObject>();
				return _leader;
			}
			protected set => _leader = value;
		}

		public static string PlayerCurrentStateName = "";
		public bool groundedStoreOld;

		public static bool PlayerDebugMode;
		public GameObject spindashDust;

		[Header("States")] public string stateRead;
		public string nextGroundStateRead, nextAirStateRead, statePeeloutRead, stateAbilityRead;
		public PlayerState CurrentState, NextGroundState, NextAirState, StatePeelout, StateAbility;

		[Header("Enums")]
		public CharacterType characterID = CharacterType.Sonic;
		public ShieldType shield = ShieldType.None;

		public AnimationType
			animationReserve =
				AnimationType
					.AirWalk; // what anim to return to after SpringTwirl/SpringDiagonal has finished and the player is falling downwards

		public SuperState superState = SuperState.None;
		public DeathType deathType = DeathType.None;

		[HideInInspector] public bool invertGravity;
		[HideInInspector] public bool sidekick, isChibi, isTransforming, visible, flailing;
		public ControlInput control = new ControlInput();
		[HideInInspector] public bool leftStore, rightStore, upStore, downStore;
		[HideInInspector] public bool applyJumpCap;
		[HideInInspector] public float spindashCharge;
		[HideInInspector] public bool forceRespawn;
		[HideInInspector] public bool rollstate;
		[HideInInspector] public float pushing, skidding;
		[HideInInspector] public bool balancingRight, balancingLeft, balancing;

		public float timer,
			flyCarryTimer,
			controlLock,
			uncurlTimer,
			speedShoesTimer,
			blinkTimer,
			scrollDelay,
			superRingLossTimer,
			abilityTimer,
			jumpSquishTimer,
			invincibleTimer,
			skiddingDustTimer; // timers

		[HideInInspector] public int superBlendAmount;
		[HideInInspector] public int superBlendState;
		public float jumpAbilityState;
		[HideInInspector] public int scoreBonus;
		public int jumpOffset;
		[HideInInspector] public int controllerID;
		[HideInInspector] public int underwater; // 0 = not in water, 1 = in palette water, else water entityID
		[HideInInspector] public float fixedStepDelta;
		[HideInInspector] public float gravityStrength;
		[HideInInspector] public float abilitySpeed;
		[HideInInspector] public float minJogVelocity;
		[HideInInspector] public float minRunVelocity;
		[HideInInspector] public float minDashVelocity;

		[Header("Physics Variables")]
		[HideInInspector]
		public float topSpeed;

		[HideInInspector] public float acceleration;
		[HideInInspector] public float deceleration;
		[HideInInspector] public float airAcceleration;
		[HideInInspector] public float airDeceleration;
		[HideInInspector] public float skidSpeed;
		[HideInInspector] public float rollingFriction;
		[HideInInspector] public float jumpStrength;
		[HideInInspector] public float jumpCap;
		[HideInInspector] public float rollingDeceleration;
		[HideInInspector] public bool ctrlLockSlide;
		[HideInInspector] public bool jumpedThisFrame;

		[Header("References")] public Animator animator;
		public PlayerInput playerInput;

		public static PlayerCamera PlayerCamera => PlayerCamera.Instance;

		protected static readonly int[] SonicPhysicsTable = {
			0x60000, 0xC00, 0x1800, 0x600, 0x8000, 0x600, 0x68000, -0x40000, 0x30000, 0x600, 0xC00, 0x300, 0x4000,
			0x300, 0x38000, -0x20000,
			0xA0000, 0x3000, 0x6000, 0x1800, 0x10000, 0x600, 0x80000, -0x40000, 0x50000, 0x1800, 0x3000, 0xC00, 0x8000,
			0x300, 0x38000, -0x20000,
			0xC0000, 0x1800, 0x3000, 0xC00, 0x8000, 0x600, 0x68000, -0x40000, 0x60000, 0xC00, 0x1800, 0x600, 0x4000,
			0x300, 0x38000, -0x20000,
			0xC0000, 0x1800, 0x3000, 0xC00, 0x8000, 0x600, 0x80000, -0x40000, 0x60000, 0xC00, 0x1800, 0x600, 0x4000,
			0x300, 0x38000, -0x20000
		};

		// public static float GetFloatFromPhysicsTable(int[] table, int index)
		// {
		//     if (table.Length < 64) return 0;
		//
		//     var num = table[index] / 65536f;
		//     return num;
		// }

		protected static float[] ConvertHexArrayToFloatArray(int[] table)
		{
			var tableNew = new float[64];

			for (var i = 0; i < table.Length; i++)
			{
				tableNew[i] = table[i] / 65536f;
			}

			return tableNew;
		}
	}
}