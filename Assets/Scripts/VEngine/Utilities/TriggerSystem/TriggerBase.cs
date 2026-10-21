using UnityEngine;
using UnityEngine.Tilemaps;
using VEngine.Utilities.Helpers;

namespace VEngine.Utilities.TriggerSystem
{
    [System.Serializable]
    public class TriggerBase : Tile
    {
        public Texture2D PreviewTexture { get; private set; }
        public static Tilemap TriggerTilemap => GameObject.FindWithTag("Trigger").GetComponent<Tilemap>();
        public static LayerMask TriggerMask => LayerMask.GetMask("trigger");

        public virtual void Trigger(Vector3Int triggerPosition, Entity.Entity entity)
        {
        }

        public TriggerBase()
        {
            flags = TileFlags.None;
            colliderType = ColliderType.Grid;
        }

        public static TriggerInfo GetTriggerInfo(Vector3Int triggerPos)
        {
            var info = new TriggerInfo();

            var tile = (Tile)TriggerTilemap.GetTile(triggerPos);
            // round because tile angle sometimes returns long decimals: 90 -> 89.999999999
            var tileRotation =
                VectorHelper.Vector3Round(TriggerTilemap.GetTransformMatrix(triggerPos).rotation.eulerAngles);
            info.Tile = tile;
            info.TileRotation = tileRotation;

            return info;
        }

        public Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            return PreviewTexture;
        }
    }

    public class TriggerInfo
    {
        public Tile Tile;
        public Vector3 TileRotation;
    }
}