using Objects.Player.Constants;
using VEngine.Entity.Constants;
using UnityEngine;
using Collision = VEngine.Collision.Collision;

namespace Objects.Player.States
{
	public class TubeAirRoll : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public TubeAirRoll() : base(PlayerState.TubeAirRoll) { }

		public override void HandleState()
		{
			self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true, 0);
			self.HandleGroundRotation();
			self.rotation = 0;

			var localLeft = self.control.left;
			var localRight = self.control.right;
			if (self.controlLock > 0)
			{
				localLeft = false;
				localRight = false;
				self.controlLock -= self.stepDelta;
			}

			self.HandleAirFriction();

			self.control.left = localLeft;
			self.control.right = localRight;
			self.applyJumpCap = false;

			self.NextGroundState = PlayerState.TubeRoll;

			if (!self.onGround)
			{
				self.HandleAirMovement();
				self.rotation = 0;
			}
			else
			{
				self.CurrentState = PlayerState.TubeRoll;

				self.GravityFalse();
			}

			Collision.ProcessCollisionHorizontal(self, self.outerbox, self.innerbox);

			Collision.ProcessObjectMovement(self, self.outerbox, self.innerbox);

			self.groundedStore = self.onGround;
			Collision.ProcessCollisionVertical(self, self.outerbox, self.innerbox);
		}
	}
}