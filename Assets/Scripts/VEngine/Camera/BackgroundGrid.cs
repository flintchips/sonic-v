using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VEngine.Camera;
using UnityEditor;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BackgroundGrid : MonoBehaviour
{
	UnityEngine.Camera playerCamera;

	void Awake()
	{
		playerCamera = VEngine.Camera.PlayerCamera.Instance.GetComponent<UnityEngine.Camera>();
	}

	void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
	}

	internal void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
	}

	public void OnBeginCameraRendering(ScriptableRenderContext context, UnityEngine.Camera camera)
	{
		HandleGridTransform(camera);
	}

	void HandleGridTransform(UnityEngine.Camera camera)
	{

		transform.position = (Vector2)camera.transform.position;
		float size = camera.orthographicSize * 2;
		transform.localScale = new Vector3(size * camera.aspect, size, 1);
	}
}
