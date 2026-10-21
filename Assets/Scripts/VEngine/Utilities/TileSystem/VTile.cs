using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Utilities;
using VEngine.Utilities.Helpers;

namespace VEngine.Utilities.TileSystem
{
    [CreateAssetMenu(menuName = "Sonic-V/VTile", fileName = "VTile")]
    [System.Serializable]
    public class VTile : Tile
    {
        public ArrayInfo arrayInfo = new();
        public int[] ArrayT => arrayInfo.arrayT;
        public int[] ArrayB => arrayInfo.arrayB;
        public int[] ArrayL => arrayInfo.arrayL;
        public int[] ArrayR => arrayInfo.arrayR;

        public VTile()
        {
            flags = TileFlags.None;
            colliderType = ColliderType.Grid;
        }

        [FormerlySerializedAs("GetArrays")] [InspectorButton("GetArrayInfoFromTex")]
        public bool getArrays;

        public bool flagged;
        public float angle;

        private void GetArrayInfoFromTex()
        {
            GetArrayInfo();
        }

        public void GetArrayInfo()
        {
            var spriteTexture = SpriteHelper.Sprite2Texture(sprite);
            var info = new ArrayInfo();

            // topArray
            for (var x = 0; x < spriteTexture.width; x++)
            {
                for (var y = spriteTexture.height - 1; y >= 0; y--)
                {
                    if (CheckPixel(spriteTexture, x, y))
                    {
                        info.arrayT[x] = y + 1;
                        break;
                    }
                    else if (y == 0)
                    {
                        info.arrayT[x] = 0;
                    }
                }
            }

            // bottomArray
            for (var x = 0; x < spriteTexture.width; x++)
            {
                for (var y = 0; y < spriteTexture.height; y++)
                {
                    if (CheckPixel(spriteTexture, x, y))
                    {
                        info.arrayB[x] = 16 - y;
                        break;
                    }
                    else if (y == 15)
                    {
                        info.arrayB[x] = 0;
                    }
                }
            }

            // leftArray
            for (var y = 0; y < spriteTexture.height; y++)
            {
                for (var x = 0; x < spriteTexture.width; x++)
                {
                    if (CheckPixel(spriteTexture, x, y))
                    {
                        info.arrayL[y] = 16 - x;
                        break;
                    }

                    if (x == 15)
                    {
                        info.arrayL[y] = 0;
                    }
                }
            }

            // rightArray
            for (var y = 0; y < spriteTexture.height; y++)
            {
                for (var x = spriteTexture.width - 1; x >= 0; x--)
                {
                    if (CheckPixel(spriteTexture, x, y))
                    {
                        info.arrayR[y] = x + 1;
                        break;
                    }

                    if (x == 0)
                    {
                        info.arrayR[y] = 0;
                    }
                }
            }

            arrayInfo = info;
        }

        public bool CheckPixel(Texture2D texture, int x, int y)
        {
            var col = texture.GetPixel(x, y);
            return col.a >= 0.5f;
        }
    }

    [System.Serializable]
    public class ArrayInfo
    {
        public int[] arrayT;
        public int[] arrayB;
        public int[] arrayL;
        public int[] arrayR;

        public ArrayInfo()
        {
            this.arrayT = new int[16];
            this.arrayB = new int[16];
            this.arrayL = new int[16];
            this.arrayR = new int[16];
        }
    }
}