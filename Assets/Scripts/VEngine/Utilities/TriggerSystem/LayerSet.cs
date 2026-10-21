using UnityEngine;
using VEngine.Entity.Constants;

namespace VEngine.Utilities.TriggerSystem
{
    [CreateAssetMenu(menuName = "Sonic-V/Triggers/LayerSet", fileName = "LayerFlip")]
    [System.Serializable]
    public class LayerSet : TriggerBase
    {
        public bool requireGrounded;
        public CollisionLayer layer;

        public override void Trigger(Vector3Int triggerPosition, Entity.Entity entity)
        {
            if (requireGrounded && !entity.onGround) return;
            SetLayer(entity);
        }

        public void SetLayer(Entity.Entity entity)
        {
            entity.collisionLayer = layer;
        }
    }
}