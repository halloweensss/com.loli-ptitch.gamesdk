using System;

namespace GameSDK.RemoteConfigs
{
    internal interface ICachedInfo
    {
        string Key { get; }
        bool IsAlive { get; }
        Type ValueType { get; }
        object Value { get; }
        object Target { get; }
        void TrySet(object value);
    }
}