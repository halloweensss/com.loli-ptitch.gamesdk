using System;

namespace GameSDK.RemoteConfigs
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RemoteValueAttribute : Attribute
    {
        public readonly string Key;

        public RemoteValueAttribute(string key) => Key = key;
    }
}