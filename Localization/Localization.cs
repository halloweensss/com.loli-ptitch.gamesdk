using System;
using System.Collections.Generic;
using System.Linq;
using GameSDK.Core;
using TMPro;
using UnityEngine;

namespace GameSDK.Localization
{
    public class Localization : IGameService
    {
        private static readonly Localization Instance = new();
        private readonly Dictionary<string, LocalizationDatabase> _databases = new(32);
        private readonly Dictionary<string, LocalizationDatabase> _keysDatabase = new(32);
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new(32);
        private readonly Dictionary<string, HashSet<TMP_Text>> _tmpTexts = new(32);

        private string _currentLanguage;
        private Dictionary<string, Language> _languages = new(32);

        public string ServiceName => "Localization";
        public static event Action<string> OnLanguageChanged;
        public static event Action<string, string> OnCurrentLanguageKeyAdded;

        public static void AddDatabase(LocalizationDatabase database)
        {
            Instance.AddDatabaseInternal(database);
        }

        public static string GetValue(string key)
        {
            return Instance.GetValueInternal(key);
        }

        public static void ChangeLanguage(string code)
        {
            Instance.ChangeLanguageInternal(code);
        }

        public static void AddTMPText(string key, TMP_Text text)
        {
            Instance.AddTMPTextInternal(key, text);
        }

        public static void RemoveTMPText(string key, TMP_Text text)
        {
            Instance.RemoveTMPTextInternal(key, text);
        }

        public static void UpdateTMPTexts()
        {
            Instance.UpdateTMPTextsInternal();
        }

        private void AddTMPTextInternal(string key, TMP_Text text)
        {
            if (text == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[Localization]: TMP Text for add is NULL!");
#endif
                return;
            }

            if (_tmpTexts.TryGetValue(key, out var texts) == false)
            {
                texts = new HashSet<TMP_Text>();
                _tmpTexts.Add(key, texts);
            }

            if (texts.Contains(text))
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[Localization]: TMP Text already has been added with key [{key}]!");
#endif
                return;
            }

            texts.Add(text);

            text.text = GetValue(key);
        }

        private void RemoveTMPTextInternal(string key, TMP_Text text)
        {
            if (text == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[Localization]: TMP Text for remove is NULL!");
#endif
                return;
            }

            if (_tmpTexts.TryGetValue(key, out var texts) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[Localization]: Key [{key}] for remove TMP Text is not contains!");
#endif
                return;
            }

            if (texts.Contains(text) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[Localization]: TMP Text for remove is not contain with id [{key}]!");
#endif
                return;
            }

            texts.Remove(text);
        }

        private void UpdateTMPTextsInternal()
        {
            foreach (var text in _tmpTexts)
                UpdateTMPTextsInternal(text.Key);
        }

        private void UpdateTMPTextsInternal(string id)
        {
            if (_tmpTexts.TryGetValue(id, out var texts) == false) return;

            foreach (var text in texts)
            {
                if (text == null) continue;

                text.text = GetValue(id);
            }
        }

        private void ChangeLanguageInternal(string code)
        {
            if (_currentLanguage == code) return;

            if (_languages.ContainsKey(code) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[Localization]: Language is not contains with code [{code}]!");
#endif
                return;
            }

            _currentLanguage = code;

            UpdateTMPTexts();

            OnLanguageChanged?.Invoke(_currentLanguage);
        }

        private void AppendLocalization(LocalizationDatabase database, LocalizedLanguage localizedLanguage)
        {
            if (_localizations.TryGetValue(localizedLanguage.Language.Code, out var localization) == false)
            {
                localization = new Dictionary<string, string>(localizedLanguage.Text.Count);
                _localizations.Add(localizedLanguage.Language.Code, localization);
            }

            foreach (var text in localizedLanguage.Text)
            {
                if (localization.TryAdd(text.Key, text.Value) == false)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[Localization]: Localized text [{text.Value}] has already been added with key [{text.Key}] and language [{localizedLanguage.Language.Name}]!");
#endif
                }

                _keysDatabase.TryAdd(text.Key, database);

                if (_currentLanguage == localizedLanguage.Language.Code)
                {
                    UpdateTMPTextsInternal(text.Key);

                    OnCurrentLanguageKeyAdded?.Invoke(text.Key, text.Value);
                }
            }
        }

        private void AddDatabaseInternal(LocalizationDatabase database)
        {
            if (_databases.TryAdd(database.Id, database) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Localization]: Database {database.Id} has already been added!");
#endif
                return;
            }

            foreach (var language in database.Languages)
                AppendLocalization(database, language);
        }

        private string GetValueInternal(string key)
        {
            if (string.IsNullOrEmpty(_currentLanguage))
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[Localization]: Localization language not selected!");
#endif
                return key;
            }

            if (TryGetValueFromLanguage(key, _currentLanguage, out var text))
                return text;

#if UNITY_EDITOR
            Debug.LogWarning(
                $"[Localization]: Text with key [{key}] is not contains in language [{_currentLanguage}]!");
#endif

            if (_keysDatabase.TryGetValue(key, out var database) == false)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    $"[Localization]: Text with key [{key}] is not contains in localizations!");
#endif
                return key;
            }

#if UNITY_EDITOR
            Debug.LogWarning(
                $"[Localization]: Text with key [{key}] is not contains in default language in database [{database.Id}]!");
#endif

            return key;


            bool TryGetValueFromLanguage(string key, string languageCode, out string text)
            {
                text = key;

                if (_localizations.TryGetValue(languageCode, out var localization) == false) return false;
                if (localization.TryGetValue(key, out text)) return true;

                return false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeInternal()
        {
            Instance._languages = LanguageProperties.Languages.ToDictionary(el => el.Code, el => el);
            Instance._languages.TryGetValue("en", out var language);
            Instance._currentLanguage = language.Code;
        }
    }
}