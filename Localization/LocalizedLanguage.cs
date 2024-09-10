using System.Collections.Generic;

namespace GameSDK.Localization
{
    [System.Serializable]
    public class LocalizedLanguage
    {
#if UNITY_EDITOR
        public string Name => Language.Name;
#endif
        public Language Language;
        public List<LocalizedText> Text = new();
    }
}