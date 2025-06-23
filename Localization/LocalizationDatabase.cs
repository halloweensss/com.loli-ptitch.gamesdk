using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameSDK.Core.PropertyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GameSDK.Localization
{
    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "GameSDK/LocalizationDatabase")]
    public class LocalizationDatabase : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] [ArrayElementTitle("Language", "Name")] 
        private List<LocalizedLanguage> _languages;
        
#if UNITY_EDITOR
        [SerializeField] private string _sheetID;
        [SerializeField] private string _sheetGID;
#endif

        internal string Id => _id;
        public List<LocalizedLanguage> Languages => _languages;

#if UNITY_EDITOR
        public void AddLanguage()
        {
            GenericMenu menu = new();

            foreach ( var language in GetAllLanguages() )
                menu.AddItem( new GUIContent( language.Name ), false, AddLanguageInternal, language );

            menu.ShowAsContext();

            List<Language> GetAllLanguages()
            {
                return LanguageProperties.Languages
                    .Where(el => _languages.Exists(lang => lang.Language.Code == el.Code) == false)
                    .OrderBy(el => el.Name).ToList();
            }

            void AddLanguageInternal( object languageObject )
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
        
        public async void LoadFormSheet()
        {
            string newLine = System.Environment.NewLine;

            string url = $"https://docs.google.com/spreadsheets/d/{_sheetID}/export?format=tsv&gid={_sheetGID}";

            using UnityWebRequest webRequest = UnityWebRequest.Get( url );

            var asyncOp = webRequest.SendWebRequest();
            
            string title = "Downloading";
            string info = $"Downloading \"{_sheetID}\" sheet";
                
            int progressId = Progress.Start( title, info );
                
            while ( asyncOp.isDone == false )
            {
                Progress.Report( progressId, asyncOp.progress );
                await Task.Yield();
            }
                
            Progress.Remove( progressId );
            
            if ( webRequest.result != UnityWebRequest.Result.Success )
            {
                Debug.LogError( $"[LocalizationDatabase]: {webRequest.error}" );
                return;
            }

            string rawdata = webRequest.downloadHandler.text;
            string[] lines = rawdata.Split(newLine);
            
            if (lines.Length == 0)
            {
                Debug.LogError("[LocalizationDatabase]: Sheet is empty!");
                return;
            }
            
            _languages.Clear();
            
            string[] headers = lines[0].Split('\t');

            Dictionary<string, LocalizedLanguage> languages = new(headers.Length - 1);

            Dictionary<string, Language> languagesCode =
                LanguageProperties.Languages.ToDictionary(el => el.Code, el => el);
            
            List<string> languageCodes = new(headers.Length - 1);
            
            for (int i = 1; i < headers.Length; i++)
            {
                var header = headers[i].Trim();
                var languageCode = header.Split("_")[0];

                if (languagesCode.TryGetValue(languageCode, out var language) == false)
                {
                    Debug.LogError( $"[LocalizationDatabase]: Wrong language: {languageCode}" );
                    continue;
                }
                
                LocalizedLanguage localizedLanguage = new()
                {
                    Language = language,
                    Text = new(64)
                };
                
                _languages.Add(localizedLanguage);
                languages.Add(languageCode, localizedLanguage);
                languageCodes.Add(languageCode);
            }
            
            title = "Importing";
            info = $"Importing \"{_sheetID}\" sheet";
                
            progressId = Progress.Start( title, info );
            
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue; // Пропускаем комментарии и пустые строки

                string[] values = line.Split('\t');

                if (values.Length != headers.Length)
                {
                    Debug.LogError("CSV row does not match header length at line " + i);
                    continue;
                }

                var key = values[0].ToUpper();

                for (int j = 1; j < values.Length; j++)
                {
                    var code = languageCodes[j - 1];
                    if (languages.TryGetValue(code, out var language) == false)
                    {
                        Debug.LogError( $"[LocalizationDatabase]: Wrong language: {code}" );
                        continue;
                    }
                    
                    language.Text.Add(new LocalizedText { Key = key, Value = values[j] });
                }
                
                Progress.Report( progressId, (1f * i) / (lines.Length - 1) );
                await Task.Yield();
            }
            
            Progress.Remove( progressId );
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
        
        public void Clear()
        {
            _languages.Clear();
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
#endif
    }
}