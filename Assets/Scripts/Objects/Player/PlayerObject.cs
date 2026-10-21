using System;
using Objects.Player.Constants;
using Objects.Player.Debug;
using Objects.Player.Input;
using Objects.Player.States;
using Objects.SpinDust;
using Objects.SkidDust;
using UnityEngine;
using VEngine.Animation;
using VEngine.Audio;
using VEngine.Camera.Constants;
using VEngine.Collision;
using VEngine.Entity.Constants;
using VEngine.Math;
using VEngine.Utilities.Helpers;
using VEngine.Camera;
using Collision = VEngine.Collision.Collision;
using Math = VEngine.Math.Math;

namespace Objects.Player
{
	public class PlayerObject : PlayerVars
	{
		public PlayerStateMachine StateMachine { get; private set; }
		private void OnEnable()
		{
			StateMachine = new PlayerStateMachine(this);
		}

		protected override void ObjectAwake()
		{
			loaded = true;
			if (!Leader)
			{
				Leader = this;
			}
		}

		protected override void ObjectStart()
		{
			CurrentState = PlayerState.Ground;
			NextGroundState = PlayerState.None;
			NextAirState = PlayerState.None;

			position = transform.position;

			animator.speed = 12;
			animator.SetSpriteAnimation(characterID, AnimationType.Idle, true, 0);

			UpdatePhysicsState();

			minJogVelocity = 3.5f;
			minRunVelocity = 6;
			minDashVelocity = 12;
		}

		protected override void ObjectUpdate()
		{
			PlayerUpdate();
		}

		public void FixedUpdate()
		{
			if (substeps < 1 || onObject) substeps = 1;

			substeps = 1 +
					   Mathf.FloorToInt(velocity.magnitude /
										3.2f); // substeps should cap at 6 while going at max velocity.

			stepDelta = 1f / substeps;
			for (var i = 0; i < substeps; i++)
			{
				isLastSubstep = true;
				if (i < (substeps - 1)) isLastSubstep = false;
				ObjectFixedUpdate();
			}
		}

		protected override void ObjectFixedUpdate()
		{
			if (control.q)
			{
				PlayerDebugMode = !PlayerDebugMode;
				DebugMode.Speed = velocity.magnitude;
				if (!PlayerDebugMode)
				{
					collisionMode = CollisionState.Floor;
					onObject = false;
					attachedObject = null;
					solidObject = null;
				}
			}

			if (!PlayerDebugMode)
			{
				DebugMode.Speed = 0;
				PlayerFixedUpdate();
				PlayerLateFixedUpdate();
			}
			else
			{
				DebugMode.DebugFixedUpdate(this);

				groundVel = 0;
				velocity.x = DebugMode.Velocity.x;
				velocity.y = DebugMode.Velocity.y;
				CurrentState = PlayerState.Air;
				collisionMode = CollisionState.None;
				PlayerCamera.disableYOffset = true;
			}

			control.jumpPress = false;
			control.q = false;

			leftStore = control.left;
			rightStore = control.right;
			upStore = control.up;
			downStore = control.down;

			jumpedThisFrame = false;
		}

		public void PlayerDraw()
		{
			transform.position = VectorHelper.Vector2FloorToPixel(position);

			if (!PlayerDebugMode) animator.ProcessAnimation();
			SpriteHandleOffsetAndRotation();

			if (animator.animationID != AnimationType.Jump) jumpSquishTimer = 0;
			if (jumpSquishTimer > 0)
			{
				var squishOffset = new Vector3(jumpSquishTimer / 18f, jumpSquishTimer / 24f, 0);
				if (direction == Flip.X) squishOffset.x = -squishOffset.x;

				animator.spriteObject.transform.localScale =
					new Vector3(1 - (jumpSquishTimer / 36), 1 + (jumpSquishTimer / 48f), 1);
				animator.spriteObject.transform.position += squishOffset;

				jumpSquishTimer -= stepDelta * (jumpSquishTimer / 15f) + stepDelta;
				if (jumpSquishTimer < 0) jumpSquishTimer = 0;
			}
			else
			{
				animator.spriteObject.transform.localScale = new Vector3(1, 1, 1);
			}

			animator.spriteRenderer.enabled = PlayerDebugMode || visible;

			if (!isLastSubstep) return;
			PlayerCamera.CameraUpdate();
			PlayerCamera.CameraLateUpdate();
		}

		private void SpriteHandleOffsetAndRotation()
		{
			animator.spriteRenderer.flipX = direction is Flip.X or Flip.XY;
			animator.spriteRenderer.flipY = direction is Flip.Y or Flip.XY;

			var flipVector = Vector2.zero;
			flipVector.x = animator.spriteRenderer.flipX ? 0.5f : -0.5f;
			flipVector.y = animator.spriteRenderer.flipY ? -0.5f : 0.5f;

			// resolution of box that the sprites are centered on.
			// Sprite pivot points should always be at the top left of this box
			var offsetBounds = new Vector2(60, 48) + Vector2.up * 9 + Vector2.right * 4;
			var spriteOffset = Vector2.zero;
			spriteOffset += flipVector * (offsetBounds / 16f);
			spriteOffset.y += (animator.animationID == AnimationType.Jump || animator.animationID == AnimationType.AltJump) ? -PixelUnitHelper.P2U(2) : 0;
			switch (collisionMode)
			{
				case CollisionState.Roof:
					spriteOffset.y += PixelUnitHelper.P2U(1);
					break;
				case CollisionState.RWall:
					spriteOffset.x += PixelUnitHelper.P2U(1);
					break;
				case CollisionState.Floor:
				case CollisionState.LWall:
				case CollisionState.None:
				default:
					break;
			}

			var tempRot = rotation;
			if (animator.animationID == AnimationType.Push)
			{
				spriteOffset.x += PixelUnitHelper.P2U(2) * (animator.spriteRenderer.flipX ? -1 : 1);
			}

			switch (animator.animationID)
			{
				case AnimationType.Idle:
				case AnimationType.Push:
				case AnimationType.Waiting:
				case AnimationType.LookUp:
				case AnimationType.IdleTurn:
				case AnimationType.Jump:
				case AnimationType.AltJump:
				case AnimationType.Crouch:
				case AnimationType.SpinDash:
				case AnimationType.SpinDashPress:
				case AnimationType.Skid:
				case AnimationType.SkidTurn:
					if (collisionMode != CollisionState.Roof)
						tempRot = 0;
					break;
				default:
					break;
			}

			var spritePosition = transform.position;
			var rotatedSpriteOffset = Rotation.RotatePointAroundPivot(spriteOffset + (Vector2)spritePosition,
				spritePosition + new Vector3(PixelUnitHelper.P2U(0.5f), 0, 0), tempRot);
			animator.spriteObject.transform.position =
				VectorHelper.Vector2Floor((rotatedSpriteOffset + new Vector2(0, PixelUnitHelper.P2U(0.5f))) * 16f) /
				16f;
			animator.spriteObject.transform.rotation = Quaternion.Euler(Vector3.forward * tempRot);
		}

		private void PlayerUpdate()
		{

			if (playerInput != null)
				playerInput.InputUpdate();
			if (control == null) control = playerInput.control;
		}

		private void PlayerFixedUpdate()
		{
			if (PlayerCamera)
			{
				if (scrollDelay > 0)
				{
					scrollDelay -= stepDelta;
					if (scrollDelay < 0) scrollDelay = 0;
					if (scrollDelay == 0)
						PlayerCamera.SetState(CameraState.FollowXY);
				}
				else if (CurrentState != PlayerState.LookUp && CurrentState != PlayerState.Crouch)
				{
					switch (PlayerCamera.lookPos.y)
					{
						case < 0:
							{
								PlayerCamera.lookPos.y += 2 * stepDelta;
								if (PlayerCamera.lookPos.y > 0) PlayerCamera.lookPos.y = 0;
								break;
							}
						case > 0:
							{
								PlayerCamera.lookPos.y -= 2 * stepDelta;
								if (PlayerCamera.lookPos.y < 0) PlayerCamera.lookPos.y = 0;
								break;
							}
					}
				}
			}

			if (speedShoesTimer > 0)
			{
				speedShoesTimer -= stepDelta;
				if (speedShoesTimer == 0)
				{
					UpdatePhysicsState();
				}
			}

			if (CurrentState != PlayerState.Hurt)
			{
				if (blinkTimer > 0)
				{
					blinkTimer -= stepDelta;
					visible = !((Mathf.Floor(blinkTimer) % 4 == 0) || ((Mathf.Floor(blinkTimer) + 1) % 4 == 0));
				}
				else
				{
					visible = true;
				}
			}

			if (uncurlTimer > 0)
			{
				uncurlTimer -= stepDelta;
				if (uncurlTimer < 0) uncurlTimer = 0;
			}

			HandleSuperForm();

			if (collisionFlagH == (1 | 2) || collisionFlagV == (1 | 2))
				deathType = DeathType.DieUseSfx;

			collisionFlagH = 0;
			collisionFlagV = 0;

			StateMachine.Run(CurrentState);

			flailing = false;
			invertGravity = false;

			if (outerbox == null)
			{
				outerbox = new Hitbox();
				innerbox = new Hitbox();
				hitbox = new Hitbox();

				outerbox = null;
			}

			groundedStoreOld = groundedStore;
			groundedStore = onGround;
			balancing = false;

			if (animator.animationID == AnimationType.Jump || animator.animationID == AnimationType.AltJump)
			{
				if ((animator.animStore.animationID != AnimationType.Jump && animator.animStore.animationID != AnimationType.AltJump))
				{
					position.y += PixelUnitHelper.P2U(jumpOffset);
				}
				outerbox = new Hitbox(10, 15);
				innerbox = new Hitbox(6, 15);
				hitbox = new Hitbox(8, 12);
			}
			else
			{
				outerbox = new Hitbox(10, 19);
				innerbox = new Hitbox(9, 19);
				hitbox = new Hitbox(8, 16);
				if (animator.animStore != null)
				{
					if ((animator.animStore.animationID == AnimationType.Jump || animator.animStore.animationID == AnimationType.AltJump) &&
						collisionMode == CollisionState.Floor)
					{
						position.y -= PixelUnitHelper.P2U(jumpOffset);
					}
				}
			}
		}

		private void PlayerLateFixedUpdate()
		{
			directionStore = direction;
			if (onGround)
			{
				if (NextGroundState != PlayerState.None)
				{
					CurrentState = NextGroundState;
					NextGroundState = PlayerState.None;
				}

				if (!PlayerCamera) return;
				
				if (animator.animationID == AnimationType.Jump || animator.animationID == AnimationType.AltJump)
					PlayerCamera.adjustY = -jumpOffset;
				else
					PlayerCamera.adjustY = 0;
				
				if(!groundedStoreOld) PlayerCamera.adjustY = 0;

			}
			else if (NextAirState != PlayerState.None)
			{
				CurrentState = NextAirState;
				NextAirState = PlayerState.None;
			}
		}

		private void UpdatePhysicsState()
		{
			var tablePtr = ConvertHexArrayToFloatArray(SonicPhysicsTable);

			var tablePos = 0;
			if (underwater != 0)
			{
				gravityStrength = 0.125f;
				tablePos = 1;
				if (speedShoesTimer >= 0)
					gravityStrength = 0.0625f;
			}
			else
			{
				gravityStrength = 0.34375f;
				if (speedShoesTimer >= 0)
					gravityStrength = 0.21875f;
			}

			var decelShift = 0;
			if (superState == SuperState.Super)
			{
				tablePos |= 2;
				decelShift = 2;
			}

			if (speedShoesTimer > 0)
			{
				tablePos |= 4;
				decelShift = 1;
			}

			var tableID = 8 * tablePos;
			topSpeed = tablePtr[tableID];
			acceleration = tablePtr[tableID + 1];
			deceleration = tablePtr[tableID + 1] * Math.BitShift(decelShift);
			airAcceleration = tablePtr[tableID + 2];
			airDeceleration = tablePtr[tableID + 3];
			skidSpeed = tablePtr[tableID + 4];
			rollingFriction = tablePtr[tableID + 5];
			jumpStrength = tablePtr[tableID + 6];
			jumpCap = tablePtr[tableID + 7];
			rollingDeceleration = 0.125f;

			if (speedShoesTimer < 0)
			{
				topSpeed *= Math.BitShift(1);
				acceleration *= Math.BitShift(1);
				airAcceleration *= Math.BitShift(1);
				skidSpeed *= Math.BitShift(1);
				rollingFriction = airDeceleration * Math.BitShift(1);
				airDeceleration *= Math.BitShift(1);
			}

			if (!isChibi) return;
			topSpeed -= topSpeed * Math.BitShift(3);
			acceleration -= acceleration * Math.BitShift(4);
			airAcceleration -= airAcceleration * Math.BitShift(4);
			jumpStrength -= jumpStrength * Math.BitShift(3);
			jumpCap -= jumpCap * Math.BitShift(3);
		}

		private bool waitingChanceSkip = false;

		private void HandleIdleAnimation()
		{
			animator.speed = 64;
			switch (characterID)
			{
				case CharacterType.Sonic:
					if (superState != SuperState.Super)
					{
						if (timer < 240)
						{
							timer += stepDelta;
							animator.SetSpriteAnimation(characterID, AnimationType.Idle, true);
						}
						else
						{
							timer += stepDelta;
							if (animator.animationID == AnimationType.Waiting)
							{
								if (animator.frameID != 10 && waitingChanceSkip)
								{
									waitingChanceSkip = false;
								}

								if (animator.frameID == 10)
								{
									if (isLastSubstep)
									{
										if (!waitingChanceSkip)
										{
											float rand = UnityEngine.Random.Range(0f, 5f);
											if (rand > 0.5f)
											{

												animator.SetSpriteAnimation(characterID, AnimationType.Waiting, true, 5, true, 5, true);
											}
											else
											{
												animator.SetSpriteAnimation(characterID, AnimationType.Waiting, true, 10, true, 5, true);
												waitingChanceSkip = true;
											}
										}
									}
								}

								if (animator.frameID == 20)
									timer = 240;
							}
							else
								animator.SetSpriteAnimation(characterID, AnimationType.Waiting, true, null, true, 5);
						}
					}
					else
					{
						animator.SetSpriteAnimation(characterID, AnimationType.Idle, true);
					}
					break;
				case CharacterType.Tails:
				case CharacterType.SonicTails:
				default: break;
			}
		}

		private static void HandleSuperForm()
		{
		}

		public void HandleGroundRotation()
		{
			angle = Rotation.Clamp0360(angle);
			rotation = Rotation.Clamp0360(rotation);

			var flatGround = (angle is <= 5.625f or >= 354.375f);

			float targetRotation = 0;
			if (angle is > 26.5f and < 333.5f)
				targetRotation = angle;

			var rotate = Mathf.Abs(Rotation.AngleDifference(targetRotation, rotation)) * 0.75f + (flatGround ? 10 : 0);

			float shift = ((Mathf.Abs(groundVel) <= 6) ? 1 : 0) + 1;

			float slow = (controlLock > 0 && velocity.y < 0) ? 0.75f : 1f;
			var oldRotation = rotation;
			rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, rotate * Math.BitShift(shift) * stepDelta * slow);
			if (oldRotation < targetRotation && rotation > targetRotation) rotation = targetRotation;
			else if (oldRotation > targetRotation && rotation < targetRotation) rotation = targetRotation;
		}

		public void HandleGroundMovement()
		{
			if (controlLock > 0)
			{
				controlLock -= stepDelta;

				if (controlLock < 0)
				{
					controlLock = 0;

				}
				if (!invertGravity)
					groundVel -= Mathf.Sin(angle * Mathf.Deg2Rad) * Math.BitShift(3) * stepDelta;
			}
			else
			{
				bool localLeft;
				bool localRight;

				if (invertGravity)
				{
					localRight = control.left;
					localLeft = control.right;
				}
				else
				{
					localLeft = control.left;
					localRight = control.right;
				}

				if (localLeft)
				{
					if (groundVel > -topSpeed)
					{
						if (groundVel <= 0)
						{
							groundVel -= acceleration * stepDelta;
						}
						else
						{
							if (collisionMode == CollisionState.Floor && groundVel > 4 && Mathf.Abs(angle) < 22.5f)
							{
								direction = Flip.None;
								skidding = 24;
								//if (skiddingDustTimer == 0) InitSkidDust();
							}

							if (groundVel < skidSpeed)
								groundVel = -Mathf.Abs(skidSpeed);
							else
								groundVel -= skidSpeed * stepDelta;
						}
					}

					if (groundVel <= 0 && skidding < 1)
						direction = Flip.X;
				}

				if (localRight)
				{
					if (groundVel < topSpeed)
					{
						if (groundVel >= 0)
						{
							groundVel += acceleration * stepDelta;
						}
						else
						{
							if (collisionMode == CollisionState.Floor && groundVel < -4 && Mathf.Abs(angle) < 22.5f)
							{
								direction = Flip.X;
								skidding = 24;
								//if (skiddingDustTimer == 0) InitSkidDust();
							}

							if (groundVel > -skidSpeed)
								groundVel = Mathf.Abs(skidSpeed);
							else
								groundVel += skidSpeed * stepDelta;
						}
					}

					if (groundVel >= 0 && skidding < 1)
						direction = Flip.None;
				}

				if (localLeft || localRight)
				{
					if (!invertGravity)
					{
						groundVel -= Mathf.Sin(angle * Mathf.Deg2Rad) * Math.BitShift(3) * stepDelta;

						if (control.right)
						{
							if (!control.left)
							{
								if (angle is > 39.375f and < 180f && groundVel is > -2.5f and < 2f)
								{
									controlLock = 30;
								}
							}
						}
						else if (control.left)
						{
							if (angle is > 180 and < 320.625f && groundVel is > -2f and < 2.5f)
							{
								controlLock = 30;
							}
						}
					}
				}
				else
				{
					if (groundVel <= 0)
					{
						if (Mathf.Abs(angle) > 45f)
						{
							groundVel += acceleration * stepDelta;
						}
						else
							groundVel += deceleration * stepDelta;

						if (groundVel > 0)
							groundVel = 0;
					}
					else
					{
						if (Mathf.Abs(angle) > 45f)
						{
							groundVel -= acceleration * stepDelta;
						}
						else
							groundVel -= deceleration * stepDelta;

						if (groundVel < 0)
							groundVel = 0;
					}

					if (!invertGravity)
					{
						if (groundVel is > 0.125f or < -0.125f)
							groundVel -= Mathf.Sin(angle * Mathf.Deg2Rad) * Math.BitShift(3) * stepDelta;

						switch (angle)
						{
							case > 270 and < 320.625f:
								{
									if (Mathf.Abs(groundVel) < 1)
										controlLock = 30;
									break;
								}
							case > 39.375f and < 90:
								{
									if (Mathf.Abs(groundVel) < 1)
										controlLock = 30;
									break;
								}
						}
					}
				}
			}

			if (Mathf.Abs(groundVel) > 16) groundVel = 16 * Mathf.Sign(groundVel);

			if (!(!invertGravity && collisionMode != CollisionState.Floor &&
				  ((int)collisionMode <= (int)CollisionState.RWall))) return;
			if (angle is < 90 or > 270 || Mathf.Abs(groundVel) > 2) return;
			velocity.x = groundVel * Mathf.Cos(angle * Mathf.Deg2Rad);
			velocity.y = groundVel * Mathf.Sin(angle * Mathf.Deg2Rad);
			onGround = false;
			angle = 0;
			collisionMode = CollisionState.Floor;
		}

		public void HandleAirMovement()
		{
			GravityTrue();

			Collision.ProcessObjectMovement(this, outerbox, innerbox);

			velocity.y -= gravityStrength * stepDelta;

			if (velocity.y > -jumpCap && animator.animationID == AnimationType.Jump && !control.jumpHold && applyJumpCap && velocity.y > 0)
			{
				velocity.x -= velocity.x * Math.BitShift(5) * stepDelta;
				velocity.y = -jumpCap;
				applyJumpCap = false;
			}

			if (Mathf.Abs(velocity.y) > 16) velocity.y = 16 * Mathf.Sign(velocity.y);

			collisionMode = CollisionState.Floor;
			pushing = 0;

			HandleAirRotation();

			Collision.ProcessCollisionHorizontal(this, outerbox, innerbox);
			groundedStore = onGround;
			Collision.ProcessCollisionVertical(this, outerbox, innerbox);
		}

		public void HandleAirFriction()
		{
			if (velocity.x < 0)
			{
				velocity.x += airDeceleration * stepDelta;
				if (velocity.x > 0) velocity.x = 0;
			}

			if (velocity.x > 0)
			{
				velocity.x -= airDeceleration * stepDelta;
				if (velocity.x < 0) velocity.x = 0;
			}

			if (control.left)
			{
				if (velocity.x > -topSpeed) velocity.x -= airAcceleration * stepDelta;

				direction = Flip.X;
			}

			if (!control.right) return;
			if (velocity.x < topSpeed) velocity.x += airAcceleration * stepDelta;

			direction = Flip.None;
		}

		public void HandleAirRotation()
		{
			rotation = Rotation.Clamp0360(rotation);
			if (rotation == 0) return;
			if (!(rotation >= 180))
			{
				if (!(rotation > 0)) return;
				rotation -= 4 * stepDelta;

				if (rotation <= 0) rotation = 0;
			}
			else
			{
				rotation += 4 * stepDelta;

				if (rotation is >= 0 and < 180) rotation = 0;
			}
		}

		public void GravityTrue()
		{
			skidding = 0;
			if (!PlayerCamera) return;
			if (!ctrlLockSlide)
				PlayerCamera.disableYOffset = true;
		}

		public void GravityFalse()
		{
			if (PlayerCamera)
				PlayerCamera.disableYOffset = false;

			jumpAbilityState = 0;
		}

		public void ActionJump()
		{
			if (collisionMode == CollisionState.Floor)
			{
				position.y -= PixelUnitHelper.P2U(jumpOffset);
				jumpSquishTimer = 12;
			}
			controlLock = 0;
			onGround = false;
			AudioManager.PlayPersistant("Jump");

			position.y += gravityStrength;

			var jumpForce = jumpStrength;
			velocity.x = (groundVel * Mathf.Cos(angle * Mathf.Deg2Rad) - jumpForce * Mathf.Sin(angle * Mathf.Deg2Rad));
			velocity.y = (groundVel * Mathf.Sin(angle * Mathf.Deg2Rad) + jumpForce * Mathf.Cos(angle * Mathf.Deg2Rad));

			// SolidObject solidObj = null;
			onObject = false;
			attachedObject = null;
			solidObject = null;

			if (PlayerCamera)
			{
				PlayerCamera.disableYOffset = true;
			}

			animator.SetSpriteAnimation(characterID, AnimationType.Jump, true, null);
			animator.speed = Mathf.RoundToInt((Mathf.Abs(groundVel) * 40) + 100);

			if (animator.speed > 180)
				animator.speed = 180;

			angle = 0;
			collisionMode = CollisionState.Floor;
			skidding = 0;
			applyJumpCap = true;
			jumpAbilityState = 1;
			jumpedThisFrame = true;

			CurrentState = PlayerState.Air;
		}

		public void ActionSpindash()
		{
			spindashDust = Instantiate(Resources.Load("Prefabs/SpindashDust")) as GameObject;
			if (spindashDust != null)
			{
				var spindashDustScript = spindashDust.GetComponent<SpindashDust>();
				var spriteRend = spindashDustScript.GetComponent<ObjectAnimator>().spriteObject
					.GetComponent<SpriteRenderer>();
				float dustPosXOffset = (direction == Flip.None) ? PixelUnitHelper.P2U(4) : -PixelUnitHelper.P2U(4);
				spindashDustScript.position = position - new Vector2(dustPosXOffset, PixelUnitHelper.P2U(GroundModeHeightRadius));
				spindashDustScript.playerObject = this;
				spindashDustScript.followPlayer = true;
				spriteRend.flipX = (direction != Flip.None);
			}

			animator.SetSpriteAnimation(characterID, AnimationType.SpinDashPress, true, 0);
			animator.speed = 100;
			CurrentState = PlayerState.Spindash;
			abilityTimer = 2;
			spindashCharge = 2;
			AudioManager.PlayPersistant("Charge");
		}

		public void ActionRoll()
		{
			PlayerCamera.adjustY = -PixelUnitHelper.P2U(jumpOffset);
			animator.SetSpriteAnimation(characterID, AnimationType.Jump, true, null, true);

			pushing = 0;
			skidding = 0;
			CurrentState = PlayerState.Roll;
		}

		public void HandleRollDeceleration()
		{
			var initialVel = groundVel;
			if (control.right && groundVel < -0.5f && CurrentState != PlayerState.TubeRoll)
				groundVel += rollingDeceleration * stepDelta;

			if (control.left && groundVel > 0.5f && CurrentState != PlayerState.TubeRoll)
				groundVel -= rollingDeceleration * stepDelta;

			if (groundVel != 0)
			{
				if (groundVel < 0)
				{
					groundVel += rollingFriction * stepDelta;

					if (-Mathf.Sin(angle * Mathf.Deg2Rad) >= 0)
						groundVel += 0.078125f * -Mathf.Sin(angle * Mathf.Deg2Rad) * stepDelta;
					else
						groundVel += 0.3125f * -Mathf.Sin(angle * Mathf.Deg2Rad) * stepDelta;

					if (groundVel < -16)
						groundVel = -16;
				}
				else
				{
					groundVel -= rollingFriction * stepDelta;

					if (-Mathf.Sin(angle * Mathf.Deg2Rad) <= 0)
						groundVel += 0.078125f * -Mathf.Sin(angle * Mathf.Deg2Rad) * stepDelta;
					else
						groundVel += 0.3125f * -Mathf.Sin(angle * Mathf.Deg2Rad) * stepDelta;

					if (groundVel > 16)
						groundVel = 16;
				}
			}
			else
			{
				groundVel += 0.3125f * -Mathf.Sin(angle * Mathf.Deg2Rad) * stepDelta;
			}

			switch (collisionMode)
			{
				case CollisionState.Floor:
					if (CurrentState == PlayerState.TubeRoll)
					{
						if (Mathf.Abs(groundVel) < 0.5f)
						{
							groundVel = direction switch
							{
								Flip.Y => 4,
								Flip.XY => -4,
								Flip.X => 4,
								_ => -4
							};
						}
					}
					else
					{
						if ((groundVel >= 0 && initialVel <= 0) || (groundVel <= 0 && initialVel >= 0))
						{
							groundVel = 0;
							CurrentState = PlayerState.Ground;
						}
					}

					break;

				case CollisionState.LWall:
				case CollisionState.RWall:
					if (angle is >= 90 and <= 270 && (CurrentState != PlayerState.TubeRoll))
					{
						if (Mathf.Abs(groundVel) < 2)
						{
							velocity.x = groundVel * Mathf.Cos(angle * Mathf.Deg2Rad);
							velocity.y = groundVel * Mathf.Sin(angle * Mathf.Deg2Rad);
							onGround = false;
							angle = 0;
							collisionMode = CollisionState.Floor;
						}
					}

					break;

				case CollisionState.Roof:
					if (invertGravity)
					{
						if ((groundVel >= 0 && initialVel <= 0) || (groundVel <= 0 && initialVel >= 0))
						{
							groundVel = 0;

							CurrentState = PlayerState.Ground;
						}
					}
					else
					{
						if (angle is >= 90 and <= 270 && (CurrentState != PlayerState.TubeRoll))
						{
							if (Mathf.Abs(groundVel) < 2)
							{
								velocity.x = groundVel * Mathf.Cos(angle * Mathf.Deg2Rad);
								velocity.y = groundVel * Mathf.Sin(angle * Mathf.Deg2Rad);
								onGround = false;
								angle = 0;
								collisionMode = CollisionState.Floor;
							}
						}
					}

					break;
				case CollisionState.None:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void InitSkidDust()
		{
			skiddingDustTimer = 3f;
			if (Mathf.Abs(angle) > 22.5f) return;

			GameObject skiddingDust = SkiddingDust.InitializePrefab();
			var skiddingDustScript = skiddingDust.GetComponent<SkiddingDust>();
			skiddingDustScript.Initialize(this, (direction != Flip.None));
		}

		public void HandleGroundAnimation()
		{
			if (skidding > 0)
			{
				if (animator.animationID != AnimationType.Skid)
				{
					AudioManager.PlayOneShot("Skid");
					animator.SetSpriteAnimation(characterID, AnimationType.Skid, true, null, true, 1);
					animator.speed = 64;
					if (Mathf.Abs(groundVel) < 6)
						skidding -= 8;
				}
				else
				{
					var spd = animator.speed;
					bool res = false;
					if (direction != Flip.None)
					{
						if (groundVel >= 0)
							res = true;
					}
					else if (groundVel <= 0)
						res = true;

					if (skiddingDustTimer == 0 && !res)
					{
						InitSkidDust();
					}

					if (skiddingDustTimer > 0)
					{
						skiddingDustTimer -= stepDelta;
						if (skiddingDustTimer <= 0) skiddingDustTimer = 0;
					}

					if (res)
					{
						if (animator.frameID == 2)
						{
							skidding = 1;

							if (direction == Flip.None)
								direction = Flip.X;
							else
								direction = Flip.None;

							if (groundVel == 0)
								animator.SetSpriteAnimation(characterID, AnimationType.Idle, true);
							else
								animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, null, true, 1);
						}

					}
					animator.speed = spd;
				}

				skidding -= stepDelta;
			}
			else
			{
				skiddingDustTimer = 0;
				if (pushing is > -3 and < 3)
				{
					if ((groundVel != 0) || (angle is >= 45f and <= 315 && !invertGravity))
					{
						timer = 0;

						var absVelocity = Mathf.Abs(groundVel);

						if (direction != directionStore &&
							(animator.animationID == AnimationType.Idle ||
							 animator.animationID == AnimationType.Walk) &&
							animator.animationID != AnimationType.IdleTurn && angle == 0)
						{
							animator.speed = 128;
							animator.SetSpriteAnimation(characterID, AnimationType.IdleTurn, false, 0);
						}
						else if (absVelocity < minJogVelocity)
						{
							switch (animator.animationID)
							{
								case AnimationType.Jog when animator.frameID == 3:
									animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, 1, false, 1);
									break;
								case AnimationType.Jog:
									{
										if (animator.frameID == 7)
											animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, 1, false, 1);
										break;
									}
								case AnimationType.AirWalk:
									animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, animator.frameID,
										false, 1);
									break;
								case AnimationType.Idle:
									animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, 0, true, 1);
									break;
								case AnimationType.IdleTurn:
								case AnimationType.Waiting:
								case AnimationType.LookUp:
								case AnimationType.Crouch:
								case AnimationType.Walk:
								case AnimationType.Run:
								case AnimationType.Dash:
								case AnimationType.Jump:
								case AnimationType.Skid:
								case AnimationType.SkidTurn:
								case AnimationType.SpinDash:
								case AnimationType.SpinDashPress:
								case AnimationType.Push:
								case AnimationType.Hurt:
								case AnimationType.Die:
								case AnimationType.SpringCs:
								case AnimationType.SpringDiagonal:
								case AnimationType.SpringTwirl:
								case AnimationType.AltJump:
								default:
									{
										if (animator.animationID != AnimationType.IdleTurn)
										{
											animator.SetSpriteAnimation(characterID, AnimationType.Walk, true, 1, true, 1);
										}
										else if (animator.frameID == 2)
										{
											animator.SetSpriteAnimation(characterID, AnimationType.Idle, true);
										}

										break;
									}
							}

							animator.speed = (int)((absVelocity * 16) + 48);
							minJogVelocity = 3.5f;
						}
						else if (absVelocity < minRunVelocity)
						{
							if (animator.animationID != AnimationType.Walk)
							{
								animator.SetSpriteAnimation(characterID, AnimationType.Jog, true, 1, false, 1);
							}
							else if (animator.frameID == 8 && animator.animationID != AnimationType.Jog)
							{
								animator.SetSpriteAnimation(characterID, AnimationType.Jog, true, 0, false, 1);
							}

							animator.speed = (int)((absVelocity * 16) + 32);
							minJogVelocity = 2.5f;
							minRunVelocity = 6f;
						}
						else if (absVelocity < minDashVelocity)
						{
							animator.SetSpriteAnimation(characterID, AnimationType.Run, true,
								animator.animationID == AnimationType.Dash ? 1 : 0, false, 1);

							animator.speed = (int)(Mathf.Min((absVelocity * 16) + 32, 512));
							minRunVelocity = 5.5f;
							minDashVelocity = 12;
						}
						else
						{
							if (animator.animationID == AnimationType.Run && animator.frameID == 1)
								animator.SetSpriteAnimation(characterID, AnimationType.Dash, true, 0, false, 1);
							else
								animator.SetSpriteAnimation(characterID, AnimationType.Dash, true, 1, false, 1);
							minDashVelocity = 11.5f;
						}
					}
					else
					{
						var leftSense = sensors[0].info.collided;
						var midSense = sensors[1].info.collided;
						var rightSense = sensors[2].info.collided;

						if (!onObject)
						{
							if (!midSense)
							{
								if (leftSense && !rightSense) balancingLeft = true;
								if (rightSense && !leftSense) balancingRight = true;
								balancing = (leftSense || rightSense);
								if (balancing) animator.SetSpriteAnimation(characterID, AnimationType.Balance, true);
							}
						}
						else if (solidObject != null)
						{
							if (position.x < solidObject.position.x - PixelUnitHelper.P2U(solidObject.outerbox.WidthRadius)) balancing = true;
							if (position.x > solidObject.position.x + PixelUnitHelper.P2U(solidObject.outerbox.WidthRadius)) balancing = true;
							if (balancing) animator.SetSpriteAnimation(characterID, AnimationType.Balance, true);
						}


						if (onGround && !balancing)
							HandleIdleAnimation();
					}
				}
				else
				{
					pushing = Mathf.Clamp(pushing, -3, 3);
					timer = 0;
					animator.SetSpriteAnimation(characterID, AnimationType.Push, true, 2);
				}
			}
		}
	}
}