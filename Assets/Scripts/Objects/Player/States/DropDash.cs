using Objects.Player.Constants;
using VEngine.Entity.Constants;
using VEngine.Math;
using VEngine.Utilities.Helpers;

namespace Objects.Player.States
{
	public class DropDash : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public DropDash() : base(PlayerState.DropDash) { }
		public override void HandleState()
		{
			if (self.onGround)
			{
				self.GravityFalse();

				if (self.control.right)
					self.direction = Flip.None;
				if (self.control.left)
					self.direction = Flip.X;

				float vel = 8;
				float cap = 12;
				if (self.superState == SuperState.Super)
				{
					vel = 12;
					cap = 13;

					if (PlayerVars.PlayerCamera)
						PlayerVars.PlayerCamera.ShakeScreen(6, 6);
				}
				if (self.direction == Flip.X)
				{
					if (self.velocity.x <= 0)
					{
						self.groundVel = self.groundVel * Math.BitShift(2) - vel;

						if (self.groundVel < -cap)
							self.groundVel = -cap;
					}
					else if (self.angle != 0)
					{
						self.groundVel = self.groundVel * Math.BitShift(1) - vel;
					}
					else
					{
						self.groundVel = -vel;
					}
				}
				else
				{
					if (self.velocity.x >= 0)
					{
						self.groundVel = vel + self.groundVel * Math.BitShift(2);

						if (self.groundVel > cap)
							self.groundVel = cap;
					}
					else if (self.angle != 0)
					{
						self.groundVel = vel + self.groundVel * Math.BitShift(1);
					}
					else
					{
						self.groundVel = vel;
					}
				}

				PlayerVars.PlayerCamera.previousPositions.Clear();
				self.scrollDelay = 8;

				/*
				EntityDust *dust = CREATE_ENTITY(Dust, self, self->position.x, self->position.y);
				RSDK.SetSpriteAnimation(Dust->aniFrames, 2, &dust->animator, true, 0);
				dust->state = Dust_State_DustPuff;
				dust->position.y += RSDK.GetHitbox(&self->animator, 0)->bottom << 16;
				dust->direction = self->direction;
				dust->drawGroup = self->drawGroup;

				RSDK.StopSfx(Player->sfxDropdash);
				if (self.superState == SuperState.Super)
					RSDK.PlaySfx(Player->sfxPeelRelease, false, 255);
				else
					RSDK.PlaySfx(Player->sfxRelease, false, 255);
				*/

				self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true, 0);
				if (self.collisionMode != CollisionState.None)
					self.position.y += PixelUnitHelper.P2U(self.jumpOffset);
				self.pushing = 0;
				self.CurrentState = PlayerState.Roll;
			}
			else if (self.control.jumpHold)
			{
				self.HandleAirFriction();
				self.HandleAirMovement();

				self.animator.speed += 8;
				if (self.animator.speed > 180)
					self.animator.speed = 180;

				if (self.jumpAbilityState == 0)
					ExitDropDash();
			}
			else
			{
				ExitDropDash();
			}
		}

		private void ExitDropDash()
		{
			self.jumpAbilityState = 0;
			self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true, 0);
			self.CurrentState = PlayerState.Air;
		}
	}
}

