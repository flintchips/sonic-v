using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VEngine.Utilities.Texture;

namespace VEngine.Effects.IndexedColor
{
	[System.Serializable]
	public class Palette32
	{
		public Palette32Colors array = new Palette32Colors();

		public Palette32(Palette32Colors colors)
		{
			this.array = colors;
		}

		public Palette32(Texture2D texture)
		{
			Palette32Colors new_colors = new Palette32Colors();
			Palette32Color[] colorArray = new Palette32Color[32];
			for (int i = 0; i < 32; i++)
			{
				colorArray[i] = new Palette32Color(Color.black, Color.black);
			}

			int filled = 0;
			for (int x = 0; x < texture.width; x++)
			{
				for (int y = 0; y < texture.height; y++)
				{
					Color pixel = texture.GetPixel(x, y);
					bool add = true;

					for (int i = 0; i < filled; i++)
					{
						if (add)
						{
							if (colorArray[i].from == pixel)
							{
								add = false;
							}
						}
					}

					if (add)
					{
						colorArray[filled].from = pixel;
						colorArray[filled].to = pixel;
						filled++;
					}
				}
			}

			this.array.colors = colorArray;
		}
	}

	[System.Serializable]
	public class Palette32Color
	{
		public Color from = Color.black;
		public Color to = Color.black;

		public Palette32Color(Color from, Color to)
		{
			this.from = from;
			this.to = to;
		}
	}

	[System.Serializable]
	public class Palette32Colors
	{
		public Palette32Color[] colors = new Palette32Color[32];

		public Color[] GetFromColors()
		{
			Color[] colorArray = new Color[32];
			for (int i = 0; i < 32; i++)
			{
				colorArray[i] = colors[i].from.linear;
			}
			return colorArray;
		}

		public Color[] GetToColors()
		{
			Color[] colorArray = new Color[32];
			for (int i = 0; i < 32; i++)
			{
				colorArray[i] = colors[i].to.linear;
			}
			return colorArray;
		}

		public void ResetToColors(Color[] replaceColors)
		{
			for (int i = 0; i < 32; i++)
			{
				colors[i].to = colors[i].from;
			}
		}

		public void SetToColors(Color[] replaceColors)
		{
			for (int i = 0; i < 32; i++)
			{
				if (replaceColors.Length > i)
					colors[i].to = replaceColors[i];
				else
					colors[i].to = colors[i].from;
			}
		}

		public Palette32Colors()
		{
			this.colors = new Palette32Color[32];
		}
	}
}