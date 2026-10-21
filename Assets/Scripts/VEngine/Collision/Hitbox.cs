using System;

namespace VEngine.Collision
{
    [Serializable]
    public class Hitbox
    {
        // BOUNDS
        public int bottom;
        public int top;
        public int left;
        public int right;
        public int WidthRadius => right;
        public int HeightRadius => top;

        public Hitbox(int widthRadius = 0, int heightRadius = 0)
        {
            left = -widthRadius;
            right = widthRadius;
            bottom = -heightRadius;
            top = heightRadius;
        }

        public Hitbox(int left, int right, int bottom, int top)
        {
            this.left = left;
            this.right = right;
            this.bottom = bottom;
            this.top = top;
        }
    }
}