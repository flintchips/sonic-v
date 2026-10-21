using Objects.Player.Constants;

namespace Objects.Player.States
{
	public class LookUp : Utilities.BaseState<PlayerState, PlayerObject>
	{
		public LookUp() : base(PlayerState.LookUp) { }
		public override void HandleState()
		{
			if (self.invertGravity) self.rotation = 180;

			self.GravityFalse();

			self.NextAirState = PlayerState.Air;

			if (self.control.up)
			{
				self.animator.speed = 64;
				self.control.left = false;
				self.control.right = false;

				self.HandleGroundMovement();

				self.animator.SetSpriteAnimation(self.characterID, AnimationType.LookUp, true, null, true, 1);

				if (self.timer < 60)
				{
					self.timer += self.stepDelta;
				}
				else if (PlayerVars.PlayerCamera)
				{
					if (self.invertGravity)
					{
						if (PlayerVars.PlayerCamera.lookPos.y > -96)
							PlayerVars.PlayerCamera.lookPos.y -= 2 * self.stepDelta;
					}
					else
					{
						if (PlayerVars.PlayerCamera.lookPos.y < 96)
							PlayerVars.PlayerCamera.lookPos.y += 2 * self.stepDelta;
					}
				}

				if (!self.control.jumpPress) return;
				try
				{
					self.StateMachine.Run(PlayerState.Peelout);
				}
				catch
				{
					self.ActionJump();
				}
			}
			else
			{
				if (self.upStore) self.animator.frameID = 0;

				self.animator.speed = 64;

				if ((self.animator.frameID == 1) || self.control.left || self.control.right)
				{
					self.CurrentState = PlayerState.Ground;
				}

				if (self.control.jumpPress)
				{
					self.ActionJump();
				}
			}
		}
	}
}