using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VEngine.Effects.IndexedColor
{
	[ExecuteAlways]
	public class Palette32Effect : MonoBehaviour
	{
		public SpriteRenderer spriteRenderer;
		public Palette32 palette;

		void OnEnable()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			palette ??= new Palette32(spriteRenderer.sprite.texture);
		}

		void Update()
		{
			spriteRenderer.sharedMaterial.SetColorArray("_from_colors", palette.array.GetFromColors());
			spriteRenderer.sharedMaterial.SetColorArray("_to_colors", palette.array.GetToColors());
			spriteRenderer.sharedMaterial.SetColor("_2ndColor", spriteRenderer.sharedMaterial.GetColorArray("_to_colors")[1]);
		}
	}
}