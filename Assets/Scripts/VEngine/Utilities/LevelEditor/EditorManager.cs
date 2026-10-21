using UnityEngine;

namespace VEngine.Utilities.LevelEditor
{
    [ExecuteAlways]
    public class EditorManager : MonoBehaviour
    {
        private bool first = true;

        private void OnEnable()
        {
            first = true;
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                HandleMouseOverlap();
                HandleSceneDrag();
                HandleSceneZoom();
            }

            HandleGridBackground();

            first = false;
        }

        public new UnityEngine.Camera camera;
        public GameObject bgGrid;
        public Material bgGridMaterial;

        private void HandleGridBackground()
        {
            if (first)
            {
                bgGridMaterial = bgGrid.GetComponent<SpriteRenderer>().sharedMaterial;
            }

            if (!camera) return;
            if (!bgGrid) return;

            // make the grid's sprite cover the cameras view, the shader does all the grid drawing based on world pos
            Vector3 bgScale = Vector2.one;
            var orthographicSize = camera.orthographicSize;
            bgScale.x = orthographicSize * 2 * (16f / 9f);
            bgScale.y = orthographicSize * 2;
            bgGrid.transform.localScale = bgScale;

            bgGridMaterial.SetFloat(CamScale, orthographicSize);
        }

        private Vector3 dragOrigin;
        private Vector3 mousePosStore;
        private bool dragging;

        public void HandleSceneDrag()
        {
            if (!camera) return;

            if (mouseHoldScene)
            {
                var move = camera.ScreenToWorldPoint(Input.mousePosition) - mousePosStore;
                camera.transform.position -= move;
            }

            mousePosStore = camera.ScreenToWorldPoint(Input.mousePosition);
        }

        public void HandleSceneZoom()
        {
            if (mouseOverScene)
                camera.orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * camera.orthographicSize;

            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, 1, Mathf.Infinity);
        }


        private bool mouseOverScene;
        private bool mouseHoldScene;
        private static readonly int CamScale = Shader.PropertyToID("_camScale");

        public void HandleMouseOverlap()
        {
            if (Input.mousePosition.x is >= 296f and < 1920 && Input.mousePosition.y is < 980f and >= 0)
                mouseOverScene = true;
            else
                mouseOverScene = false;

            if (Input.GetMouseButton(1))
            {
                if (mouseOverScene)
                    mouseHoldScene = true;
            }
            else mouseHoldScene = false;
            // also for other overlaps
        }

        public void Play()
        {
            Debug.Log("Play");
        }
    }
}