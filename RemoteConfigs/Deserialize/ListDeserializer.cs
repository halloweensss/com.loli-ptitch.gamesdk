using System;
using Newtonsoft.Json;

namespace GameSDK.RemoteConfigs.Deserialize
{
    public class ListDeserializer : IDeserializerObject
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        public bool Check(object obj, Type type)
        {
            return obj != null && typeof(System.Collections.IList).IsAssignableFrom(type);
        }

        public object Deserialize(string value, object obj, Type type)
        {
            var elementType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);

            var tempArray = (Array)JsonConvert.DeserializeObject(value, elementType.MakeArrayType(), _settings);
            if (tempArray == null)
                return obj;

            var list = (System.Collections.IList)obj;
            list.Clear();
            foreach (var item in tempArray)
                list.Add(item);

            return obj;
        }
    }
}