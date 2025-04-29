using System;
using System.Reflection;

namespace GameSDK.RemoteConfigs
{
    internal class FieldCachedInfo : ObjectCacheInfo
    {
        private readonly FieldInfo _field;

        public override Type ValueType => _field.FieldType;
        public override object Value => _weakReference.TryGetTarget(out var target) == false ? null : _field.GetValue(target);

        public FieldCachedInfo(string key, object obj, FieldInfo fieldInfo) : base(key, obj)
        {
            _field = fieldInfo;
        }
        
        public override void TrySet(object value)
        {
            if (_weakReference.TryGetTarget(out var target) == false)
                return;
            
            _field.SetValue(target, value);
        }
    }
}