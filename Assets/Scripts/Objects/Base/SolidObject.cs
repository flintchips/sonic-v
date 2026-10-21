using Objects.Stage;
using UnityEngine;
using VEngine.Entity;
using VEngine.Entity.Constants;
using VEngine.Utilities.Helpers;
using System.Collections.Generic;

namespace Objects.Base
{
	public class SolidObject : StageObject
	{
		public bool solid = true;
		public bool jumpThrough;
		public CollisionLayer layerGroup = CollisionLayer.Ground;
		public Vector2 hitboxOffset;

		[HideInInspector] public List<Entity> beforeCollisionEntities;
		[HideInInspector] public List<Direction> beforeCollisionDirections;

		[HideInInspector] public List<Entity> onCollisionEntities;
		[HideInInspector] public List<Direction> onCollisionDirections;

		[HideInInspector] public List<Entity> duringCollisionEntities;
		[HideInInspector] public List<Direction> duringCollisionDirections;

		[HideInInspector] public List<Entity> horizontalCollisionEntities;
		[HideInInspector] public List<float> horizontalCollisionMovement;
		[HideInInspector] public List<float> horizontalCollisionVelocity;

		protected override void ObjectFixedUpdate()
		{
			position += velocity / 16f;
			if (solid)
				HandleObjectCollision();
			transform.position = VectorHelper.Vector2Floor((position) * 16f) / 16f;
		}

		protected override void LateObjectFixedUpdate()
		{
			if (solid)
			{
				for (int i = 0; i < beforeCollisionEntities.Count; i++)
				{
					BeforeCollision(beforeCollisionEntities[i], beforeCollisionDirections[i]);
				}
				for (int i = 0; i < onCollisionEntities.Count; i++)
				{
					OnCollision(onCollisionEntities[i], onCollisionDirections[i]);
				}
				for (int i = 0; i < duringCollisionEntities.Count; i++)
				{
					DuringCollision(duringCollisionEntities[i], duringCollisionDirections[i]);
				}
			}
		}

		private void BeforeCollisionAdd(Entity entity, Direction direction)
		{
			beforeCollisionEntities.Add(entity);
			beforeCollisionDirections.Add(direction);
		}

		private void OnCollisionAdd(Entity entity, Direction direction)
		{
			onCollisionEntities.Add(entity);
			onCollisionDirections.Add(direction);
		}

		private void DuringCollisionAdd(Entity entity, Direction direction)
		{
			duringCollisionEntities.Add(entity);
			duringCollisionDirections.Add(direction);
		}

		protected void HandleObjectCollision(bool recursed = false)
		{
			var oldPos = position;
			position += hitboxOffset;
			beforeCollisionEntities.Clear();
			beforeCollisionDirections.Clear();
			onCollisionDirections.Clear();
			onCollisionEntities.Clear();
			onCollisionDirections.Clear();
			duringCollisionEntities.Clear();
			duringCollisionDirections.Clear();
			foreach (var entity in Utilities.Stage.Instance.loadedEntities)
			{
				// if collision is disabled
				if (entity.collisionMode == CollisionState.None) continue;
				if (jumpThrough && entity.velocity.y > 0) continue;

				var combinedXRadius = outerbox.WidthRadius + entity.GroundModeWidthRadius + 1;
				var combinedYRadius = outerbox.HeightRadius + entity.GroundModeHeightRadius;
				bool standingOn = false;
				if (entity.onObject)
				{
					// stick and move the player with the object
					if ((entity.attachedObject != null) && (entity.attachedObject == this.gameObject))
					{
						// check for falling off object
						var xLeftDistance =
							(Mathf.FloorToInt(entity.position.x * 16) - Mathf.FloorToInt(position.x * 16)) +
							combinedXRadius;

						if ((xLeftDistance <= 0) || (xLeftDistance >= (combinedXRadius * 2)))
						{
							entity.onObject = false;
							entity.attachedObject = null;
							entity.solidObject = null;
						}
						else
						{
							// move the player if they're still attached
							entity.onGround = true;
							entity.position.y = PixelUnitHelper.P2U(combinedYRadius) + position.y;
							entity.position.x += velocity.x / 16f;
							entity.transform.position = VectorHelper.Vector2Floor((entity.position) * 16f) / 16f;

							OnCollisionAdd(entity, Direction.Down);
							DuringCollisionAdd(entity, Direction.Up);

							//OnCollision(entity, Direction.Down);
							//DuringCollision(entity, Direction.Up);
							standingOn = true;
						}

					}
				}

				// only collide if the entities collision layer matches the objects layer
				if (!CheckLayerGrouping(entity)) continue;

				var leftDifference = (Mathf.FloorToInt(entity.position.x * 16) - Mathf.FloorToInt(position.x * 16)) +
									 combinedXRadius;
				var topDifference = (Mathf.FloorToInt(entity.position.y * 16) - Mathf.FloorToInt(position.y * 16)) - 4 +
									combinedYRadius;

				var xOverlap = CheckXOverlap(leftDifference, combinedXRadius);
				var yOverlap = CheckYOverlap(topDifference, combinedYRadius);

				if (!(xOverlap && yOverlap)) continue;

				int xDistance;
				int yDistance;

				// on the left
				if (entity.position.x <= position.x)
				{
					xDistance = leftDifference;
				}
				// on the right
				else
				{
					xDistance = leftDifference - (combinedXRadius * 2);
				}

				// on the bottom
				if (entity.position.y < position.y)
				{
					yDistance = -(topDifference + 4);
				}
				// on the top
				else
				{
					yDistance = (topDifference - (combinedYRadius * 2));
				}

				// vertical collision
				if (!standingOn && ((Mathf.Abs(xDistance) > Mathf.Abs(yDistance + 4) || (Mathf.Abs(yDistance) <= 4)) &&
					!(leftDifference <= 0) && !(leftDifference >= (combinedXRadius * 2))))
				{
					// pop the player out downwards
					if (entity.position.y < position.y)
					{
						if (jumpThrough) continue;

						switch (entity.velocity.y)
						{
							case 0 when entity.onGround:
								Debug.Log("Die From Crushing");
								continue;
							case < 0:
								continue;
						}

						if (yDistance >= 0) continue;

						if (!recursed)
						{
							position = oldPos;
							BeforeCollisionAdd(entity, Direction.Up);

							//BeforeCollision(entity, Direction.Up);
							HandleObjectCollision(true);
							return;
						}
						entity.position.y -= PixelUnitHelper.P2U(yDistance);
						entity.velocity.y = 0;

						OnCollisionAdd(entity, Direction.Down);
						DuringCollisionAdd(entity, Direction.Down);

						//OnCollision(entity, Direction.Down);
						//DuringCollision(entity, Direction.Down);
					}
					// pop the player out upwards
					else
					{
						switch (yDistance)
						{
							case >= 16:
							case < -12 when jumpThrough:
								continue;
						}

						yDistance += 4;

						// if the Player is too far to the right
						// !!! This is always False now !!!
						if (leftDifference < 0) continue;

						// if the Player is too far to the left
						if (leftDifference > (combinedXRadius * 2)) continue;

						// dont land if moving up
						if (entity.velocity.y > 0) continue;

						// dont land on it if already standing on ground of the same surface value
						bool higherThanSurface = false;
						for (int i = 0; i < 3; i++)
						{
							if (entity.sensors[i].info.distance <= yDistance) higherThanSurface = true;
						}
						if (higherThanSurface)
						{
							continue;
						}

						if (!recursed)
						{
							position = oldPos;
							BeforeCollisionAdd(entity, Direction.Up);

							//BeforeCollision(entity, Direction.Up);
							HandleObjectCollision(true);
							return;
						}

						// land on object
						entity.position.y -= PixelUnitHelper.P2U(yDistance);
						entity.onGround = true;
						entity.velocity.y = 0;
						entity.angle = 0;
						entity.groundVel = entity.velocity.x;

						entity.attachedObject = this.gameObject;
						entity.solidObject = this;
						entity.onObject = true;

						OnCollisionAdd(entity, Direction.Up);
						DuringCollisionAdd(entity, Direction.Up);

						//OnCollision(entity, Direction.Up);
						//DuringCollision(entity, Direction.Up);
					}
				}
				// horizontal collision
				else if (Mathf.Abs(yDistance) > 4 && !jumpThrough)
				{
					if (!recursed)
					{
						position = oldPos;
						BeforeCollisionAdd(entity, Direction.Up);

						//BeforeCollision(entity, Direction.Up);
						HandleObjectCollision(true);
						return;
					}

					if ((entity.position.x <= position.x && entity.velocity.x > 0) ||
						(entity.position.x > position.x && entity.velocity.x < 0))
					{
						var vel = entity.velocity;
						entity.velocity.x = 0;
						entity.groundVel = 0;

						switch (vel.x)
						{
							case < 0:
								entity.position.x = (Mathf.Floor(entity.position.x * 16)) / 16f;

								OnCollisionAdd(entity, Direction.Right);
								DuringCollisionAdd(entity, Direction.Right);

								//OnCollision(entity, Direction.Right);
								//DuringCollision(entity, Direction.Right);
								break;
							case > 0:
								entity.position.x = (Mathf.Floor(entity.position.x * 16)) / 16f;
								OnCollisionAdd(entity, Direction.Left);
								DuringCollisionAdd(entity, Direction.Left);

								//OnCollision(entity, Direction.Left);
								//DuringCollision(entity, Direction.Left);
								break;
						}
					}

					// push the player out
					entity.position.x -= PixelUnitHelper.P2U(xDistance);
				}
			}

			position = oldPos;
		}

		private static bool CheckXOverlap(int leftDifference, int combinedXRadius)
		{
			// the Player is too far to the left to be touching?
			if (leftDifference < 0) return false;

			// the Player is too far to the right to be touching?
			return leftDifference <= (combinedXRadius * 2);
		}

		private static bool CheckYOverlap(int topDifference, int combinedYRadius)
		{
			// the Player is too far above to be touching
			if (topDifference < -4) return false;

			// the Player is too far down to be touching
			return topDifference <= (combinedYRadius * 2);
		}

		private bool CheckLayerGrouping(Entity entity)
		{
			switch (layerGroup)
			{
				case CollisionLayer.Ground:
					return entity.collisionLayer is CollisionLayer.Ground or CollisionLayer.GroundA
						or CollisionLayer.GroundB;

				case CollisionLayer.GroundA:
				case CollisionLayer.A:
					return entity.collisionLayer is CollisionLayer.GroundA or CollisionLayer.A;

				case CollisionLayer.GroundB:
				case CollisionLayer.B:
					return entity.collisionLayer is CollisionLayer.GroundB or CollisionLayer.B;

				case CollisionLayer.None:
				default: return false;
			}
		}

		protected virtual void BeforeCollision(Entity entity, Direction direction)
		{
		}

		protected virtual void OnCollision(Entity entity, Direction direction)
		{
		}

		protected virtual void DuringCollision(Entity entity, Direction direction)
		{
		}
	}
}