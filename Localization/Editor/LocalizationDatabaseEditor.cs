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

            if (GUILayout.Button("Translate All From Default"))
            {
                localizationDatabase.Translate();
            }

            if (GUILayout.Button("Translate Concrete From Default"))
            {
                localizationDatabase.TranslateConcrete();
            }

            if (GUILayout.Button("Change Default Language"))
            {
                localizationDatabase.ChangeDefaultLanguage();
            }
        }
    }
}