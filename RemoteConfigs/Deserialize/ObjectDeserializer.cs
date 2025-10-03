using System;
using Newtonsoft.Json;

namespace GameSDK.RemoteConfigs.Deserialize
{
    public class ObjectDeserializer : IDeserializerObject
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        public bool Check(object obj, Type type) => obj != null;

        public object Deserialize(string value, object obj, Type type)
        { 
            JsonConvert.PopulateObject(value, obj, _settings);
            return obj;
        }
    }
}