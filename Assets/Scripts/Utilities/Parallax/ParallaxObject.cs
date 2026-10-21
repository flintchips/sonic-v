using Objects.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utilities.Parallax
{
	[ExecuteAlways]
	public class ParallaxObject : ObjectBase
	{
		public Vector2 factor;

		public void OnBeginCameraRendering(ScriptableRenderContext context, UnityEngine.Camera camera)
		{
			ParallaxManager.Instance.focusPosition = camera.transform.position;
			transform.position = (Vector3)ParallaxManager.Instance.GetParallaxPosition(position, factor) +
									 new Vector3(0, 0, transform.position.z);
		}
#if UNITY_EDITOR
		private Tool lastTool = Tool.None;
#endif
		private void OnEnable()
		{
			RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
#if UNITY_EDITOR
			lastTool = Tools.current;
			Tools.current = Tool.None;
#endif
		}

		private void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
#if UNITY_EDITOR
			Tools.current = lastTool;
#endif
		}

	}
}