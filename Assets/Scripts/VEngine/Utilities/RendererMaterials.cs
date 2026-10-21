using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class SpriteUtilities : MonoBehaviour
{
	private new Renderer renderer;
	public List<Material> materials;

	void Update()
	{
		if (renderer == null) renderer = GetComponent<Renderer>();
		renderer.materials = materials.ToArray();
	}
}
