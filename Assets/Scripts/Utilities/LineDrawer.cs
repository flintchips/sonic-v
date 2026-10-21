using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;

namespace Utilities
{
    public class LineDrawer : MonoBehaviour
    {
        // Struct to hold both position and color for the GPU
        private struct LineData
        {
            public Vector4 coords; // x1, y1, x2, y2
            public Color color;
        }

        private static LineDrawer _instance;
        private static List<LineData> _lineBatch = new List<LineData>();
        private GraphicsBuffer _buffer;
        private ComputeBuffer _computeBuffer;
        private Material _lineMat;
        private static List<LineData> _persistentLines = new List<LineData>();
        private const string MaterialResourcePath = "Resources/Utilities/PixelLine";

        public static void ClearPersistentLines()
        {
            _persistentLines.Clear();
        }

        private static void Initialize()
        {
            if (_instance != null) return;

            GameObject go = new GameObject("Runtime_LineDrawer");
            // avoid HideAndDontSave - can be problematic in builds
            _instance = go.AddComponent<LineDrawer>();
            DontDestroyOnLoad(go);
        }

        private RenderTexture EnsureRenderTexture(RenderTexture targ, int w = 384, int h = 216)
        {
            if (targ != null && targ.IsCreated()) return targ;
            try
            {
                RenderTexture rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
                rt.filterMode = FilterMode.Point;
                rt.useMipMap = false;
                rt.autoGenerateMips = false;
                rt.Create();
                return rt;
            }
            catch (Exception ex)
            {
                Debug.LogError("[LineDrawer] Failed to create RenderTexture: " + ex);
                return null;
            }
        }

        private Material GetOrCreateMaterial()
        {
            if (_lineMat != null) return _lineMat;

            // 1) Try Resources (recommended: create a Material asset and place in Resources/LineDrawer/PixelLineMat)
            Material resMat = Resources.Load<Material>(MaterialResourcePath);
            if (resMat != null)
            {
                _lineMat = new Material(resMat); // instance so we can modify
                return _lineMat;
            }

            // 2) Try Shader.Find
            Shader sh = Shader.Find(MaterialResourcePath);
            if (sh != null)
            {
                _lineMat = new Material(sh);
                return _lineMat;
            }

            // 3) Fallback to a simple unlit shader so we can at least see something and log error
            Debug.LogError("[LineDrawer] PixelLine shader/material not found. Please add a Material at Resources/" + MaterialResourcePath + " or add 'Utilities/PixelLine' to Always Included Shaders.");
            Shader fallbackShader = Shader.Find("Unlit/Color");
            if (fallbackShader != null)
            {
                _lineMat = new Material(fallbackShader);
                return _lineMat;
            }

            Debug.LogError("[LineDrawer] No fallback shader available. Lines will not render.");
            return null;
        }

        public static Vector2Int WorldToScreenPixel(Vector2 world)
        {
            // take world position and convert to pixel coordinates based on PixelCamera
            // each pixel is 1/16th of a unit
            // positive y is down in pixel coordinates
            Camera cam = Camera.main; // Ensure this is your PixelCamera
            if (cam == null) return Vector2Int.zero;
            Vector2 camRelative = world - new Vector2(cam.transform.position.x, cam.transform.position.y);
            Vector2 screenPos = camRelative * 16f;
            screenPos.x += 384 / 2f;
            screenPos.y = (216 / 2f) - screenPos.y;
            return new Vector2Int(Mathf.FloorToInt(screenPos.x + 0.001f), Mathf.FloorToInt(screenPos.y - 0.001f));
        }

        // Inside LineDrawer or a Helper class
        public static void DrawWorldLine(Vector3 startWorld, Vector3 endWorld, Color color)
        {
            if (_instance == null) Initialize();
            // Convert world positions to screen/pixel coordinates
            Vector2 screenStart = WorldToScreenPixel(startWorld);
            Vector2 screenEnd = WorldToScreenPixel(endWorld);

            // Pass the pixel coordinates (0 to 384, etc.) to our drawer
            DrawLine(screenStart, screenEnd, color);
        }

        public static void DrawWorldLinePersistent(Vector3 startWorld, Vector3 endWorld, Color color)
        {
            if (_instance == null) Initialize();
            Vector2 screenStart = WorldToScreenPixel(startWorld);
            Vector2 screenEnd = WorldToScreenPixel(endWorld);
            DrawLinePersistant(screenStart, screenEnd, color);
        }

        public static void DrawWorldPixelPersistent(Vector3 worldPos, Color color)
        {
            if (_instance == null) Initialize();
            Vector2 p = WorldToScreenPixel(worldPos);
            DrawLinePersistant(p, p, color);
        }

        public static void DrawWorldPixel(Vector3 worldPos, Color color)
        {
            if (_instance == null) Initialize();
            Vector2 p = WorldToScreenPixel(worldPos);
            // A 1px line to itself effectively draws a pixel with our shader logic
            DrawLine(p, p, color);
        }

        public static void DrawWorldAnchor(Vector3 worldPos, Color color)
        {
            // draws 1 black pixel at the center with 4 white pixels around it
            Vector2 p = WorldToScreenPixel(worldPos);
            DrawLine(p, p, Color.black);
            DrawLine(p + new Vector2(-1, 0), p + new Vector2(-1, 0), color);
            DrawLine(p + new Vector2(1, 0), p + new Vector2(1, 0), color);
            DrawLine(p + new Vector2(0, -1), p + new Vector2(0, -1), color);
            DrawLine(p + new Vector2(0, 1), p + new Vector2(0, 1), color);
        }

        public static void DrawWorldAnchorPersistant(Vector3 worldPos, Color color)
        {
            if (_instance == null) Initialize();
            // draws 1 black pixel at the center with 4 white pixels around it
            Vector2 p = WorldToScreenPixel(worldPos);
            DrawLinePersistant(p, p, Color.black);
            DrawLinePersistant(p + new Vector2(-1, 0), p + new Vector2(-1, 0), color);
            DrawLinePersistant(p + new Vector2(1, 0), p + new Vector2(1, 0), color);
            DrawLinePersistant(p + new Vector2(0, -1), p + new Vector2(0, -1), color);
            DrawLinePersistant(p + new Vector2(0, 1), p + new Vector2(0, 1), color);
        }

        public static void DrawLinePersistant(Vector2 start, Vector2 end, Color color)
        {
            if (_instance == null) Initialize();
            _persistentLines.Add(new LineData
            {
                coords = new Vector4(start.x, start.y, end.x, end.y),
                color = color
            });
        }


        // Overload with Color
        public static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            if (_instance == null) Initialize();
            _lineBatch.Add(new LineData
            {
                coords = new Vector4(start.x, start.y, end.x, end.y),
                color = color
            });
        }

        // Default to Magenta
        public static void DrawLine(Vector2 start, Vector2 end)
        {
            DrawLine(start, end, Color.magenta);
        }

        public static void RenderLines(RenderTexture target)
        {
            if (_instance == null || _lineBatch.Count == 0 || target == null) return;
            _instance.InternalRender(target);
        }

        private void InternalRender(RenderTexture target)
        {
            if (_lineMat == null) _lineMat = GetOrCreateMaterial();

            // Merge line batches
            int totalLines = _lineBatch.Count + _persistentLines.Count;
            if (totalLines == 0 || target == null) return;

            LineData[] combined = new LineData[totalLines];
            _lineBatch.CopyTo(combined, 0);
            _persistentLines.CopyTo(combined, _lineBatch.Count);

            // Create or resize buffer
            int stride = sizeof(float) * 8;
            if (_buffer == null || _buffer.count < totalLines)
            {
                _buffer?.Dispose();
                _buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, Mathf.Max(totalLines, 128), stride);
            }

            _buffer.SetData(combined);

            // Command buffer setup (same as before)
            CommandBuffer cb = CommandBufferPool.Get("PixelLine");
            cb.SetRenderTarget(target);
            cb.SetGlobalBuffer("_LineBuffer", _buffer);
            cb.SetGlobalVector("_RTSize", new Vector4(target.width, target.height, 1f / target.width, 1f / target.height));
            cb.DrawProcedural(Matrix4x4.identity, _lineMat, 0, MeshTopology.Triangles, 6, totalLines);
            Graphics.ExecuteCommandBuffer(cb);
            CommandBufferPool.Release(cb);

            // Clear only the temporary batch
            _lineBatch.Clear();
        }


        private void OnDestroy()
        {
            try { _computeBuffer?.Dispose(); } catch { }
        }
    }
}
