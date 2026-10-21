using UnityEngine;

namespace VEngine.Utilities.Helpers
{
    public abstract class SpriteHelper
    {
        public static Texture2D Sprite2Texture(Sprite sprite)
        {
            var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height);

            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }
    }
}