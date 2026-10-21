using UnityEngine;
using VEngine.Entity.Constants;

namespace VEngine.Utilities.Helpers
{
    public abstract class VectorHelper
    {
        public static Vector3 Vector3Round(Vector3 vector3)
        {
            return new Vector3(Mathf.Round(vector3.x), Mathf.Round(vector3.y), Mathf.Round(vector3.z));
        }
        
        public static Vector2 Vector2FloorToPixel(Vector2 vector)
        {
            return VectorHelper.Vector2Floor(vector * 16f) / 16f;
        }

        public static Vector2Int DirectionToVector(Direction direction)
        {
            return direction switch
            {
                Direction.Down => Vector2Int.down,
                Direction.Left => Vector2Int.left,
                Direction.Up  => Vector2Int.up,
                Direction.Right => Vector2Int.right,
                _ => Vector2Int.zero
            };	
        }

        public static Vector2Int Vector2FloorToInt(Vector2 vector2)
        {
            return new Vector2Int(Mathf.FloorToInt(vector2.x), Mathf.FloorToInt(vector2.y));
        }

        public static Vector2 Vector2Floor(Vector2 vector2)
        {
            return new Vector2(Mathf.Floor(vector2.x), Mathf.Floor(vector2.y));
        }

        public static Vector3Int Vector3FloorToInt(Vector3 vector3)
        {
            return new Vector3Int(Mathf.FloorToInt(vector3.x), Mathf.FloorToInt(vector3.y), Mathf.FloorToInt(vector3.z));
        }

        public static Vector3 Vector3Floor(Vector3 vector3)
        {
            return new Vector3(Mathf.Floor(vector3.x), Mathf.Floor(vector3.y), Mathf.Floor(vector3.z));
        }
    }
}