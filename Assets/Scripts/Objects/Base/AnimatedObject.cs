using UnityEngine;
using VEngine.Animation;

namespace Objects.Base
{
    [RequireComponent(typeof(ObjectAnimator))]
    public class AnimatedObject : ObjectBase
    {
        public string defaultAnim = "default";
        public int speed;
        public bool loop;

        private ObjectAnimator animator;

        private void Start()
        {
            animator = GetComponent<ObjectAnimator>();

            animator.SetSpriteAnimation(defaultAnim, loop, null, false);
            animator.speed = speed;
        }

        private void FixedUpdate()
        {
            animator.ProcessAnimation();
        }
    }
}