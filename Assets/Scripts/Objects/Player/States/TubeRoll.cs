using Objects.Player.Constants;
using VEngine.Entity.Constants;
using UnityEngine;
using Collision = VEngine.Collision.Collision;

namespace Objects.Player.States
{
	public class TubeRoll : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public TubeRoll() : base(PlayerState.TubeRoll) { }

		public override void HandleState()
		{
			var collisonModeStore = self.collisionMode;
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

			self.HandleRollDeceleration();

			self.control.left = localLeft;
			self.control.right = localRight;
			self.applyJumpCap = false;

			self.collisionMode = collisonModeStore;

			if (self.collisionMode == CollisionState.Roof && self.angle is < 202.5f and > 157.5f)
			{
				self.collisionMode = CollisionState.Floor;
				self.CurrentState = PlayerState.TubeAirRoll;
			}
			else if (!self.onGround)
			{
				self.CurrentState = PlayerState.TubeAirRoll;
				self.HandleAirMovement();
			}
			else
			{
				self.animator.speed = (int)(Mathf.Abs(self.groundVel) * 40) + 48;

				self.GravityFalse();
			}

			Collision.ProcessObjectMovement(self, self.outerbox, self.innerbox);

			self.groundedStore = self.onGround;
			Collision.ProcessCollisionVertical(self, self.outerbox, self.innerbox);

			self.onGround = self.groundedStore;
		}
	}
}