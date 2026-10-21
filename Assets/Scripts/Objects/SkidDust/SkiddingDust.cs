using Objects.Player;
using Objects.Stage;
using UnityEngine;
using UnityEngine.Serialization;
using VEngine.Animation;
using VEngine.Utilities.Helpers;
using VEngine.Entity.Constants;
using Objects.Player.Constants;

namespace Objects.SkidDust
{
	public class SkiddingDust : StageObject
	{
		public ObjectAnimator objectAnimator;
		[HideInInspector]
		public PlayerObject playerObject;

		public static GameObject InitializePrefab()
		{
			return Instantiate(Resources.Load("Prefabs/SkiddingDust")) as GameObject;
		}

		public void Initialize(PlayerObject player, bool flip)
		{
			playerObject = player;
			objectAnimator = GetComponent<ObjectAnimator>();
			var spriteRenderer = objectAnimator.spriteObject.GetComponent<SpriteRenderer>();
			transform.localScale = flip ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
			transform.position = VectorHelper.Vector2Floor(playerObject.position * 16f) / 16f + new Vector2((flip ? 1 / 32f : 0), 0);

			objectAnimator.speed = 128;
			objectAnimator.SetSpriteAnimation("dustPuff", false, 0);
		}

		protected override void ObjectFixedUpdate()
		{
			objectAnimator.ProcessAnimation();

			if (objectAnimator.frameID == 10)
			{
				loaded = false;
				Destroy(this.gameObject);
			}
		}
	}
}