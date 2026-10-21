using Objects.Player.Constants;

namespace Objects.Player.States
{
	public class Crouch : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public Crouch() : base(PlayerState.Crouch) { }

		public override void HandleState()
		{
			if (self.invertGravity) self.rotation = 180;

			self.control.left = false;
			self.control.right = false;

			self.HandleGroundMovement();

			self.GravityFalse();

			self.NextAirState = PlayerState.Air;

			if (self.control.down)
			{
				self.animator.speed = 64;
				self.animator.SetSpriteAnimation(self.characterID, AnimationType.Crouch, true, null, true, 1);

				if (self.timer < 60)
				{
					self.timer += self.stepDelta;
				}
				else if (PlayerVars.PlayerCamera)
				{
					if (self.invertGravity)
					{
						if (PlayerVars.PlayerCamera.lookPos.y < 96)
						{
							PlayerVars.PlayerCamera.lookPos.y += 2 * self.stepDelta;
							if (PlayerVars.PlayerCamera.lookPos.y > 96) PlayerVars.PlayerCamera.lookPos.y = 96;
						}
					}
					else
					{
						if (PlayerVars.PlayerCamera.lookPos.y > -96)
						{
							PlayerVars.PlayerCamera.lookPos.y -= 2 * self.stepDelta;
							if (PlayerVars.PlayerCamera.lookPos.y < -96) PlayerVars.PlayerCamera.lookPos.y = -96;
						}
					}
				}

				if (self.control.jumpPress)
				{
					self.ActionSpindash();
				}
			}
			else
			{
				if (self.downStore) self.animator.frameID = 0;

				self.animator.speed = 64;

				if (self.animator.frameID == 1 || self.control.left || self.control.right) self.CurrentState = PlayerState.Ground;

				if (self.control.jumpPress)
				{
					self.ActionJump();
				}
			}
		}
	}
}