using UnityEditor;
using UnityEngine;
using VEngine.Utilities.Helpers;
using VEngine.Camera;
using UnityEngine.Rendering;

namespace Utilities.Parallax
{
	[ExecuteAlways]
	public class ParallaxManager : Singleton<ParallaxManager>
	{
		public Vector2 focusPosition;

		public void OnBeginCameraRendering(ScriptableRenderContext context, UnityEngine.Camera camera)
		{
			SetFocusPosition(camera);
		}

		private void SetFocusPosition(UnityEngine.Camera camera)
		{
			focusPosition = camera.transform.position;
		}

		public Vector2 GetParallaxPosition(Vector2 position, Vector2 factor)
		{
			return VectorHelper.Vector2FloorToPixel(position + (focusPosition - position) * factor);
		}

		void OnEnable()
		{
			RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
		}

		internal void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
		}
	}
}