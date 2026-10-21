using System;
using UnityEngine;
using VEngine.Entity.Constants;

namespace VEngine.Collision
{
	[Serializable]
	public class Sensor
	{
		public bool enabled = true;
		public Entity.Entity entity;
		public Vector2 position = Vector2.zero;
		public Direction direction = Direction.Down;
		public CollisionLayer collisionLayer = CollisionLayer.Ground;
		public CollisionInfo info = new();
		public bool Collided => enabled && info.collided;
		public int Distance => enabled ? info.distance : 32;
		public bool Flagged => info.flagged;
		public float Angle => info.angle;
		public bool layerOverlap;

		public Sensor() { }
		public Sensor(Sensor sensor)
		{
			this.enabled = sensor.enabled;
			this.entity = sensor.entity;
			this.position = sensor.position;
			this.direction = sensor.direction;
			this.collisionLayer = sensor.collisionLayer;
			this.info = sensor.info;
			this.layerOverlap = sensor.layerOverlap;
		}
	}
}