using System.Collections.Generic;
using Objects.Player;
using UnityEngine;
using VEngine.Camera.Constants;
using VEngine.Utilities.Helpers;
using Utilities;
using VEngine.Collision;

namespace VEngine.Camera
{
	public class PlayerCamera : MonoBehaviour
	{
		public static PlayerCamera _instance;
		public Vector2 position;
		public Vector2 velocity;
		public Vector2 relativeCenter;
		public Vector2 lastPos;
		public float adjustY;
		public float adjustX;
		public Vector2 shakePos;
		public Vector2 lookPos;
		public bool disableYOffset;
		public bool lockToY;
		public float boundsL;
		public float boundsR;
		public float boundsT;
		public float boundsB;
		public Vector2 targetPosition;
		public float cameraDelay;
		public List<Vector2> previousPositions = new();
		public string stateRead;

		private delegate void State();
		private State state;
		private static PlayerObject Target => PlayerVars.Leader;
		public bool collisionViewMode = false;

		public static PlayerCamera Instance
		{
			get
			{
				if (_instance) return _instance;
				_instance = FindObjectOfType<PlayerCamera>();
				return _instance;
			}
		}

		private void Awake()
		{
			if (!_instance)
			{
				_instance = this;
			}
			else
			{
				if (_instance != this)
					Destroy(gameObject);
			}

			SetState(CameraState.FollowXY);
		}

		public void LateUpdate()
		{
            LineDrawer.DrawLine(new Vector2(-10, -10), new Vector2(-10, -10));
            if (Input.GetKeyDown(KeyCode.C))
            {
                collisionViewMode = !collisionViewMode;
            }
        }

        public void CameraUpdate()
        {
  
        }

        public void CameraLateUpdate()
		{
			stateRead = state != null ? state.Method.Name : "";

			lastPos.x = position.x;
			lastPos.y = position.y;

			targetPosition = VectorHelper.Vector2Floor(Target.transform.position * 16f) / 16f;

			relativeCenter = position - targetPosition;

			if (Target.scrollDelay > 0)
			{
				if (cameraDelay < Target.scrollDelay)
					cameraDelay = Target.scrollDelay;
			}
			else
			{
				if (cameraDelay > 0)
				{
					cameraDelay--;
					if (cameraDelay <= 0)
					{
						cameraDelay = 0;
					}
				}
			}

			state ??= FollowXY;
			StateRun(state);

			relativeCenter = position - targetPosition;

			SetBounds();

			if (shakePos.x != 0)
			{
				if (shakePos.x <= 0)
					shakePos.x = -1 - shakePos.x;
				else
					shakePos.x = -shakePos.x;
			}

			if (shakePos.y != 0)
			{
				if (shakePos.y <= 0)
					shakePos.y = -1 - shakePos.y;
				else
					shakePos.y = -shakePos.y;
			}

			if (!disableYOffset)
			{
				if (Mathf.Abs(position.y - targetPosition.y) >= PixelUnitHelper.P2U(1f))
				{
					var solidObjectVelY = (Target.solidObject != null) ? Target.solidObject.velocity.y : 0;
					position.y += -relativeCenter.y / 8f + ((Target.velocity.y + solidObjectVelY) / 16f);

					if (!(Mathf.Abs(position.y - targetPosition.y) >= PixelUnitHelper.P2U(1f)))
					{
						lockToY = true;
					}
				}
				else
				{
					lockToY = true;
				}
			}
			else
			{
				lockToY = false;
			}

			// lock to y position 
			var posStore = position.y;
			if (lockToY)
			{
				position.y = targetPosition.y;
			}
			var change = position.y - posStore;

			// vector the player has moved in the past frame
			velocity.x = position.x - lastPos.x;
			velocity.y = position.y - lastPos.y;

			if (lockToY && Mathf.Abs(change) > Mathf.Abs(velocity.y))
			{
				position.y = posStore + Mathf.Sign(change) * (Mathf.Abs(velocity.y) + PixelUnitHelper.P2U(1) * (Time.deltaTime * 60f));
			}

			// caps camera movement speed
			const float camSpeedCap = 15;
			if (velocity.magnitude > (camSpeedCap / 16f))
			{
				position = lastPos + (velocity.normalized * camSpeedCap / 16f);
			}

			var newPosition = (position + (lookPos + new Vector2(adjustX, adjustY)) / 16f);
			var currentPosition =
				(Vector2)(Vector3)VectorHelper.Vector3FloorToInt(
					((Vector3)newPosition + new Vector3(0, PixelUnitHelper.P2U(0.5f), -10)) * 16f) / 16f;

			AddPositionQueue(currentPosition);

			transform.position = (Vector3)GetPreviousPosition((int)(cameraDelay)) +
								 new Vector3(0, 0, -10);
		}

		public void ShakeScreen(int x, int y)
		{
			shakePos.x = x;
			shakePos.y = y;
		}


		private void AddPositionQueue(Vector2 pos)
		{
			previousPositions.Add(pos);

			while (previousPositions.Count > 120)
			{
				previousPositions.RemoveAt(0);
			}
		}

		private Vector2 GetPreviousPosition(int framesAgo)
		{
			var posList = new List<Vector2>(previousPositions);
			posList.Reverse();
			return framesAgo < (posList.Count - 1) ? posList[framesAgo] : posList[posList.Count - 1];
		}

		private static void SetBounds()
		{
		}

		private void HandleHBounds()
		{
			var posStore = position.x;
			if (relativeCenter.x < PixelUnitHelper.P2U(boundsL))
			{
				position.x = targetPosition.x + PixelUnitHelper.P2U(boundsL);
			}
			else if (relativeCenter.x > PixelUnitHelper.P2U(boundsR))
			{
				position.x = targetPosition.x + PixelUnitHelper.P2U(boundsR);
			}
			var change = position.x - posStore;

			if (Mathf.Abs(change) > Mathf.Abs(velocity.x))
			{
				position.x = posStore + Mathf.Sign(change) * (Mathf.Abs(velocity.x) + PixelUnitHelper.P2U(1) * (Time.deltaTime * 60f));
			}
		}

		private void HandleVBounds()
		{
			var posStore = position.y;
			if (relativeCenter.y < PixelUnitHelper.P2U(boundsB))
			{
				position.y = targetPosition.y + PixelUnitHelper.P2U(boundsB);
			}
			else if (relativeCenter.y > PixelUnitHelper.P2U(boundsT))
			{
				position.y = targetPosition.y + PixelUnitHelper.P2U(boundsT);
			}
			var change = position.y - posStore;

			if (Mathf.Abs(change) > Mathf.Abs(velocity.y))
			{
				position.y = posStore + Mathf.Sign(change) * (Mathf.Abs(velocity.y) + PixelUnitHelper.P2U(1) * (Time.deltaTime * 60f));
			}
		}

		private void FollowXY()
		{
			if (!Target) return;
			HandleHBounds();
			if (disableYOffset)
			{
				HandleVBounds();
			}
		}

		private static void FollowX()
		{
		}

		private static void FollowY()
		{
		}

		public void SetState(CameraState newState)
		{
			state = newState switch
			{
				CameraState.FollowXY => FollowXY,
				CameraState.FollowX => FollowX,
				CameraState.FollowY => FollowY,
				_ => state
			};
		}

		private void StateRun(State stateMethod)
		{
			state = stateMethod;
			stateMethod();
		}
	}
}