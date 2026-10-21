using System.Collections;
using System.Runtime.CompilerServices;
using Objects.Player.Constants;
using VEngine.Entity.Constants;
using VEngine.Math;

namespace Objects.Player.States
{
	public class JumpAbility : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public JumpAbility() : base(PlayerState.JumpAbility) { }

		public override void HandleState()
		{
			switch (self.characterID)
			{
				case CharacterType.Sonic: SonicAbility(); break;
				case CharacterType.Tails: TailsAbility(); break;
			}
		}

		private void SonicAbility()
		{
			bool dropdashDisabled = self.jumpAbilityState <= 1;

			if (self.jumpAbilityState == 1)
			{
				if (self == PlayerVars.Leader)
				{
					if (self.control.jumpPress)
					{
						//EntityShield *shield = RSDK_GET_ENTITY(Player->playerCount + RSDK.GetEntitySlot(self), Shield);
						if (self.invincibleTimer != 0)
						{
							/*
							if (shield->classID != Shield->classID || shield->shieldAnimator.animationID != SHIELDANI_INSTA) {
								if (!(globals->medalMods & MEDAL_NODROPDASH))
									++self->jumpAbilityState;
							}*/
						}
						else
						{
							switch (self.shield)
							{
								case ShieldType.None:
								/*
								if (globals->medalMods & MEDAL_INSTASHIELD) {
									self.invincibleTimer  = -8;
									self.jumpAbilityState = 0;
									RSDK.PlaySfx(Shield->sfxInstaShield, false, 255);
									RSDK.ResetEntity(shield, Shield->classID, self);
									shield->inkEffect = INK_ADD;
									shield->alpha     = 0x100;
									RSDK.SetSpriteAnimation(Shield->aniFrames, SHIELDANI_INSTA, &shield->shieldAnimator, true, 0);
									shield->state = Shield_State_Insta;
								}*/
								// [Fallthrough]
								case ShieldType.Blue:
									// returns 0 if dropdash (bit 4) is disabled
									// returns 2 if dropdash is enabled
									self.jumpAbilityState = 2;
									break;
								/*
								case ShieldType.Bubble:
									self.velocity.x       = self.velocity.x * Math.BitShift(1);
									self.velocity.y       = -8;
									self.state           = Player_State_BubbleBounce;
									self->nextGroundState = StateMachine_None;
									self->nextAirState    = StateMachine_None;
									RSDK.SetSpriteAnimation(Shield->aniFrames, SHIELDANI_BUBBLEATTACKDADD, &shield->fxAnimator, true, 0);
									RSDK.SetSpriteAnimation(Shield->aniFrames, SHIELDANI_BUBBLEATTACKD, &shield->shieldAnimator, true, 0);
									shield->state = Shield_State_BubbleDrop;
									RSDK.PlaySfx(Shield->sfxBubbleBounce, false, 255);
									break;

								case SHIELD_FIRE:
									self->jumpAbilityState = 0;
									self->velocity.x       = self->direction == FLIP_X ? -0x80000 : 0x80000;
									self->velocity.y       = 0;
									RSDK.SetSpriteAnimation(Shield->aniFrames, SHIELDANI_FIREATTACK, &shield->shieldAnimator, true, 0);
									shield->state     = Shield_State_FireDash;
									shield->direction = self->direction;
									if (self->camera && !Zone->autoScrollSpeed) {
										self->scrollDelay   = 15;
										self->camera->state = Camera_State_FollowY;
									}
									RSDK.PlaySfx(Shield->sfxFireDash, false, 255);
									break;

								case SHIELD_LIGHTNING:
									self->jumpAbilityState = 0;
									self->velocity.y       = self->invertGravity ? 0x58000 : -0x58000;
									shield->state          = Shield_State_LightningSparks;
									RSDK.PlaySfx(Shield->sfxLightningJump, false, 255);
									break;
								*/
								default: break;
							}
						}
					}
					return;
				}

				dropdashDisabled = true;
			}

			if (!dropdashDisabled && self.control.jumpHold)
			{
				self.jumpAbilityState += self.stepDelta;
				if (self.jumpAbilityState >= 22)
				{
					self.CurrentState = PlayerState.DropDash;
					self.NextGroundState = PlayerState.None;
					self.NextAirState = PlayerState.None;
					self.animator.SetSpriteAnimation(self.characterID, AnimationType.AltJump, true, 0);
					//RSDK.PlaySfx(Player->sfxDropdash, false, 255);
				}
			}
		}

		private void TailsAbility()
		{

		}
	}
}