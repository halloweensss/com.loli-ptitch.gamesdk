using System;

namespace GameSDK.RemoteConfigs
{
    internal class ObjectCacheInfo : ICachedInfo
    {
        protected readonly WeakReference<object> _weakReference;

        protected ObjectCacheInfo(string key, object obj)
        {
            Key = key;
            _weakReference = new WeakReference<object>(obj);
        }

        public string Key { get; }

        public virtual bool IsAlive
        {
            get
            {
                if(_weakReference.TryGetTarget(out var target) == false)
                    return false;

                return target != null;
            }
        }

        public virtual Type ValueType => null;
        public virtual object Value => this;
        public object Target => _weakReference.TryGetTarget(out var target) ? target : null;

        public virtual void TrySet(object value)
        {
        }
    }
}