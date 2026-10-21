using UnityEngine;
using VEngine.Entity.Constants;

namespace VEngine.Utilities.TriggerSystem
{
    [CreateAssetMenu(menuName = "Sonic-V/Triggers/LayerFlip", fileName = "LayerFlip")]
    [System.Serializable]
    public class LayerFlip : TriggerBase
    {
        public bool requireGrounded;
        public TriggerInfo TriggerInfo;

        public override void Trigger(Vector3Int triggerPosition, Entity.Entity entity)
        {
            TriggerInfo = GetTriggerInfo(triggerPosition);

            if (requireGrounded && !entity.onGround) return;
            SwitchLayer(entity);
        }

        public void SwitchLayer(Entity.Entity entity)
        {
            var newLayer = entity.collisionLayer;
            var flipped = (TriggerInfo.TileRotation.y == 180);
            switch (TriggerInfo.TileRotation.z)
            {
                case 0:
                    if ((!flipped && (entity.velocity.x > 0)) || (flipped && (entity.velocity.x < 0)))
                    {
                        newLayer = CollisionLayer.GroundA;
                    }
                    else if (entity.velocity.x != 0)
                    {
                        newLayer = CollisionLayer.GroundB;
                    }

                    if (entity.velocity.x == 0)
                    {
                        var layerA = entity.direction != Flip.X;

                        if (flipped) layerA = !layerA;

                        newLayer = layerA ? CollisionLayer.GroundA : CollisionLayer.GroundB;
                    }

                    break;
                case 90:
                    if ((!flipped && (entity.velocity.y > 0)) || (flipped && (entity.velocity.y > 0)))
                    {
                        newLayer = CollisionLayer.GroundA;
                    }
                    else if (entity.velocity.y != 0)
                    {
                        newLayer = CollisionLayer.GroundB;
                    }

                    break;
                case 180:
                    if ((!flipped && (entity.velocity.x < 0)) || (flipped && (entity.velocity.x > 0)))
                    {
                        newLayer = CollisionLayer.GroundA;
                    }
                    else if (entity.velocity.x != 0)
                    {
                        newLayer = CollisionLayer.GroundB;
                    }

                    if (entity.velocity.x == 0)
                    {
                        var layerA = entity.direction is not Flip.None or Flip.X;

                        if (flipped) layerA = !layerA;

                        newLayer = layerA ? CollisionLayer.GroundA : CollisionLayer.GroundB;
                    }

                    break;
                case 270:
                    if ((!flipped && (entity.velocity.y < 0)) || (flipped && (entity.velocity.y < 0)))
                    {
                        newLayer = CollisionLayer.GroundA;
                    }
                    else if (entity.velocity.x != 0)
                    {
                        newLayer = CollisionLayer.GroundB;
                    }

                    break;
            }

            entity.collisionLayer = newLayer;
        }
    }
}