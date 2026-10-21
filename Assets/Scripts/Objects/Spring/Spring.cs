using Objects.Base;
using Objects.Player;
using Objects.Spring.Constants;
using UnityEditor;
using UnityEngine;
using VEngine.Animation;
using VEngine.Collision;
using VEngine.Entity;
using VEngine.Entity.Constants;
using VEngine.Utilities.Helpers;
using VEngine.Audio;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace Objects.Spring
{
	[ExecuteAlways]
	public class Spring : SolidObject
	{
#if UNITY_EDITOR
		private static Sprite _yellowPreviewSprite;
		private static Sprite _yellowPreviewSpriteDiagonal;
		
		private static Sprite _redPreviewSprite;
		private static Sprite _redPreviewSpriteDiagonal;
		
		private static Sprite _greyPreviewSprite;
		private static Sprite _greyPreviewSpriteDiagonal;
		
		private static Sprite _bluePreviewSprite;
		private static Sprite _bluePreviewSpriteDiagonal;
#endif
		private static ObjectAnims _yellowAnims;
		private static ObjectAnims _redAnims;
		private static ObjectAnims _greyAnims;
		private static ObjectAnims _blueAnims;

		[SerializeField] private SpringDirection springDirection;
		[SerializeField] private SpringType springType;
		private SpriteRenderer spriteRenderer;
		private int solidityTimer;
		private Entity lastCollidedEntity;
		[SerializeField] private GameObject spriteObject;
		[SerializeField] private ObjectAnimator objectAnimator;
		private int[] surfaceOffsetArray = new int[32];
		static float SpringPowerThisFrame;
		static Vector2 EntityPositionStartFrame;
		static Vector2 EntityVelocityStartFrame;
		static bool StartFrame;
		static bool ForceThisFrame;
		private float SpringPower
		{
			get
			{
				return springType switch
				{
					SpringType.Grey => 6,
					SpringType.Yellow => 9,
					SpringType.Red => 12,
					SpringType.Blue => 14,
					_ => 10
				};
			}
		}

		private bool diagonal;

		public new void Start()
		{
			FindAnims();
			FindPreviewSprites();
		}

		private void FindAnims()
		{
			_yellowAnims ??= Resources.Load<ObjectAnims>("Objects/Spring/YellowSpring");
			_redAnims ??= Resources.Load<ObjectAnims>("Objects/Spring/RedSpring");
			_greyAnims ??= Resources.Load<ObjectAnims>("Objects/Spring/GreySpring");
			_blueAnims ??= Resources.Load<ObjectAnims>("Objects/Spring/BlueSpring");
		}

		private void FindPreviewSprites()
		{
#if UNITY_EDITOR
			_greyPreviewSprite ??= ObjectAnimator.GetAnimation(_greyAnims, "idle").frames[0];
			_greyPreviewSpriteDiagonal ??= ObjectAnimator.GetAnimation(_greyAnims, "idleDiagonal").frames[0];
			_yellowPreviewSprite ??= ObjectAnimator.GetAnimation(_yellowAnims, "idle").frames[0];
			_yellowPreviewSpriteDiagonal ??= ObjectAnimator.GetAnimation(_yellowAnims, "idleDiagonal").frames[0];
			_redPreviewSprite ??= ObjectAnimator.GetAnimation(_redAnims, "idle").frames[0];
			_redPreviewSpriteDiagonal ??= ObjectAnimator.GetAnimation(_redAnims, "idleDiagonal").frames[0];
			_bluePreviewSprite ??= ObjectAnimator.GetAnimation(_blueAnims, "idle").frames[0];
			_bluePreviewSpriteDiagonal ??= ObjectAnimator.GetAnimation(_blueAnims, "idleDiagonal").frames[0];
#endif
		}

		public new void Update()
		{
			FindAnims();
#if UNITY_EDITOR
			if (Application.isPlaying) return;
			if (spriteObject == null || !Selection.Contains(gameObject)) return;
			spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
			FindPreviewSprites();
			if (spriteRenderer == null) return;

			diagonal = ((springDirection == SpringDirection.UpLeft) ||
							(springDirection == SpringDirection.UpRight) ||
							(springDirection == SpringDirection.DownLeft) ||
							(springDirection == SpringDirection.DownRight));

			switch (springDirection)
			{
				case SpringDirection.UpLeft:
					transform.eulerAngles = new Vector3(0, 0, 0);
					spriteObject.transform.localPosition = new Vector3(1f, -1f, 0);
					spriteRenderer.flipX = true;
					break;
				case SpringDirection.Up:
					transform.eulerAngles = new Vector3(0, 0, 0);
					spriteObject.transform.localPosition = new Vector3(-1, -0.5f, 0);
					spriteRenderer.flipX = false;
					break;
				case SpringDirection.UpRight:
					transform.eulerAngles = new Vector3(0, 0, 0);
					spriteObject.transform.localPosition = new Vector3(-1f, -1f, 0);
					spriteRenderer.flipX = false;
					break;
				case SpringDirection.Right:
					transform.eulerAngles = new Vector3(0, 0, 270);
					spriteObject.transform.localPosition = new Vector3(-1f, -0.5f, 0);
					spriteRenderer.flipX = false;
					break;
				case SpringDirection.Left:
					transform.eulerAngles = new Vector3(0, 0, 90);
					spriteObject.transform.localPosition = new Vector3(-1f, -0.5f, 0);
					spriteRenderer.flipX = false;
					break;
				case SpringDirection.DownLeft:
					transform.eulerAngles = new Vector3(0, 0, 90);
					spriteObject.transform.localPosition = new Vector3(1f, -1f, 0);
					spriteRenderer.flipX = true;
					break;
				case SpringDirection.Down:
					transform.eulerAngles = new Vector3(0, 0, 180);
					spriteObject.transform.localPosition = new Vector3(-1f, -0.5f, 0);
					spriteRenderer.flipX = false;
					break;
				case SpringDirection.DownRight:
					transform.eulerAngles = new Vector3(0, 0, 270);
					spriteObject.transform.localPosition = new Vector3(-1f, -1f, 0);
					spriteRenderer.flipX = false;
					break;
			}

			switch(springType)
			{
				case SpringType.Yellow:
					spriteRenderer.sprite = !diagonal ? _yellowPreviewSprite : _yellowPreviewSpriteDiagonal;
					break;
				case SpringType.Red:
					spriteRenderer.sprite = !diagonal ? _redPreviewSprite : _redPreviewSpriteDiagonal;
					break;
				case SpringType.Grey:
					spriteRenderer.sprite = !diagonal ? _greyPreviewSprite : _greyPreviewSpriteDiagonal;
					break;
				case SpringType.Blue:
					spriteRenderer.sprite = !diagonal ? _bluePreviewSprite : _bluePreviewSpriteDiagonal;
					break;
				default: break;
			}
#endif
		}

		protected override void ObjectOnLoad()
		{
			switch (springDirection)
			{
				case SpringDirection.Up:
					jumpThrough = true;
					break;
				default:
					jumpThrough = false;
					break;
			}
			diagonal = ((springDirection == SpringDirection.UpLeft) ||
							(springDirection == SpringDirection.UpRight) ||
							(springDirection == SpringDirection.DownLeft) ||
							(springDirection == SpringDirection.DownRight));

			objectAnimator = GetComponent<ObjectAnimator>();
			objectAnimator.spriteObject = spriteObject;

			switch (springType)
			{
				case SpringType.Grey:
					objectAnimator.objectAnims = _greyAnims;
					outerbox.left = -8;
					outerbox.right = 8;
					break;
				case SpringType.Yellow:
					objectAnimator.objectAnims = _yellowAnims;
					outerbox.left = -16;
					outerbox.right = 16;
					break;
				case SpringType.Red:
					objectAnimator.objectAnims = _redAnims;
					outerbox.left = -16;
					outerbox.right = 16;
					break;
				case SpringType.Blue:
					objectAnimator.objectAnims = _blueAnims;
					outerbox.left = -16;
					outerbox.right = 16;
					break;
				default: break;
			}
			objectAnimator.speed = 96;

			if (!diagonal)
				objectAnimator.SetSpriteAnimation("idle", false, 0, true);
			else
				objectAnimator.SetSpriteAnimation("idleDiagonal", false, 0, true);

			position = transform.position;
			// set spring hitbox from spring direction
			if (springDirection != SpringDirection.Left && springDirection != SpringDirection.Right)
			{
				if (springDirection != SpringDirection.Up && springDirection != SpringDirection.Down)
					outerbox = new Hitbox(16, 16);
				else
				{
					switch (springType)
					{
						case SpringType.Grey:
							outerbox = new Hitbox(8, 8);
							break;
						case SpringType.Yellow:
							outerbox = new Hitbox(14, 8);
							break;
						case SpringType.Red:
							outerbox = new Hitbox(15, 8);
							break;
						case SpringType.Blue:
							outerbox = new Hitbox(16, 8);
							break;
						default: break;
					}
				}

			}
			else
			{
				switch (springType)
				{
					case SpringType.Grey:
						outerbox = new Hitbox(8, 8);
						break;
					case SpringType.Yellow:
						outerbox = new Hitbox(8, 14);
						break;
					case SpringType.Red:
						outerbox = new Hitbox(8, 15);
						break;
					case SpringType.Blue:
						outerbox = new Hitbox(8, 16);
						break;
					default: break;
				}
			}
			SetSurfaceOffsetArray();
		}

		private void SetSurfaceOffsetArray()
		{
			// offset array for diagnal springs
			int[] offsetArrayDefault =
			{
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				-1, -2, -3, -4, -5, -6, -7, -8, -9, -10, -11, -12, -13, -14,
				-15, -16, -17, -18, -19, -20, -21, -22
			};

			switch (springDirection)
			{
				case SpringDirection.DownRight:
				case SpringDirection.UpRight:
					surfaceOffsetArray = offsetArrayDefault;
					break;
				case SpringDirection.DownLeft:
				case SpringDirection.UpLeft:
					surfaceOffsetArray = offsetArrayDefault;
					System.Array.Reverse(surfaceOffsetArray);
					break;
				case SpringDirection.Up:
				case SpringDirection.Right:
				case SpringDirection.Down:
				case SpringDirection.Left:
				default:
					surfaceOffsetArray = new int[32];
					break;
			}
		}

		protected override void ObjectFixedUpdate()
		{
			if ((objectAnimator.animationID == "activate" || objectAnimator.animationID == "activateDiagonal") && objectAnimator.frameID == objectAnimator.frameCount)
			{
				if (!diagonal)
					objectAnimator.SetSpriteAnimation("idle", false, 0, true);
				else
					objectAnimator.SetSpriteAnimation("idleDiagonal", false, 0, true);
			}
			objectAnimator.ProcessAnimation();

			if (solidityTimer > 0)
			{
				solid = false;
				solidityTimer--;
			}
			else
				solid = true;

			if (solid)
				HandleObjectCollision();
#if !UNITY_EDITOR
			transform.position = VectorHelper.Vector2Floor((position) * 16f) / 16f;
#else
			position = transform.position;
#endif
			ForceThisFrame = false;
			SpringPowerThisFrame = 0;
		}

		protected override void LateObjectFixedUpdate()
		{
			base.LateObjectFixedUpdate();
		}

		protected override void BeforeCollision(Entity entity, Direction direction)
		{
			if (surfaceOffsetArray.Length < 32) SetSurfaceOffsetArray();

			var leftXPosition = Mathf.FloorToInt((entity.position.x - (position.x - 1f)) * 16f);
			switch (springDirection)
			{
				case SpringDirection.UpRight:
				case SpringDirection.UpLeft:
					{
						if (entity.position.y < position.y) break;
						if (leftXPosition is >= 0 and < 32)
						{
							hitboxOffset.y = PixelUnitHelper.P2U(surfaceOffsetArray[leftXPosition]);
						}
						else if (leftXPosition < 0 && leftXPosition >= -entity.GroundModeWidthRadius)
						{
							hitboxOffset.y = PixelUnitHelper.P2U(surfaceOffsetArray[0]);
						}
						else if (leftXPosition >= 32 && leftXPosition < (32 + entity.GroundModeWidthRadius + 1))
						{
							hitboxOffset.y = PixelUnitHelper.P2U(surfaceOffsetArray[31]);
						}
						else
							hitboxOffset.y = 0;

						break;
					}
				case SpringDirection.DownRight:
				case SpringDirection.DownLeft when entity.position.y < position.y:
					{
						if (entity.position.y > position.y) break;
						if (leftXPosition is >= 0 and < 32)
						{
							hitboxOffset.y = -PixelUnitHelper.P2U(surfaceOffsetArray[leftXPosition]);
						}
						else hitboxOffset.y = 0;

						break;
					}
			}
		}

		protected override void OnCollision(Entity entity, Direction direction)
		{
			if (ForceThisFrame && SpringPowerThisFrame < SpringPower)
			{
				ForceThisFrame = false;
			}

			if (CheckDirection(direction) == false) return;

			var playerObject = (PlayerObject)entity;

			bool activated = true;

			switch (springDirection)
			{
				case SpringDirection.UpLeft:
					if (!(entity.position.x < position.x + PixelUnitHelper.P2U(6)))
					{
						activated = false;
						break;
					}
					if (entity.position.y < position.y && entity.velocity.y < 0)
					{
						activated = false;
						break;
					}
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y -= PixelUnitHelper.P2U(8);
						entity.velocity = new Vector2(-1, 1).normalized * SpringPower;
					}
					break;

				case SpringDirection.Up:
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y -= PixelUnitHelper.P2U(8);
						entity.velocity.y = SpringPower;
					}
					break;

				case SpringDirection.UpRight:
					if (!(entity.position.x > position.x - PixelUnitHelper.P2U(6)))
					{
						activated = false;
						break;
					}
					if (entity.position.y < position.y && entity.velocity.y < 0)
					{
						activated = false;
						break;
					}
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y -= PixelUnitHelper.P2U(8);
						entity.velocity = new Vector2(1, 1).normalized * SpringPower;
					}
					break;

				case SpringDirection.DownLeft:
					if (!(entity.position.x < position.x + PixelUnitHelper.P2U(6)))
					{
						activated = false;
						break;
					}
					if (entity.position.y > position.y && entity.velocity.y > 0)
					{
						activated = false;
						break;
					}
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y += PixelUnitHelper.P2U(8);
						entity.velocity = new Vector2(-1, -1).normalized * SpringPower;
					}
					break;

				case SpringDirection.Down:
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y += PixelUnitHelper.P2U(8);
						entity.velocity.y = -SpringPower;
					}
					break;

				case SpringDirection.DownRight:
					if (!(entity.position.x > position.x - PixelUnitHelper.P2U(6)))
					{
						activated = false;
						break;
					}
					if (entity.position.y > position.y && entity.velocity.y > 0)
					{
						activated = false;
						break;
					}
					if (!ForceThisFrame)
					{
						entity.onGround = false;
						entity.position.y += PixelUnitHelper.P2U(8);
						entity.velocity = new Vector2(1, -1).normalized * SpringPower;
					}
					break;

				case SpringDirection.Left:
					if (!ForceThisFrame)
					{
						if (playerObject != null) playerObject.controlLock = 16;
						entity.position.x += PixelUnitHelper.P2U(8);
						entity.direction = Flip.X;
						entity.velocity.x = -SpringPower;
						entity.groundVel = -SpringPower;
					}
					break;

				case SpringDirection.Right:
					if (!ForceThisFrame)
					{
						if (playerObject != null) playerObject.controlLock = 16;
						entity.position.x -= PixelUnitHelper.P2U(8);
						entity.direction = Flip.None;
						entity.velocity.x = SpringPower;
						entity.groundVel = SpringPower;
					}
					break;
			}

			if (activated)
			{
				ForceThisFrame = true;
				SpringPowerThisFrame = SpringPower;
				if (lastCollidedEntity == entity && solidityTimer > 0)
					return;
				else
				{
					lastCollidedEntity = entity;
					solidityTimer = 4;
				}

				entity.onObject = false;
				entity.attachedObject = null;
				entity.solidObject = null;

				if (playerObject != null)
				{
					playerObject.applyJumpCap = false;
					playerObject.jumpAbilityState = 0;
					playerObject.skidding = 0;
				}

				var diagonal = ((springDirection == SpringDirection.UpLeft) ||
								(springDirection == SpringDirection.UpRight) ||
								(springDirection == SpringDirection.DownLeft) ||
								(springDirection == SpringDirection.DownRight));
				if (!diagonal)
					objectAnimator.SetSpriteAnimation("activate", false, null, true, 0, true);
				else
					objectAnimator.SetSpriteAnimation("activateDiagonal", false, null, true, 0, true);

				AudioManager.PlayPersistant("Spring");
			}
			hitboxOffset = Vector2.zero;
		}

		private bool CheckDirection(Direction direction)
		{
			switch (direction)
			{
				case Direction.Up:
					if (springDirection == SpringDirection.Up || springDirection == SpringDirection.UpLeft ||
						springDirection == SpringDirection.UpRight)
						return true;
					break;
				case Direction.Right:
					if (springDirection == SpringDirection.Right || springDirection == SpringDirection.UpRight ||
						springDirection == SpringDirection.DownRight)
						return true;
					break;
				case Direction.Down:
					if (springDirection == SpringDirection.Down || springDirection == SpringDirection.DownLeft ||
						springDirection == SpringDirection.UpLeft || springDirection == SpringDirection.DownRight ||
						springDirection == SpringDirection.UpRight)
						return true;
					break;
				case Direction.Left:
					if (springDirection == SpringDirection.Left || springDirection == SpringDirection.DownLeft ||
						springDirection == SpringDirection.UpLeft)
						return true;
					break;
			}

			return false;
		}
	}
}