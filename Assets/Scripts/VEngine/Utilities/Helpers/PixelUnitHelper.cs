using UnityEngine;

namespace VEngine.Utilities.Helpers
{
    public abstract class PixelUnitHelper
    {
        public static float P2U(float pix)
        {
            return pix / 16f;
        }

        public static float U2P(float unit)
        {
            return Mathf.Floor(unit * 16);
        }

        public static Vector2 P2toU2(Vector2 p2)
        {
            return new Vector2(P2U(p2.x), P2U(p2.y));
        }

        public static Vector2 U2toP2(Vector2 u2)
        {
            return new Vector2(U2P(u2.x), U2P(u2.y));
        }
    }
}