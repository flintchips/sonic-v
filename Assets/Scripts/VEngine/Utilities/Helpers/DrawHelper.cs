using UnityEngine;

namespace VEngine.Utilities.Helpers
{
    public abstract class DrawHelper
    {
        public static Rect AddRect(Rect a, Rect b)
        {
            return new Rect(a.x + b.x, a.y + b.y, a.width + b.width, a.height + b.height);
        }
        
        public static void Line(Vector2 start, Vector2 end, Color col)
        {
            var x = Mathf.FloorToInt(start.x * 16f);
            var y = Mathf.FloorToInt(start.y * 16f);
            var x2 = Mathf.FloorToInt(end.x * 16f);
            var y2 = Mathf.FloorToInt(end.y * 16f);
            var w = x2 - x;
            var h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            dx1 = w switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => dx1
            };
            dy1 = h switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => dy1
            };
            dx2 = w switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => dx2
            };
            var longest = Mathf.Abs(w);
            var shortest = Mathf.Abs(h);
            if (!(longest > shortest))
            {
                longest = Mathf.Abs(h);
                shortest = Mathf.Abs(w);
                dy2 = h switch
                {
                    < 0 => -1,
                    > 0 => 1,
                    _ => dy2
                };
                dx2 = 0;
            }

            var numerator = longest >> 1;
            for (var i = 0; i <= longest; i++)
            {
                DrawPixel(new Vector2(x / 16f, y / 16f), col);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        public static void DrawPixel(Vector2 pos, Color col)
        {
            var x = Mathf.FloorToInt(pos.x * 16f);
            var y = Mathf.FloorToInt(pos.y * 16f);
            var pointA = new Vector2(x - 0.1f, y - 0.1f) / 16f;
            var pointB = new Vector2(x + 0.9f, y + 0.9f) / 16f;
            Debug.DrawLine(pointA, pointB, col);
        }

        public static void DrawAnchor(Vector2 pos)
        {
            DrawPixel(new Vector2(pos.x, pos.y), Color.black);
            DrawPixel(new Vector2(pos.x - PixelUnitHelper.P2U(1), pos.y), Color.white);
            DrawPixel(new Vector2(pos.x + PixelUnitHelper.P2U(1), pos.y), Color.white);
            DrawPixel(new Vector2(pos.x, pos.y - PixelUnitHelper.P2U(1)), Color.white);
            DrawPixel(new Vector2(pos.x, pos.y + PixelUnitHelper.P2U(1)), Color.white);
        }
    }
}