using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using GameSDK.Core;
using GameSDK.Core.Properties;
using GameSDK.Time;
using UnityEngine;

namespace Plugins.YaGames.Time
{
    public class YaTime : ITimeApp
    {
        private static readonly YaTime _instance = new();
        public PlatformServiceType PlatformService => PlatformServiceType.YaGames;
        public InitializationStatus InitializationStatus => InitializationStatus.Initialized;
        
        private bool _processing = false;
        private long _lastTimestamp = 0;
        
        public Task<long> GetTimestamp()
        {
#if !UNITY_EDITOR
            _processing = true;
            
            YaGamesServerTime(Callback);
            
            while(_processing)
                Task.Yield();
            
            _processing = false;
            return Task.FromResult(_lastTimestamp);
#endif
            return Task.FromResult(0L);
            
            [MonoPInvokeCallback(typeof(Action))]
            static void Callback(string result)
            {
                _instance._lastTimestamp = long.Parse(result);
                _instance._processing = false;
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterInternal()
        {
            GameSDK.Time.Time.Instance.Register(_instance);
        }
        
        [DllImport("__Internal")]
        private static extern void YaGamesServerTime(Action<string> callback);
    }
}