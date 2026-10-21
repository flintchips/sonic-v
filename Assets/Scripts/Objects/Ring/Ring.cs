using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objects.Base;
using VEngine.Entity;
using VEngine.Entity.Constants;
using Utilities;
using VEngine.Animation;
using VEngine.Audio;

namespace Objects.Ring
{
	public class Ring : HitboxObject
	{
		static bool panRight;
		ObjectAnimator animator;
		bool shimmer = false;

		protected override void ObjectOnLoad()
		{
			animator = GetComponent<ObjectAnimator>();
			animator.speed = 64;
		}

		protected override void ObjectFixedUpdate()
		{
			if (animator.animationID == "Shimmer" && shimmer)
			{
				if (animator.frameID == 14)
				{
					loaded = false;
					Destroy(this.gameObject);
				}
			}
			else
			{
				float beatLengthHalf = 120 / Utilities.Stage.Instance.songBpm;
				int ringFrameID = Mathf.FloorToInt(((Time.time % beatLengthHalf) / beatLengthHalf) * 8);
				animator.SetSpriteAnimation("Rotate", true, ringFrameID, true);
				animator.frameID = ringFrameID;
				if (active)
					HandleHitboxCollision();
			}
			animator.ProcessAnimation();
		}

		protected override void OnEnter(Entity entity)
		{
			AudioManager.PlayPersistant("Ring");
			float panStrength = 50;
			float pan = panStrength;
			if (panRight)
			{
				panRight = false;
			}
			else
			{
				pan *= -1;
				panRight = true;
			}
			AudioManager.GetPersistantEventInstance("Ring").setParameterByName("Pan", panStrength);
			shimmer = true;
			animator.SetSpriteAnimation("Shimmer", true);
			Utilities.Stage.Instance.ringCount++;
			animator.SetSpriteAnimation("Shimmer", true);
		}
	}
}