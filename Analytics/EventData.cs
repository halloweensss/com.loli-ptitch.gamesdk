using System.Collections.Generic;

namespace GameSDK.Analytics
{
    public class EventData
    {
        private string _id = string.Empty;
        private AnalyticsProviderType _providers = AnalyticsProviderType.Default;
        private readonly Dictionary<string, object> _parameters = new(16);
        
        public string Id => _id;
        public AnalyticsProviderType Providers => _providers;
        public Dictionary<string, object> Parameters => _parameters;

        internal void Initialize(string id, Dictionary<string, object> parameters, AnalyticsProviderType providers = AnalyticsProviderType.Default)
        {
            Clear();
            
            _id = id;
            _providers = providers;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    _parameters[parameter.Key] = parameter.Value;
                }
            }
        }

        internal void RemoveProvider(AnalyticsProviderType provider) => 
            _providers &= ~provider;

        private void Clear()
        {
            _id = string.Empty;
            _providers = AnalyticsProviderType.Default;
            _parameters.Clear();
        }
        
        internal static EventData Create() => new();
        internal static void Clear(EventData data) => data.Clear();
    }
}