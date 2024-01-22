using UnityEngine;

namespace GameSDK.Localization
{
    public class LocalizationLanguageChanger : MonoBehaviour
    {
        [SerializeField] private string _code;

        public void ChangeLanguage()
        {
            Localization.ChangeLanguage(_code);
        }
    }
}