using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using VEngine.Utilities.Helpers;

namespace VEngine.Utilities.TileSystem.Editor
{
	[CustomEditor(typeof(VTile))]
	public class VTileInspector : UnityEditor.Editor
	{
		public Texture2D previewTexture;
		private UnityEditor.Editor editor;
		public Tile Tile => (target as Tile);
		private VTile Vtile => (target as VTile);

		static Rect LastRect => GUILayoutUtility.GetLastRect();

		public void OnEnable()
		{
			var dummyTile = CreateInstance<Tile>();
			var tileEditorType = CreateEditor(dummyTile).GetType();

			editor = CreateEditor(Vtile, tileEditorType);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			
			GUI.Label(DrawHelper.AddRect(LastRect, new Rect(0, 20, 0, 0)), "Tile Preview");
			
			GUILayout.Space(128);
			
			previewTexture = SpriteHelper.Sprite2Texture(Vtile.sprite);
			if(previewTexture.filterMode != FilterMode.Point) previewTexture.filterMode = FilterMode.Point;
			
			var previewRect = new Rect(LastRect.x, LastRect.y + 30, 64, 64);
			EditorGUI.DrawTextureTransparent(previewRect, previewTexture, ScaleMode.StretchToFill);
			
			var center = new Vector2(previewRect.x + 32, previewRect.y + 32);
			
			if(!Vtile.flagged)
			{
				// draw the green helper line based on the tile angle
				var pos1 = Vector2.zero;
				var pos2 = Vector2.zero;
				
				pos1.x = center.x + Mathf.Cos(Vtile.angle * Mathf.Deg2Rad) * 32;
				pos1.y = center.y - Mathf.Sin(Vtile.angle * Mathf.Deg2Rad) * 32;
				
				pos2.x = center.x + Mathf.Cos(Vtile.angle * Mathf.Deg2Rad) * -32;
				pos2.y = center.y - Mathf.Sin(Vtile.angle * Mathf.Deg2Rad) * -32;
				
				Handles.color = Color.red;
				Handles.DrawLine(new Vector3(pos1.x, pos1.y, -1), new Vector3(pos2.x, pos2.y, -1), 2);
			}

			if (!EditorGUI.EndChangeCheck()) return;
			editor.Repaint();
			EditorUtility.SetDirty(target);
		}
		
		// Draws the icon in the asset folder for the collision tile
		public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
		{
			try
			{
				if (Vtile.sprite != null && Vtile != null)
				{
					var spritePreview = SpriteHelper.Sprite2Texture(Vtile.sprite); // Get sprite texture
					if(spritePreview == null) spritePreview = new Texture2D(16, 16);
					var pixels = spritePreview.GetPixels();
					for (var i = 0; i < pixels.Length; i++)
					{
						pixels[i] *= Vtile.color; // Tint
					}
					spritePreview.SetPixels(pixels);
					spritePreview.Apply();
					
					spritePreview.filterMode = FilterMode.Point;
					
					var preview = new Texture2D(width, height);
					EditorUtility.CopySerialized(spritePreview, preview); // Returning the original texture causes an editor crash
					
					if(spritePreview != previewTexture)
					{
						previewTexture = spritePreview;
					}
					
					return preview;
				}
				editor.Repaint();
				return null;
			}
			catch
			{
				editor.Repaint();
				return null;
			}
		}
	}
}
