using UnityEditor;

namespace VEngine.Entity.Editor
{
    [CustomEditor(typeof(Entity))]
    public class EntityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}