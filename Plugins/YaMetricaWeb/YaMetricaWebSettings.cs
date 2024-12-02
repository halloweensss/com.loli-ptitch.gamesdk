using UnityEngine;

namespace GameSDK.Plugins.YaMetricaWeb
{
    [CreateAssetMenu(fileName = "YaMetricaWebSettings", menuName = "GameSDK/Plugins/YaMetricaWeb/Settings")]
    public class YaMetricaWebSettings : ScriptableObject
    {
        [SerializeField] private bool _isEnable = true;
        [SerializeField] private bool _debugMode = true;
        [SerializeField] private string _counterId;
        [SerializeField] private bool _sendPageOpenInInitialize;
        [SerializeField] private MetricaParameters _parameters;

        public bool IsEnable => _isEnable;
        public string CounterId => _counterId;
        public bool SendPageOpenInInitialize => _sendPageOpenInInitialize;

        public MetricaParameters Parameters => _parameters;

        public string GetJsonParameters()
        {
            return JsonUtility.ToJson(_parameters);
        }

        public string GetInitializeParameters()
        {
            var parameters = new InitializeParameters
            {
                CounterId = _counterId,
                DebugMode = _debugMode
            };

            return JsonUtility.ToJson(parameters);
        }
    }
}