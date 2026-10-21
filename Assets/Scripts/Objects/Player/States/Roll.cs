using UnityEngine;
using Collision = VEngine.Collision.Collision;
using Objects.Player.Constants;

namespace Objects.Player.States
{
	public class Roll : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Roll() : base(PlayerState.Roll) { }
		public override void HandleState()
		{
			self.rollstate = true;
			self.HandleGroundRotation();
			self.rotation = 0;
			self.HandleRollDeceleration();
			if (Mathf.Abs(self.groundVel) > 16) self.groundVel = 16 * Mathf.Sign(self.groundVel);
			self.applyJumpCap = false;
			if (!self.onGround)
			{
				self.CurrentState = PlayerState.Air;
				self.angle = 0;
				self.rollstate = false;
				self.HandleAirMovement();
			}

			self.animator.speed = (int)(Mathf.Abs(self.groundVel) * 40) + 48;

			if (self.animator.speed > 240) self.animator.speed = 240;

			self.GravityFalse();

			if (self.control.jumpPress)
			{
				self.rollstate = false;
				self.ActionJump();
			}

			Collision.ProcessCollisionHorizontal(self, self.outerbox, self.innerbox);

			Collision.ProcessObjectMovement(self, self.outerbox, self.innerbox);

			self.groundedStore = self.onGround;
			Collision.ProcessCollisionVertical(self, self.outerbox, self.innerbox);
		}
	}
}