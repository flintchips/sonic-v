using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using VEngine.Utilities.Helpers;

namespace VEngine.Utilities.TriggerSystem.Editor
{
    [CustomEditor(typeof(TriggerBase), true)]
    public class TriggerInspector : UnityEditor.Editor
    {
        private UnityEditor.Editor editor;

        public Texture2D previewTexture;
        public Tile Tile => (target as Tile);
        public TriggerBase Trigger => (target as TriggerBase);

        public void OnEnable()
        {
            var dummyTile = CreateInstance<Tile>();
            var tileEditorType = CreateEditor(dummyTile).GetType();

            editor = CreateEditor(Tile, tileEditorType);
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            if (!EditorGUI.EndChangeCheck()) return;
            editor.Repaint();
            EditorUtility.SetDirty(target);
        }

        // Draws the icon in the asset folder for the trigger.
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            try
            {
                if (Trigger.sprite != null && Trigger != null)
                {
                    var spritePreview = SpriteHelper.Sprite2Texture(Trigger.sprite); // Get sprite texture
                    if (spritePreview == null) spritePreview = new Texture2D(16, 16);
                    var pixels = spritePreview.GetPixels();
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] *= Trigger.color; // Tint
                    }

                    spritePreview.SetPixels(pixels);
                    spritePreview.Apply();

                    spritePreview.filterMode = FilterMode.Point;

                    var preview = new Texture2D(width, height);
                    EditorUtility.CopySerialized(spritePreview,
                        preview); // Returning the original texture causes an editor crash
                    previewTexture = preview;

                    return preview;
                }

                editor.Repaint();
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}