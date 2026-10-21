using Objects.Player;
using Objects.Player.Constants;
using UnityEngine;
using VEngine.Audio;
using VEngine.Entity.Constants;
using VEngine.Utilities.Helpers;
using VEngine.Camera;
using VEngine.Collision;

namespace VEngine.Utilities.TriggerSystem
{
	[CreateAssetMenu(menuName = "Sonic-V/Triggers/ForceSpin", fileName = "ForceSpin")]
	[System.Serializable]
	public class ForceSpin : TriggerBase
	{
		private Vector2 position;
		public TriggerInfo TriggerInfo;
		private bool flipped;

		public override void Trigger(Vector3Int triggerPosition, Entity.Entity entity)
		{
			position = (Vector2)(Vector3)(triggerPosition) + new Vector2(0.5f, 0.5f);
			var playerObject = (PlayerObject)entity as PlayerObject;
			TriggerInfo = GetTriggerInfo(triggerPosition);
			flipped = TriggerInfo.TileRotation.y == 180;

			if (playerObject != null)
				ActivateForceSpin(playerObject);
		}

		private void ActivateForceSpin(PlayerObject playerObject)
		{
			var pivotPos = playerObject.position;
			var pivotVel = playerObject.velocity;

			if (TriggerInfo.TileRotation.z == 0)
			{
				if (playerObject.velocity.x > 0)
				{
					if (!flipped)
					{
						if (playerObject.CurrentState != PlayerState.TubeRoll &&
							playerObject.CurrentState != PlayerState.TubeAirRoll) return;
						playerObject.CurrentState = playerObject.onGround
							? PlayerState.Roll
							: PlayerState.Air;
						playerObject.NextGroundState = PlayerState.None;
						playerObject.NextAirState = PlayerState.None;
						playerObject.rotation = 0;
						playerObject.direction = Flip.None;
					}
					else
					{
						SetPlayerState(playerObject);
					}
				}
				else
				{
					if (flipped)
					{
						if (playerObject.CurrentState != PlayerState.TubeRoll &&
							playerObject.CurrentState != PlayerState.TubeAirRoll) return;
						playerObject.CurrentState = playerObject.onGround
							? PlayerState.Roll
							: PlayerState.Air;
						playerObject.NextGroundState = PlayerState.None;
						playerObject.NextAirState = PlayerState.None;
						playerObject.rotation = 0;
						playerObject.direction = Flip.X;
					}
					else
					{
						SetPlayerState(playerObject);
					}
				}
			}
			else
			{
				if (playerObject.velocity.y < 0)
				{
					if (TriggerInfo.TileRotation.z == 270)
					{
						if (playerObject.CurrentState != PlayerState.TubeRoll &&
							playerObject.CurrentState != PlayerState.TubeAirRoll) return;
						playerObject.CurrentState = playerObject.onGround
							? PlayerState.Roll
							: PlayerState.Air;
						playerObject.NextGroundState = PlayerState.None;
						playerObject.NextAirState = PlayerState.None;
						playerObject.rotation = 0;
						playerObject.direction = Flip.None;
					}
					else
					{

						if (!flipped)
						{
							playerObject.collisionMode = CollisionState.RWall;
							playerObject.position.x += PixelUnitHelper.P2U(4);
							playerObject.angle = 90;
							playerObject.groundVel = playerObject.velocity.y;
						}
						else
						{
							playerObject.collisionMode = CollisionState.LWall;
							playerObject.position.x -= PixelUnitHelper.P2U(4);
							playerObject.angle = 270;
							playerObject.groundVel = -playerObject.velocity.y;
						}
						SetPlayerState(playerObject);
						playerObject.onGround = true;
					}
				}
				else
				{
					if (TriggerInfo.TileRotation.z == 90)
					{
						if (playerObject.CurrentState != PlayerState.TubeRoll &&
							playerObject.CurrentState != PlayerState.TubeAirRoll) return;
						playerObject.CurrentState = playerObject.onGround
							? PlayerState.Roll
							: PlayerState.Air;
						playerObject.NextGroundState = PlayerState.None;
						playerObject.NextAirState = PlayerState.None;
						playerObject.rotation = 0;
						playerObject.direction = Flip.X;
					}
					else
					{
						if (flipped)
						{
							playerObject.collisionMode = CollisionState.RWall;
							playerObject.position.x += PixelUnitHelper.P2U(4);
							playerObject.angle = 90;
							playerObject.groundVel = playerObject.velocity.y;
						}
						else
						{
							playerObject.collisionMode = CollisionState.LWall;
							playerObject.position.x -= PixelUnitHelper.P2U(4);
							playerObject.angle = 270;
							playerObject.groundVel = -playerObject.velocity.y;
						}
						SetPlayerState(playerObject);
						playerObject.onGround = true;
					}
				}
			}
		}

		private void SetPlayerState(PlayerObject playerObject)
		{
			if (playerObject.CurrentState == PlayerState.TubeRoll ||
				playerObject.CurrentState == PlayerState.TubeAirRoll) return;

			if (playerObject.animator.animationID != AnimationType.Jump)
			{
				AudioManager.PlayOneShot("Roll");
				playerObject.pushing = 0;
			}

			playerObject.CurrentState = PlayerState.TubeRoll;
			playerObject.NextGroundState = PlayerState.None;
			playerObject.NextAirState = PlayerState.None;

			if (!(Mathf.Abs(playerObject.groundVel) < 1)) return;
			if (!flipped)
				playerObject.groundVel = -2f;
			else
				playerObject.groundVel = 2f;
		}
	}
}