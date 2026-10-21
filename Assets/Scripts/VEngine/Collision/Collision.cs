using System;
using System.Collections.Generic;
using System.Linq;
using Objects.Player;
using Objects.Player.Constants;
using UnityEngine;
using UnityEngine.Tilemaps;
using VEngine.Entity.Constants;
using VEngine.Math;
using VEngine.Utilities.Helpers;
using VEngine.Utilities.TileSystem;
using VEngine.Utilities.TriggerSystem;

namespace VEngine.Collision
{
	[Serializable]
	public static class Collision
	{
		public static LayerMask NullMask => LayerMask.GetMask("Null");
		public static LayerMask GroundMask => LayerMask.GetMask("Ground");
		public static LayerMask AMask => LayerMask.GetMask("A");
		public static LayerMask BMask => LayerMask.GetMask("B");
		public static Tilemap GroundMap;
		public static Tilemap AMap;
		public static Tilemap BMap;

		private static Collider2D[] _colliderArray = new Collider2D[2];
		private static Collider2D[] _colliderArrayNew = new Collider2D[2];
		private static Collider2D[] _triggerColliderArray = new Collider2D[2];

		public static CollisionInfo GetTileCollision(Sensor sensor, bool recursed = false)
		{
			CollisionInfo recursedInfo = null;
			if (!recursed && (sensor.collisionLayer == CollisionLayer.GroundA || sensor.collisionLayer == CollisionLayer.GroundB))
			{
				Sensor recursionSensor = new Sensor(sensor);
				recursionSensor.collisionLayer = sensor.collisionLayer == CollisionLayer.GroundA ? CollisionLayer.A : CollisionLayer.B;
				recursedInfo = GetTileCollision(recursionSensor, true);
				sensor.collisionLayer = CollisionLayer.Ground;
			}

			GroundMap ??= GameObject.FindWithTag("Ground").GetComponent<Tilemap>();
			AMap ??= GameObject.FindWithTag("A").GetComponent<Tilemap>();
			BMap ??= GameObject.FindWithTag("B").GetComponent<Tilemap>();

			var layerMask = CollisionLayerToMask(sensor.collisionLayer);
			_colliderArray = new Collider2D[2];
			var size = Physics2D.OverlapPointNonAlloc((Vector2)VectorHelper.Vector2FloorToInt(sensor.position * 16f) / 16f + new Vector2(1 / 32f, 1 / 32f), _colliderArray, layerMask);
			var onTilemap = size > 0;

			var collisionInfo = new CollisionInfo();

			VTile vTile = null;
			var tileRotation = Vector3.zero;
			var tilePosition = (Vector3Int)new Vector2Int(Mathf.FloorToInt(sensor.position.x),
				Mathf.FloorToInt(sensor.position.y));
			var tilePositionStore = tilePosition; // position for second recursion for layer overlapping;
			Tilemap tilemap = null;

			collisionInfo.collided = false;
			collisionInfo.point = Vector2.zero;
			collisionInfo.angle = 0f;
			collisionInfo.flagged = false;
			collisionInfo.height = 0;
			collisionInfo.distance = 32;
			collisionInfo.vtile = null;
			collisionInfo.tileRotation = Vector3.zero;

			var extended = false;

			if (onTilemap)
			{
				tilemap = GetTilemap(_colliderArray, sensor.collisionLayer, sensor);
			}
			else
			{
				var oldTilePos = tilePosition;

				if (Extend(sensor.collisionLayer, sensor.direction, ref tilemap, ref vTile, ref tilePosition,
						ref _colliderArray, ref sensor))
				{
					onTilemap = _colliderArray is { Length: > 0 };
					extended = true;
				}
			}

			if (tilemap != null)
			{
				vTile = tilemap.GetTile(tilePosition) as VTile;
				if (vTile != null)
				{
					collisionInfo.flagged = vTile.flagged;
					collisionInfo.vtile = vTile;

					tileRotation = tilemap.GetTransformMatrix(tilePosition).rotation.eulerAngles;
					collisionInfo.tileRotation = tileRotation;

					var height = GetHeight(vTile, sensor.position, sensor.direction, tilePosition, tileRotation);
					var distance = GetDistance(sensor.position, sensor.direction, tilePosition, height);
					collisionInfo.collided = true;

					collisionInfo.height = height;
					collisionInfo.distance = distance;
					collisionInfo.angle = GetVTileAngle(vTile, tileRotation, sensor.entity, sensor.direction);

					var oldTilePos = tilePosition;
					var changed = false;

					switch (height)
					{
						case 16 when !extended:
							{
								if (Regress(sensor.collisionLayer, sensor.direction, ref tilemap, ref vTile,
										ref tilePosition,
										ref _colliderArray, ref sensor))
								{
									changed = true;
								}

								break;
							}
						case 0 when !extended:
							{
								if (Extend(sensor.collisionLayer, sensor.direction, ref tilemap, ref vTile,
										ref tilePosition,
										ref _colliderArray, ref sensor))
								{
									changed = true;
								}

								break;
							}
					}

					if (changed)
					{
						collisionInfo.flagged = vTile.flagged;
						collisionInfo.vtile = vTile;

						tileRotation = tilemap.GetTransformMatrix(tilePosition).rotation.eulerAngles;
						collisionInfo.tileRotation = tileRotation;

						height = GetHeight(vTile, sensor.position, sensor.direction, tilePosition, tileRotation);
						distance = GetDistance(sensor.position, sensor.direction, tilePosition, height);
						collisionInfo.collided = true;

						collisionInfo.height = height;
						collisionInfo.distance = distance;
						collisionInfo.angle = GetVTileAngle(vTile, tileRotation, sensor.entity, sensor.direction);
					}
				}
			}

			sensor.info = collisionInfo;
			if (recursedInfo != null && !recursed)
			{
				if (recursedInfo.distance < collisionInfo.distance)
				{
					sensor.info = recursedInfo;
				}
			}

			return sensor.info;
		}

		private static bool Extend(CollisionLayer collisionLayer, Direction direction, ref Tilemap tilemap,
			ref VTile vtile,
			ref Vector3Int tilePosition, ref Collider2D[] colliderArray, ref Sensor sensor)
		{
			var layerMask = CollisionLayerToMask(collisionLayer);
			var tilePositionNew = tilePosition;
			tilePositionNew += (Vector3Int)VectorHelper.DirectionToVector(direction);

			var tileCheckPosition = (Vector3)tilePositionNew;
			_colliderArrayNew = new Collider2D[2];
			var size = Physics2D.OverlapPointNonAlloc(tileCheckPosition, _colliderArrayNew, layerMask);
			var onTilemapNew = size > 0;

			if (!onTilemapNew) return false;
			var tilemapNew = GetTilemap(_colliderArrayNew, collisionLayer, sensor);

			if (tilemapNew == null) return false;
			var vTileNew = (VTile)tilemapNew.GetTile(tilePositionNew);
			if (vTileNew == null) return false;
			tilemap = tilemapNew;
			vtile = vTileNew;
			tilePosition = tilePositionNew;
			colliderArray = _colliderArrayNew;

			return true;
		}

		private static bool Regress(CollisionLayer collisionLayer, Direction direction, ref Tilemap tilemap,
			ref VTile vtile,
			ref Vector3Int tilePosition, ref Collider2D[] colliderArray, ref Sensor sensor)
		{
			var layerMask = CollisionLayerToMask(collisionLayer);
			var tilePositionNew = tilePosition;
			tilePositionNew -= (Vector3Int)VectorHelper.DirectionToVector(direction);

			var tileCheckPosition = (Vector3)tilePositionNew;
			_colliderArrayNew = new Collider2D[2];
			var size = Physics2D.OverlapPointNonAlloc(tileCheckPosition, _colliderArrayNew, layerMask);
			var onTilemapNew = size > 0;

			if (!onTilemapNew) return false;
			var tilemapNew = GetTilemap(_colliderArrayNew, collisionLayer, sensor);

			if (tilemapNew == null) return false;
			var vTileNew = (VTile)tilemapNew.GetTile(tilePositionNew);
			if (vTileNew == null) return false;
			tilemap = tilemapNew;
			vtile = vTileNew;
			tilePosition = tilePositionNew;
			colliderArray = _colliderArrayNew;

			return true;
		}

		public static int GetHeight(VTile vTile, Vector2 anchorPoint, Direction direction, Vector3Int tilePosition,
			Vector3 tileRotation)
		{
			var height = 0;
			var flipped = (tileRotation.y == 180);
			var tileRot = Mathf.Round(tileRotation.z);

			Vector3 localTilePosition = anchorPoint - (Vector2)(Vector3)tilePosition;

			switch (direction)
			{
				case Direction.Down:
				case Direction.Up:
					{
						switch (tileRot)
						{
							case 0 when !flipped:
								{
									height = direction == Direction.Up
										? vTile.ArrayB[Mathf.FloorToInt(localTilePosition.x * 16)]
										: vTile.ArrayT[Mathf.FloorToInt(localTilePosition.x * 16)];
									break;
								}
							case 0 when direction == Direction.Up:
								height = vTile.ArrayB[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 0:
								height = vTile.ArrayT[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 180 when !flipped:
								{
									height = direction == Direction.Up
										? vTile.ArrayT[15 - Mathf.FloorToInt(localTilePosition.x * 16)]
										: vTile.ArrayB[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
									break;
								}
							case 180 when direction == Direction.Up:
								height = vTile.ArrayT[Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 180:
								height = vTile.ArrayB[Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 90 when !flipped:
								{
									height = direction == Direction.Up
										? vTile.ArrayL[15 - Mathf.FloorToInt(localTilePosition.x * 16)]
										: vTile.ArrayR[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
									break;
								}
							case 90 when direction == Direction.Up:
								height = vTile.ArrayL[Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 90:
								height = vTile.ArrayR[Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 270 when !flipped:
								{
									height = direction == Direction.Up
										? vTile.ArrayR[Mathf.FloorToInt(localTilePosition.x * 16)]
										: vTile.ArrayL[Mathf.FloorToInt(localTilePosition.x * 16)];
									break;
								}
							case 270 when direction == Direction.Up:
								height = vTile.ArrayR[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
							case 270:
								height = vTile.ArrayL[15 - Mathf.FloorToInt(localTilePosition.x * 16)];
								break;
						}

						break;
					}
				case Direction.Right:
				case Direction.Left:
					{
						height = tileRot switch
						{
							0 when !flipped => direction == Direction.Left
								? vTile.ArrayR[Mathf.FloorToInt(localTilePosition.y * 16)]
								: vTile.ArrayL[Mathf.FloorToInt(localTilePosition.y * 16)],
							0 when direction == Direction.Left => vTile.ArrayL[Mathf.FloorToInt(localTilePosition.y * 16)],
							0 => vTile.ArrayR[Mathf.FloorToInt(localTilePosition.y * 16)],
							180 when !flipped => direction == Direction.Left
								? vTile.ArrayL[15 - Mathf.FloorToInt(localTilePosition.y * 16)]
								: vTile.ArrayR[15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							180 when direction == Direction.Left => vTile.ArrayR[
								15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							180 => vTile.ArrayL[15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							90 when !flipped => direction == Direction.Left
								? vTile.ArrayB[Mathf.FloorToInt(localTilePosition.y * 16)]
								: vTile.ArrayT[Mathf.FloorToInt(localTilePosition.y * 16)],
							90 when direction == Direction.Left => vTile.ArrayT[Mathf.FloorToInt(localTilePosition.y * 16)],
							90 => vTile.ArrayB[Mathf.FloorToInt(localTilePosition.y * 16)],
							270 when !flipped => direction == Direction.Left
								? vTile.ArrayT[15 - Mathf.FloorToInt(localTilePosition.y * 16)]
								: vTile.ArrayB[15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							270 when direction == Direction.Left => vTile.ArrayB[
								15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							270 => vTile.ArrayT[15 - Mathf.FloorToInt(localTilePosition.y * 16)],
							_ => height
						};

						break;
					}
			}

			return height;
		}

		public static int GetDistance(Vector2 anchorPoint, Direction direction, Vector3Int tilePosition, int height)
		{
			var tileRelativePos = (Vector3)anchorPoint - tilePosition;

			var distance = direction switch
			{
				Direction.Down => (int)(tileRelativePos.y * 16f) - height,
				Direction.Up => -((int)(tileRelativePos.y * 16f) - 15 + height),
				Direction.Left => (int)(tileRelativePos.x * 16f) - height,
				Direction.Right => -((int)(tileRelativePos.x * 16f) - 15 + height),
				_ => 32
			};

			return distance;
		}

		public static Tilemap GetTilemap(Collider2D[] colliderArray, CollisionLayer collisionLayer,
			Sensor sensor = null)
		{
			return collisionLayer switch
			{
				CollisionLayer.None => null,
				CollisionLayer.Ground => GroundMap,
				CollisionLayer.A => AMap,
				CollisionLayer.B => BMap,
				CollisionLayer.GroundA => GroundMap,
				CollisionLayer.GroundB => GroundMap,
				_ => null
			};
		}

		public static Hitbox CollisionOuter = new();
		public static Hitbox CollisionInner = new();

		public static Entity.Entity CollisionEntity;
		private static Sensor[] _sensors = new Sensor[10];

		public static void ProcessObjectMovement(Entity.Entity entity, Hitbox outerBox, Hitbox innerBox)
		{
			if (entity == null || outerBox == null || innerBox == null) return;
			CollisionOuter = outerBox;
			CollisionInner = innerBox;

			if (entity.tileCollisionsType == TileCollisionType.None) return;
			CollisionEntity = entity;

			if (CollisionEntity.onGround)
			{
				CollisionEntity.velocity.x =
					CollisionEntity.groundVel * Mathf.Cos(CollisionEntity.angle * Mathf.Deg2Rad);
				CollisionEntity.velocity.y =
					CollisionEntity.groundVel * Mathf.Sin(CollisionEntity.angle * Mathf.Deg2Rad);
			}
			else
			{
				CollisionEntity.groundVel = CollisionEntity.velocity.x;
			}

			CollisionEntity.position += CollisionEntity.velocity / 16f * CollisionEntity.stepDelta;
		}

		public static void ProcessCollisionHorizontal(Entity.Entity entity, Hitbox outerBox, Hitbox innerBox)
		{
			if (entity == null || outerBox == null || innerBox == null) return;
			CollisionOuter = outerBox;
			CollisionInner = innerBox;

			if (entity.tileCollisionsType == TileCollisionType.None) return;
			CollisionEntity = entity;

			ProcessCollision(true);
		}

		public static void ProcessCollisionVertical(Entity.Entity entity, Hitbox outerBox, Hitbox innerBox)
		{
			if (entity == null || outerBox == null || innerBox == null) return;
			CollisionOuter = outerBox;
			CollisionInner = innerBox;

			if (entity.tileCollisionsType == TileCollisionType.None) return;
			CollisionEntity = entity;

			ProcessCollision();
			ProcessTriggers(_sensors[4]);
		}

		private static void ProcessTriggers(Sensor centerSensor)
		{
			_triggerColliderArray = new Collider2D[2];
			var size = Physics2D.OverlapPointNonAlloc(centerSensor.position, _triggerColliderArray, TriggerBase.TriggerMask);
			var onTilemap = size > 0;

			if (!onTilemap) return;
			var gridPos = VectorHelper.Vector3FloorToInt(centerSensor.position);
			var tile = TriggerBase.GetTriggerInfo(gridPos).Tile as TriggerBase;

			if (tile != null)
				tile.Trigger(gridPos, centerSensor.entity);
		}

		private static void ProcessCollision(bool horizontal = false)
		{
			if (_sensors.Length != 10) _sensors = new Sensor[10];

			if (!horizontal)
			{
				if (_sensors[7] != null && _sensors[8] != null && CollisionEntity.onGround)
					CollisionEntity.collisionMode = UpdateCollisionMode(CollisionEntity.angle, CollisionEntity.sensors,
						CollisionEntity);
			}

			var playerObject = CollisionEntity as PlayerObject;
			if (playerObject != null)
			{
				if (playerObject.onGround && !playerObject.groundedStoreOld && playerObject.controlLock > 0 && playerObject.velocity.y < 0)
				{
					if (CollisionEntity.collisionMode == CollisionState.LWall ||
					CollisionEntity.collisionMode == CollisionState.RWall)
					{
						playerObject.ctrlLockSlide = true;
					}
				}

				if (playerObject.ctrlLockSlide) CollisionEntity.collisionMode = CollisionState.Floor;

				if (playerObject.controlLock == 0 || !playerObject.onGround || playerObject.velocity.y >= 0 || Mathf.Abs(playerObject.angle) < 36)
					playerObject.ctrlLockSlide = false;
			}

			if (CollisionEntity.sensors.Length == 0) CollisionEntity.sensors = _sensors;

			for (var i = 0; i < 10; ++i)
			{
				_sensors[i] = new Sensor
				{
					position = Vector2.zero,
					entity = CollisionEntity,
					direction = CollisionHelper.CollisionModeToDirection(CollisionEntity.collisionMode),
					collisionLayer = CollisionEntity.collisionLayer,
					info =
					{
						angle = CollisionEntity.angle,
						collided = false
					}
				};
			}

			_sensors[4].position = CollisionEntity.position;
			_sensors[9].position = CollisionEntity.position;

			_sensors[9].direction =
				CollisionHelper.CollisionModeToDirection(
					CollisionHelper.CollisionModeReversed(CollisionEntity.collisionMode));

			_sensors = SetActiveSensors(_sensors);
			_sensors = SetPathGripSensors(_sensors);

			// whem mode is either floor or roof
			var verticalMode = CollisionEntity.ModeVector.x == 0;

			// account for delay with collision mode switch
			if (verticalMode)
			{
				_sensors[7].position.x += CollisionEntity.velocity.x / 16f * CollisionEntity.stepDelta;
				_sensors[8].position.x += CollisionEntity.velocity.x / 16f * CollisionEntity.stepDelta;
			}
			else
			{
				_sensors[7].position.y += CollisionEntity.velocity.y / 16f * CollisionEntity.stepDelta;
				_sensors[8].position.y += CollisionEntity.velocity.y / 16f * CollisionEntity.stepDelta;
			}


			foreach (var sensor in _sensors)
			{
				// update sensors
				if (sensor.enabled)
				{
					if (horizontal && (sensor == _sensors[3]))
						GetTileCollision(sensor);
					else if (sensor != _sensors[3])
					{
						GetTileCollision(sensor);
					}
				}
			}
			CollisionEntity.sensors = _sensors;

			var absHorizontalSpeed =
				Mathf.Abs(verticalMode ? CollisionEntity.velocity.x : CollisionEntity.velocity.y);

			var winningSensor = GetWinningSensorDown(CollisionEntity.angle);

			var winningSensorUp = GetWinningSensorUp(CollisionEntity.angle);

			var stickRadius = CollisionEntity.onGround ? Mathf.Min(Mathf.Abs(absHorizontalSpeed) + 4, 14) : 0;

			if (playerObject != null)
			{
				if (playerObject.jumpedThisFrame) stickRadius = 0;
			}

			var angleDistanceThreshold =
			(Mathf.Abs(Rotation.AngleDifference(CollisionEntity.angle, winningSensor.Angle)) >= 45 &&
			 CollisionEntity.onGround);

			// center of the player is directly inside something
			var clippingInTile = (_sensors[4].Distance <= -1 && _sensors[9].Distance <= -1);

			if (horizontal && _sensors[3].Distance < 0 && !clippingInTile)
			{
				var pushOffset = Vector2.zero;
				if (CollisionEntity.ModeVector.x == 0)
					pushOffset.x =
						PixelUnitHelper.P2U(
							CollisionEntity.groundVel * Mathf.Cos(CollisionEntity.angle * Mathf.Deg2Rad)) *
						CollisionEntity.stepDelta;
				else
					pushOffset.y =
						PixelUnitHelper.P2U(
							CollisionEntity.groundVel * Mathf.Sin(CollisionEntity.angle * Mathf.Deg2Rad)) *
						CollisionEntity.stepDelta;

				if (CollisionEntity.onGround)
				{
					bool fall = false;
					// fall if running into a wall while not in floor mode
					switch (CollisionEntity.collisionMode)
					{
						case CollisionState.Roof:
							fall = true;
							CollisionEntity.velocity.x = 0;
							break;
						case CollisionState.LWall:
							pushOffset = Vector2.zero;
							fall = true;
							CollisionEntity.velocity.y = 0;
							break;
						case CollisionState.RWall:
							fall = true;
							CollisionEntity.velocity.y = 0;
							_sensors[3].info.distance += CollisionEntity.outerbox.HeightRadius;
							break;
						default: break;
					}

					if (fall)
					{
						CollisionEntity.onGround = false;
						CollisionEntity.angle = 0;
					}

					if (CollisionEntity.groundVel > 0)
					{
						CollisionEntity.position +=
							PixelUnitHelper.P2U(_sensors[3].Distance + 1) * CollisionEntity.ModeVectorRight +
							pushOffset;
						CollisionEntity.position.x = Mathf.Floor(CollisionEntity.position.x * 16) / 16f;
						CollisionEntity.groundVel = 0;
					}

					if (CollisionEntity.groundVel < 0)
					{
						CollisionEntity.position -=
							PixelUnitHelper.P2U(_sensors[3].Distance) * CollisionEntity.ModeVectorRight - pushOffset;
						CollisionEntity.position.x = Mathf.Floor(CollisionEntity.position.x * 16) / 16f;
						CollisionEntity.groundVel = 0;
					}
				}
				else
				{
					if (CollisionEntity.velocity.x > 0)
					{
						CollisionEntity.position.x += PixelUnitHelper.P2U(_sensors[3].Distance + 1);
						CollisionEntity.position.x = Mathf.Floor(CollisionEntity.position.x * 16) / 16f;
						CollisionEntity.velocity.x = 0;
					}

					if (CollisionEntity.velocity.x < 0)
					{
						CollisionEntity.position.x -= PixelUnitHelper.P2U(_sensors[3].Distance);
						CollisionEntity.position.x = Mathf.Floor(CollisionEntity.position.x * 16) / 16f;
						CollisionEntity.velocity.x = 0;
					}
				}
			}

			if (horizontal) return;

			// Colliding Down
			if (winningSensor.Distance <= stickRadius && !clippingInTile && !angleDistanceThreshold)
			{
				CollisionEntity.angle = winningSensor.Angle;
				CollisionEntity.onGround = true;

				Vector2 flooredSensorPosition;
				if (verticalMode)
					flooredSensorPosition = new Vector2(winningSensor.position.x,
						Mathf.Floor(winningSensor.position.y * 16f) / 16f);
				else
					flooredSensorPosition = new Vector2(Mathf.Floor(winningSensor.position.x * 16f) / 16f,
						winningSensor.position.y);

				const float verticalOffset = 0;

				// reposition vertically
				CollisionEntity.position = flooredSensorPosition +
										(-PixelUnitHelper.P2U(-CollisionEntity.outerbox.bottom -
																winningSensor.Distance) + verticalOffset) *
										CollisionEntity.ModeVector;

				if (winningSensor == _sensors[0])
					CollisionEntity.position += PixelUnitHelper.P2U(-CollisionEntity.innerbox.left + 1) *
												CollisionEntity.ModeVectorRight;

				// sensors[1] is in the middle and doesnt need horizontal repositioning

				if (winningSensor == _sensors[2])
					CollisionEntity.position += PixelUnitHelper.P2U(-CollisionEntity.innerbox.right) *
												CollisionEntity.ModeVectorRight;

				// Get groundspeed when landing
				if (!CollisionEntity.groundedStore)
				{
					var speed = CollisionEntity.angle switch
					{
						< 180 and < 22.5f => CollisionEntity.velocity.x,
						< 180 and >= 45 => (Mathf.Abs(CollisionEntity.velocity.x) <=
											Mathf.Abs(CollisionEntity.velocity.y)
							? CollisionEntity.velocity.y
							: CollisionEntity.velocity.x),
						< 180 => (Mathf.Abs(CollisionEntity.velocity.x) <=
								Mathf.Abs(CollisionEntity.velocity.y * Math.Math.BitShift(1))
							? (CollisionEntity.velocity.y * Math.Math.BitShift(1))
							: CollisionEntity.velocity.x),
						> 337.5f => CollisionEntity.velocity.x,
						<= 315f => (Mathf.Abs(CollisionEntity.velocity.x) <= Mathf.Abs(CollisionEntity.velocity.y)
							? -CollisionEntity.velocity.y
							: CollisionEntity.velocity.x),
						_ => (Mathf.Abs(CollisionEntity.velocity.x) <=
							Mathf.Abs(CollisionEntity.velocity.y * Math.Math.BitShift(1))
							? -(CollisionEntity.velocity.y * Math.Math.BitShift(1))
							: CollisionEntity.velocity.x)
					};

					if (speed < -24)
						speed = -24;

					if (speed > 24)
						speed = 24;

					CollisionEntity.groundVel = speed;
					CollisionEntity.velocity.x = speed;
					CollisionEntity.velocity.y = 0;
				}
			}

			// Colliding Up
			else if (winningSensorUp.Distance <= 0 && !clippingInTile && !CollisionEntity.groundedStore)
			{
				// reposition vertically
				CollisionEntity.position.y += PixelUnitHelper.P2U(winningSensorUp.Distance);

				CollisionEntity.angle = winningSensorUp.Angle;

				// on hitting head on ceiling
				var speed = 0f;
				var reattach = false;

				if (CollisionEntity.angle is < 135f or > 225f)
				{
					if (Mathf.Abs(CollisionEntity.velocity.y) > Mathf.Abs(CollisionEntity.velocity.x))
					{
						// mostly up
						if (CollisionEntity.velocity.y > 0)
						{
							speed = CollisionEntity.velocity.y *
									Mathf.Sign(Mathf.Sin(CollisionEntity.angle * Mathf.Deg2Rad));
							if (Mathf.Abs(speed) >= 3)
								reattach = true;
						}
					}
				}

				if (speed < -24)
					speed = -24;

				if (speed > 24)
					speed = 24;

				if (reattach)
				{
					CollisionEntity.groundVel = speed;
					CollisionEntity.onGround = true;
				}
				else
				{
					CollisionEntity.onGround = false;
					if (CollisionEntity.velocity.y > 0) CollisionEntity.velocity.y = 0;

					if (Mathf.Abs(CollisionEntity.velocity.x) > Mathf.Abs(CollisionEntity.velocity.y))
					{
						if (CollisionEntity.velocity.x != 0 && (CollisionEntity.angle is >= 225 or <= 135))
							CollisionEntity.velocity.x -= CollisionEntity.velocity.x / 16f;
					}

					CollisionEntity.angle = 0;
					CollisionEntity.collisionMode = CollisionState.Floor;
				}
			}
			else
			{
				if (!CollisionEntity.onObject)
					CollisionEntity.onGround = false;
				CollisionEntity.collisionMode = CollisionState.Floor;
				CollisionEntity.groundVel = CollisionEntity.velocity.x;
				CollisionEntity.angle = 0;
			}

			if (CollisionEntity.onGround)
				CollisionEntity.collisionMode =
					UpdateCollisionMode(CollisionEntity.angle, CollisionEntity.sensors, CollisionEntity);
		}

		public static Sensor[] SetPathGripSensors(Sensor[] sensors)
		{
			float offset = 0;
			var pushOffset = Vector2.zero;
			bool left;

			var playerObject = CollisionEntity as PlayerObject;

			if (CollisionEntity.onGround)
			{
				if (CollisionEntity.angle == 0 && playerObject == null)
				{
					offset = PixelUnitHelper.P2U(-8);
				}
				else if (playerObject != null)
				{
					if (CollisionEntity.angle == 0)
					{
						offset = PixelUnitHelper.P2U(-8);
					}
					else
					{
						offset = 0;
					}

					if (playerObject.animator.animStore.animationID == AnimationType.Jump)
					{
						if (CollisionEntity.angle == 0)
						{
							offset = 0;
						}
						else
						{
							offset = PixelUnitHelper.P2U(4);
						}
					}
				}

				if (CollisionEntity.ModeVector.x == 0)
					pushOffset.x =
						PixelUnitHelper.P2U(
							CollisionEntity.groundVel * Mathf.Cos(CollisionEntity.angle * Mathf.Deg2Rad)) *
						CollisionEntity.stepDelta;
				else
					pushOffset.y =
						PixelUnitHelper.P2U(
							CollisionEntity.groundVel * Mathf.Sin(CollisionEntity.angle * Mathf.Deg2Rad)) *
						CollisionEntity.stepDelta;

				left = CollisionEntity.groundVel < 0;

				if (CollisionEntity.groundVel == 0)
				{
					if (playerObject != null)
					{
						left = (playerObject.direction == Flip.X);
					}
				}
			}
			else
			{
				left = CollisionEntity.velocity.x < 0;

				if (CollisionEntity.velocity.x == 0)
				{
					if (playerObject != null)
					{
						left = (playerObject.direction == Flip.X);
					}
				}
			}

			switch (CollisionEntity.collisionMode)
			{
				case CollisionState.Floor:
					sensors[0].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[1].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[2].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[3].position.y = sensors[4].position.y + offset;

					sensors[0].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionInner.left) -
											PixelUnitHelper.P2U(1);
					sensors[1].position.x = sensors[4].position.x;
					sensors[2].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionInner.right);
					if (left)
						sensors[3].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.left) -
												PixelUnitHelper.P2U(1);
					else
						sensors[3].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.right);

					sensors[3].direction = (left) ? Direction.Left : Direction.Right;

					sensors[5].direction = Direction.Up;
					sensors[6].direction = Direction.Up;

					sensors[5].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[6].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);

					sensors[5].position.x = sensors[0].position.x;
					sensors[6].position.x = sensors[2].position.x;

					sensors[7].direction = Direction.Left;
					sensors[7].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[7].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionInner.right);

					sensors[8].direction = Direction.Right;
					sensors[8].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[8].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionInner.left) -
											PixelUnitHelper.P2U(1);
					break;

				case CollisionState.LWall:
					sensors[0].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[1].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[2].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[3].position.x = sensors[4].position.x;

					sensors[0].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionInner.left) +
											PixelUnitHelper.P2U(1);
					sensors[1].position.y = sensors[4].position.y;
					sensors[2].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionInner.right);
					if (left)
						sensors[3].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.left);
					else
						sensors[3].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.right) -
												PixelUnitHelper.P2U(1);

					sensors[3].direction = (left) ? Direction.Up : Direction.Down;

					sensors[5].direction = Direction.Right;
					sensors[6].direction = Direction.Right;

					sensors[5].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[6].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);

					sensors[5].position.y = sensors[0].position.y;
					sensors[6].position.y = sensors[2].position.y;

					sensors[7].direction = Direction.Up;
					sensors[7].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionInner.right);
					sensors[7].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);

					sensors[8].direction = Direction.Down;
					sensors[8].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionInner.left) -
											PixelUnitHelper.P2U(1);
					sensors[8].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);
					break;

				case CollisionState.Roof:
					sensors[0].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[1].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[2].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[3].position.y = sensors[4].position.y - offset;

					sensors[0].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionInner.left) +
											PixelUnitHelper.P2U(1);
					sensors[1].position.x = sensors[4].position.x;
					sensors[2].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionInner.right);
					if (left)
						sensors[3].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.left);
					else
						sensors[3].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.right) -
												PixelUnitHelper.P2U(1);

					sensors[3].direction = (left) ? Direction.Right : Direction.Left;

					sensors[5].direction = Direction.Down;
					sensors[6].direction = Direction.Down;

					sensors[5].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[6].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);

					sensors[5].position.x = sensors[0].position.x;
					sensors[6].position.x = sensors[2].position.x;

					sensors[7].direction = Direction.Right;
					sensors[7].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[7].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionInner.right);

					sensors[8].direction = Direction.Left;
					sensors[8].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[8].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionInner.left) +
											PixelUnitHelper.P2U(1);
					break;

				case CollisionState.RWall:
					sensors[0].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[1].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[2].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					sensors[3].position.x = sensors[4].position.x;

					sensors[0].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionInner.left) -
											PixelUnitHelper.P2U(1);
					sensors[1].position.y = sensors[4].position.y;
					sensors[2].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionInner.right);
					if (CollisionEntity.groundVel <= 0)
						sensors[3].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.left) -
												PixelUnitHelper.P2U(1);
					else
						sensors[3].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.right);

					sensors[3].direction = (left) ? Direction.Down : Direction.Right;

					sensors[5].direction = Direction.Left;
					sensors[6].direction = Direction.Left;

					sensors[5].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);
					sensors[6].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionOuter.bottom);

					sensors[5].position.y = sensors[0].position.y;
					sensors[6].position.y = sensors[2].position.y;

					sensors[7].direction = Direction.Down;
					sensors[7].position.x = sensors[4].position.x + PixelUnitHelper.P2U(CollisionInner.right);
					sensors[7].position.y = sensors[4].position.y + PixelUnitHelper.P2U(CollisionOuter.bottom);

					sensors[8].direction = Direction.Up;
					sensors[8].position.x = sensors[4].position.x - PixelUnitHelper.P2U(CollisionInner.left) +
											PixelUnitHelper.P2U(1);
					sensors[8].position.y = sensors[4].position.y - PixelUnitHelper.P2U(CollisionOuter.bottom) -
											PixelUnitHelper.P2U(1);
					break;

				case CollisionState.None:
				default: break;
			}

			sensors[3].position += pushOffset;

			return sensors;
		}

		private static Sensor[] SetActiveSensors(Sensor[] sensors)
		{
			if (CollisionEntity.onGround)
			{
				sensors[5].enabled = false;
				sensors[6].enabled = false;
			}
			else
			{
				if (!(Mathf.Abs(CollisionEntity.velocity.y) > Mathf.Abs(CollisionEntity.velocity.x))) return sensors;
				switch (CollisionEntity.velocity.y)
				{
					// mostly up
					case > 0:
						sensors[0].enabled = false;
						sensors[1].enabled = false;
						sensors[2].enabled = false;
						break;
					// mostly down
					case < 0:
						sensors[5].enabled = false;
						sensors[6].enabled = false;
						break;
				}
			}

			return sensors;
		}

		private static void DrawSensorHelpers()
		{
		}

		private static void ProcessAirCollision_Up()
		{
		}

		private static void ProcessAirCollision_Down()
		{
		}

		private static CollisionState UpdateCollisionMode(float angle, IReadOnlyList<Sensor> entitySensors,
			Entity.Entity entity)
		{
			var mode = entity.collisionMode;
			var leftGroundAngle = 0f;
			var rightGroundAngle = 0f;

			const float maxChangeAngle = 45f;

			var leftDeltaAngle = Mathf.Abs(Rotation.AngleDifference(entity.angle, entitySensors[7].info.angle));
			var rightDeltaAngle = Mathf.Abs(Rotation.AngleDifference(entity.angle, entitySensors[8].info.angle));

			if (entitySensors[7].Distance <= 0)
			{
				if (leftDeltaAngle <= maxChangeAngle)
				{
					leftGroundAngle = entitySensors[7].Angle;
				}
			}

			if (entitySensors[8].Distance <= 0)
			{
				if (rightDeltaAngle <= maxChangeAngle)
				{
					rightGroundAngle = entitySensors[8].Angle;
				}
			}

			if ((entitySensors[7].Distance <= 0 || entitySensors[8].Distance <= 0) && (entity.groundVel != 0))
			{
				if (entity.groundVel < 0)
				{
					if (entitySensors[7].info.collided)
					{
						if (leftDeltaAngle <= maxChangeAngle)
						{
							mode = GetGroundModeFromAngle(mode, leftGroundAngle);
						}
					}
					else
					{
						if (rightDeltaAngle <= maxChangeAngle)
						{
							mode = GetGroundModeFromAngle(mode, rightGroundAngle);
						}
					}

					if (entitySensors[8].info.collided)
					{
						mode = GetGroundModeFromAngle(mode, entity.angle);
					}
				}
				else
				{
					if (entitySensors[8].info.collided)
					{
						if (rightDeltaAngle <= maxChangeAngle)
						{
							mode = GetGroundModeFromAngle(mode, rightGroundAngle);
						}
					}
					else
					{
						if (leftDeltaAngle <= maxChangeAngle)
						{
							mode = GetGroundModeFromAngle(mode, leftGroundAngle);
						}
					}

					if (entitySensors[7].info.collided)
					{
						mode = GetGroundModeFromAngle(mode, entity.angle);
					}
				}
			}
			else if (!entity.onGround)
			{
				mode = GetGroundModeFromAngle(mode, entity.angle);
			}

			return mode;
		}

		private static CollisionState GetGroundModeFromAngle(CollisionState mode, float a)
		{
			return a switch
			{
				> 315f or < 45f => CollisionState.Floor,
				> 45f and < 135f => CollisionState.RWall,
				> 135f and < 225f => CollisionState.Roof,
				> 225f and < 315f => CollisionState.LWall,
				_ => mode
			};
		}

		private static float GetVTileAngle(VTile tile, Vector3 tileRotation, Entity.Entity entity, Direction direction)
		{
			if (tile.flagged && entity != null)
			{
				switch (direction)
				{
					case Direction.Down:
						return 0;
					case Direction.Right:
						return 90;
					case Direction.Up:
						return 180;
					case Direction.Left:
						return 270;
				}
			}

			var tileAngle = Rotation.Clamp0360(tile.angle + Mathf.Round(tileRotation.z));
			var flipped = Mathf.Round(tileRotation.y) == 180f;
			tileAngle *= flipped ? -1 : 1;

			return !flipped ? tileAngle : Rotation.Clamp0360(tileAngle);
		}

		private static Sensor GetWinningSensorDown(float entityAngle)
		{
			var competingSensors = new List<Sensor>() { _sensors[0], _sensors[1], _sensors[2] };
			var lowestValue = 64;
			var chosenSensor = competingSensors[0];

			for (var i = 0; i < 3; i++)
			{
				var sensor = competingSensors[i];
				if (lowestValue <= sensor.Distance) continue;
				lowestValue = sensor.Distance;
				chosenSensor = sensor;
			}

			var equalSensors = new List<Sensor> { chosenSensor };

			var winningSensor = chosenSensor;
			for (var i = 0; i < 3; i++)
			{
				var sensor = competingSensors[i];
				if (chosenSensor.Distance == sensor.Distance)
				{
					equalSensors.Add(sensor);
				}
			}

			foreach (var sensor in equalSensors.Where(sensor => sensor != winningSensor).Where(sensor =>
						 Rotation.AngleDifference(entityAngle, winningSensor.Angle) >
						 Rotation.AngleDifference(entityAngle, sensor.Angle)))
			{
				winningSensor = sensor;
			}

			return winningSensor;
		}

		private static Sensor GetWinningSensorUp(float entityAngle)
		{
			var competingSensors = new List<Sensor>() { _sensors[5], _sensors[6] };
			var lowestValue = 64;
			var chosenSensor = competingSensors[0];

			for (var i = 0; i < 2; i++)
			{
				var sensor = competingSensors[i];
				if (lowestValue <= sensor.Distance) continue;
				lowestValue = sensor.Distance;
				chosenSensor = sensor;
			}

			var equalSensors = new List<Sensor>();
			if (equalSensors == null) throw new ArgumentNullException(nameof(equalSensors));
			equalSensors.Add(chosenSensor);

			var winningSensor = chosenSensor;

			return winningSensor;
		}

		public static LayerMask CollisionLayerToMask(CollisionLayer layer)
		{
			return layer switch
			{
				CollisionLayer.None => NullMask,
				CollisionLayer.Ground => GroundMask,
				CollisionLayer.A => AMask,
				CollisionLayer.B => BMask,
				CollisionLayer.GroundA => (GroundMask | AMask),
				CollisionLayer.GroundB => (GroundMask | BMask),
				_ => NullMask
			};
		}
	}
}