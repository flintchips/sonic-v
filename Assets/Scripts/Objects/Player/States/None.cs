using System;
using Objects.Player.Constants;
using UnityEngine;

namespace Objects.Player.States
{
	public class None : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public None() : base(PlayerState.None) { }

		public override void HandleState()
		{
			// nothing...
		}
	}
}