using System;
using Objects.Player.Constants;
using UnityEngine;
using VEngine.Audio;
using VEngine.Entity.Constants;
using Collision = VEngine.Collision.Collision;

namespace Objects.Player.States
{
	public class Ground : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Ground() : base(PlayerState.Ground) { }

		private const double Tolerance = 0.0001;
		public override void HandleState()
		{
			if (!self.onGround)
			{
				self.CurrentState = PlayerState.Air;
				self.HandleAirMovement();
			}
			else
			{
				if (self.groundVel == 0)
				{
					if (self.control.left)
						self.pushing -= self.stepDelta;
					else if (self.control.right)
						self.pushing += self.stepDelta;
					else
						self.pushing = 0;
				}
				else
				{
					if (!self.control.left && !self.control.right)
						self.pushing = 0;
					else if (Mathf.Abs(self.groundVel) > 1f) self.pushing = 0;
				}

				if (self.invertGravity)
				{
					if (self.collisionMode != CollisionState.Roof) self.pushing = 0;
				}
				else if (self.collisionMode != CollisionState.Floor) self.pushing = 0;

				self.HandleGroundRotation();
				self.HandleGroundMovement();

				Collision.ProcessCollisionHorizontal(self, self.outerbox, self.innerbox);

				Collision.ProcessObjectMovement(self, self.outerbox, self.innerbox);

				self.groundedStore = self.onGround;
				Collision.ProcessCollisionVertical(self, self.outerbox, self.innerbox);

				self.GravityFalse();

				self.NextAirState = PlayerState.Air;

				self.HandleGroundAnimation();

				if (self.control.jumpPress)
				{
					self.ActionJump();
					self.timer = 0;
				}
				else
				{
					if (self.groundVel != 0)
					{
						var minRollVel = self.CurrentState == PlayerState.Crouch ? 1.0625f : 0.53125f;
						if (!(Mathf.Abs(self.groundVel) >= minRollVel) || self.control.left || self.control.right || !self.control.down) return;
						self.ActionRoll();
						AudioManager.PlayPersistant("Roll");
					}
					else if (((self.angle is < 45f or > 315f) && (self.collisionMode == CollisionState.Floor)) ||
							 (self.invertGravity && Math.Abs(self.angle - 180f) < Tolerance))
					{
						if (self.control.up)
						{
							self.animator.SetSpriteAnimation(self.characterID, AnimationType.LookUp, false, 0);
							self.timer = 0;
							self.CurrentState = PlayerState.LookUp;
						}
						else if (self.control.down)
						{
							self.animator.SetSpriteAnimation(self.characterID, AnimationType.Crouch, false, 0);
							self.timer = 0;
							self.CurrentState = PlayerState.Crouch;
						}
					}
				}
			}
		}
	}
}