using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Objects.Player.Constants;
using Objects.Player.States;
using System;

namespace Objects.Player
{
	public class PlayerStateMachine : Utilities.StateMachine<PlayerState, PlayerObject>
	{
		public PlayerStateMachine(PlayerObject self) : base(self)
		{
			InitState(PlayerState.None, new None());
			InitState(PlayerState.Ground, new Ground());
			InitState(PlayerState.Air, new Air());
			InitState(PlayerState.Hurt, new Hurt());
			InitState(PlayerState.Crouch, new Crouch());
			InitState(PlayerState.LookUp, new LookUp());
			InitState(PlayerState.JumpAbility, new JumpAbility());
			InitState(PlayerState.Spindash, new Spindash());
			InitState(PlayerState.Peelout, new Peelout());
			InitState(PlayerState.DropDash, new DropDash());
			InitState(PlayerState.Roll, new Roll());
			InitState(PlayerState.TubeRoll, new TubeRoll());
			InitState(PlayerState.TubeAirRoll, new TubeAirRoll());
		}
	}
}