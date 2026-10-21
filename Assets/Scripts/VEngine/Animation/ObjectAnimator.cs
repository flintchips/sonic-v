using Objects.Stage;
using UnityEngine;

namespace VEngine.Animation
{
	[AddComponentMenu("SonicV/ObjectAnimator")]
	public class ObjectAnimator : MonoBehaviour
	{
		public int frameID, frameCount, speed;
		public string animationID;
		public ObjectAnims objectAnims;
		public ObjectAnimation objectAnimation;
		[HideInInspector] public ObjectAnimation objectAnimationStore;
		[HideInInspector] public bool looping;
		public GameObject spriteObject;
		[HideInInspector] public SpriteRenderer spriteRenderer;
		[HideInInspector] public StageObject stageObject;

		private bool forceStart;
		private int loopTo;
		private float timer;

		private void Start()
		{
			stageObject = GetComponent<StageObject>();
			if (spriteObject != null)
				spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
		}

		private void ResetFrameTimer()
		{
			if (speed == 0) return;
			timer = 60f / (speed / 5.33f);
		}

		public void SetSpriteAnimation(string animName, bool loopAnim = false, int? startFrame = null,
			bool forceAnim = true, int loopToFrame = 0, bool fastStart = false)
		{
			loopTo = loopToFrame;
			forceStart = forceAnim;

			if (animationID != animName)
			{
				animationID = animName;

				objectAnimation = GetAnimation(animationID);

				frameCount = objectAnimation.frames?.Count ?? 0;

				startFrame ??= 0;
				frameID = (int)startFrame;
			}

			looping = loopAnim;

			if (fastStart && spriteRenderer != null) spriteRenderer.sprite = objectAnimation.frames[frameID];
		}

		public void ProcessAnimation()
		{
			objectAnimation = GetAnimation(animationID);
			if (objectAnimation != null && objectAnimation.frames != null)
			{
				frameCount = objectAnimation.frames.Count;
			}
			else
			{
				frameCount = 0;
			}

			if (timer > 0 && speed != 0)
			{
				timer--;
			}

			if (forceStart && objectAnimationStore != objectAnimation)
			{
				timer = 0f;
			}

			if (timer <= 0)
			{
				timer = 0;
				RunNextFrame();
			}

			objectAnimationStore = objectAnimation;
		}

		private void RunNextFrame()
		{
			if (spriteRenderer == null) return;
			if (objectAnimation == null || objectAnimation.frames == null || objectAnimation.frames.Count == 0)
			{
				return;
			}

			if (frameID < objectAnimation.frames.Count)
			{
				spriteRenderer.sprite = objectAnimation.frames[frameID];
				frameID++;
				ResetFrameTimer();
			}
			else
			{
				spriteRenderer.sprite = objectAnimation.frames[^1];
				if (looping) frameID = loopTo;
				ResetFrameTimer();
			}
		}

		private ObjectAnimation GetAnimation(string animName)
		{
			if (objectAnims == null) return new ObjectAnimation(animName, null);
			if (objectAnims.animations == null) return new ObjectAnimation(animName, null);
			for (var i = 0; i < objectAnims.animations.Count; i++)
			{
				if (objectAnims.animations[i].name == animName)
				{
					return objectAnims.animations[i];
				}

				if (i == objectAnims.animations.Count - 1)
				{
					return new ObjectAnimation(animName, null);
				}
			}

			return new ObjectAnimation(animName, null);
		}

		public static ObjectAnimation GetAnimation(ObjectAnims anims, string animName)
		{
			if (anims == null) return new ObjectAnimation(animName, null);
			if (anims.animations == null) return new ObjectAnimation(animName, null);
			for (var i = 0; i < anims.animations.Count; i++)
			{
				if (anims.animations[i].name == animName)
				{
					return anims.animations[i];
				}

				if (i == anims.animations.Count - 1)
				{
					return new ObjectAnimation(animName, null);
				}
			}

			return new ObjectAnimation(animName, null);
		}
	}
}