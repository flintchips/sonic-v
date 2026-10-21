using UnityEditor;
using UnityEngine;
using VEngine.Utilities.Helpers;

namespace Utilities.Parallax.Editor
{
    [CustomEditor(typeof(ParallaxObject)), CanEditMultipleObjects]
    public sealed class ParallaxObjectEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var parallaxObject = (ParallaxObject)target;

            EditorGUI.BeginChangeCheck();
            var position = parallaxObject.transform.position;
            var newTargetPosition = Handles.PositionHandle(position, Quaternion.identity) +
                                    ((Vector3)parallaxObject.position - position);
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(parallaxObject, "Change Look At Target Position");
            parallaxObject.position = VectorHelper.Vector2FloorToPixel(newTargetPosition);
            parallaxObject.transform.position = VectorHelper.Vector2FloorToPixel(
                ParallaxManager.Instance.GetParallaxPosition(parallaxObject.position, parallaxObject.factor));
        }
    }
}