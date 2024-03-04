using System;

namespace GameSDK.Core.Tools
{
    [Serializable]
    public struct KeyValueStruct<T> where T : IComparable
    {
        public string key;
        public T value;
        
        public KeyValueStruct(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}