using UnityEditor;
using UnityEngine;

namespace GameSDK.Localization.Editor
{
    [CustomEditor(typeof(LocalizationDatabase))]
    public class LocalizationDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            LocalizationDatabase localizationDatabase = (LocalizationDatabase)target;
            if(GUILayout.Button("Add Language"))
            {
                localizationDatabase.AddLanguage();
            }

            if (GUILayout.Button("Load Form Sheet"))
            {
                localizationDatabase.LoadFormSheet();
            }
            
            if (GUILayout.Button("Clear"))
            {
                localizationDatabase.Clear();
            }
        }
    }
}