using System;
using UnityEngine;
using VEngine.Utilities.TileSystem;

namespace VEngine.Collision
{
    [Serializable]
    public class CollisionInfo
    {
        public bool collided;
        public Vector2 point = Vector2.zero;
        public float angle;
        public bool flagged;
        public int height;
        public int distance = 32;
        public VTile vtile;
        public Vector3 tileRotation = Vector3.zero;
    }
}