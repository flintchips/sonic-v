using Objects.Base;
using UnityEngine;
using VEngine.Utilities.Helpers;

namespace Objects.Platforms
{
    public class MovingPlatformObject : SolidObject
    {
        private Vector2 startPosition;
        public float angle;
        public float rotationSpeed;
        public float radius;

        public void Awake()
        {
            startPosition = transform.position;
        }

        protected override void ObjectOnLoad()
        {
            position = startPosition;
        }

        protected override void ObjectFixedUpdate()
        {
            var oldPos = position;

            position.x = Mathf.Floor((startPosition.x + (Mathf.Cos(angle * Mathf.Deg2Rad)) * radius) * 16f) / 16f;
            position.y = Mathf.Floor((startPosition.y + (Mathf.Sin(angle * Mathf.Deg2Rad)) * radius) * 16f) / 16f;

            velocity = (position - oldPos) * 16f;

            angle += rotationSpeed;

            transform.position = VectorHelper.Vector2Floor((position) * 16f) / 16f;

            if (solid)
                HandleObjectCollision();
        }
    }
}