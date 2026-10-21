using Objects.Player;
using Objects.Player.Constants;
using UnityEngine;
using UnityEngine.Serialization;

namespace VEngine.Animation
{
	[AddComponentMenu("SonicV/VAnimator")]
	public class Animator : MonoBehaviour
	{
		[HideInInspector] public int frameID, frameCount, speed, end;
		public AnimationType animationID;
		[HideInInspector] public CharacterType characterID;
		[HideInInspector] public CharacterAnims characterAnims;
		[HideInInspector] public new Animation animation;
		[HideInInspector] public Animation animStore; // animation used on last frame
		[HideInInspector] public bool looping;
		public GameObject spriteObject;
		[HideInInspector] public SpriteRenderer spriteRenderer;

		[FormerlySerializedAs("player")]
		[HideInInspector]
		public PlayerObject playerObject;

		private bool forceStart;
		private static CharacterAnims[] _allCharacterAnims;
		private int loopTo;
		private float timer;

		private void Start()
		{
			playerObject = GetComponent<PlayerObject>();
			if (spriteObject != null)
				spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
		}

		private void ResetFrameTimer()
		{
			if (speed == 0) return;
			timer = 60f / (speed / 6f);
		}

		public void SetSpriteAnimation(CharacterType character, AnimationType anim, bool loopAnim = false,
			int? startFrame = null, bool forceAnim = true, int loopToFrame = 0, bool startOnItself = false)
		{

			loopTo = loopToFrame;
			forceStart = forceAnim;
			characterID = character;

			var characterAnimations = GetCharacterAnims(characterID);
			characterAnims = characterAnimations;

			if (animationID != anim)
			{
				animationID = anim;

				animation = GetAnimation(characterAnims, animationID);

				frameCount = animation.frames?.Count ?? 0;

				startFrame ??= 0;
				frameID = (int)startFrame;
			}

			if (startOnItself)
			{
				startFrame ??= 0;
				frameID = (int)startFrame;
			}

			looping = loopAnim;
		}

		public void ProcessAnimation()
		{
			if (characterAnims == null)
			{
				characterAnims = GetCharacterAnims(characterID);
			}

			animation = GetAnimation(characterAnims, animationID);
			frameCount = animation is { frames: not null } ? animation.frames.Count : 0;

			if (timer > 0 && speed != 0)
			{
				timer--;
			}

			if (forceStart && animStore != animation)
			{
				timer = 0f;
			}

			if (timer <= 0)
			{
				timer = 0;
				RunNextFrame();
			}

			animStore = animation;
		}

		private void RunNextFrame()
		{
			if (spriteRenderer == null) return;
			if (animation?.frames == null || animation.frames.Count == 0)
			{
				return;
			}

			if (frameID < (animation.frames.Count - 1))
			{
				spriteRenderer.sprite = animation.frames[frameID];
				frameID++;
				ResetFrameTimer();
			}
			else
			{
				spriteRenderer.sprite = animation.frames[Mathf.Min(frameID, animation.frames.Count - 1)];
				if (looping) frameID = loopTo;
				ResetFrameTimer();
			}
		}

		// private Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float angle)
		// {
		//     var dir = (Vector3)point - (Vector3)pivot; // get point direction relative to pivot
		//     dir = Quaternion.Euler(0, 0, angle) * dir; // rotate it
		//     Vector2 newPoint = dir + (Vector3)pivot; // calculate rotated point
		//     return new Vector2(newPoint.x, newPoint.y); // return it
		// }

		private static void ReloadCharacterAnims()
		{
			_allCharacterAnims = Resources.LoadAll<CharacterAnims>("Characters");
		}

		private static Animation GetAnimation(CharacterAnims characterAnims, AnimationType anim)
		{
			if (characterAnims == null) return new Animation(anim, null);
			if (characterAnims.animations == null) return new Animation(anim, null);

			for (var i = 0; i < characterAnims.animations.Count; i++)
			{
				if (characterAnims.animations[i].animationID == anim)
				{
					return characterAnims.animations[i];
				}

				if (i == characterAnims.animations.Count - 1)
				{
					return new Animation(anim, null);
				}
			}

			return new Animation(anim, null);
		}

		private static CharacterAnims GetCharacterAnims(CharacterType characterID)
		{
			if (_allCharacterAnims == null) ReloadCharacterAnims();

			var errored = false;

			if (_allCharacterAnims == null) return null;
			for (var i = 0; i < _allCharacterAnims.Length; i++)
			{
				if (_allCharacterAnims[i].character == characterID)
				{
					return _allCharacterAnims[i];
				}

				if (i != _allCharacterAnims.Length - 1) continue;
				if (errored == false)
				{
					errored = true;
					ReloadCharacterAnims();
					i = 0;
				}
				else
				{
					return null;
				}
			}

			return null;
		}
	}
}