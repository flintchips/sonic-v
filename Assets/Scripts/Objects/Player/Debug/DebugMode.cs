using UnityEngine;

namespace Objects.Player.Debug
{
    public abstract class DebugMode
    {
        public static float Speed;
        public static Vector2 Velocity;

        public static void DebugFixedUpdate(PlayerObject entity)
        {
            DebugHandleMovement(entity);
        }

        private static void DebugHandleMovement(PlayerVars playerObject)
        {
            var moved = false;

            Velocity = Vector2.zero;
            var dir = Vector2.zero;

            if (playerObject.control.down)
            {
                dir.y -= 1;
                moved = true;
            }

            if (playerObject.control.up)
            {
                dir.y += 1;
                moved = true;
            }

            if (playerObject.control.left)
            {
                dir.x -= 1;
                moved = true;
            }

            if (playerObject.control.right)
            {
                dir.x += 1;
                moved = true;
            }

            Velocity = dir.normalized * Speed;
            playerObject.position += Velocity / 16f * playerObject.stepDelta;

            if (!moved || !PlayerVars.PlayerDebugMode)
            {
                Speed = 0;
            }
            else
            {
                Speed += 0.046875f;
                if (Speed > 16f)
                    Speed = 16f;
            }
        }
    }
}