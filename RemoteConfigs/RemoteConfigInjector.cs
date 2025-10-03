using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GameSDK.RemoteConfigs
{
    public class RemoteConfigInjector
    {
        private readonly ConditionalWeakTable<object, List<ICachedInfo>> _cachedObjectValues = new();
        private readonly Dictionary<string, List<ICachedInfo>> _cachedValues = new(32);

        private readonly List<IDeserializerObject> _deserializerObjects = new()
        {
            new Deserialize.EmptyDeserializer(),
            new Deserialize.ArrayDeserializer(),
            new Deserialize.ListDeserializer(),
            new Deserialize.ObjectDeserializer(),
        };
        
        private readonly RemoteConfigs _remoteConfigs;

        public RemoteConfigInjector(RemoteConfigs remoteConfigs)
        {
            _remoteConfigs = remoteConfigs;
        }

        public void Register(params object[] targets)
        {
            foreach (var obj in targets)
                Register(obj);
        }

        public void Register(object target)
        {
            if (_cachedObjectValues.TryGetValue(target, out _))
            {
                Debug.LogWarning($"[GameSDK.RemoteConfigs]: Object {target.GetType().Name} already registered.");
                return;
            }

            RegisterFields(target);
            RegisterProperties(target);
            UpdateValues(target);
        }

        public void UpdateValues(params object[] targets)
        {
            foreach (var obj in targets)
                UpdateValues(obj);
        }

        public void UpdateValues(object target)
        {
            if (_cachedObjectValues.TryGetValue(target, out _) == false)
                Register(target);

            if (_cachedObjectValues.TryGetValue(target, out var cachedInfos) == false)
                return;

            foreach (var cachedInfo in cachedInfos)
            {
                UpdateCachedInfo(cachedInfo);
            }
        }

        public void UpdateValues(string key)
        {
            if (_cachedValues.TryGetValue(key, out var cachedInfos) == false)
                return;

            foreach (var cachedInfo in cachedInfos)
            {
                UpdateCachedInfo(cachedInfo);
            }
            
            CleanupDeadInfos(key);
        }

        public void UpdateValues()
        {
            foreach (var cachedInfos in _cachedValues.Values)
            foreach (var cachedInfo in cachedInfos)
            {
                UpdateCachedInfo(cachedInfo);
            }
            
            CleanupDeadInfos();
        }

        private void UpdateCachedInfo(ICachedInfo cachedInfo)
        {
            if (cachedInfo.IsAlive == false)
                return;

            var key = cachedInfo.Key;

            if (_remoteConfigs.TryGetValueInternal(key, out var remoteValue) == false)
            {
                Debug.LogWarning($"[GameSDK.RemoteConfigs]: Key {key} not found in remote values.");
                return;
            }

            InjectValue(cachedInfo, remoteValue);
        }

        private void InjectValue(ICachedInfo cachedInfo, RemoteConfigValue remoteValue)
        {
            var valueType = cachedInfo.ValueType;
            var typeCode = Type.GetTypeCode(valueType);

            var value = cachedInfo.Value;

            if (typeCode == TypeCode.Object)
            {
                var stringValue = remoteValue.StringValue;

                foreach (var deserializerObject in _deserializerObjects)
                {
                    if (deserializerObject.Check(value, valueType) == false)
                        continue;

                    value = deserializerObject.Deserialize(stringValue, value, valueType);
                    break;
                }

                cachedInfo.TrySet(value);
                return;
            }

            if (remoteValue.TryGetValue(valueType, out var newValue) == false)
            {
                Debug.LogWarning(
                    $"[GameSDK.RemoteConfigs]: Key {cachedInfo.Key} not found in remote values with type {valueType.Name}.");
                return;
            }

            if (newValue == null)
                return;

            if (value == newValue)
                return;

            cachedInfo.TrySet(newValue);
        }

        private void RegisterProperties(object obj)
        {
            var properties = obj.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy
            );

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<RemoteValueAttribute>(true);

                if (attribute == null)
                    continue;

                var key = attribute.Key;
                var cachedProperty = new PropertyCachedInfo(key, obj, property);

                RegisterCachedInfo(cachedProperty);
            }
        }

        private void RegisterFields(object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                                 BindingFlags.NonPublic |
                                                 BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<RemoteValueAttribute>(true);

                if (attribute == null)
                    continue;

                var key = attribute.Key;
                var cachedProperty = new FieldCachedInfo(key, obj, field);

                RegisterCachedInfo(cachedProperty);
            }
        }
        
        private void RegisterCachedInfo(ICachedInfo cachedInfo)
        {
            var key = cachedInfo.Key;
            if (_cachedValues.TryGetValue(key, out var cachedInfos) == false)
            {
                cachedInfos = new List<ICachedInfo>(4);
                _cachedValues.Add(key, cachedInfos);
            }

            cachedInfos.Add(cachedInfo);

            var obj = cachedInfo.Target;

            if (_cachedObjectValues.TryGetValue(obj, out cachedInfos) == false)
            {
                cachedInfos = new List<ICachedInfo>(4);
                _cachedObjectValues.Add(obj, cachedInfos);
            }

            cachedInfos.Add(cachedInfo);
        }

        private void CleanupDeadInfos()
        {
            foreach (var key in _cachedValues.Keys)
            {
                CleanupDeadInfos(key);
            }
        }

        private void CleanupDeadInfos(string key)
        {
            if (_cachedValues.TryGetValue(key, out var list) == false)
                return;

            int aliveIndex = 0;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].IsAlive == false)
                    continue;

                if (aliveIndex != i)
                    list[aliveIndex] = list[i];
                aliveIndex++;
            }

            if (aliveIndex < list.Count)
                list.RemoveRange(aliveIndex, list.Count - aliveIndex);
        }
    }
}