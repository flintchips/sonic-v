using UnityEngine;

namespace VEngine.Math
{
    public abstract class Rotation
    {
        public static float Clamp0360(float angle)
        {
            var result = angle - Mathf.CeilToInt(angle / 360f) * 360f;
            if (result < 0)
            {
                result += 360f;
            }
            return result;
        }

        public static Vector2 RotatePointAroundPivot(Vector2 point, Vector2 pivot, float a)
        {
            var s = Mathf.Sin(a * Mathf.Deg2Rad);
            var c = Mathf.Cos(a * Mathf.Deg2Rad);
			
            var p = point;

            // translate point back to origin:
            p.x -= pivot.x;
            p.y -= pivot.y;
			
            // rotate point
            var xNew = p.x * c - p.y * s + pivot.x;
            var yNew = p.x * s + p.y * c + pivot.y;

            // translate point back:
            p.x = xNew;
            p.y = yNew;
            return p;
        }

        public static float AngleDifference(float angle1, float angle2)
        {
            var diff = ( angle2 - angle1 + 180 ) % 360 - 180;
            return diff < -180 ? diff + 360 : diff;
        }
    }
}