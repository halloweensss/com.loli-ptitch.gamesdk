using UnityEngine;

namespace GameSDK.Localization
{
    [System.Serializable]
    public class LocalizedText
    {
        public string Key;
        [Multiline] public string Value;
    }
}