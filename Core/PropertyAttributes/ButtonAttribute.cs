using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameSDK.Core.PropertyAttributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string name;

        public ButtonAttribute(string name)
        {
            this.name = name;
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(Object), true, isFallback = false)]
    [CanEditMultipleObjects]
    public class ButtonAttributeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (var targetObject in targets)
            {
                var methods = targetObject.GetType().GetMethods();

                foreach (var method in methods)
                {
                    var attribute = (ButtonAttribute)method.GetCustomAttribute(typeof(ButtonAttribute), true);
                    if(attribute == null) continue;
                    
                    if (GUILayout.Button(attribute.name))
                    {
                        method.Invoke(targetObject, null);
                    }
                }
            }
        }
    }

#endif
}