using System;
using TMPro;
using UnityEngine;

namespace GameSDK.Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizationTMP : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private TMP_Text _text;

        private void Awake()
        {
            Localization.AddTMPText(_key, _text);
        }

        private void OnDestroy()
        {
            Localization.RemoveTMPText(_key, _text);
        }

        private void OnValidate()
        {
            if(_text != null) return;
            
            _text = GetComponent<TMP_Text>();
        }
    }
}