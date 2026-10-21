using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using VEngine.DebugInfo;

[ExecuteAlways]
public class PixelCamera : MonoBehaviour
{
	private RenderTexture RT;
	private Material aspectMaterial;
	private Camera m_Camera;
	private float pixelsPerUnit;

	void Awake()
	{
		Instantiate();
	}

	void Instantiate()
	{
		aspectMaterial = new Material(Shader.Find("SonicV/AspectCorrection"));
		m_Camera = GetComponent<Camera>();
		m_Camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		RT = new RenderTexture(384, 216, 0);
		RT.useDynamicScale = true;
		RT.filterMode = FilterMode.Point;
		RenderTexture.active = RT;
	}

	// Snap camera position to pixels using Camera.worldToCameraMatrix.
	void PixelSnap()
	{
		Vector3 cameraPosition = m_Camera.transform.position;
		Vector3 roundedCameraPosition = FloorToPixel(cameraPosition);
		Vector3 offset = roundedCameraPosition - cameraPosition;
		offset.z = -offset.z;

		// Get world to local camera matrix without scale
		var invPos = Matrix4x4.TRS(cameraPosition + offset, Quaternion.identity, Vector3.one).inverse;
		var invRot = Matrix4x4.Rotate(m_Camera.transform.rotation).inverse;
		var scaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));

		// Calculate inverse TRS
		m_Camera.worldToCameraMatrix = scaleMatrix * invRot * invPos;
	}

	Vector3 FloorToPixel(Vector3 vector)
	{
		pixelsPerUnit = 108 / m_Camera.orthographicSize;
		Vector3 flooredVector = vector;
		flooredVector.x = Mathf.Floor(vector.x * pixelsPerUnit) / pixelsPerUnit;
		flooredVector.y = Mathf.Floor(vector.y * pixelsPerUnit) / pixelsPerUnit;
		flooredVector.z = Mathf.Floor(vector.z * pixelsPerUnit) / pixelsPerUnit;
		return flooredVector;
	}

	void OnBeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (!m_Camera || !RT || !aspectMaterial) Instantiate();

		foreach (Camera camera in cameras)
		{
			if (camera == m_Camera)
			{
				PixelSnap();

				if (m_Camera.orthographicSize >= 6.75f)
				{
					RT.Resize(384, 216);
				}
				else
				{
					float size = m_Camera.orthographicSize * 32;
					RT.Resize(Mathf.FloorToInt(Mathf.FloorToInt(size) * (16f / 9f)), Mathf.FloorToInt(size));
				}

				m_Camera.targetTexture = RT;
			}
		}
	}

    void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (Camera camera in cameras)
        {
            if (camera == m_Camera)
            {
                // Simply call the static method; if lines exist, it renders them.
                Utilities.LineDrawer.RenderLines(RT);

                aspectMaterial.SetFloat("RenderedAspect", (float)Screen.width / Screen.height);
                m_Camera.targetTexture = null;
                Graphics.Blit(RT, (RenderTexture)null, aspectMaterial);
            }
        }
    }

    void OnEnable()
	{
		RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
		RenderPipelineManager.endContextRendering += OnEndContextRendering;
	}

	internal void OnDisable()
	{
		RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
		RenderPipelineManager.endContextRendering -= OnEndContextRendering;

		m_Camera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
		m_Camera.ResetWorldToCameraMatrix();
	}
}

public static class Extension
{
	public static void Resize(this RenderTexture renderTexture, int width, int height)
	{
		if (renderTexture && (renderTexture.width != width || renderTexture.height != height))
		{
			renderTexture.Release();
			renderTexture.width = width;
			renderTexture.height = height;
		}
	}
}
