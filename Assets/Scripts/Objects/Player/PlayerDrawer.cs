using Objects.Player.Constants;
using UnityEngine;
using VEngine.Entity.Constants;
using VEngine.Utilities.Helpers;
using Utilities;
using VEngine.Camera;   

namespace Objects.Player
{
	public class PlayerDrawer : MonoBehaviour
	{
		private PlayerObject self;

		private void Awake()
		{
			self = GetComponent<PlayerObject>();
        }

		private void FixedUpdate()
		{
			if (self == null) return;
			self.PlayerDraw();

            LineDrawer.ClearPersistentLines();
            if (PlayerCamera._instance.collisionViewMode)
			{
                DrawCollisionSensors();
            }
		}

		private void DrawCollisionSensors()
		{
            // A
            LineDrawer.DrawWorldLinePersistent(self.position + (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)),
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)),
                self.sensors[0].enabled ? Color.green : Color.green * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)), Color.white);

            LineDrawer.DrawWorldPixelPersistent(
                self.position + ((Vector2)(Quaternion.Euler(new Vector3(0, 0, self.angle)) * -Vector3.up) * -PixelUnitHelper.P2U(self.outerbox.bottom)), Color.white);

            // 8
            LineDrawer.DrawWorldLinePersistent(self.position + (-self.ModeVector * PixelUnitHelper.P2U(self.innerbox.left - 1)),
                self.position + (self.ModeVectorRight * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (-self.ModeVector * PixelUnitHelper.P2U(self.innerbox.left - 1)),
                self.sensors[8].enabled ? Color.green : Color.green * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (self.ModeVectorRight * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (-self.ModeVector * PixelUnitHelper.P2U(self.innerbox.left - 1)), Color.white);

            // B
            LineDrawer.DrawWorldLinePersistent(self.position + (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)),
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)),
                self.sensors[1].enabled ? Color.cyan : Color.cyan * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)), Color.white);

            // 7
            LineDrawer.DrawWorldLinePersistent(self.position + (self.ModeVector * PixelUnitHelper.P2U(self.innerbox.right)),
                self.position + (-self.ModeVectorRight * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVector * PixelUnitHelper.P2U(self.innerbox.right)),
                self.sensors[7].enabled ? Color.cyan : Color.cyan * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (-self.ModeVectorRight * -PixelUnitHelper.P2U(self.outerbox.bottom)) +
                (self.ModeVector * PixelUnitHelper.P2U(self.innerbox.right)), Color.white);

            // C
            LineDrawer.DrawWorldLinePersistent(
                self.position + (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)) +
                (self.ModeVector * -PixelUnitHelper.P2U(1)),
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.top)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)),
                self.sensors[5].enabled ? Color.blue : Color.blue * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.top)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.left - 1)), Color.white);

            // D
            LineDrawer.DrawWorldLinePersistent(
                self.position + (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)) +
                (self.ModeVector * -PixelUnitHelper.P2U(1)),
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.top)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)),
                self.sensors[6].enabled ? Color.yellow : Color.yellow * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(
                self.position + (self.ModeVector * -PixelUnitHelper.P2U(self.outerbox.top)) +
                (self.ModeVectorRight * PixelUnitHelper.P2U(self.innerbox.right)), Color.white);

            float offset = 0;
            bool left;

            if (self.onGround)
            {
                if (self.angle == 0 && self == null)
                {
                    offset = PixelUnitHelper.P2U(-8);
                }
                else if (self != null)
                {
                    if (self.angle == 0)
                    {
                        offset = PixelUnitHelper.P2U(-8);
                    }
                    else
                    {
                        offset = 0;
                    }

                    if (self.animator.animStore.animationID == AnimationType.Jump)
                    {
                        if (self.angle == 0)
                        {
                            offset = 0;
                        }
                        else
                        {
                            offset = PixelUnitHelper.P2U(4);
                        }
                    }
                }

                left = self.groundVel < 0;

                if (self.groundVel == 0)
                {
                    if (self != null)
                    {
                        left = (self.direction == Flip.X);
                    }
                }
            }
            else
            {
                left = self.velocity.x < 0;

                if (self.velocity.x == 0)
                {
                    if (self != null)
                    {
                        left = (self.direction == Flip.X);
                    }
                }
            }

            var pushSensorsCenter = self.position;

            switch (self.collisionMode)
            {
                case CollisionState.Floor:
                    pushSensorsCenter.y += offset;
                    break;
                case CollisionState.RWall:
                    pushSensorsCenter.x -= offset;
                    break;
                case CollisionState.Roof:
                    pushSensorsCenter.y -= offset;
                    break;
                case CollisionState.LWall:
                    pushSensorsCenter.x += offset;
                    break;
                case CollisionState.None:
                default: break;
            }

            var pushEdge = pushSensorsCenter;
            if (!left)
                pushEdge += self.ModeVectorRight * PixelUnitHelper.P2U(self.outerbox.right);
            else
                pushEdge += self.ModeVectorRight * PixelUnitHelper.P2U(self.outerbox.left - 1);

            LineDrawer.DrawWorldLinePersistent(pushSensorsCenter, pushEdge, self.sensors[3].enabled ? Color.red : Color.red * 0.75f);
            LineDrawer.DrawWorldPixelPersistent(pushEdge, Color.white);

            // center
            LineDrawer.DrawWorldAnchorPersistant(self.position, Color.white);
        }
	}
}