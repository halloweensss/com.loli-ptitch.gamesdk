using System;
using System.Collections.Generic;
using System.Linq;
using GameSDK.Core.PropertyAttributes;
using UnityEditor;
using UnityEngine;

namespace GameSDK.Localization
{
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "GameSDK/LocalizationDatabase")]
    public class LocalizationDatabase : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private LocalizedLanguage _defaultLanguage;
        [SerializeField] [ArrayElementTitle("Language", "Name")] 
        private List<LocalizedLanguage> _languages;

        internal string Id => _id;
        internal LocalizedLanguage DefaultLanguage => _defaultLanguage;
        internal List<LocalizedLanguage> Languages => _languages;

#if UNITY_EDITOR
        public void AddLanguage()
        {
            GenericMenu menu = new();

            foreach ( var language in GetAllLanguages() )
                menu.AddItem( new GUIContent( language.Name ), false, AddLanguage, language );

            menu.ShowAsContext();

            List<Language> GetAllLanguages()
            {
                return LanguageProperties.Languages
                    .Where(el => _languages.Exists(lang => lang.Language.Code == el.Code) == false)
                    .OrderBy(el => el.Name).ToList();
            }

            void AddLanguage( object languageObject )
            {
                var language = (languageObject is Language o ? o : default);
                
                if (string.IsNullOrEmpty(language.Code))
                {
                    Debug.LogError( "[LocalizationDatabase]: Wrong language!" );
                    return;
                }

                _languages.Add(new LocalizedLanguage()
                {
                    Language = new Language()
                        { Code = language.Code, Name = language.Name, NativeName = language.NativeName },
                    Text = new ()
                });
            }
        }
        
        public async void Translate()
        {
            var baseLanguage = _defaultLanguage;

            if (baseLanguage == null)
            {
                Debug.LogError( "[LocalizationDatabase]: Wrong default language!" );
                return;
            }
            
            foreach (var language in _languages)
            {
                foreach (var text in baseLanguage.Text)
                {
                    var translateText = await GameSDK.Localization.Translate.Process(baseLanguage.Language.Code,language.Language.Code, text.Value);

                    var textTranslated = language.Text.FirstOrDefault(el => el.Key == text.Key);

                    if (textTranslated == null)
                    {
                        textTranslated = new LocalizedText() { Key = text.Key };
                        language.Text.Add(textTranslated);
                    }

                    textTranslated.Value = translateText;
                }
            }
        }
        
        public async void TranslateConcrete()
        {
            GenericMenu menu = new();

            foreach ( var language in GetAllLanguages() )
                menu.AddItem( new GUIContent( language.Language.Name ), false, TranslateConcreteLanguage, language );

            menu.ShowAsContext();

            List<LocalizedLanguage> GetAllLanguages()
            {
                return _languages;
            }

            async void TranslateConcreteLanguage( object languageObject )
            {
                var language = (languageObject is LocalizedLanguage o ? o : null);
                
                if (language == null)
                {
                    Debug.LogError( "[LocalizationDatabase]: Wrong language!" );
                    return;
                }

                var defaultLanguage = _defaultLanguage;

                foreach (var text in defaultLanguage.Text)
                {
                    var translateText = await GameSDK.Localization.Translate.Process(defaultLanguage.Language.Code,language.Language.Code, text.Value);

                    var textTranslated = language.Text.FirstOrDefault(el => el.Key == text.Key);

                    if (textTranslated == null)
                    {
                        textTranslated = new LocalizedText() { Key = text.Key };
                        language.Text.Add(textTranslated);
                    }

                    textTranslated.Value = translateText;
                }
            }
        }
        
        public void ChangeDefaultLanguage()
        {
            GenericMenu menu = new();

            foreach ( var language in GetAllLanguages() )
                menu.AddItem( new GUIContent( language.Language.Name ), false, ChangeLanguage, language );

            menu.ShowAsContext();

            List<LocalizedLanguage> GetAllLanguages()
            {
                return _languages;
            }

            void ChangeLanguage( object languageObject )
            {
                var language = (languageObject is LocalizedLanguage o ? o : null);
                
                if (language == null)
                {
                    Debug.LogError( "[LocalizationDatabase]: Wrong language!" );
                    return;
                }

                var defaultLanguage = _defaultLanguage;

                _defaultLanguage = language;

                if (_languages.Contains(language))
                {
                    _languages.Remove(language);
                }

                if (_languages.Exists(el => el.Language.Code == defaultLanguage.Language.Code) == false)
                {
                    _languages.Add(defaultLanguage);
                }
            }
        }
#endif
    }

    [System.Serializable]
    internal class LocalizedLanguage
    {
        #if UNITY_EDITOR
        public string Name => Language.Name;
        #endif
        public Language Language;
        public List<LocalizedText> Text;
    }

    [System.Serializable]
    internal class LocalizedText
    {
        public string Key;
        public string Value;
    }
}