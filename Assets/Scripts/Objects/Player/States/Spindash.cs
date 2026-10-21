using Objects.Player.Constants;
using Objects.SpinDust;
using UnityEngine;
using VEngine.Audio;
using VEngine.Entity.Constants;

namespace Objects.Player.States
{
	public class Spindash : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Spindash() : base(PlayerState.Spindash) { }
		private float spinPressTimer;
		public override void HandleState()
		{
			if (self.control.jumpPress)
			{
				self.abilityTimer += 2;

				if (self.abilityTimer > 12) self.abilityTimer = 12;

				if (self.spindashCharge < 6) self.spindashCharge++;

				if (self.spindashCharge < 0) self.spindashCharge = 0;

				self.animator.SetSpriteAnimation(self.characterID, AnimationType.SpinDashPress, true, self.animator.frameID);
				spinPressTimer = 15;
				AudioManager.PlayPersistant("Charge");
				//AudioManager.PlayOneShot("Charge");
			}
			else
			{
				spinPressTimer -= self.stepDelta;
				if (spinPressTimer <= 0)
					spinPressTimer = 0;
				self.abilityTimer -= self.abilityTimer / 32 * self.stepDelta;
			}

			if (self.animator.animationID == AnimationType.SpinDashPress && spinPressTimer == 0)
			{
				self.animator.SetSpriteAnimation(self.characterID, AnimationType.SpinDash, true, self.animator.frameID);
			}

			if (self.control.down) return;
			var spindashDustScript = self.spindashDust.GetComponent<SpindashDust>();
			spindashDustScript.InitiateDissipate();
			self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true, 0);

			float vel = 0;

			if (self.abilityTimer != 0)
				vel = self.spindashCharge * (self.abilityTimer / 12) + 7;

			vel = Mathf.Clamp(vel, -12, 12);

			PlayerVars.PlayerCamera.previousPositions.Clear();
			self.scrollDelay = 12;

			if (self.direction != Flip.None)
				self.groundVel = -vel;
			else
				self.groundVel = vel;

			AudioManager.StopPersistant("Charge");
			AudioManager.PlayPersistant("Release");

			self.animator.SetSpriteAnimation(self.characterID, AnimationType.Jump, true, 0);

			self.pushing = 0;
			self.CurrentState = PlayerState.Roll;
		}
	}
}