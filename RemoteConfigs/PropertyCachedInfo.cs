using System;
using System.Reflection;

namespace GameSDK.RemoteConfigs
{
    internal class PropertyCachedInfo : ObjectCacheInfo
    {
        private readonly PropertyInfo _property;
        public override Type ValueType => _property.PropertyType;
        
        public override object Value => _weakReference.TryGetTarget(out var target) == false ? null : _property.GetValue(target);

        public PropertyCachedInfo(string key, object obj, PropertyInfo propertyInfo) : base(key, obj)
        {
            _property = propertyInfo;
        }

        public override void TrySet(object value)
        {
            if (_weakReference.TryGetTarget(out var target) == false)
                return;

            if (_property.CanWrite == false)
                return;

            _property.SetValue(target, value);
        }
    }
}