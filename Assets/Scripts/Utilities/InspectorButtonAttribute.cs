using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// This attribute can only be applied to fields because its
    /// associated PropertyDrawer only operates on fields (either
    /// public or tagged with the [SerializeField] attribute) in
    /// the target MonoBehaviour.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class InspectorButtonAttribute : PropertyAttribute
    {
        private const float KDefaultButtonWidth = 80;

        public readonly string MethodName;

        public static float ButtonWidth => KDefaultButtonWidth;

        public InspectorButtonAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorButtonAttribute))]
    public class InspectorButtonPropertyDrawer : PropertyDrawer
    {
        private MethodInfo eventMethodInfo;

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            var inspectorButtonAttribute = (InspectorButtonAttribute)attribute;
            var buttonRect = new Rect(position.x + (position.width - InspectorButtonAttribute.ButtonWidth) * 0.5f,
                position.y, InspectorButtonAttribute.ButtonWidth, position.height);
            if (!GUI.Button(buttonRect, label.text)) return;
            var eventOwnerType = prop.serializedObject.targetObject.GetType();
            var eventName = inspectorButtonAttribute.MethodName;

            if (eventMethodInfo == null)
                eventMethodInfo = eventOwnerType.GetMethod(eventName,
                    BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (eventMethodInfo != null)
                eventMethodInfo.Invoke(prop.serializedObject.targetObject, null);
            else
                Debug.LogWarning($"InspectorButton: Unable to find method {eventName} in {eventOwnerType}");
        }
    }
#endif
}