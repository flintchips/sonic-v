using Objects.Player.Constants;
using UnityEngine;
using VEngine.Entity.Constants;
using VEngine.Math;
using Collision = VEngine.Collision.Collision;
using VEngine.Camera;
using VEngine.Utilities.Helpers;

namespace Objects.Player.States
{
	public class Air : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Air() : base(PlayerState.Air) { }
		public override void HandleState()
		{
			self.HandleAirFriction();

			if (self.onGround)
			{
				self.CurrentState = PlayerState.Ground;

				self.GravityFalse();
			}
			else
			{
				self.HandleAirMovement();
				self.NextGroundState = PlayerState.Ground;

				if (self.velocity.y > 0)
				{
					if (self.animator.animationID >= AnimationType.SpringTwirl)
					{
						if (self.animator.animationID <= AnimationType.SpringDiagonal)
						{
							self.animator.SetSpriteAnimation(self.characterID, self.animationReserve, true, 0);
						}
						else if (self.animator.animationID == AnimationType.SpringCs && (self.animator.frameID == 0))
						{
							self.animator.SetSpriteAnimation(self.characterID, AnimationType.AirWalk, true, 0);
						}
					}
				}

				switch (self.animator.animationID)
				{
					case AnimationType.Idle:
					case AnimationType.Walk:
						if (self.animator.speed < 64) self.animator.speed = 64;
						self.animator.SetSpriteAnimation(self.characterID, AnimationType.Walk, true, self.animator.frameID);
						break;

					case AnimationType.LookUp:
					case AnimationType.Crouch:
					case AnimationType.SkidTurn:
						if (self.animator.speed < 64) self.animator.speed = 64;
						self.animator.SetSpriteAnimation(self.characterID, AnimationType.Walk, true, self.animator.frameID);
						break;

					case AnimationType.Jog:
						if (self.animator.speed < 64) self.animator.speed = 64;
						self.animator.SetSpriteAnimation(self.characterID, AnimationType.Walk, true, self.animator.frameID);
						break;

					case AnimationType.Jump:
						if (self.velocity.y >= self.jumpCap)
						{
							self.StateMachine.Run(PlayerState.JumpAbility);
						}
						break;

					case AnimationType.Skid:
						self.animator.SetSpriteAnimation(self.characterID, AnimationType.Walk, true, self.animator.frameID);
						if (self.skidding > 0)
						{
							self.skidding -= self.stepDelta;
							if (self.skidding < 0) self.skidding = 0;
						}

						break;

					case AnimationType.SpinDash:
						self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true);
						break;

					case AnimationType.IdleTurn:
					case AnimationType.Waiting:
					case AnimationType.AirWalk:
					case AnimationType.Run:
					case AnimationType.Dash:
					case AnimationType.SpinDashPress:
					case AnimationType.Push:
					case AnimationType.Hurt:
					case AnimationType.Die:
					case AnimationType.SpringCs:
					case AnimationType.SpringDiagonal:
					case AnimationType.SpringTwirl:
					case AnimationType.AltJump:
					default: break;
				}
			}
		}
	}
}