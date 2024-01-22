using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameSDK.Core.PropertyAttributes
{
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string CallbackMethodName { get; private set; }

        public OnValueChangedAttribute(string callbackMethodName)
        {
            CallbackMethodName = callbackMethodName;
        }
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnChangedCallAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property);
            if (!EditorGUI.EndChangeCheck()) return;
        
            OnValueChangedAttribute at = attribute as OnValueChangedAttribute;
            MethodInfo method = property.serializedObject.targetObject.GetType().GetMethods().First(m => m.Name == at.CallbackMethodName);

            if (!method.GetParameters().Any())
                method.Invoke(property.serializedObject.targetObject, null);
        }
    }

#endif
}