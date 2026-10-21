using Objects.Player;
using Objects.Stage;
using UnityEngine;
using UnityEngine.Serialization;
using VEngine.Animation;
using VEngine.Utilities.Helpers;
using VEngine.Entity.Constants;
using Objects.Player.Constants;

namespace Objects.SpinDust
{
	public class SpindashDust : StageObject
	{
		public ObjectAnimator objectAnimator;
		[FormerlySerializedAs("player")]
		[HideInInspector]
		public PlayerObject playerObject;

		[HideInInspector] public bool followPlayer;
		private bool dissipate;

		public new void Start()
		{

			transform.position = VectorHelper.Vector2Floor((playerObject.position -
															new Vector2(0,
																PixelUnitHelper.P2U(playerObject
																	.GroundModeHeightRadius))) * 16f) / 16f;
			objectAnimator = GetComponent<ObjectAnimator>();
			objectAnimator.speed = 128;
			objectAnimator.SetSpriteAnimation("dust", true, 0, true, 16);
		}

		public void InitiateDissipate()
		{
			dissipate = true;
			followPlayer = false;
			playerObject.spindashDust = null;
		}

		protected override void ObjectFixedUpdate()
		{
			if (playerObject.CurrentState != PlayerState.Spindash)
				InitiateDissipate();
			switch (dissipate)
			{
				case true when objectAnimator.animationID == "dust" && objectAnimator.frameID >= 16:
					// wait till the dust anim has at least reached its looping stage to start dissipating
					objectAnimator.speed = 128;
					objectAnimator.SetSpriteAnimation("dissipate", true, 0);
					break;
				case true:
					objectAnimator.speed = 256;
					break;
			}

			objectAnimator.ProcessAnimation();
			if (objectAnimator.animationID == "dissipate" && objectAnimator.frameID == 16)
			{
				loaded = false;
				Destroy(this.gameObject);
			}
			float dustPosXOffset = (playerObject.direction == Flip.None) ? -PixelUnitHelper.P2U(4) : PixelUnitHelper.P2U(4);
			if (followPlayer)
				transform.position = VectorHelper.Vector2Floor((playerObject.position -
																new Vector2(dustPosXOffset,
																	PixelUnitHelper.P2U(playerObject
																		.GroundModeHeightRadius))) * 16f) / 16f;
		}
	}
}